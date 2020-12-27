local component = require("component")

local m = component.modem
local bat = component.ic2_te_mfsu

SERVER_ADDR = "086c88d8-6dbc-4ae2-9136-311bdd180482"
SERVER_PORT = 32325

-- Inicializar la batería

m.send(SERVER_ADDR, SERVER_PORT, "batInit--" .. math.floor(bat.getCapacity()) .. "@" .. math.floor(bat.getEnergy()))

-- Ir enviando el estado de la batería

while true do
  m.send(SERVER_ADDR, SERVER_PORT, "batSet--" .. math.floor(bat.getEnergy()))
  os.sleep(1)
end