--[[
MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
]]--

local component = require("component")
local computer = require("computer")
local robot = require("robot")
local shell = require("shell")
local sides = require("sides")
local event = require("event")
local thread = require("thread")

local m = component.modem
local inv = component.inventory_controller
local gpsEnabled = component.isAvailable("navigation")

SERVER_ADDR = "086c88d8-6dbc-4ae2-9136-311bdd180482"
SERVER_PORT = 32325

if not component.isAvailable("robot") then
  io.stderr:write("can only run on robots")
  return
end

--[[ ERPCraft ]]--

--- Devuelve un table (array) con la cadena separada por el delimitador especificado
local function strSplit(str, delim)
  local split = {}
  local substr = str
  
  repeat
    table.insert(split, string.sub(substr, 0, string.find(substr, delim) - 1))
    substr = string.sub(substr, string.find(substr, delim) + 1, string.len(substr))
  until not string.find(substr, delim)
  table.insert(split, substr)
  
  return split
end

m.open(SERVER_PORT)

--- Avisa al servidor de que el robot está disponible
local function robotOnline()
  local str = "robOnline--" .. robot.name() .. ";" .. math.floor(computer.energy()) .. ";" .. math.floor(computer.maxEnergy()) .. ";"
  if not gpsEnabled then
    str = str .. "0;0;0"
  else
    local posX, posY, posZ = component.navigation.getPosition()
    str = str .. math.floor(posX) .. ";" .. math.floor(posY) .. ";" .. math.floor(posZ)
  end
  m.send(SERVER_ADDR, SERVER_PORT, str)
end

local function robotBateria()
  local str = "robBat--" .. math.floor(computer.energy())
  m.send(SERVER_ADDR, SERVER_PORT, str)
end

--- Enviar un log al servidor
-- @param titulo string texto corto
-- @param mensaje string texto largo
local function l(titulo, mensaje)
  m.send(SERVER_ADDR, SERVER_PORT, "robLog--" .. titulo .. "@@" .. mensaje)
end

--- Marcar la orden de minado actual como finalizada
local function ordenMinadoFin()
  local str = "robOrdenMinadoFin--"
  m.send(SERVER_ADDR, SERVER_PORT, str)
end

--- Envía el contenido del inventario al servidor
-- @param drop true = para la orden de minado (terminado, dropeando), false = para el robot (minando)
local function setRobotInventario(drop, numSlots)
  local str = ""
  numSlots = numSlots or 16
  for i = 1,numSlots,1 do
    local slot = inv.getStackInInternalSlot(i)
    if slot == nil then
      str = str .. "@0@;"
    else
	  if (slot.oreNames.n > 0) then
        str = str .. slot.name .. "@" .. math.floor(slot.size) .. "@" .. slot.oreNames[1] .. ";"
	  else
	    str = str .. slot.name .. "@" .. math.floor(slot.size) .. "@;"
	  end
    end
  end
  
  if drop then -- se está dropeando todo lo que hay en la orden de minado
    m.send(SERVER_ADDR, SERVER_PORT, "robOrdenMinadoInv--" .. str)
  else -- se informa del inventario del robot
    m.send(SERVER_ADDR, SERVER_PORT, "robInventario--" .. str)
  end
end

--- Envía la posición GPS actual dle robot al servidor
local function setRobotGps()
  if not gpsEnabled then
    return
  end
  local posX, posY, posZ = component.navigation.getPosition()
  local str = "robGPS--" .. math.floor(posX) .. "@" .. math.floor(posY) .. "@" .. math.floor(posZ)
  
  m.send(SERVER_ADDR, SERVER_PORT, str)
end

--- Función a ejecutar en otro hilo que responderá los pings del servidor
local function responderPing()
  while true do
    local _, _, from, port, _, message = event.pull("modem_message")
    print("Got a message from " .. from .. " on port " .. port .. ": " .. tostring(message))
  
    if (message == "PING") then
      m.send(SERVER_ADDR, SERVER_PORT, "PONG--")
	elseif (string.sub(message, 0, 3) == "CMD") then
	  f = load(string.sub(message, 6))
	  f()
    end
  end
end

-- INICIALIZACIÓN

robotOnline()
l("ONLINE", "Robot online!")

--[[ /ERPCraft ]]--



local options = {}
local size = 0

local r = component.robot
local x, y, z, f = 0, 0, 0, 0
local ordenGPS = {}
local energiaRecarga = 0 -- declarado al recibir orden del servidor, con bateria por debajo de este limite recargar
local updateOrden -- forward declaration
local modoMinado = "O" -- modos de recarga del arma O = Optimo, E = Economico
local dropSide = "1" -- "0" = left, "1" = forward, "3" = right
local dropping = false -- avoid recursing into drop()
local delta = {[0] = function() x = x + 1 updateOrden() end, [1] = function() y = y + 1 updateOrden() end,
               [2] = function() x = x - 1 updateOrden() end, [3] = function() y = y - 1 updateOrden() end}

--[[ OBTENER ORDEN DE MIANDO ]]--

--- Obtiene una orden de minado del servior y ajusta las variables globales necesarias
local function getOrdenMinado()
  m.send(SERVER_ADDR, SERVER_PORT, "robOrdenMinado--");
  local _, _, _, _, _, message = event.pull("modem_message")
  
  if (message == "PING") then
    m.send(SERVER_ADDR, SERVER_PORT, "PONG--")
	return getOrdenMinado()
  elseif (string.sub(message, 0, 3) == "CMD") then
	f = load(string.sub(message, 6))
	f()
	return getOrdenMinado()
  end
  if message == "IDLE" then
    return false
  else
    -- decodificar el mensaje
    print("message " .. message)
    local str = strSplit(message, ";")
    --local s, posX, posY, posZ, facing, gpsX, gpsY, gpsZ, energiaRecarga, modoMinado, shutdown = message:match("([^;]+);([^;]+)")
    
    size = tonumber(str[1])
    x = tonumber(str[2])
    y = tonumber(str[3])
    z = tonumber(str[4])
    f = tonumber(str[5])
	ordenGPS[1] = tonumber(str[6])
	ordenGPS[2] = tonumber(str[7])
	ordenGPS[3] = tonumber(str[8])
	energiaRecarga = tonumber(str[9])
	modoMinado = str[10]
    if tonumber(str[11]) == 1 then
      options.s = true
    end
	dropSide = str[12]
  
    print("size " .. size .. " x " .. x .. " y " .. y .. " z " .. z .. " f " .. f .. " modoMinado " .. modoMinado)
	return true
  end
end

-- ACTUALIZACIÓN ORDEN DE MINADO

--- Actualiza el estado de la orden de minado (x,y,z,f)
-- Tiene forward declaration
function updateOrden()
  m.send(SERVER_ADDR, SERVER_PORT, "robOrdenMinadoUpdate--" .. math.floor(x) .. ";" .. math.floor(y) .. ";" .. math.floor(z) .. ";" .. math.floor(f))
end

--[[ /OBTENER ORDEN DE MIANDO ]]--

-- Inicializar las variables de minado de la orden del servidor
if not getOrdenMinado() then
  os.exit()
end
print("Iniciando orden de minado size " .. size)
setRobotInventario()
setRobotGps()
--os.exit()																												---- QUITAR!!!! -----

thread.create(function()
  responderPing()
end)

thread.create(function()
  while true do
    robotBateria()
    setRobotInventario()
	setRobotGps()
    os.sleep(1)
  end
end)

local function turnRight()
  robot.turnRight()
  f = (f + 1) % 4
  updateOrden()
end

local function turnLeft()
  robot.turnLeft()
  f = (f - 1) % 4
  updateOrden()
end

local function turnTowards(side)
  if f == side - 1 then
    turnRight()
  else
    while f ~= side do
      turnLeft()
    end
  end
end

local checkedDrop -- forward declaration
local clearBlock -- forward declaration

if modoMinado == "O" then -- Optimo
  clearBlock = function(side, cannotRetry)
  setRobotInventario()
   while r.suck(side) do
     checkedDrop()
   end
   local result, reason = r.swing(side)
   if result then
     checkedDrop()
     else
       local _, what = r.detect(side)
       if cannotRetry and what ~= "air" and what ~= "entity" then
         return false
       end
     end
     return true
  end
elseif modoMinado == "E" then -- Economico
  clearBlock = function(side, cannotRetry)
  --setRobotInventario()
  while r.suck(side) do
     checkedDrop()
   end
   local optimo = component.geolyzer.analyze(side).harvestLevel > 1
   if optimo then
     robot.select(16)
	 inv.equip()
	 robot.select(1)
   end
   local result, reason = r.swing(side)
   if optimo then
     robot.select(16)
     inv.equip()
	 robot.select(1)
   end
   if result then
     checkedDrop()
     else
       local _, what = r.detect(side)
       if cannotRetry and what ~= "air" and what ~= "entity" then
         return false
       end
     end
     return true
  end
end

local function tryMove(side)
  side = side or sides.forward
  local tries = 10
  while not r.move(side) do
    tries = tries - 1
    if not clearBlock(side, tries < 1) then
      return false
    end
  end
  if side == sides.down then
    z = z + 1
  elseif side == sides.up then
    z = z - 1
  else
    delta[f]()
  end
  return true
end

local function moveTo(tx, ty, tz, backwards)
  local axes = {
    function()
      while z > tz do
        tryMove(sides.up)
      end
      while z < tz do
        tryMove(sides.down)
      end
    end,
    function()
      if y > ty then
        turnTowards(3)
        repeat tryMove() until y == ty
      elseif y < ty then
        turnTowards(1)
        repeat tryMove() until y == ty
      end
    end,
    function()
      if x > tx then
        turnTowards(2)
        repeat tryMove() until x == tx
      elseif x < tx then
        turnTowards(0)
        repeat tryMove() until x == tx
      end
    end
  }
  if backwards then
    for axis = 3, 1, -1 do
      axes[axis]()
    end
  else
    for axis = 1, 3 do
      axes[axis]()
    end
  end
end

--- Función que se ejecuta cuando se comprueba si hay que hacer drop del inventario,
-- que comprueba los nieveles de energía del robot, con los niveles de la orden de minado
-- @return true si necesita volver al punto inicial a regargar, false si no lo necesita
local function checkEnergia()
  return (computer.energy() <= energiaRecarga)
end

--- Función que se ejecuta cuando se comprueba si hay que hacer drop del inventario,
-- que comprueba que la herramienta actual aún tiene durabilidad
-- @return true si hay una herramienta equipada con durabilidad restante,
-- false si no hay herramienta o está agotada
local checkTool -- function - forward declaration

-- Establecer que funcion utilitzar segun el modo de minado, par no calcularlo cada vez
if modoMinado == "E" then
  checkTool = function()
    return robot.durability() == nil or robot.durability() == 0 or robot.count(16) == 0
  end
elseif modoMinado == "O" then
  checkTool = function()
    return robot.durability() == nil or robot.durability() == 0
  end
end

local function recargarTool()
  local numSlots = inv.getInventorySize(1) -- top
  
  if modoMinado == "O" then -- Optimo
    for i = 1, numSlots, 1 do
	  if (inv.getStackInSlot(1, i) ~= nil) then
	    robot.select(1)
	    inv.suckFromSlot(1, i)
		inv.equip()
		break
	  end
	end
	inv.equip()
  elseif modoMinado == "E" then -- Economico
    -- ¿se ha agotado la herramienta optima o economica?
	if robot.durability() == nil or robot.durability() == 0 then
	-- cambiar la herramienta economica
	  for i = 1, numSlots, 1 do
	    local slot = inv.getStackInSlot(1, i)
	    if (slot ~= nil and slot.name == "minecraft:stone_pickaxe") then
	      inv.suckFromSlot(1, i)
		  robot.select(1)
		  inv.equip()
		  robot.dropDown()
	  	  break
	    end
	  end
	
	
	elseif robot.count(16) == 0 then
	  -- cambiar la herramienta optima
	  for i = 1, numSlots, 1 do
	    local slot = inv.getStackInSlot(1, i)
	    if (slot ~= nil and (slot.name == "minecraft:iron_pickaxe" or slot.name == "minecraft:golden_pickaxe" or slot.name == "minecraft:diamond_pickaxe")) then
		  robot.select(16)
	      inv.suckFromSlot(1, i)
	  	  break
	    end
	  end
	end
  end
end

function checkedDrop(force)
  local empty = 0
  for slot = 1, 16 do
    if robot.count(slot) == 0 then
      empty = empty + 1
    end
  end
  if not dropping and empty == 0 or force and empty < 16 or checkEnergia() or checkTool() then
    local ox, oy, oz, of = x, y, z, f
    dropping = true
    moveTo(0, 0, 0)
    turnTowards(2)

	local numSlots = 16
	if modoMinado == "E" then
	  numSlots = 15
	end
	-- antes de tirar tot el inventari, enviarlo al servidor,
    -- es el resultat de la ordre de minat
	setRobotInventario(true, numSlots)
    -- tirar tot el inventari
	if dropSide == "0" then
	  robot.turnLeft()
	elseif dropSide == "2" then
	  robot.turnRight()
	end
	for pos=1,numSlots,1 do
	  robot.select(pos)
	  while robot.count(pos) > 0 do
	    -- comprobar si hay items y buscar un slot disponible en el cofre
		local internalStackName = inv.getStackInInternalSlot(pos).name
		local oreName = ""
		if (inv.getStackInInternalSlot(pos).oreNames.n > 0) then
		  oreName = inv.getStackInInternalSlot(pos).oreNames[1]
		end
	    for freePos=1,27,1 do
		  local stackInSlot = inv.getStackInSlot(3, freePos)
		  local oreNameInSlot = ""
		  if (stackInSlot ~= nil and inv.getStackInSlot(3, freePos).oreNames.n > 0) then
		    oreNameInSlot = inv.getStackInSlot(3, freePos).oreNames[1]
		  end
	      if stackInSlot == nil or (stackInSlot.name == internalStackName and oreName == oreNameInSlot and stackInSlot.size < 64) then
	        inv.dropIntoSlot(3, freePos)
	        break
	      end
	    end
	  end
	end
    robot.select(1)
	if dropSide == "0" then
	  robot.turnRight()
	elseif dropSide == "2" then
	  robot.turnLeft()
	end
    
    -- intercambiar la herramienta si es necesario
	if checkTool() then
      recargarTool()
	end
	
	-- esperar a recarregar la batería (esperar al 99% per evitar error per un mentre es descarrega)
	repeat until ((computer.energy() / computer.maxEnergy()) * 100 >= 99)

    dropping = false
    moveTo(ox, oy, oz, true)
    turnTowards(of)
  end
end

local function step()
  local inicio = os.clock()

  clearBlock(sides.down)
  if not tryMove() then
    return false
  end
  clearBlock(sides.up)
  
  --setRobotInventario()
  --l('Benchmark', 'Tiempo de step ' .. (os.clock() - x))
  return true
end

local function turn(i)
  if i % 2 == 1 then
    turnRight()
  else
    turnLeft()
  end
end

local function digLayer()
  --[[ We move in zig-zag lines, clearing three layers at a time. This means we
       have to differentiate at the end of the last line between even and odd
       sizes on which way to face for the next layer:
       For either size we rotate once to the right. For even sizes this will
       cause the next layer to be dug out rotated by ninety degrees. For odd
       ones the return path is symmetrical, meaning we just turn around.

       Examples for two layers:

       s--x--x      e--x--x      s--x--x--x      x--x  x--x
             |            |               |      |  |  |  |
       x--x--x  ->  x--x--x      x--x--x--x      x  x  x  x
       |            |            |           ->  |  |  |  |
       x--x--e      x--x--s      x--x--x--x      x  x  x  x
                                          |      |  |  |  |
                                 e--x--x--x      s  x--x  e

       Legend: s = start, x = a position, e = end, - = a move
  ]]
  for i = 1, size do
    for j = 1, size - 1 do
      if not step() then
        return false
      end
    end
    if i < size then
      -- End of a normal line, move the "cap".
      turn(i)
      if not step() then
        return false
      end
      turn(i)
    else
      turnRight()
      if size % 2 == 1 then
        turnRight()
      end
      for i = 1, 3 do
        if not tryMove(sides.down) then
          return false
        end
      end
    end
  end
  return true
end

function moverseEntreOrdenes(origen_X, origen_Z, destinoX, destinoZ)
  if destinoZ > origen_Z then
    for i = origen_Z,destinoZ,1 do
      r.forward()
    end
  elseif destinoZ < origen_Z then
    for i = origen_Z,destinoZ,1 do
      r.back()
    end
  end
  
  if origen_X > destinoX then
    r.turnRight()
    for i = destinoX,origen_X,1 do
      r.forward()
    end
    r.turnLeft()
  elseif origen_X < destinoX then
    r.turnLeft()
    for i = origen_X,destinoX,1 do
      r.forward()
    end
    r.turnRight()
  end
end

while true
do
  repeat until not digLayer()
  moveTo(0, 0, 0)
  turnTowards(0)
  checkedDrop(true)

  ordenMinadoFin()
  
  local gps = ordenGPS
  if not getOrdenMinado() then
    os.exit()
  else
    moverseEntreOrdenes(gps[1], gps[3], ordenGPS[1], ordenGPS[3])
  end
end

if options.s then
  computer.shutdown()
end