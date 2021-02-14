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

local m = component.modem
local me = component.me_interface

SERVER_ADDR = "086c88d8-6dbc-4ae2-9136-311bdd180482"
SERVER_PORT = 32325

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

local function craft(articulo, cantidad)
  local craftables = me.getCraftables()
  for i=1,craftables.n,1 do
    local craftable = craftables[i]
	if (craftable.getItemStack().name == articulo) then
	  request = craftable.request(cantidad)
	  while (not request.isCanceled()) and (not request.isDone()) do end
	  return request.isDone()
	end
  end
  return false
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
	local ok = craft(str[2], tonumber(str[3]))
	
	-- marcar como completado
	if ok then
	  m.send(SERVER_ADDR, SERVER_PORT, "setOrdFab--" .. id .. ";OK;")
	else
	  m.send(SERVER_ADDR, SERVER_PORT, "setOrdFab--" .. id .. ";ERR;1")
	end
  end
end
