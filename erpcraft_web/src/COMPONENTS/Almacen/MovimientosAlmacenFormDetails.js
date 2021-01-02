import { Component } from "react";


class MovimientosAlmacenFormDetails extends Component {
    constructor({ movimiento, updateMovimientoAlmacen }) {
        super();

        this.movimiento = movimiento;
        this.updateMovimientoAlmacen = updateMovimientoAlmacen;

        this.aceptar = this.aceptar.bind(this);
    }

    componentDidMount() {
        window.$('#movimientoAlmacenForm').modal({ show: true });
    }

    aceptar() {
        const movimientoAlmacen = this.movimiento;
        console.log(this.movimiento);
        movimientoAlmacen.descripcion = this.refs.dsc.value;

        this.updateMovimientoAlmacen(movimientoAlmacen);
        window.$('#movimientoAlmacenForm').modal('hide');
    }

    render() {
        return <div class="modal fade" id="movimientoAlmacenForm" tabindex="-1" role="dialog" aria-labelledby="movimientoAlmacenFormLabel" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="movimientoAlmacenFormLabel">Movimiento de almac&eacute;n</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <div className="localizador">
                            <label>Almac&eacute;n</label>
                            <input type="text" ref="almName" className="form-control" readOnly={true} defaultValue={this.movimiento.almacen} />
                        </div>
                        <div className="localizador">
                            <label>Art&iacute;culo</label>
                            <input type="text" ref="artName" className="form-control" readOnly={true} defaultValue={this.movimiento.articulo} />
                        </div>
                        <label>Cantidad</label>
                        <input type="number" ref="cant" className="form-control" defaultValue={this.movimiento.cantidad} readOnly={true} />
                        <label>Descripci&oacute;n</label>
                        <textarea rows="5" ref="dsc" className="form-control" defaultValue={this.movimiento.descripcion}></textarea>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" onClick={this.aceptar}>Aceptar</button>
                        <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

export default MovimientosAlmacenFormDetails;


