import { Component } from "react";
import ReactDOM from 'react-dom';

/* IMG */

// COMPUTER CASE
import teir1case from './../../IMG/robot_composicion/case/tier1case.png';
import teir2case from './../../IMG/robot_composicion/case/tier2case.png';
import teir3case from './../../IMG/robot_composicion/case/tier3case.png';

// CPU
import cpu1 from './../../IMG/robot_composicion/cpu/cpu1.png';
import cpu2 from './../../IMG/robot_composicion/cpu/cpu2.png';
import cpu3 from './../../IMG/robot_composicion/cpu/cpu3.png';

// RAM
import ram1 from './../../IMG/robot_composicion/ram/ram1.png';
import ram15 from './../../IMG/robot_composicion/ram/ram1.5.png';
import ram2 from './../../IMG/robot_composicion/ram/ram2.png';
import ram25 from './../../IMG/robot_composicion/ram/ram2.5.png';
import ram3 from './../../IMG/robot_composicion/ram/ram3.png';
import ram35 from './../../IMG/robot_composicion/ram/ram3.5.png';

// EEPROM
import eepromBios from './../../IMG/robot_composicion/eeprom/eeprom.png';

// HDD
import hdd1 from './../../IMG/robot_composicion/hdd/hdd1.png';
import hdd2 from './../../IMG/robot_composicion/hdd/hdd2.png';
import hdd3 from './../../IMG/robot_composicion/hdd/hdd3.png';

// CARD
import datacard1 from './../../IMG/robot_composicion/card/datacard1.png';
import datacard2 from './../../IMG/robot_composicion/card/datacard2.png';
import datacard3 from './../../IMG/robot_composicion/card/datacard3.png';
import gpu1 from './../../IMG/robot_composicion/card/gpu1.png';
import gpu2 from './../../IMG/robot_composicion/card/gpu2.png';
import gpu3 from './../../IMG/robot_composicion/card/gpu3.png';
import internet_card from './../../IMG/robot_composicion/card/internet_card.png';
import lan_card from './../../IMG/robot_composicion/card/lan_card.png';
import linked_card from './../../IMG/robot_composicion/card/linked_card.png';
import redstone_card1 from './../../IMG/robot_composicion/card/redstone_card1.png';
import redstone_card2 from './../../IMG/robot_composicion/card/redstone_card2.png';
import wlan1 from './../../IMG/robot_composicion/card/wlan1.png';
import wlan2 from './../../IMG/robot_composicion/card/wlan2.png';

// CONTAINER
import disk_drive from './../../IMG/robot_composicion/container/disk_drive.png';
import card_container1 from './../../IMG/robot_composicion/container/card_container1.png';
import card_container2 from './../../IMG/robot_composicion/container/card_container2.png';
import card_container3 from './../../IMG/robot_composicion/container/card_container3.png';
import upgrade_container1 from './../../IMG/robot_composicion/container/upgrade_container1.png';
import upgrade_container2 from './../../IMG/robot_composicion/container/upgrade_container2.png';
import upgrade_container3 from './../../IMG/robot_composicion/container/upgrade_container3.png';

// UPGRADE
import screen1 from './../../IMG/robot_composicion/upgrade/screen1.png';
import keyboard from './../../IMG/robot_composicion/upgrade/keyboard.png';
import geolyzer from './../../IMG/robot_composicion/upgrade/geolyzer.png';
import angel_upgrade from './../../IMG/robot_composicion/upgrade/angel_upgrade.png';
import battery_upgrade1 from './../../IMG/robot_composicion/upgrade/battery_upgrade1.png';
import battery_upgrade2 from './../../IMG/robot_composicion/upgrade/battery_upgrade2.png';
import battery_upgrade3 from './../../IMG/robot_composicion/upgrade/battery_upgrade3.png';
import chunkloader_upgrade from './../../IMG/robot_composicion/upgrade/chunkloader_upgrade.png';
import crafting_upgrade from './../../IMG/robot_composicion/upgrade/crafting_upgrade.png';
import upgradedatabase1 from './../../IMG/robot_composicion/upgrade/upgradedatabase1.png';
import upgradedatabase2 from './../../IMG/robot_composicion/upgrade/upgradedatabase2.png';
import upgradedatabase3 from './../../IMG/robot_composicion/upgrade/upgradedatabase3.png';
import experience_upgrade from './../../IMG/robot_composicion/upgrade/experience_upgrade.png';
import generator_upgrade from './../../IMG/robot_composicion/upgrade/generator_upgrade.png';
import hover_upgrade_1 from './../../IMG/robot_composicion/upgrade/hover_upgrade_1.png';
import hover_upgrade_2 from './../../IMG/robot_composicion/upgrade/hover_upgrade_2.png';
import inventory_upgrade from './../../IMG/robot_composicion/upgrade/inventory_upgrade.png';
import inventory_controller_upgrade from './../../IMG/robot_composicion/upgrade/inventory_controller_upgrade.png';
import navigation_upgrade from './../../IMG/robot_composicion/upgrade/navigation_upgrade.png';
import upgradepiston from './../../IMG/robot_composicion/upgrade/upgradepiston.png';
import sign_upgrade from './../../IMG/robot_composicion/upgrade/sign_upgrade.png';
import solar_generator_upgrade from './../../IMG/robot_composicion/upgrade/solar_generator_upgrade.png';
import upgradetank from './../../IMG/robot_composicion/upgrade/upgradetank.png';
import upgradetankcontroller from './../../IMG/robot_composicion/upgrade/upgradetankcontroller.png';
import tractor_beam from './../../IMG/robot_composicion/upgrade/tractor_beam.png';


const computerCase = ["Computer Case (Tier 1)", "Computer Case (Tier 2)", "Computer Case (Tier 3)"];
const computerCaseImg = [teir1case, teir2case, teir3case];

const cpu = ["Central Processing Unit (CPU) (Tier 1)", "Central Processing Unit (CPU) (Tier 2)", "Central Processing Unit (CPU) (Tier 3)"];
const cpuImg = [cpu1, cpu2, cpu3];

const ram = ["Memory (Tier 1)", "Memory (Tier 1.5)", "Memory (Tier 2)", "Memory (Tier 2.5)", "Memory (Tier 3)", "Memory (Tier 3.5)"];
const ramImg = [ram1, ram15, ram2, ram25, ram3, ram35];

const eeprom = ["EEPROM (lua BIOS)"];
const eepromImg = [eepromBios];

const hdd = ["Hard Disk Drive (Tier 1) (1MB)", "Hard Disk Drive (Tier 2) (2MB)", "Hard Disk Drive (Tier 3) (4MB)"];
const hddImg = [hdd1, hdd2, hdd3];

const card = ["Data Card (Tier 1)", "Data Card (Tier 2)", "Data Card (Tier 3)", "Graphics Card (Tier 1)", "Graphics Card (Tier 2)", "Graphics Card (Tier 3)",
    "Internet Card", "Linked Card", "Network Card", "Redstone Card (Tier 1)", "Redstone Card (Tier 2)", "Wireless Network Card (Tier 1)",
    "Wireless Network Card (Tier 2)"];
const cardImg = [datacard1, datacard2, datacard3, gpu1, gpu2, gpu3, internet_card, lan_card, linked_card, redstone_card1, redstone_card2, wlan1, wlan2];

const container = ["Disk Drive", "Card Container (Tier 1)", "Card Container (Tier 2)", "Card Container (Tier 3)", "Upgrade Container (Tier 1)",
    "Upgrade Container (Tier 2)", "Upgrade Container (Tier 3)"];
const containerImg = [disk_drive, card_container1, card_container2, card_container3, upgrade_container1, upgrade_container2, upgrade_container3];

const upgrade = ["Screen (Tier 1)", "Keyboard", "Geolyzer", "Angel Upgrade", "Battery Upgrade (Tier 1)", "Battery Upgrade (Tier 2)", "Battery Upgrade (Tier 3)",
    "Chunkloader Upgrade", "Crafting Upgrade", "Database Upgrade (Tier 1)", "Database Upgrade (Tier 2)", "Database Upgrade (Tier 3)", "Experience Upgrade",
    "Generator Upgrade", "Hover Upgrade (Tier 1)", "Hover Upgrade (Tier 2)", "Inventory Upgrade", "Inventory Controller Upgrade", "Navigation Upgrade",
    "Piston Upgrade", "Sign I/O Upgrade", "Solar Generator Upgrade", "Tank Upgrade", "Tank Controller Upgrade", "Tractor Beam Upgrade"];
const upgradeImg = [screen1, keyboard, geolyzer, angel_upgrade, battery_upgrade1, battery_upgrade2, battery_upgrade3, chunkloader_upgrade,
    crafting_upgrade, upgradedatabase1, upgradedatabase2, upgradedatabase3, experience_upgrade, generator_upgrade, hover_upgrade_1, hover_upgrade_2,
    inventory_upgrade, inventory_controller_upgrade, navigation_upgrade, upgradepiston, sign_upgrade, solar_generator_upgrade, upgradetank,
    upgradetankcontroller, tractor_beam];

class RobotComposicion extends Component {
    constructor({ robotId, ensamblado, robotSetEnsablado }) {
        super();

        this.robotSetEnsablado = robotSetEnsablado;

        if (ensamblado != null) {
            this.robot = ensamblado;
            this.robot.robotId = robotId;
        } else {
            this.robot = {
                robotId: robotId,
                computerCase: -1,
                upgrade1: -1,
                upgrade2: -1,
                upgrade3: -1,
                upgrade4: -1,
                upgrade5: -1,
                upgrade6: -1,
                upgrade7: -1,
                upgrade8: -1,
                upgrade9: -1,
                container1: -1,
                container2: -1,
                container3: -1,
                card1: -1,
                card2: -1,
                card3: -1,
                cpu: -1,
                ram1: -1,
                ram2: -1,
                eeprom: -1,
                hdd: -1
            };
        }

        this.renderComponents = this.renderComponents.bind(this);
        this.renderCPU = this.renderCPU.bind(this);
        this.renderEeprom = this.renderEeprom.bind(this);
        this.renderHDD = this.renderHDD.bind(this);
        this.renderCard = this.renderCard.bind(this);
        this.renderContainer = this.renderContainer.bind(this);
        this.renderUpgrade = this.renderUpgrade.bind(this);
        this.aceptar = this.aceptar.bind(this);
    }

    componentDidMount() {
        window.$('#robotComposicionModal').modal({ show: true });
        this.renderComponents();
        this.printComposicion();
    }

    printComposicion() {
        if (this.robot.computerCase != -1) {
            this.refs.case.src = computerCaseImg[this.robot.computerCase];
        }
        if (this.robot.upgrade1 != -1) {
            this.refs.upgrade1.src = upgradeImg[this.robot.upgrade1];
        }
        if (this.robot.upgrade2 != -1) {
            this.refs.upgrade2.src = upgradeImg[this.robot.upgrade2];
        }
        if (this.robot.upgrade3 != -1) {
            this.refs.upgrade3.src = upgradeImg[this.robot.upgrade3];
        }
        if (this.robot.upgrade4 != -1) {
            this.refs.upgrade4.src = upgradeImg[this.robot.upgrade4];
        }
        if (this.robot.upgrade5 != -1) {
            this.refs.upgrade5.src = upgradeImg[this.robot.upgrade5];
        }
        if (this.robot.upgrade6 != -1) {
            this.refs.upgrade6.src = upgradeImg[this.robot.upgrade6];
        }
        if (this.robot.upgrade7 != -1) {
            this.refs.upgrade7.src = upgradeImg[this.robot.upgrade7];
        }
        if (this.robot.upgrade8 != -1) {
            this.refs.upgrade8.src = upgradeImg[this.robot.upgrade8];
        }
        if (this.robot.upgrade9 != -1) {
            this.refs.upgrade9.src = upgradeImg[this.robot.upgrade9];
        }
        if (this.robot.container1 != -1) {
            this.refs.container1.src = containerImg[this.robot.container1];
        }
        if (this.robot.container2 != -1) {
            this.refs.container2.src = containerImg[this.robot.container2];
        }
        if (this.robot.container3 != -1) {
            this.refs.container3.src = containerImg[this.robot.container3];
        }
        if (this.robot.card1 != -1) {
            this.refs.card1.src = cardImg[this.robot.card1];
        }
        if (this.robot.card2 != -1) {
            this.refs.card2.src = cardImg[this.robot.card2];
        }
        if (this.robot.card3 != -1) {
            this.refs.card3.src = cardImg[this.robot.card3];
        }
        if (this.robot.cpu != -1) {
            this.refs.cpu.src = cpuImg[this.robot.cpu];
        }
        if (this.robot.ram1 != -1) {
            this.refs.ram1.src = ramImg[this.robot.ram1];
        }
        if (this.robot.ram2 != -1) {
            this.refs.ram2.src = ramImg[this.robot.ram2];
        }
        if (this.robot.eeprom != -1) {
            this.refs.eeprom.src = eepromImg[this.robot.eeprom];
        }
        if (this.robot.hdd != -1) {
            this.refs.hdd.src = ramImg[this.robot.hdd];
        }
    }

    renderComponents() {
        ReactDOM.render(computerCase.map((element, i) => {
            return <tr onClick={() => {
                this.refs.case.src = computerCaseImg[i];
                this.robot.computerCase = i;
            }}>
                <td><img src={computerCaseImg[i]} /></td>
                <td>{element}</td>
            </tr>
        }), document.getElementById("renderComponents"));
    }

    renderCPU() {
        ReactDOM.render(cpu.map((element, i) => {
            return <tr onClick={() => {
                this.refs.cpu.src = cpuImg[i];
                this.robot.cpu = i;
            }}>
                <td><img src={cpuImg[i]} /></td>
                <td>{element}</td>
            </tr>
        }), document.getElementById("renderComponents"));
    }

    renderRAM(pos) {
        ReactDOM.render(ram.map((element, i) => {
            return <tr onClick={() => {
                if (pos == 1) {
                    this.refs.ram1.src = ramImg[i];
                    this.robot.ram1 = i;
                } else if (pos == 2) {
                    this.refs.ram2.src = ramImg[i];
                    this.robot.ram2 = i;
                }
            }}>
                <td><img src={ramImg[i]} /></td>
                <td>{element}</td>
            </tr>
        }), document.getElementById("renderComponents"));
    }

    renderEeprom() {
        ReactDOM.render(eeprom.map((element, i) => {
            return <tr onClick={() => {
                this.refs.eeprom.src = eepromImg[i];
                this.robot.eeprom = i;
            }}>
                <td><img src={eepromImg[i]} /></td>
                <td>{element}</td>
            </tr>
        }), document.getElementById("renderComponents"));
    }

    renderHDD() {
        ReactDOM.render(hdd.map((element, i) => {
            return <tr onClick={() => {
                this.refs.hdd.src = hddImg[i];
                this.robot.hdd = i;
            }}>
                <td><img src={hddImg[i]} /></td>
                <td>{element}</td>
            </tr>
        }), document.getElementById("renderComponents"));
    }

    renderCard(pos) {
        ReactDOM.render(card.map((element, i) => {
            return <tr onClick={() => {
                if (pos == 1) {
                    this.refs.card1.src = cardImg[i];
                    this.robot.card1 = i;
                } else if (pos == 2) {
                    this.refs.card2.src = cardImg[i];
                    this.robot.card2 = i;
                } else if (pos == 3) {
                    this.refs.card3.src = cardImg[i];
                    this.robot.card3 = i;
                }
            }}>
                <td><img src={cardImg[i]} /></td>
                <td>{element}</td>
            </tr>
        }), document.getElementById("renderComponents"));
    }

    renderContainer(pos) {
        ReactDOM.render(container.map((element, i) => {
            return <tr onClick={() => {
                if (pos == 1) {
                    this.refs.container1.src = containerImg[i];
                    this.robot.container1 = i;
                } else if (pos == 2) {
                    this.refs.container2.src = containerImg[i];
                    this.robot.container2 = i;
                } else if (pos == 3) {
                    this.refs.container3.src = containerImg[i];
                    this.robot.container3 = i;
                }
            }}>
                <td><img src={containerImg[i]} /></td>
                <td>{element}</td>
            </tr>
        }), document.getElementById("renderComponents"));
    }

    renderUpgrade(pos) {
        ReactDOM.render(upgrade.map((element, i) => {
            return <tr onClick={() => {
                if (pos == 1) {
                    this.refs.upgrade1.src = upgradeImg[i];
                    this.robot.upgrade1 = i;
                } else if (pos == 2) {
                    this.refs.upgrade2.src = upgradeImg[i];
                    this.robot.upgrade2 = i;
                } else if (pos == 3) {
                    this.refs.upgrade3.src = upgradeImg[i];
                    this.robot.upgrade3 = i;
                } else if (pos == 4) {
                    this.refs.upgrade4.src = upgradeImg[i];
                    this.robot.upgrade4 = i;
                } else if (pos == 5) {
                    this.refs.upgrade5.src = upgradeImg[i];
                    this.robot.upgrade5 = i;
                } else if (pos == 6) {
                    this.refs.upgrade6.src = upgradeImg[i];
                    this.robot.upgrade6 = i;
                } else if (pos == 7) {
                    this.refs.upgrade7.src = upgradeImg[i];
                    this.robot.upgrade7 = i;
                } else if (pos == 8) {
                    this.refs.upgrade8.src = upgradeImg[i];
                    this.robot.upgrade8 = i;
                } else if (pos == 9) {
                    this.refs.upgrade9.src = upgradeImg[i];
                    this.robot.upgrade9 = i;
                }
            }}>
                <td><img src={upgradeImg[i]} /></td>
                <td>{element}</td>
            </tr>
        }), document.getElementById("renderComponents"));
    }

    aceptar() {
        this.robotSetEnsablado(this.robot);
        window.$('#robotComposicionModal').modal('hide');
    }

    render() {
        return <div className="modal fade bd-example-modal-xl" tabIndex="-1" role="dialog" aria-labelledby="robotComposicionModal"
            id="robotComposicionModal" aria-hidden="true">
            <div className="modal-dialog modal-xl" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="exampleModalLabel">Ensamblado del robot</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <div className="form-row">
                            <div className="col">
                                <table className="table table-dark" id="robotComposicionComponentes">
                                    <thead>
                                        <tr>
                                            <th scope="col">#</th>
                                            <th scope="col">Nombre</th>
                                        </tr>
                                    </thead>
                                    <tbody id="renderComponents"></tbody>
                                </table>
                            </div>
                            <div className="col">
                                <div className="form-row">
                                    <div className="col">
                                        <div id="computerCase" className="slot" onClick={this.renderComponents}>
                                            <img ref="case" />
                                        </div>
                                    </div>
                                    <div className="col">
                                        <table>
                                            <tbody>
                                                <tr>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderUpgrade(1) }}>
                                                            <img ref="upgrade1" />
                                                        </div>
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderUpgrade(2) }}>
                                                            <img ref="upgrade2" />
                                                        </div>
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderUpgrade(3) }}>
                                                            <img ref="upgrade3" />
                                                        </div>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderUpgrade(4) }}>
                                                            <img ref="upgrade4" />
                                                        </div>
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderUpgrade(5) }}>
                                                            <img ref="upgrade5" />
                                                        </div>
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderUpgrade(6) }}>
                                                            <img ref="upgrade6" />
                                                        </div>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderUpgrade(7) }}>
                                                            <img ref="upgrade7" />
                                                        </div>
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderUpgrade(8) }}>
                                                            <img ref="upgrade8" />
                                                        </div>
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderUpgrade(9) }}>
                                                            <img ref="upgrade9" />
                                                        </div>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderContainer(1) }}>
                                                            <img ref="container1" />
                                                        </div>
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderContainer(2) }}>
                                                            <img ref="container2" />
                                                        </div>
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderContainer(3) }}>
                                                            <img ref="container3" />
                                                        </div>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                    <div className="col">
                                        <table>
                                            <tbody>
                                                <tr>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderCard(1) }}>
                                                            <img ref="card1" />
                                                        </div>
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={this.renderCPU}>
                                                            <img ref="cpu" />
                                                        </div>
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={this.renderEeprom}>
                                                            <img ref="eeprom" />
                                                        </div>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderCard(2) }}>
                                                            <img ref="card2" />
                                                        </div>
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderRAM(1) }}>
                                                            <img ref="ram1" />
                                                        </div>
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={this.renderHDD}>
                                                            <img ref="hdd" />
                                                        </div>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderCard(3) }}>
                                                            <img ref="card3" />
                                                        </div>
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderRAM(2) }}>
                                                            <img ref="ram2" />
                                                        </div>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                        <button type="button" className="btn btn-primary" onClick={this.aceptar}>Aceptar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

export default RobotComposicion;


