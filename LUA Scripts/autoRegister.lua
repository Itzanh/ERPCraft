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

local m = component.modem

SERVER_ADDR = "086c88d8-6dbc-4ae2-9136-311bdd180482"
SERVER_PORT = 32325



print("Introduce la contraseña de autoregistro del servidor")
server_pwd = io.read()

str = server_pwd .. ";" .. robot.name() .. ";" .. math.floor(robot.inventorySize()) .. ";" .. math.floor(computer.energy()) .. ";" .. math.floor(computer.maxEnergy())

if component.isAvailable("generator") then
  str = str .. ";1;" .. math.floor(component.generator.count())
else
  str = str .. ";0;0"
end

if component.isAvailable("navigation") then
  local posX, posY, posZ = component.navigation.getPosition()
  str = str .. ";1;" .. math.floor(posX) .. ";" .. math.floor(posY) .. ";" .. math.floor(posZ)
else
  str = str .. ";0;0;0;0"
end

if component.isAvailable("inventory_controller") then
  str = str .. ";1"
else
  str = str .. ";0"
end

if component.isAvailable("geolyzer") then
  str = str .. ";1"
else
  str = str .. ";0"
end

m.send(SERVER_ADDR, SERVER_PORT, "robRegister--" .. str)