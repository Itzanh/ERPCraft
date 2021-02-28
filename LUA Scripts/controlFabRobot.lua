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
local event = require("event")
local robot = require("robot")

local m = component.modem
local inv = component.inventory_controller
local crafter = component.crafting 

SERVER_ADDR = "086c88d8-6dbc-4ae2-9136-311bdd180482"
SERVER_PORT = 32325

local SIDE = 3

m.open(SERVER_PORT)

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



local function storeInInventory(minecraftID, cantidad)
  local numSlots = inv.getInventorySize(SIDE) -- forward
  for i=1,numSlots,1 do
    if inv.getStackInSlot(SIDE, i) ~= nil and inv.getStackInSlot(SIDE, i).name == minecraftID then
	  inv.suckFromSlot(SIDE, i, cantidad)
	  return
	end
  end
end

local function craft(cantidad, art_slot1, cant_slot1, art_slot2, cant_slot2, art_slot3, cant_slot3, art_slot4, cant_slot4, art_slot5, cant_slot5, art_slot6, cant_slot6, art_slot7, cant_slot7, art_slot8, cant_slot8, art_slot9, cant_slot9)
  for i=1,cantidad,1 do
    -- mover items al inventario
    if art_slot1 ~= "" then
	  robot.select(1)
	  storeInInventory(art_slot1, cant_slot1)
	end
	if art_slot2 ~= "" then
	  robot.select(2)
	  storeInInventory(art_slot2, cant_slot2)
	end
	if art_slot3 ~= "" then
	  robot.select(3)
	  storeInInventory(art_slot3, cant_slot3)
	end
	if art_slot4 ~= "" then
	  robot.select(5)
	  storeInInventory(art_slot4, cant_slot4)
	end
	if art_slot5 ~= "" then
	  robot.select(6)
	  storeInInventory(art_slot5, cant_slot5)
	end
	if art_slot6 ~= "" then
	  robot.select(7)
	  storeInInventory(art_slot6, cant_slot6)
	end
	if art_slot7 ~= "" then
	  robot.select(9)
	  storeInInventory(art_slot7, cant_slot7)
	end
	if art_slot8 ~= "" then
	  robot.select(10)
	  storeInInventory(art_slot8, cant_slot8)
	end
	if art_slot9 ~= "" then
	  robot.select(11)
	  storeInInventory(art_slot9, cant_slot9)
	end
	
	-- craftear
	robot.select(1)
	crafter.craft(1)
	
	-- devolver items al inventario
	for pos=1,16,1 do
	  robot.select(pos)
	  if robot.count(pos) > 0 then
	    -- comprobar si hay items y buscar un slot disponible en el cofre
		local numSlots = inv.getInventorySize(SIDE) -- forward
	    for freePos=1,numSlots,1 do
	      if inv.getStackInSlot(SIDE, freePos) == nil or inv.getStackInSlot(SIDE, freePos).name == inv.getStackInInternalSlot(pos).name then
	        inv.dropIntoSlot(SIDE, freePos)
	        break
	      end
	    end
	  end
	end
	robot.select(1)
  end
end

local function smelt(cantidad, articulo, lado)
  print("cantidad " .. cantidad .. " articulo " .. articulo .. " lado " .. lado)
  robot.select(1)
  storeInInventory(articulo, cantidad)
  
  if lado == 0 then -- horno a la izquierda
    robot.turnLeft()
	robot.up()
	robot.forward()
	inv.dropIntoSlot(0,1,cantidad)
	while (inv.getStackInSlot(0,1) ~= nil) do os.sleep(10) end
	robot.back()
	robot.down()
	robot.down()
	robot.forward()
	inv.suckFromSlot(1,1,cantidad)
	robot.back()
	robot.up()
	robot.turnRight()
  elseif lado == 1 then -- horno a la derecha
    robot.turnRight()
	robot.up()
	robot.forward()
	inv.dropIntoSlot(0,1,cantidad)
	while (inv.getStackInSlot(0,1) ~= nil) do os.sleep(10) end
	robot.back()
	robot.down()
	robot.down()
	robot.forward()
	inv.suckFromSlot(1,1,cantidad)
	robot.back()
	robot.up()
	robot.turnLeft()
  end
  
  -- devolver items al inventario
	for pos=1,16,1 do
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
end

while true do
  m.send(SERVER_ADDR, SERVER_PORT, "getOrdFab--")
  local _, _, _, _, _, message = event.pull("modem_message")
  print(message)
  
  -- esperar un crafteo
  if message == "PING" then
    m.send(SERVER_ADDR, SERVER_PORT, "PONG--")
  elseif message == "IDLE" then
    print("IDLE!")
	os.sleep(10)
  else
    local str = strSplit(message, ";")
    
	-- craftear
	local id = tonumber(str[1])
	if (str[3] == "C") then
	  SIDE = tonumber(str[22])
	  craft(tonumber(str[2]), str[4], tonumber(str[5]), str[6], tonumber(str[7]), str[8], tonumber(str[9]), str[10], tonumber(str[11]), str[12], tonumber(str[13]), str[14], tonumber(str[15]), str[16], tonumber(str[17]), str[18], tonumber(str[19]), str[20], tonumber(str[21]))
	elseif (str[3] == "S") then
	  smelt(tonumber(str[2]), str[4], tonumber(str[5]))
	end
	
	-- marcar como completado
	m.send(SERVER_ADDR, SERVER_PORT, "setOrdFab--" .. id .. ";OK;")
  end
end
