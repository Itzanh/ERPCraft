import { Component } from "react";
import ReactDOM from 'react-dom';

import robotIco from './../../IMG/robot.png';
import RobotLogs from "./RobotLogs";
import RobotComposicion from "./RobotComposicion";

// IMG
import onlineIco from './../../IMG/robot_estado/online.svg';
import minandoIco from './../../IMG/robot_estado/minando.png';
import offlineIco from './../../IMG/robot_estado/offline.svg';
import conPerdidaIco from './../../IMG/robot_estado/con_perdida.svg';
import sinBateriaIco from './../../IMG/robot_estado/sin_bateria.svg';
import RobotTerminal from "./RobotTerminal";

const estadosRobot = {
    "O": "Online",
    "M": "Minando",
    "F": "Offline",
    "L": "Conexión perdida",
    "B": "Sin batería"
};

const imagenEstadosRobot = {
    "O": onlineIco,
    "M": minandoIco,
    "F": offlineIco,
    "L": conPerdidaIco,
    "B": sinBateriaIco
};

class RobotForm extends Component {
    constructor({ robot, handleCancelar, handleAddRobot, handleEditRobot, handleEliminar, handleLogs, handleDeleteLogs, handleClearLogs, handleComando }) {
        super();

        this.robot = robot;

        this.handleCancelar = handleCancelar;
        this.handleAddRobot = handleAddRobot;
        this.handleEditRobot = handleEditRobot;
        this.handleEliminar = handleEliminar;
        this.handleLogs = handleLogs;
        this.handleDeleteLogs = handleDeleteLogs;
        this.handleClearLogs = handleClearLogs;
        this.handleComando = handleComando;

        this.calcularPorcentajeBateria = this.calcularPorcentajeBateria.bind(this);
        this.eliminarPrompt = this.eliminarPrompt.bind(this);
        this.guardarRobot = this.guardarRobot.bind(this);
        this.eliminar = this.eliminar.bind(this);
        this.terminal = this.terminal.bind(this);
        this.aceptar = this.aceptar.bind(this);
        this.logs = this.logs.bind(this);
    }

    componentDidMount() {
        this.calcularPorcentajeBateria();
    }

    calcularPorcentajeBateria() {
        const porcentaje = Math.floor((parseInt(this.refs.energiaActual.value) / parseInt(this.refs.totalEnergia.value)) * 100) + '%';
        this.refs.barraBateria.style.width = porcentaje;
        this.refs.barraBateria.innerText = porcentaje;
    }

    getRobot() {
        const robot = {};
        robot.id = this.robot ? this.robot.id : 0;
        robot.name = this.refs.name.value;
        if (this.refs.uuid.value == '') {
            robot.uuid = null;
        } else {
            robot.uuid = this.refs.uuid.value;
        }
        robot.tier = parseInt(this.refs.tier.value);
        robot.totalEnergia = parseInt(this.refs.totalEnergia.value);
        robot.energiaActual = parseInt(this.refs.energiaActual.value);
        robot.upgradeGenerador = this.refs.upgradeGenerador.checked;
        robot.itemsGenerador = parseInt(this.refs.itemsGenerador.value);
        robot.numeroSlots = parseInt(this.refs.numeroSlots.value);
        robot.upgradeGps = this.refs.upgradeGps.checked;
        robot.posX = parseInt(this.refs.posX.value);
        robot.posY = parseInt(this.refs.posY.value);
        robot.posZ = parseInt(this.refs.posZ.value);
        robot.descripcion = this.refs.descripcion.value;
        robot.offsetPosX = parseInt(this.refs.offX.value);
        robot.offsetPosY = parseInt(this.refs.offY.value);
        robot.offsetPosZ = parseInt(this.refs.offZ.value);
        robot.off = this.refs.off.checked;

        return robot;
    }

    guardarRobot() {
        this.robot = this.getRobot();
        if (this.robot && this.robot.id == 0) { // creación del robot
            this.handleAddRobot(this.robot).then((response) => {
                this.robot.id = response;
                if (this.refs.id) {
                    this.refs.id.innerText = "ID: " + response;
                }
            }, () => {
                alert("No se ha podido añadir el robot");
            });

        } else { // modificación del robot
            this.handleEditRobot(this.robot).then(null, () => {
                alert("No se ha podido guardar el robot");
            });
        }
    }

    aceptar() {
        this.guardarRobot();
        this.handleCancelar();
    }

    eliminarPrompt() {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderRobotFormModal'));
        ReactDOM.render(<RobotFormDeleteConfirm
            robotName={this.robot.name}
            robotId={this.robot.id}
            handleEliminar={this.eliminar}
        />, document.getElementById('renderRobotFormModal'));
    }

    eliminar() {
        this.handleEliminar(this.robot.id).then(() => {
            this.handleCancelar();
        }, () => {
            alert("No se ha podido eliminar el robot");
        });
    }

    logs() {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderRobotFormModal'));
        ReactDOM.render(<RobotLogs
            idRobot={this.robot.id}
            handleLogs={this.handleLogs}
            handleDeleteLogs={this.handleDeleteLogs}
            handleClearLogs={this.handleClearLogs}
        />, document.getElementById('renderRobotFormModal'));
    }

    terminal() {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderRobotFormModal'));
        ReactDOM.render(<RobotTerminal
            handleEnviar={(comando) => {
                return this.handleComando(this.robot.id, comando);
            }}
        />, document.getElementById('renderRobotFormModal'));
    }

    composicion() {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderRobotFormModal'));
        ReactDOM.render(<RobotComposicion

        />, document.getElementById('renderRobotFormModal'));
    }

    formatearFechaTiempo(fechaTiempo) {
        const fecha = new Date(fechaTiempo);
        return fecha.getDate() + '/'
            + (fecha.getMonth() + 1) + '/'
            + fecha.getFullYear() + ' '
            + fecha.getHours() + ':'
            + fecha.getMinutes() + ':'
            + fecha.getSeconds();
    }

    render() {
        return <div id="editRobot">
            <div id="renderRobotFormModal"></div>
            <div id="robotTitle">
                <img src={robotIco} />
                <h3>Robot</h3>
                <p ref="id">ID: {this.robot != null ? this.robot.id : 0}</p>
            </div>

            <div className="form-row" id="detallesBasicos">
                <div className="col">
                    <label>Nombre</label>
                    <input type="text" className="form-control" placeholder="Nombre" ref="name" defaultValue={this.robot != null ? this.robot.name : ''} />
                </div>
                <div className="col">
                    <label>UUID</label>
                    <input type="text" className="form-control" placeholder="Identificador &uacute;nico" ref="uuid" defaultValue={this.robot != null ? this.robot.uuid : ''} />
                </div>
                <div className="col">
                    <label>Tier</label>
                    <input type="number" className="form-control" placeholder="Tier" min="1" max="3" ref="tier" defaultValue={this.robot != null ? this.robot.tier : '1'} />
                </div>
            </div>

            <div className="form-row" id="detallesEnergia">
                <div className="col">
                    <label>Total energ&iacute;a</label>
                    <input type="number" className="form-control" placeholder="Total energ&iacute;a" min="0" max="65535" ref="totalEnergia" onChange={this.calcularPorcentajeBateria} defaultValue={this.robot != null ? this.robot.totalEnergia : '20500'} />
                </div>
                <div className="col">
                    <label>Energ&iacute;a actual</label>
                    <input type="number" className="form-control" placeholder="Energ&iacute;a actual" min="0" max="65535" ref="energiaActual" onChange={this.calcularPorcentajeBateria} defaultValue={this.robot != null ? this.robot.energiaActual : '0'} />
                </div>
                <div className="col">
                    <div className="progress">
                        <div className="progress-bar" role="progressbar" style={{ width: '0%' }} aria-valuemin="0" aria-valuemax="100" ref="barraBateria">0%</div>
                    </div>
                </div>
                <div className="col">
                    <input type="checkbox" name="upgrade_gen" ref="upgradeGenerador" defaultChecked={this.robot != null && this.robot.upgradeGenerador} />
                    <label>&iquest;Upgrade de generador?</label>
                </div>
                <div className="col">
                    <label>Numero de &iacute;tems en el generador</label>
                    <input type="number" className="form-control" placeholder="Numero de &iacute;tems" min="0" max="64" ref="itemsGenerador" defaultValue={this.robot != null ? this.robot.itemsGenerador : '0'} />
                </div>
            </div>

            <div className="form-row" id="panelCentral">
                <div className="col">
                    <h5>Inventario</h5>
                    <div className="form-row" id="inventarioDetallesBasicos">
                        <div className="col">
                            <label>N&uacute;mero de slots</label>
                            <input type="number" className="form-control" placeholder="N&uacute;mero de slots" min="0" max="64" ref="numeroSlots" defaultValue={this.robot != null ? this.robot.numeroSlots : '16'} />
                        </div>
                        <div className="col">
                            <label>N&uacute;mero de stacks usados</label>
                            <input type="number" className="form-control" readOnly defaultValue={this.robot != null ? this.robot.numeroStacks : '0'} />
                        </div>
                        <div className="col">
                            <label>&Iacute;tems en el inventario</label>
                            <input type="number" className="form-control" readOnly defaultValue={this.robot != null ? this.robot.numeroItems : '0'} />
                        </div>
                    </div>

                    <div className="row row-cols-1 row-cols-md-4" id="inventarioContenido">

                    </div>

                    <div id="robotRecordDetails">
                        <div className="form-row">
                            <div className="col">
                                <label>Fecha de creaci&oacute;n</label>
                                <input type="text" className="form-control" defaultValue={this.robot != null ? this.formatearFechaTiempo(this.robot.dateAdd) : ''} readOnly={true} />
                            </div>
                            <div className="col">
                                <label>Fecha de modificaci&oacute;n</label>
                                <input type="text" className="form-control" defaultValue={this.robot != null ? this.formatearFechaTiempo(this.robot.dateUpd) : ''} readOnly={true} />
                            </div>
                            <div className="col">
                                <p><img src={imagenEstadosRobot[this.robot != null ? this.robot.estado : 'F']} />{estadosRobot[this.robot != null ? this.robot.estado : 'F']}</p>
                            </div>
                        </div>
                        <div className="form-row">
                            <div className="col">
                                <label>Fecha de conexi&oacute;n</label>
                                <input type="text" className="form-control" defaultValue={this.robot != null ? this.formatearFechaTiempo(this.robot.fechaConexion) : ''} readOnly={true} />
                            </div>
                            <div className="col">
                                <label>Fecha de desconexi&oacute;n</label>
                                <input type="text" className="form-control" defaultValue={this.robot != null ? this.formatearFechaTiempo(this.robot.fechaDesconexion) : ''} readOnly={true} />
                            </div>
                            <div className="col">
                                <input type="checkbox" defaultChecked={this.robot != null && this.robot.off} ref="off" />
                                <label>&iquest;Desactivado?</label>
                            </div>
                        </div>
                    </div>

                </div>
                <div className="col">
                    <h5>Posici&oacute;n GPS</h5>
                    <div className="form-row" id="gpsDetallesBasicos">
                        <div className="col">
                            <input type="checkbox" name="upgrade_gps" ref="upgradeGps" defaultChecked={this.robot != null && this.robot.upgradeGps} />
                            <label>&iquest;Upgrade de GPS?</label>
                        </div>
                        <div className="col">
                            <label>Posici&oacute;n X</label>
                            <input type="number" className="form-control" placeholder="Posici&oacute;n X" min="-32768" max="32768" ref="posX" defaultValue={this.robot != null ? this.robot.posX : '0'} />
                        </div>
                        <div className="col">
                            <label>Posici&oacute;n Y</label>
                            <input type="number" className="form-control" placeholder="Posici&oacute;n Y" min="-32768" max="32768" ref="posY" defaultValue={this.robot != null ? this.robot.posY : '0'} />
                        </div>
                        <div className="col">
                            <label>Posici&oacute;n Z</label>
                            <input type="number" className="form-control" placeholder="Posici&oacute;n Z" min="-32768" max="32768" ref="posZ" defaultValue={this.robot != null ? this.robot.posZ : '0'} />
                        </div>

                        <div className="col">
                            <label>Offset X</label>
                            <input type="number" className="form-control" placeholder="Posici&oacute;n X" min="-32768" max="32768" ref="offX" defaultValue={this.robot != null ? this.robot.offsetPosX : '0'} />
                        </div>
                        <div className="col">
                            <label>Offset Y</label>
                            <input type="number" className="form-control" placeholder="Posici&oacute;n Y" min="-32768" max="32768" ref="offY" defaultValue={this.robot != null ? this.robot.offsetPosY : '0'} />
                        </div>
                        <div className="col">
                            <label>Offset Z</label>
                            <input type="number" className="form-control" placeholder="Posici&oacute;n Z" min="-32768" max="32768" ref="offZ" defaultValue={this.robot != null ? this.robot.offsetPosZ : '0'} />
                        </div>
                    </div>
                    <div id="robotGpsContainer">
                        <table className="table table-dark">
                            <thead>
                                <tr>
                                    <th scope="col">#</th>
                                    <th scope="col">Posici&oacute;n X</th>
                                    <th scope="col">Posici&oacute;n Y</th>
                                    <th scope="col">Posici&oacute;n Z</th>
                                </tr>
                            </thead>
                            <tbody id="robotGPS"></tbody>
                        </table>
                    </div>

                    <h5>Descripci&oacute;n</h5>
                    <textarea className="form-control" ref="descripcion" defaultValue={this.robot != null ? this.robot.descripcion : ''}></textarea>

                </div>
            </div>

            <div id="botonesInferiores">
                <button type="button" className="btn btn-danger" onClick={this.eliminarPrompt}>Borrar</button>
                <button type="button" className="btn btn-dark" onClick={this.logs}>Logs</button>
                <button type="button" className="btn btn-dark" onClick={this.terminal}>Terminal</button>
                <button type="button" className="btn btn-dark" onClick={this.composicion}>Ensamblador</button>

                <button type="button" className="btn btn-primary" onClick={this.guardarRobot}>Guardar</button>
                <button type="button" className="btn btn-success" onClick={this.aceptar}>Aceptar</button>
                <button type="button" className="btn btn-light" onClick={this.handleCancelar}>Cancelar</button>
            </div>

        </div>;
    }
};

class RobotFormDeleteConfirm extends Component {
    constructor({ robotName, robotId, handleEliminar }) {
        super();

        this.robotName = robotName;
        this.robotId = robotId;

        this.handleEliminar = handleEliminar;

        this.eliminar = this.eliminar.bind(this);
    }

    componentDidMount() {
        window.$('#robotFormDeleteConfirm').modal({ show: true });
    }

    eliminar() {
        window.$('#robotFormDeleteConfirm').modal('hide');
        this.handleEliminar();
    }

    render() {
        return <div className="modal fade" id="robotFormDeleteConfirm" tabIndex="-1" role="dialog" aria-labelledby="robotFormDeleteConfirmLabel" aria-hidden="true">
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="robotFormDeleteConfirmLabel">Confirmar eliminaci&oacute;n</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <p>&iquest;Est&aacute;s seguro de que quieres eliminar el robot {this.robotName}#{this.robotId}?</p>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                        <button type="button" className="btn btn-danger" onClick={this.eliminar}>Eliminar</button>
                    </div>
                </div>
            </div>
        </div>
    }
}



export default RobotForm;
