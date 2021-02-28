import { Component } from "react";
import ReactDOM from 'react-dom';

import FormAlert from "../FormAlert";

class AddArticulo extends Component {
    constructor({ handleAddArticulo }) {
        super();

        this.handleAddArticulo = handleAddArticulo;

        this.addArticulo = this.addArticulo.bind(this);
    }

    componentDidMount() {
        window.$('#addArticuloModal').modal({ show: true });
    }

    showAlert(txt) {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderArticulosModalAlert'));
        ReactDOM.render(<FormAlert
            txt={txt}
        />, document.getElementById('renderArticulosModalAlert'));
    }

    addArticulo() {
        const articulo = {};
        articulo.name = this.refs.name.value;
        articulo.minecraftID = this.refs.mine_id.value;
        articulo.descripcion = this.refs.dsc.value;
        articulo.oreName = this.refs.oreName.value;

        if (articulo.name == null || articulo.name.length == "") {
            this.showAlert("El nombre del articulo no puede estar vacío.");
            return;
        }
        if (articulo.minecraftID == null || articulo.minecraftID.length == "") {
            this.showAlert("El ID de Minecraft del articulo no puede estar vacío.");
            return;
        }

        this.handleAddArticulo(articulo).then(() => {
            window.$('#addArticuloModal').modal('hide');
        }, () => {
            this.showAlert("No se ha podido guardar el articulo.");
        });
    }

    render() {
        return <div id="addArticuloModal" className="modal fade bd-example-modal-lg" tabIndex="-1" role="dialog" aria-labelledby="addArticuloModalLabel" aria-hidden="true">
            <div className="modal-dialog modal-lg" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="addArticuloModalLabel">A&ntilde;adir art&iacute;culo</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <form>
                            <div className="form-row">
                                <div className="col">
                                    <label>Nombre</label>
                                    <input type="text" className="form-control" placeholder="Nombre" ref="name" />
                                </div>
                                <div className="col">
                                    <label>ID de Minecraft</label>
                                    <input type="text" className="form-control" placeholder="ID de Minecraft" ref="mine_id" />
                                </div>
                                <div className="col">
                                    <label>Ore Name</label>
                                    <input type="text" className="form-control" placeholder="Ore Name" ref="oreName" />
                                </div>
                                <div className="col">
                                    <label>Cantidad</label>
                                    <input type="number" className="form-control" placeholder="Cantidad" readOnly defaultValue="0" />
                                </div>
                            </div>
                        </form>
                        <label>Descripcion</label>
                        <textarea className="form-control" ref="dsc"></textarea>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                        <button type="button" className="btn btn-primary" onClick={this.addArticulo}>Guardar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

export default AddArticulo;
