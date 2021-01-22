import { Component } from "react";
import ReactDOM from 'react-dom';

import FormAlert from "../FormAlert";
import usersIco from './../../IMG/users.svg';
import './../../CSS/Usuarios.css';

class Usuarios extends Component {
    constructor({ handleAdd, searchUsuarios }) {
        super();

        this.handleAdd = handleAdd;
        this.searchUsuarios = searchUsuarios;

        this.buscar = this.buscar.bind(this);
        this.add = this.add.bind(this);
    }

    buscar() {
        this.searchUsuarios({ text: this.refs.bus.value, off: this.refs.off.checked });
    }

    add() {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderUsuarioModal'));
        ReactDOM.render(<UsuarioAddForm
            handleAdd={this.handleAdd}
        />, document.getElementById('renderUsuarioModal'));
    }

    render() {
        return <div id="tabUsuarios">
            <div id="renderUsuarioModal"></div>
            <div id="renderUsuarioModalAlert"></div>
            <h3><img src={usersIco} />Usuarios</h3>
            <div className="input-group busqueda">
                <input type="text" className="form-control" ref="bus" onChange={this.buscar} />
                <div className="input-group-append">
                    <button type="button" className="btn btn-outline-success" onClick={this.buscar}>Buscar</button>
                </div>
                <input type="checkbox" className="form-control" ref="off" onChange={this.buscar} />
                <label>&iquest;Desactivado?</label>
            </div>
            <button type="button" className="btn btn-primary" onClick={this.add}>A&ntilde;adir</button>
            <table className="table table-dark" id="tableTabUsuarios">
                <thead>
                    <tr>
                        <th scope="col">#</th>
                        <th scope="col">Nombre</th>
                        <th scope="col">Ultima conexi&oacute;n</th>
                        <th scope="col">Estado</th>
                        <th scope="col">Fecha de creaci&oacute;n</th>
                        <th scope="col">Iteraciones</th>
                    </tr>
                </thead>
                <tbody id="renderUsuarios">
                </tbody>
            </table>
        </div>;
    }
};

class UsuarioAddForm extends Component {
    constructor({ handleAdd }) {
        super();

        this.handleAdd = handleAdd;

        this.aceptar = this.aceptar.bind(this);
    }

    componentDidMount() {
        window.$('#usuarioAddFormModal').modal({ show: true });
    }

    showAlert(txt) {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderUsuarioModalAlert'));
        ReactDOM.render(<FormAlert
            txt={txt}
        />, document.getElementById('renderUsuarioModalAlert'));
    }

    aceptar() {
        const name = this.refs.name.value;
        const pwd = this.refs.pwd.value;

        if (name == null || name.length == 0) {
            this.showAlert("El nombre no puede estar vacio.");
            return;
        }
        if (pwd == null || pwd.length == 0) {
            this.showAlert("La contraseña no puede estar vacia.");
            return;
        }

        this.handleAdd(this.refs.name.value, this.refs.pwd.value).then(() => {
            window.$('#usuarioAddFormModal').modal('hide');
        });
    }

    render() {
        return <div className="modal fade" id="usuarioAddFormModal" tabIndex="-1" role="dialog" aria-labelledby="usuarioAddFormModalLabel" aria-hidden="true">
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="usuarioAddFormModalLabel">Crear usuario</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <label>Nombre</label>
                        <input type="text" className="form-control" placeholder="Nombre" ref="name" />
                        <label>Contrase&ntilde;a</label>
                        <input type="password" className="form-control" placeholder="Contrase&ntilde;a" ref="pwd" />
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-primary" onClick={this.aceptar}>Agregar</button>
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};



export default Usuarios;


