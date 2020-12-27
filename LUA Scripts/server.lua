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

-- Este script permitirá al servidor real de ERPCraft comunicarse con el mundo de Minecraft a nivel de mensajes de modem
-- Una conexión por la Internet card permitirá al servidor enviar y recibir mensajes de modem a través de los sockets
-- Este servidor debe tener acceso a toda la red de Minecraft y tener integrada una Internet Card para poder funcionar



-- Includes
local inet = require("internet")
local component = require("component")
local event = require("event")
local m = component.modem
local thread = require("thread")

-- Conectarse al servidor real
local handle = inet.open("127.0.0.1", 32325)
handle:setTimeout(0.1)
-- ID del servidor + Clave secreta
handle:write("dc38400f-7514-45ec-9161-b20c65330576e4f76e04-6816-44a6-9aa4-a42df874bcd9")

-- Dispositivos conectados: Tabla de UUIDs de dispositivos que ya han enviao algún mensaje. La conexión se hace en este
-- servidor, y el timeout que los elimine de esta lista está controlado por el servidor real.
local robots = {}

-- Modem del servidor
m.open(32325)

local function buscarRobot(uuid)
  for i, id in pairs(robots) do
    if id == uuid then
      return i
    end
  end
  return 0
end

-- Enviar un dos bytes de tamaño + mensaje
local function enviarSocket(str)
  handle:write(("").char( string.len(str) ))
  handle:write(("").char(0))
  handle:write(str)
  handle:flush()
end

-- Enviar al servidor los mensajes que lleguen de los dispositivos
local function mensajeDispositivo()
  local _, _, from, _, distance, message = event.pull("modem_message")
  print("Arriva missatge")

  if buscarRobot(from) == 0 then
    table.insert(robots, from)
    local connStr = "CONN$$" .. from
    enviarSocket(connStr)
  end

  local str = "MSG$$" .. from .. "$$" .. math.floor(distance) .. "&&" .. message
  enviarSocket(str)
end

local function enviarMensaje(str)
  print("Enviant missatge...")
  uuid, msg = str:match("([^&&]+)&&([^&&]+)")
  m.send(uuid, 32325, msg)
end

-- Controla una primera conexión siempre existente con el servidor real, que controla las conexione y desconexiones de los dispositivos
local function conexionControl()
  while true
  do
    local status, dataSize = pcall(function() return handle:read(1) end)
    if (status) then
      print("Parla el servidor ")
      local dataSize = string.byte(dataSize)
      
      local data = handle:read(dataSize)
      print("Data " .. data)
      local cmd, param = data:match("([^$$]+)$$([^$$]+)")
      
      if (cmd == "DESC") then
        robots[buscarRobot(param)] = nil
      elseif (cmd == "MSG") then
        enviarMensaje(param)
	  elseif (data == "PING$$") then
	    print("Se esta fent ping")
        enviarSocket("PONG$$")
      end
    end

    os.sleep(0.1)
    
  end
end

thread.create(function()
  while true do
    mensajeDispositivo()
  end
end)

thread.create(function()
  conexionControl()
end)

print("Vaaaaa")



--