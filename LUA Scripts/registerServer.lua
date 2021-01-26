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

-- Este script permitirá al servidor real de ERPCraft registrar un nuevo servidor de OpenComputers sin introducir datos manualmente



-- Includes
local inet = require("internet")
local component = require("component")
local m = component.modem

print("Introduce la contraseña de autoregistro de ERPCraft")
server_pwd = io.read()

if server_pwd == "" then
  return;
end

for i=string.len(server_pwd),35,1 do
  server_pwd = server_pwd .. " "
end

-- Conectarse al servidor real
local handle = inet.open("127.0.0.1", 32325)
handle:setTimeout(0.1)
handle:write("AUTOREGISTER                        " .. m.address .. server_pwd)
handle:flush()
