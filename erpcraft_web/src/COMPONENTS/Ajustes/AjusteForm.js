import { Component } from "react";
import ReactDOM from 'react-dom';

import FormAlert from "../FormAlert";
import settingsIco from './../../IMG/settings.svg';

class AjusteForm extends Component {
    constructor({ ajuste, handleAdd, handleUpdate, handleActivar, handleLimpiar, handleEliminar }) {
        super();

        this.ajuste = ajuste;
        this.handleAdd = handleAdd;
        this.handleUpdate = handleUpdate;
        this.handleActivar = handleActivar;
        this.handleLimpiar = handleLimpiar;
        this.handleEliminar = handleEliminar;

        this.guardar = this.guardar.bind(this);
        this.borrar = this.borrar.bind(this);
        this.activar = this.activar.bind(this);
        this.limpiar = this.limpiar.bind(this);
    }

    showAlert(txt) {
        ReactDOM.unmountComponentAtNode(document.getElementById('ajusteFormModal'));
        ReactDOM.render(<FormAlert
            txt={txt}
        />, document.getElementById('ajusteFormModal'));
    }

    guardar() {
        const ajuste = {};
        if (this.ajuste != null)
            ajuste.id = this.ajuste.id;
        ajuste.name = this.refs.name.value;
        ajuste.limpiarRobotGps = this.refs.limpiarRobotGps.checked;
        ajuste.diasRobotGps = parseInt(this.refs.diasRobotGps.value);
        ajuste.limpiarRobotLogs = this.refs.limpiarRobotLogs.checked;
        ajuste.diasRobotLogs = parseInt(this.refs.diasRobotLogs.value);
        ajuste.limpiarDroneGps = this.refs.limpiarDroneGps.checked;
        ajuste.diasDroneGps = parseInt(this.refs.diasDroneGps.value);
        ajuste.limpiarDroneLogs = this.refs.limpiarDroneLogs.checked;
        ajuste.diasDroneLogs = parseInt(this.refs.diasDroneLogs.value);
        ajuste.limpiarBateriaHistorial = this.refs.limpiarBateriaHistorial.checked;
        ajuste.horasBateriaHistorial = parseInt(this.refs.horasBateriaHistorial.value);
        ajuste.vacuumLimpiar = this.refs.vacuumLimpiar.checked;
        ajuste.reindexLimpiar = this.refs.reindexLimpiar.checked;
        ajuste.pingInterval = parseInt(this.refs.pingInterval.value);
        ajuste.timeout = parseInt(this.refs.timeout.value);
        ajuste.puertoWeb = parseInt(this.refs.puertoWeb.value);
        ajuste.puertoOC = parseInt(this.refs.puertoOC.value);
        ajuste.hashIteraciones = parseInt(this.refs.hashIteraciones.value);
        ajuste.limpiarNotificaciones = this.refs.limpiarNotificaciones.checked;
        ajuste.horasNotificaciones = parseInt(this.refs.horasNotificaciones.value);

        if (ajuste.name == null || ajuste.name.length == 0) {
            this.showAlert("El nombre no puede estar vacio");
            return;
        }

        if (this.ajuste == null) {
            this.handleAdd(ajuste).then((id) => {
                ajuste.id = id;
                this.ajuste = ajuste;
                this.refs.id.innerText = "ID: " + id;
            });
        } else {
            this.handleUpdate(ajuste);
        }
    }

    borrar() {
        if (this.ajuste == null) {
            return;
        }
        if (this.ajuste.activado) {
            ReactDOM.unmountComponentAtNode(document.getElementById("ajusteFormModal"));
            ReactDOM.render(<AjusteFormErrorAlert

            />, document.getElementById("ajusteFormModal"));
        } else {
            ReactDOM.unmountComponentAtNode(document.getElementById("ajusteFormModal"));
            ReactDOM.render(<AjusteFormDeleteConfirm
                handleEliminar={() => {
                    this.handleEliminar(this.ajuste.id);
                    ReactDOM.unmountComponentAtNode(document.getElementById("renderAjuste"));
                }}
            />, document.getElementById("ajusteFormModal"));
        }
    }

    activar() {
        if (this.ajuste == null) {
            return;
        }
        this.handleActivar(this.ajuste.id);
    }

    async limpiar() {
        if (this.ajuste == null) {
            return;
        }
        await ReactDOM.unmountComponentAtNode(document.getElementById("ajusteFormModal"));
        await ReactDOM.render(<AjusteFormLimpieza
        />, document.getElementById("ajusteFormModal"));

        await this.handleLimpiar();
        await ReactDOM.unmountComponentAtNode(document.getElementById("ajusteFormModal"));
    }

    render() {
        return <div id="ajusteForm">
            <div id="ajusteFormModal"></div>
            <div id="ajustesTitle">
                <img src={settingsIco} />
                <h3>Configuraci&oacute;n</h3>
                <p ref="id">ID: {this.ajuste != null ? this.ajuste.id : 0}</p>
            </div>

            <div className="row">
                <div className="col">
                    <label>Nombre</label>
                    <input type="text" className="form-control" placeholder="Nombre" ref="name" defaultValue={this.ajuste != null ? this.ajuste.name : ''} />
                </div>
                <div className="col checkContainer">
                    <input type="checkbox" readOnly={true} defaultChecked={this.ajuste != null ? this.ajuste.activado : false} />
                    <label>&iquest;Activado?</label>
                </div>
            </div>

            <h3>Ajustes del servidor</h3>
            <div className="row">
                <div className="col">
                    <label>Puerto de la web</label>
                    <input type="number" className="form-control" min="1" max="65535" placeholder="Puerto de la web" ref="puertoWeb" defaultValue={this.ajuste != null ? this.ajuste.puertoWeb : 32324} />
                </div>
                <div className="col">
                    <label>Puerto de OpenComputers</label>
                    <input type="number" className="form-control" min="1" max="65535" placeholder="Puerto de OpenComputers" ref="puertoOC" defaultValue={this.ajuste != null ? this.ajuste.puertoOC : 32325} />
                </div>
                <div className="col">
                    <label>Iteraciones del hash de las contrase&ntilde;as de los usuarios</label>
                    <input type="number" className="form-control" min="1" max="1000000" placeholder="Iteraciones del hash de las contrase&ntilde;as de los usuarios" ref="hashIteraciones" defaultValue={this.ajuste != null ? this.ajuste.hashIteraciones : 5000} />
                </div>
            </div>

            <h3>Ajustes de red</h3>
            <div className="row">
                <div className="col">
                    <label>Segundos del intervalo de ping</label>
                    <input type="number" className="form-control" min="1" max="32767" placeholder="Segundos del intervalo de ping" ref="pingInterval" defaultValue={this.ajuste != null ? this.ajuste.pingInterval : 0} />
                </div>
                <div className="col">
                    <label>Segundos de timeout de los dispositivos</label>
                    <input type="number" className="form-control" min="2" max="32767" placeholder="Segundos de timeout de los dispositivos" ref="timeout" defaultValue={this.ajuste != null ? this.ajuste.timeout : 0} />
                </div>
            </div>

            <h3>Ajustes de limpieza</h3>
            <div className="row">
                <div className="col checkContainer">
                    <input type="checkbox" ref="limpiarRobotGps" defaultChecked={this.ajuste != null ? this.ajuste.limpiarRobotGps : false} />
                    <label>&iquest;Limpiar el historial de ubicaciones del Robot?</label>
                </div>
                <div className="col">
                    <label>D&iacute;as de duraci&oacute;n del historial de ubicaciones del Robot</label>
                    <input type="number" className="form-control" min="0" max="32767" placeholder="D&iacute;as de duraci&oacute;n del historial de ubicaciones del Robot" ref="diasRobotGps" defaultValue={this.ajuste != null ? this.ajuste.diasRobotGps : 0} />
                </div>
                <div className="col checkContainer">
                    <input type="checkbox" ref="limpiarRobotLogs" defaultChecked={this.ajuste != null ? this.ajuste.limpiarRobotLogs : false} />
                    <label>&iquest;Limpiar los logs del Robot?</label>
                </div>
                <div className="col">
                    <label>D&iacute;as de duraci&oacute;n de los logs del Robot</label>
                    <input type="number" className="form-control" min="0" max="32767" placeholder="D&iacute;as de duraci&oacute;n de los logs del Robot" ref="diasRobotLogs" defaultValue={this.ajuste != null ? this.ajuste.diasRobotLogs : 0} />
                </div>
            </div>

            <div className="row">
                <div className="col checkContainer">
                    <input type="checkbox" ref="limpiarDroneGps" defaultChecked={this.ajuste != null ? this.ajuste.limpiarDroneGps : false} />
                    <label>&iquest;Limpiar el historial de ubicaciones del Drone?</label>
                </div>
                <div className="col">
                    <label>D&iacute;as de duraci&oacute;n del historial de ubicaciones del Drone</label>
                    <input type="number" className="form-control" min="0" max="32767" placeholder="D&iacute;as de duraci&oacute;n del historial de ubicaciones del Drone" ref="diasDroneGps" defaultValue={this.ajuste != null ? this.ajuste.diasDroneGps : 0} />
                </div>
                <div className="col checkContainer">
                    <input type="checkbox" ref="limpiarDroneLogs" defaultChecked={this.ajuste != null ? this.ajuste.limpiarDroneLogs : false} />
                    <label>&iquest;Limpiar los logs del Drone?</label>
                </div>
                <div className="col">
                    <label>D&iacute;as de duraci&oacute;n de los logs del Drone</label>
                    <input type="number" className="form-control" min="0" max="32767" placeholder="D&iacute;as de duraci&oacute;n de los logs del Drone" ref="diasDroneLogs" defaultValue={this.ajuste != null ? this.ajuste.diasDroneLogs : 0} />
                </div>
            </div>

            <div className="row">
                <div className="col checkContainer">
                    <input type="checkbox" ref="limpiarBateriaHistorial" defaultChecked={this.ajuste != null ? this.ajuste.limpiarBateriaHistorial : false} />
                    <label>&iquest;Limpiar el historial de la bater&iacute;a?</label>
                </div>
                <div className="col">
                    <label>Horas de duraci&oacute;n del historial de la bater&iacute;a</label>
                    <input type="number" className="form-control" min="0" max="32767" placeholder="Horas de duraci&oacute;n del historial de la bater&iacute;a" ref="horasBateriaHistorial" defaultValue={this.ajuste != null ? this.ajuste.horasBateriaHistorial : 0} />
                </div>
                <div className="col checkContainer">
                    <input type="checkbox" ref="limpiarNotificaciones" defaultChecked={this.ajuste != null ? this.ajuste.limpiarNotificaciones : false} />
                    <label>&iquest;Limpiar las notificaciones leidas?</label>
                </div>
                <div className="col">
                    <label>Horas de duraci&oacute;n de las notificaciones leidas</label>
                    <input type="number" className="form-control" min="0" max="32767" placeholder="Horas de duraci&oacute;n de las notificaciones leidas" ref="horasNotificaciones" defaultValue={this.ajuste != null ? this.ajuste.horasNotificaciones : 0} />
                </div>
            </div>

            <div className="row">
                <div className="col checkContainer">
                    <input type="checkbox" ref="vacuumLimpiar" defaultChecked={this.ajuste != null ? this.ajuste.vacuumLimpiar : false} />
                    <label>&iquest;Ejecutar el comando VACUUM al terminar de limpiar?</label>
                </div>
                <div className="col checkContainer">
                    <input type="checkbox" ref="reindexLimpiar" defaultChecked={this.ajuste != null ? this.ajuste.reindexLimpiar : false} />
                    <label>&iquest;Reindexar todas las tablas al terminar de limpiar?</label>
                </div>
                <div className="col">
                </div>
                <div className="col">
                </div>
            </div>

            <button type="button" className="btn btn-success" onClick={this.guardar}>Guardar</button>
            <button type="button" className="btn btn-danger" onClick={this.borrar}>Borrar</button>
            <button type="button" className="btn btn-warning" onClick={this.activar}>Activar</button>
            <button type="button" className="btn btn-danger" onClick={this.limpiar}>Limpiar!</button>

        </div>
    }
};

class AjusteFormErrorAlert extends Component {
    componentDidMount() {
        window.$('#ajusteFormAlertModal').modal({ show: true });
    }

    render() {
        return <div className="modal fade" id="ajusteFormAlertModal" tabIndex="-1" role="dialog" aria-labelledby="ajusteFormAlertModalLabel" aria-hidden="true">
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="ajusteFormAlertModalLabel">Error</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <p>No puedes eliminar este ajuste porque es el ajuste activo. Por favor, establece otro ajuste como activo antes de intentar eliminarlo.</p>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class AjusteFormDeleteConfirm extends Component {
    constructor({ handleEliminar }) {
        super();

        this.handleEliminar = handleEliminar;

        this.borrar = this.borrar.bind(this);
    }

    componentDidMount() {
        window.$('#ajusteFormDeleteConfirmModal').modal({ show: true });
    }

    borrar() {
        window.$('#ajusteFormDeleteConfirmModal').modal('hide');
        this.handleEliminar();
    }

    render() {
        return <div className="modal fade" id="ajusteFormDeleteConfirmModal" tabIndex="-1" role="dialog" aria-labelledby="ajusteFormDeleteConfirmModal" aria-hidden="true">
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="ajusteFormDeleteConfirmModalLabel">Confirmar eliminaci&oacute;n</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <p>&iquest;Est&aacute;s seguro de que quieres eliminar este ajuste permanentemente?</p>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-danger" onClick={this.borrar}>Eliminar</button>
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class AjusteFormLimpieza extends Component {
    componentDidMount() {
        window.$('#ajusteFormLimpieza').modal({ show: true });
    }

    componentWillUnmount() {
        window.$('#ajusteFormLimpieza').modal('hide');
    }

    render() {
        return <div className="modal fade" id="ajusteFormLimpieza" tabIndex="-1" role="dialog" aria-labelledby="ajusteFormLimpiezaLabel" aria-hidden="true">
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="ajusteFormLimpiezaLabel">Ejecutando limpieza</h5>
                    </div>
                    <div className="modal-body">
                        <p>Se est&aacute; ejecutando la limpieza seg&uacute;n los ajustes activos actuales. Por favor, espere...</p>
                    </div>
                    <div className="modal-footer">
                    </div>
                </div>
            </div>
        </div>
    }
};

export default AjusteForm;


