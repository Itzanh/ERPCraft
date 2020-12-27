import { Component } from "react";
import ReactDOM from 'react-dom';

class UsuarioEditForm extends Component {
	constructor({ usuario, handleDelete, handleEdit, handlePwd }) {
		super();

		this.usuario = usuario;
		this.handleDelete = handleDelete;
		this.handleEdit = handleEdit;
		this.handlePwd = handlePwd;

		this.cambiarPwd = this.cambiarPwd.bind(this);
		this.eliminar = this.eliminar.bind(this);
		this.aceptar = this.aceptar.bind(this);
	}

	componentDidMount() {
		window.$('#usuarioEditFormModal').modal({ show: true });
	}

	cambiarPwd() {
		window.$('#usuarioEditFormModal').modal('hide');
		ReactDOM.unmountComponentAtNode(document.getElementById('renderUsuarioModal'));
		ReactDOM.render(<UsuarioCambiarContrasenya
			id={this.usuario.id}
			handlePwd={this.handlePwd}
		/>, document.getElementById('renderUsuarioModal'));
	}

	eliminar() {
		this.handleDelete(this.usuario.id).then(() => {
			window.$('#usuarioEditFormModal').modal('hide');
		});
	}

	aceptar() {
		const usuario = {};
		usuario.id = this.usuario.id;
		usuario.name = this.refs.name.value;
		usuario.off = this.refs.off.checked;
		usuario.descripcion = this.refs.dsc.value;

		this.handleEdit(usuario).then(() => {
			window.$('#usuarioEditFormModal').modal('hide');
		});
	}

	render() {
		return <div className="modal fade" id="usuarioEditFormModal" tabIndex="-1" role="dialog" aria-labelledby="usuarioEditFormModalLabel" aria-hidden="true">
			<div className="modal-dialog" role="document">
				<div className="modal-content">
					<div className="modal-header">
						<h5 className="modal-title" id="usuarioEditFormModalLabel">Editar usuario</h5>
						<button type="button" className="close" data-dismiss="modal" aria-label="Close">
							<span aria-hidden="true">&times;</span>
						</button>
					</div>
					<div className="modal-body">
						<label>Nombre</label>
						<input type="text" className="form-control" placeholder="Nombre" ref="name" defaultValue={this.usuario.name} />
						<br />
						<input type="checkbox" placeholder="&iquest;Desactivado?" ref="off" defaultChecked={this.usuario.off} />
						<label>&iquest;Desactivado?</label>
						<br />
						<label>Descripci&oacute;n</label>
						<textarea className="form-control" ref="dsc" rows="4" defaultValue={this.usuario.descripcion}></textarea>
					</div>
					<div className="modal-footer">
						<button type="button" className="btn btn-primary" onClick={this.aceptar}>Aceptar</button>
						<button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
						<button type="button" className="btn btn-warning" onClick={this.cambiarPwd}>Cambiar contrase&ntilde;a</button>
						<button type="button" className="btn btn-danger" onClick={this.eliminar}>Eliminar</button>
					</div>
				</div>
			</div>
		</div>
	}
};

class UsuarioCambiarContrasenya extends Component {
	constructor({ id, handlePwd }) {
		super();

		this.id = id;
		this.handlePwd = handlePwd;

		this.aceptar = this.aceptar.bind(this);
	}

	componentDidMount() {
		window.$('#usuarioChgPwdFormModal').modal({ show: true });
	}

	aceptar() {
		const pwd1 = this.refs.pwd1.value;
		const pwd2 = this.refs.pwd2.value;
		if (pwd1 != pwd2) {
			alert("Las contraseñas no coinciden");
			return;
		}

		this.handlePwd(this.id, this.refs.pwd.value, pwd1).then(() => {
			window.$('#usuarioChgPwdFormModal').modal('hide');
		});
	}

	render() {
		return <div className="modal fade" id="usuarioChgPwdFormModal" tabIndex="-1" role="dialog" aria-labelledby="usuarioChgPwdFormModalLabel" aria-hidden="true">
			<div className="modal-dialog" role="document">
				<div className="modal-content">
					<div className="modal-header">
						<h5 className="modal-title" id="usuarioChgPwdFormModalLabel">Cambiar contrase&ntilde;a</h5>
						<button type="button" className="close" data-dismiss="modal" aria-label="Close">
							<span aria-hidden="true">&times;</span>
						</button>
					</div>
					<div className="modal-body">
						<label>Contrase&ntilde;a actual</label>
						<input type="password" className="form-control" placeholder="Contrase&ntilde;a actual" ref="pwd" />
						<label>Contrase&ntilde;a nueva</label>
						<input type="password" className="form-control" placeholder="Contrase&ntilde;a nueva" ref="pwd1" />
						<label>Repetir contrase&ntilde;a nueva</label>
						<input type="password" className="form-control" placeholder="Repetir contrase&ntilde;a nueva" ref="pwd2" />
					</div>
					<div className="modal-footer">
						<button type="button" className="btn btn-primary" onClick={this.aceptar}>Aceptar</button>
						<button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
					</div>
				</div>
			</div>
		</div>
	}
};

export default UsuarioEditForm;
