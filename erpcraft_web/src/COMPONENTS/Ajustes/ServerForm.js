import { Component } from "react";

class ServerForm extends Component {
    constructor({ server, handleAdd, handleUpdate, handleDelete }) {
        super();

        this.server = server;
        this.handleAdd = handleAdd;
        this.handleUpdate = handleUpdate;
        this.handleDelete = handleDelete;

        this.aceptar = this.aceptar.bind(this);
        this.delete = this.delete.bind(this);
    }

    componentDidMount() {
        window.$('#serverModal').modal({ show: true });
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

        await this.handleAdd(server);
        window.$('#serverModal').modal('hide');
    }

    async update() {
        const server = {};
        server.uuid = this.refs.uuid.value;
        server.name = this.refs.name.value;
        server.descripcion = this.refs.dsc.value;

        await this.handleUpdate(server);
        window.$('#serverModal').modal('hide');
    }

    async delete() {
        await this.handleDelete(this.refs.uuid.value);
        window.$('#serverModal').modal('hide');
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
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-danger" onClick={this.delete}>Eliminar</button>
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                        <button type="button" className="btn btn-primary" onClick={this.aceptar}>Aceptar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

export default ServerForm;


