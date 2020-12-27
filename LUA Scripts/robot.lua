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



-- TESTING SCRIPT WITH EXAMPLES OF API USAGE

local component = require("component")
local event = require("event")
local m = component.modem
local robot = require("robot")
local computer = require("computer")
local inv = component.inventory_controller

m.open(32325)

str = "robOnline--" .. robot.name() .. ";" .. math.floor(computer.energy()) .. ";" .. math.floor(computer.maxEnergy()) .. ";0;0;0"

m.broadcast(32325, str)

local function l(titulo, mensaje)
  m.broadcast(32325, "robLog--" .. titulo .. "@@" .. mensaje)
end

local function setRobotInventario()
  local str = ""
  for i = 1,16,1 do
    local slot = inv.getStackInInternalSlot(i)
    if slot == nil then
      str = str .. "@0;"
    else
      str = str .. slot.name .. "@" .. math.floor(slot.size) .. ";"
    end
  end

  return str
end

local function setRobotGps()
  local posX, posY, posZ = component.navigation.getPosition()
  return math.floor(posX) .. "@" .. math.floor(posY) .. "@" .. math.floor(posZ)
end

l("ONLINE", "Robot online!")

--print("Inv " .. setRobotInventario())
m.broadcast(32325, "robInventario--" .. setRobotInventario())

--print("GPS " .. setRobotGps())
m.broadcast(32325, "robGPS--" .. setRobotGps())

m.broadcast(32325, "robOrdenMinado--")
i = 0
while true do
  local _, _, from, port, _, message = event.pull("modem_message")
  print("Got a message from " .. from .. " on port " .. port .. ": " .. tostring(message))
  
  if (message == "PING") then
    m.broadcast(32325, "PONG--")
    --m.broadcast(32325, "robOrdenMinadoUpdate--" .. math.floor(i) .. ";" .. math.floor(i) .. ";" .. math.floor(i) .. ";" .. math.floor(i))
	l("PING", "Vaig per el ping " .. i)
	m.broadcast(32325, "robInventario--" .. setRobotInventario())
	m.broadcast(32325, "robGPS--" .. setRobotGps())
    i = i + 1
  elseif (string.sub(message, 0, 3) == "CMD") then
	  f = load(string.sub(message, 6))
	  f()
  end
end