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

class RobotComposicion extends Component {
    constructor({ }) {
        super();

        this.robot = {
            case: 0,
            cpu: 0
        }

        this.renderComponents = this.renderComponents.bind(this);
        this.renderCPU = this.renderCPU.bind(this);
        this.renderEeprom = this.renderEeprom.bind(this);
    }

    componentDidMount() {
        window.$('#robotComposicionModal').modal({ show: true });
        this.renderComponents();
    }

    renderComponents() {
        ReactDOM.render(computerCase.map((element, i) => {
            return <tr onClick={() => {
                this.refs.case.src = computerCaseImg[i];
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
                } else if (pos == 2) {
                    this.refs.ram2.src = ramImg[i];
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
            }}>
                <td><img src={eepromImg[i]} /></td>
                <td>{element}</td>
            </tr>
        }), document.getElementById("renderComponents"));
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
                                                        <div id="computerCase" className="slot" />
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" />
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <div id="computerCase" className="slot" />
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" />
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <div id="computerCase" className="slot" />
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" />
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <div id="computerCase" className="slot" />
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" />
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" />
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
                                                        <div id="computerCase" className="slot" />
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
                                                        <div id="computerCase" className="slot" />
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" onClick={() => { this.renderRAM(1) }}>
                                                            <img ref="ram1" />
                                                        </div>
                                                    </td>
                                                    <td>
                                                        <div id="computerCase" className="slot" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <div id="computerCase" className="slot" />
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
                        <button type="button" className="btn btn-primary">Aceptar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

export default RobotComposicion;


