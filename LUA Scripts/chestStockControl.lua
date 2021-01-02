local component = require("component")

local m = component.modem
local inv = component.inventory_controller

SERVER_ADDR = "086c88d8-6dbc-4ae2-9136-311bdd180482"
SERVER_PORT = 32325
SIDE = 2

-- Generar cadena con el inventario

local function inventarioToString(inventario)
  local str = ""
  for key,value in pairs(inventario) do
    str = str .. key .. "@" .. value .. ";"
  end
  return str
end

local function setRobotInventario()
  local inventario = {}
  for i = 1,inv.getInventorySize(SIDE),1 do
    local slot = inv.getStackInSlot(SIDE, i)
    if slot ~= nil then
	  if (inventario[slot.name] == nil) then
	    inventario[slot.name] = math.floor(slot.size)
	  else
	    inventario[slot.name] = math.floor(inventario[slot.name] + slot.size)
	  end
    end
  end

  return inventarioToString(inventario)
end

-- Ir enviando el estado del inventario

while true do
  m.send(SERVER_ADDR, SERVER_PORT, "almacenSetInv--" ..setRobotInventario())
  os.sleep(1)
end
