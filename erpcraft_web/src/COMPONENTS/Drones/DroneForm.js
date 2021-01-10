import { Component } from "react";
import ReactDOM from 'react-dom';

import droneIco from './../../IMG/drone.png';
import DroneLogs from "./DroneLogs";
import FormAlert from "../FormAlert";

// IMG
import onlineIco from './../../IMG/robot_estado/online.svg';
import minandoIco from './../../IMG/robot_estado/minando.png';
import offlineIco from './../../IMG/robot_estado/offline.svg';
import conPerdidaIco from './../../IMG/robot_estado/con_perdida.svg';
import sinBateriaIco from './../../IMG/robot_estado/sin_bateria.svg';


const estadosDrone = {
    "O": "Online",
    "M": "Minando",
    "F": "Offline",
    "L": "Conexión perdida",
    "B": "Sin batería"
};

const imagenEstadosDrone = {
    "O": onlineIco,
    "M": minandoIco,
    "F": offlineIco,
    "L": conPerdidaIco,
    "B": sinBateriaIco
};

class DroneForm extends Component {
    constructor({ drone, droneChange, handleCancelar, handleAddDrone, handleEditDrone, handleEliminar, handleLogs, handleDeleteLogs, handleClearLogs }) {
        super();

        this.drone = drone;
        if (drone != null) {
            this.droneChange = this.droneChange.bind(this);
            droneChange(this.droneChange);
            this.handleDroneChange = droneChange;
        }

        this.handleCancelar = handleCancelar;
        this.handleAddDrone = handleAddDrone;
        this.handleEditDrone = handleEditDrone;
        this.handleEliminar = handleEliminar;
        this.handleLogs = handleLogs;
        this.handleDeleteLogs = handleDeleteLogs;
        this.handleClearLogs = handleClearLogs;

        this.calcularPorcentajeBateria = this.calcularPorcentajeBateria.bind(this);
        this.eliminarPrompt = this.eliminarPrompt.bind(this);
        this.guardarRobot = this.guardarDrone.bind(this);
        this.guardar = this.guardar.bind(this);
        this.eliminar = this.eliminar.bind(this);
        this.aceptar = this.aceptar.bind(this);
        this.logs = this.logs.bind(this);
    }

    componentDidMount() {
        this.calcularPorcentajeBateria();
    }

    componentWillUnmount() {
        this.handleDroneChange(null);
    }

    calcularPorcentajeBateria() {
        const porcentaje = Math.floor((parseInt(this.refs.energiaActual.value) / parseInt(this.refs.totalEnergia.value)) * 100) + '%';
        this.refs.barraBateria.style.width = porcentaje;
        this.refs.barraBateria.innerText = porcentaje;
    }

    droneChange(drone) {
        if (drone == null) {
            this.handleCancelar();
            return;
        }

        this.drone = drone;
        this.refs.name.value = this.drone.name;
        this.refs.uuid.value = this.drone.uuid;
        this.refs.tier.value = this.drone.tier;
        this.refs.totalEnergia.value = this.drone.totalEnergia;
        this.refs.energiaActual.value = this.drone.energiaActual;
        this.refs.upgradeGenerador.checked = this.drone.upgradeGenerador;
        this.refs.itemsGenerador.value = this.drone.itemsGenerador;
        this.refs.numeroSlots.value = this.drone.numeroSlots;
        this.refs.numeroStacks.value = this.drone.numeroStacks;
        this.refs.numeroItems.value = this.drone.numeroItems;
        this.refs.upgradeGps.checked = this.drone.upgradeGps;
        this.refs.offX.value = this.drone.offsetPosX;
        this.refs.offY.value = this.drone.offsetPosY;
        this.refs.offZ.value = this.drone.offsetPosZ;
        this.refs.posX.value = this.drone.posX;
        this.refs.posY.value = this.drone.posY;
        this.refs.posZ.value = this.drone.posZ;
        this.refs.descripcion.value = this.drone.descripcion;
        this.refs.off.checked = this.drone.off;
        this.refs.dateAdd.value = this.formatearFechaTiempo(this.drone.dateAdd);
        this.refs.dateUpd.value = this.formatearFechaTiempo(this.drone.dateUpd);
        this.refs.fechaConexion.value = this.formatearFechaTiempo(this.drone.fechaConexion);
        this.refs.fechaDesconexion.value = this.formatearFechaTiempo(this.drone.fechaDesconexion);
        this.refs.estado.innerText = estadosDrone[this.drone.estado];
        this.refs.estadoImg.src = imagenEstadosDrone[this.drone.estado];
    }

    getDrone() {
        const drone = {};
        drone.id = this.drone ? this.drone.id : 0;
        drone.name = this.refs.name.value;
        if (this.refs.uuid.value == '') {
            drone.uuid = null;
        } else {
            drone.uuid = this.refs.uuid.value;
        }
        drone.tier = parseInt(this.refs.tier.value);
        drone.totalEnergia = parseInt(this.refs.totalEnergia.value);
        drone.energiaActual = parseInt(this.refs.energiaActual.value);
        drone.upgradeGenerador = this.refs.upgradeGenerador.checked;
        drone.itemsGenerador = parseInt(this.refs.itemsGenerador.value);
        drone.numeroSlots = parseInt(this.refs.numeroSlots.value);
        drone.upgradeGps = this.refs.upgradeGps.checked;
        drone.posX = parseInt(this.refs.posX.value);
        drone.posY = parseInt(this.refs.posY.value);
        drone.posZ = parseInt(this.refs.posZ.value);
        drone.descripcion = this.refs.descripcion.value;
        drone.offsetPosX = parseInt(this.refs.offX.value);
        drone.offsetPosY = parseInt(this.refs.offY.value);
        drone.offsetPosZ = parseInt(this.refs.offZ.value);
        drone.off = this.refs.off.checked;

        return drone;
    }

    showAlert(txt) {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderDroneFormModalAlert'));
        ReactDOM.render(<FormAlert
            txt={txt}
        />, document.getElementById('renderDroneFormModalAlert'));
    }

    isValidUUID(uuid) {
        return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(uuid);
    }

    droneIsValid(drone) {
        if (drone.name == null || drone.name.length == 0) {
            this.showAlert("Debes especificar un nombre para poder continuar.");
            return false;
        }
        if (drone.uuid == null || drone.uuid.length == 0 || !this.isValidUUID(drone.uuid)) {
            this.showAlert("Debes especificar un UUID válido.");
            return false;
        }
        if (drone.tier < 1 || drone.tier > 3) {
            this.showAlert("El tier del robot está entre 1 y 3.");
            return false;
        }
        if (drone.numeroSlots < 0) {
            this.showAlert("El mínimo de slots es 0.");
            return false;
        }
        if (drone.totalEnergia < 0) {
            this.showAlert("El total de energía debe de ser mayor que 0.");
            return false;
        }
        if (drone.energiaActual < 0) {
            this.showAlert("El mínimo de energía del robot es de 0.");
            return false;
        }
        if (drone.itemsGenerador < 0) {
            this.showAlert("Los ñitems del generador deben ser 0 como mínimo.");
            return false;
        }
        return true;
    }

    guardarDrone() {
        return new Promise((resolve, reject) => {
            this.drone = this.getDrone();
            if (!this.droneIsValid(this.drone)) {
                reject(false);
                return;
            }

            if (this.drone && this.drone.id == 0) { // creación del drone
                this.handleAddDrone(this.drone).then((response) => {
                    this.drone.id = response;
                    if (this.refs.id) {
                        this.refs.id.innerText = "ID: " + response;
                    }
                    resolve();
                }, () => {
                    reject(true);
                });

            } else { // modificación del robot
                this.handleEditDrone(this.drone).then(() => {
                    resolve();
                }, () => {
                    reject(true);
                });
            }
        });
    }

    guardar() {
        this.guardarDrone().then(null, (showError) => { // error
            if (showError) {
                this.showAlert("No se ha podido guardar el drone. Revisa que todos los datos sean correctos y que se pueda conectar con el servidor.");
            }
        });
    }

    aceptar() {
        this.guardarDrone().then(() => { // ok
            this.handleCancelar();
        }, (showError) => { // error
            if (showError) {
                this.showAlert("No se ha podido guardar el drone. Revisa que todos los datos sean correctos y que se pueda conectar con el servidor.");
            }
        });
    }

    eliminarPrompt() {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderDroneFormModal'));
        ReactDOM.render(<DroneFormDeleteConfirm
            droneName={this.drone.name}
            droneId={this.drone.id}
            handleEliminar={this.eliminar}
        />, document.getElementById('renderDroneFormModal'));
    }

    eliminar() {
        this.handleEliminar(this.drone.id).then(() => {
            this.handleCancelar();
        }, () => {
            alert("No se ha podido eliminar el drone");
        });
    }

    logs() {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderDroneFormModal'));
        ReactDOM.render(<DroneLogs
            idDrone={this.drone.id}
            handleLogs={this.handleLogs}
            handleDeleteLogs={this.handleDeleteLogs}
            handleClearLogs={this.handleClearLogs}
        />, document.getElementById('renderDroneFormModal'));
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
        return <div id="editDrone">
            <div id="renderDroneFormModal"></div>
            <div id="renderDroneFormModalAlert"></div>
            <div id="droneTitle">
                <img src={droneIco} />
                <h3>Drone</h3>
                <p ref="id">ID: {this.drone != null ? this.drone.id : 0}</p>
            </div>

            <div className="form-row" id="detallesBasicos">
                <div className="col">
                    <label>Nombre</label>
                    <input type="text" className="form-control" placeholder="Nombre" ref="name" defaultValue={this.drone != null ? this.drone.name : ''} />
                </div>
                <div className="col">
                    <label>UUID</label>
                    <input type="text" className="form-control" placeholder="Identificador &uacute;nico" ref="uuid" defaultValue={this.drone != null ? this.drone.uuid : ''} />
                </div>
                <div className="col">
                    <label>Tier</label>
                    <input type="number" className="form-control" placeholder="Tier" min="1" max="3" ref="tier" defaultValue={this.drone != null ? this.drone.tier : '1'} />
                </div>
            </div>

            <div className="form-row" id="detallesEnergia">
                <div className="col">
                    <label>Total energ&iacute;a</label>
                    <input type="number" className="form-control" placeholder="Total energ&iacute;a" min="0" max="65535" ref="totalEnergia" onChange={this.calcularPorcentajeBateria} defaultValue={this.drone != null ? this.drone.totalEnergia : '5000'} />
                </div>
                <div className="col">
                    <label>Energ&iacute;a actual</label>
                    <input type="number" className="form-control" placeholder="Energ&iacute;a actual" min="0" max="65535" ref="energiaActual" onChange={this.calcularPorcentajeBateria} defaultValue={this.drone != null ? this.drone.energiaActual : '5000'} />
                </div>
                <div className="col">
                    <div className="progress">
                        <div className="progress-bar" role="progressbar" style={{ width: '0%' }} aria-valuemin="0" aria-valuemax="100" ref="barraBateria">0%</div>
                    </div>
                </div>
                <div className="col">
                    <input type="checkbox" name="upgrade_gen" ref="upgradeGenerador" defaultChecked={this.drone != null && this.drone.upgradeGenerador} />
                    <label>&iquest;Upgrade de generador?</label>
                </div>
                <div className="col">
                    <label>Numero de &iacute;tems en el generador</label>
                    <input type="number" className="form-control" placeholder="Numero de &iacute;tems" min="0" max="64" ref="itemsGenerador" defaultValue={this.drone != null ? this.drone.itemsGenerador : '0'} />
                </div>
            </div>

            <div className="form-row" id="panelCentral">
                <div className="col">
                    <h5>Inventario</h5>
                    <div className="form-row" id="inventarioDetallesBasicos">
                        <div className="col">
                            <label>N&uacute;mero de slots</label>
                            <input type="number" className="form-control" placeholder="N&uacute;mero de slots" min="0" max="64" ref="numeroSlots" defaultValue={this.drone != null ? this.drone.numeroSlots : '8'} />
                        </div>
                        <div className="col">
                            <label>N&uacute;mero de stacks usados</label>
                            <input type="number" className="form-control" ref="numeroStacks" readOnly defaultValue={this.drone != null ? this.drone.numeroStacks : '0'} />
                        </div>
                        <div className="col">
                            <label>&Iacute;tems en el inventario</label>
                            <input type="number" className="form-control" ref="numeroItems" readOnly defaultValue={this.drone != null ? this.drone.numeroItems : '0'} />
                        </div>
                    </div>

                    <div className="row row-cols-1 row-cols-md-4" id="inventarioContenido">

                    </div>

                    <div id="droneRecordDetails">
                        <div className="form-row">
                            <div className="col">
                                <label>Fecha de creaci&oacute;n</label>
                                <input type="text" className="form-control" ref="dateAdd" defaultValue={this.drone != null ? this.formatearFechaTiempo(this.drone.dateAdd) : ''} readOnly={true} />
                            </div>
                            <div className="col">
                                <label>Fecha de modificaci&oacute;n</label>
                                <input type="text" className="form-control" ref="dateUpd" defaultValue={this.drone != null ? this.formatearFechaTiempo(this.drone.dateUpd) : ''} readOnly={true} />
                            </div>
                            <div className="col">
                                <p ref="estado"><img ref="estadoImg" src={imagenEstadosDrone[this.drone != null ? this.drone.estado : 'F']} />{estadosDrone[this.drone != null ? this.drone.estado : 'F']}</p>
                            </div>
                        </div>
                        <div className="form-row">
                            <div className="col">
                                <label>Fecha de conexi&oacute;n</label>
                                <input type="text" className="form-control" ref="fechaConexion" defaultValue={this.drone != null ? this.formatearFechaTiempo(this.drone.fechaConexion) : ''} readOnly={true} />
                            </div>
                            <div className="col">
                                <label>Fecha de desconexi&oacute;n</label>
                                <input type="text" className="form-control" ref="fechaDesconexion" defaultValue={this.drone != null ? this.formatearFechaTiempo(this.drone.fechaDesconexion) : ''} readOnly={true} />
                            </div>
                            <div className="col">
                                <input type="checkbox" defaultChecked={this.drone != null && this.drone.off} ref="off" />
                                <label>&iquest;Desactivado?</label>
                            </div>
                        </div>
                    </div>

                </div>
                <div className="col">
                    <h5>Posici&oacute;n GPS</h5>
                    <div className="form-row" id="gpsDetallesBasicos">
                        <div className="col">
                            <input type="checkbox" name="upgrade_gps" ref="upgradeGps" defaultChecked={this.drone != null && this.drone.upgradeGps} />
                            <label>&iquest;Upgrade de GPS?</label>
                        </div>
                        <div className="col">
                            <label>Posici&oacute;n X</label>
                            <input type="number" className="form-control" placeholder="Posici&oacute;n X" min="-32768" max="32768" ref="posX" defaultValue={this.drone != null ? this.drone.posX : '0'} />
                        </div>
                        <div className="col">
                            <label>Posici&oacute;n Y</label>
                            <input type="number" className="form-control" placeholder="Posici&oacute;n Y" min="-32768" max="32768" ref="posY" defaultValue={this.drone != null ? this.drone.posY : '0'} />
                        </div>
                        <div className="col">
                            <label>Posici&oacute;n Z</label>
                            <input type="number" className="form-control" placeholder="Posici&oacute;n Z" min="-32768" max="32768" ref="posZ" defaultValue={this.drone != null ? this.drone.posZ : '0'} />
                        </div>

                        <div className="col">
                            <label>Offset X</label>
                            <input type="number" className="form-control" placeholder="Posici&oacute;n X" min="-32768" max="32768" ref="offX" defaultValue={this.drone != null ? this.drone.offsetPosX : '0'} />
                        </div>
                        <div className="col">
                            <label>Offset Y</label>
                            <input type="number" className="form-control" placeholder="Posici&oacute;n Y" min="-32768" max="32768" ref="offY" defaultValue={this.drone != null ? this.drone.offsetPosY : '0'} />
                        </div>
                        <div className="col">
                            <label>Offset Z</label>
                            <input type="number" className="form-control" placeholder="Posici&oacute;n Z" min="-32768" max="32768" ref="offZ" defaultValue={this.drone != null ? this.drone.offsetPosZ : '0'} />
                        </div>
                    </div>
                    <div id="droneGpsContainer">
                        <table className="table table-dark">
                            <thead>
                                <tr>
                                    <th scope="col">#</th>
                                    <th scope="col">Posici&oacute;n X</th>
                                    <th scope="col">Posici&oacute;n Y</th>
                                    <th scope="col">Posici&oacute;n Z</th>
                                </tr>
                            </thead>
                            <tbody id="droneGPS"></tbody>
                        </table>
                    </div>

                    <h5>Descripci&oacute;n</h5>
                    <textarea className="form-control" ref="descripcion" defaultValue={this.drone != null ? this.drone.descripcion : ''}></textarea>

                </div>
            </div>

            <div id="botonesInferiores">
                <button type="button" className="btn btn-danger" onClick={this.eliminarPrompt}>Borrar</button>
                <button type="button" className="btn btn-dark" onClick={this.logs}>Logs</button>

                <button type="button" className="btn btn-primary" onClick={this.guardar}>Guardar</button>
                <button type="button" className="btn btn-success" onClick={this.aceptar}>Aceptar</button>
                <button type="button" className="btn btn-light" onClick={this.handleCancelar}>Cancelar</button>
            </div>

        </div>;
    }
};

class DroneFormDeleteConfirm extends Component {
    constructor({ droneName, droneId, handleEliminar }) {
        super();

        this.droneName = droneName;
        this.droneId = droneId;

        this.handleEliminar = handleEliminar;

        this.eliminar = this.eliminar.bind(this);
    }

    componentDidMount() {
        window.$('#droneFormDeleteConfirm').modal({ show: true });
    }

    eliminar() {
        window.$('#droneFormDeleteConfirm').modal('hide');
        this.handleEliminar();
    }

    render() {
        return <div className="modal fade" id="droneFormDeleteConfirm" tabIndex="-1" role="dialog" aria-labelledby="droneFormDeleteConfirmLabel" aria-hidden="true">
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="droneFormDeleteConfirmLabel">Confirmar eliminaci&oacute;n</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <p>&iquest;Est&aacute;s seguro de que quieres eliminar el drone {this.droneName}#{this.droneId}?</p>
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



export default DroneForm;
