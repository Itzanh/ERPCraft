import { Component } from "react";
import ReactDOM from 'react-dom';

import FormAlert from "../FormAlert";

class ServerForm extends Component {
    constructor({ server, handleAdd, handleUpdate, handleDelete, handlePwd }) {
        super();

        this.server = server;
        this.handleAdd = handleAdd;
        this.handleUpdate = handleUpdate;
        this.handleDelete = handleDelete;
        this.handlePwd = handlePwd;

        this.aceptar = this.aceptar.bind(this);
        this.delete = this.delete.bind(this);
        this.pwd = this.pwd.bind(this);
    }

    componentDidMount() {
        window.$('#serverModal').modal({ show: true });
    }

    showAlert(txt) {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderServersModalAlert'));
        ReactDOM.render(<FormAlert
            txt={txt}
        />, document.getElementById('renderServersModalAlert'));
    }

    isValidUUID(uuid) {
        return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(uuid);
    }

    aceptar() {
        if (this.server == null) {
            this.add();
        } else {
            this.update();
        }
    }

    async add() {
        const server = {};
        server.uuid = this.refs.uuid.value;
        server.name = this.refs.name.value;
        server.descripcion = this.refs.dsc.value;
        server.permitirAutoregistro = this.refs.permitirAutoregistro.checked;
        server.notificacionOnline = this.refs.notificacionOnline.checked;
        server.notificacionOffline = this.refs.notificacionOffline.checked;

        if (server.uuid == null || server.uuid.length == 0 || !this.isValidUUID(server.uuid)) {
            this.showAlert("Se debe escribir un UUID valido.");
            return false;
        }
        if (server.name == null || server.name.length == 0) {
            this.showAlert("El nombre no puede estar vacio.");
            return false;
        }

        await this.handleAdd(server);
        window.$('#serverModal').modal('hide');
    }

    async update() {
        const server = {};
        server.uuid = this.refs.uuid.value;
        server.name = this.refs.name.value;
        server.descripcion = this.refs.dsc.value;
        server.permitirAutoregistro = this.refs.permitirAutoregistro.checked;
        server.notificacionOnline = this.refs.notificacionOnline.checked;
        server.notificacionOffline = this.refs.notificacionOffline.checked;
        console.log(server);

        if (server.name == null || server.name.length == 0) {
            this.showAlert("El nombre no puede estar vacio.");
            return false;
        }

        await this.handleUpdate(server);
        window.$('#serverModal').modal('hide');
    }

    async delete() {
        await this.handleDelete(this.refs.uuid.value);
        window.$('#serverModal').modal('hide');
    }

    pwd() {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderServersPwdModal"));
        ReactDOM.render(<ServerFormPwd
            handlePwd={(pwd) => {
                this.handlePwd(this.server.uuid, pwd);
            }}
        />, document.getElementById("renderServersPwdModal"));
    }

    render() {
        return <div className="modal fade" id="serverModal" tabIndex="-1" role="dialog" aria-labelledby="serverModalLabel" aria-hidden="true">
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="serverModalLabel">Servidor</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <label>UUID</label>
                        <input type="text" className="form-control" id="serverUUID" ref="uuid" placeholder="UUID" defaultValue={this.server != null ? this.server.uuid : ''} readOnly={this.server != null} />
                        <label>Nombre</label>
                        <input type="text" className="form-control" id="serverName" ref="name" placeholder="Nombre" defaultValue={this.server != null ? this.server.name : ''} />
                        <label>Descripcion</label>
                        <textarea className="form-control" ref="dsc" defaultValue={this.server != null ? this.server.descripcion : ''}></textarea>
                        <input type="checkbox" defaultChecked={this.server != null ? this.server.permitirAutoregistro : false} ref="permitirAutoregistro" />
                        <label>&iquest;Permitir autoregistrarse?</label>
                        <h6>Notificaciones</h6>
                        <input type="checkbox" defaultChecked={this.server != null ? this.server.notificacionOnline : false} ref="notificacionOnline" />
                        <label>&iquest;Notificaci&oacute;n de conexi&oacute;n?</label>
                        <br />
                        <input type="checkbox" defaultChecked={this.server != null ? this.server.notificacionOffline : false} ref="notificacionOffline" />
                        <label>&iquest;Notificaci&oacute;n de desconexi&oacute;n?</label>
                    </div>
                    <div className="modal-footer">
                        <button type="button" class="btn btn-warning" onClick={this.pwd}>Contrase&ntilde;a autoreg.</button>
                        <button type="button" className="btn btn-danger" onClick={this.delete}>Eliminar</button>
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                        <button type="button" className="btn btn-primary" onClick={this.aceptar}>Aceptar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class ServerFormPwd extends Component {
    constructor({ handlePwd }) {
        super();

        this.handlePwd = handlePwd;

        this.aceptar = this.aceptar.bind(this);
    }

    componentDidMount() {
        window.$('#serverModalPwd').modal({ show: true });
    }

    aceptar() {
        const pwd = this.refs.pwd.value;
        if (pwd.length == 0)
            return;

        this.handlePwd(pwd);
        window.$('#serverModalPwd').modal('hide');
    }

    render() {
        return <div className="modal fade" id="serverModalPwd" tabIndex="-1" role="dialog" aria-labelledby="serverModalPwdLabel" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="serverModalPwdLabel">Contrase&ntilde;a de autoregistro</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <p>La contrase&ntilde;a de autoregistro permite a que se registren dispositivos nuevos desde el juego sin necesidad de introducir sus datos y su UUID manualmente, usando una contrase&ntilde;a que se ha de configurar.</p>
                        <label>Contrase&ntilde;a</label>
                        <input type="password" className="form-control" id="serverPwd" ref="pwd" placeholder="Contrase&ntilde;a" />
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                        <button type="button" class="btn btn-primary" onClick={this.aceptar}>OK</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

export default ServerForm;


