import { Component } from "react";
import ReactDOM from 'react-dom';

import FormAlert from "../FormAlert";
import photoICO from './../../IMG/photo.svg';

class EditArticulo extends Component {
    constructor({ id, name, minecraftID, cant, descripcion, img, handleEdit, handleSubirImagen, handleQuitarImagen, handleEliminar }) {
        super();

        this.id = id;
        this.name = name;
        this.minecraftID = minecraftID;
        this.cant = cant;
        this.descripcion = descripcion;
        this.img = img;

        this.handleEdit = handleEdit;
        this.handleSubirImagen = handleSubirImagen;
        this.handleQuitarImagen = handleQuitarImagen;
        this.handleEliminar = handleEliminar;

        this.save = this.save.bind(this);
        this.subirImagen = this.subirImagen.bind(this);
        this.quitarImagen = this.quitarImagen.bind(this);
        this.eliminarArticulo = this.eliminarArticulo.bind(this);
    }

    componentDidMount() {
        window.$('#editArticuloModal').modal({ show: true });
    }

    showAlert(txt) {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderArticulosModalAlert'));
        ReactDOM.render(<FormAlert
            txt={txt}
        />, document.getElementById('renderArticulosModalAlert'));
    }

    seleccionarImagen() {
        document.getElementById("artImgFile").click();
    }

    subirImagen() {
        const files = document.getElementById("artImgFile").files;
        if (files.length == 0)
            return;
        const file = files[0];
        if (file.size == 0)
            return;
        if (file.size >= 32767) {
            alert("El tamaño máximo de los iconos es de 32Kb");
            return;
        }

        this.handleSubirImagen(this.id, file);
        this.refs.img.src = URL.createObjectURL(file);
    }

    save() {
        const articulo = {};
        articulo.id = this.id;
        articulo.name = this.refs.name.value;
        articulo.minecraftID = this.refs.minecraftID.value;
        articulo.descripcion = this.refs.descripcion.value;

        if (articulo.name == null || articulo.name.length == "") {
            this.showAlert("El nombre del articulo no puede estar vacío.");
            return;
        }
        if (articulo.minecraftID == null || articulo.minecraftID.length == "") {
            this.showAlert("El ID de Minecraft del articulo no puede estar vacío.");
            return;
        }

        this.handleEdit(articulo).then(() => {
            window.$('#editArticuloModal').modal('hide');
        }, () => {
            this.showAlert("No se ha podido guardar el articulo.");
        });
    }

    quitarImagen() {
        this.handleQuitarImagen(this.id);
        this.refs.img.src = photoICO;
    }

    eliminarArticulo() {
        this.handleEliminar(this.id);
        window.$('#editArticuloModal').modal('hide');
    }

    render() {
        return <div id="editArticuloModal" className="modal fade bd-example-modal-xl" tabIndex="-1" role="dialog" aria-labelledby="editArticuloModalLabel" aria-hidden="true">
            <div className="modal-dialog modal-xl" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="editArticuloModalLabel">Editar art&iacute;culo {this.id}</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <input type="file" id="artImgFile" ref="img" onChange={this.subirImagen} />
                        <div className="form-row" id="editArtMainForm">
                            <div className="col">
                                <img src={this.img != null ? URL.createObjectURL(this.img) : photoICO} />
                                <button type="button" className="btn btn-danger" onClick={this.quitarImagen}>Quitar</button>
                                <button type="button" className="btn btn-primary" onClick={this.seleccionarImagen}>Subir</button>
                            </div>
                            <div className="col" id="editArtDetails">
                                <form>
                                    <div className="form-row">
                                        <div className="col">
                                            <label>Nombre</label>
                                            <input type="text" className="form-control" placeholder="Nombre" ref="name" defaultValue={this.name} />
                                        </div>
                                        <div className="col">
                                            <label>ID de Minecraft</label>
                                            <input type="text" className="form-control" placeholder="ID de Minecraft" ref="minecraftID" defaultValue={this.minecraftID} />
                                        </div>
                                        <div className="col">
                                            <label>Cantidad</label>
                                            <input type="number" className="form-control" placeholder="Cantidad" readOnly defaultValue={this.cant} />
                                        </div>
                                    </div>
                                </form>
                                <label>Descripcion</label>
                                <textarea className="form-control" ref="descripcion" defaultValue={this.descripcion}></textarea>
                            </div>
                        </div>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-danger" onClick={this.eliminarArticulo}>Delete</button>
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                        <button type="button" className="btn btn-primary" onClick={this.save}>Guardar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

export default EditArticulo;
