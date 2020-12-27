import { Component } from "react";

class AddArticulo extends Component {
    constructor({ handleAddArticulo }) {
        super();

        this.handleAddArticulo = handleAddArticulo;

        this.addArticulo = this.addArticulo.bind(this);
    }

    componentDidMount() {
        window.$('#addArticuloModal').modal({ show: true });
    }

    addArticulo() {
        const articulo = {};
        articulo.name = this.refs.name.value;
        articulo.minecraftID = this.refs.mine_id.value;
        articulo.descripcion = this.refs.dsc.value;

        this.handleAddArticulo(articulo).then(() => {
            window.$('#addArticuloModal').modal('hide');
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
