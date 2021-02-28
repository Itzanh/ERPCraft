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

local m = component.modem
local me = component.me_interface

SERVER_ADDR = "086c88d8-6dbc-4ae2-9136-311bdd180482"
SERVER_PORT = 32325

-- Generar cadena con el inventario

local function setMEInventario()
  local str = ""
  local slots = me.getItemsInNetwork()
  for i=1,slots.n,1 do
    local slot = slots[i]
    if slot ~= nil then
	  str = str .. slot.name .. "@" .. math.floor(slot.size) .. "@" .. slot.label .. ";"
    end
  end
  
  return str
end

-- Ir enviando el estado del inventario

while true do
  m.send(SERVER_ADDR, SERVER_PORT, "almacenMESetInv--" ..setMEInventario())
  os.sleep(1)
end
