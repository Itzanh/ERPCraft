import { Component } from "react";


class RobotFormSlotInventario extends Component {
    constructor({ numeroSlot, cant, articulo }) {
        super();

        this.numeroSlot = numeroSlot;
        this.cant = cant;
        this.articulo = articulo;
    }

    render() {
        return <div className="card">
            <div className="form-row">
                <div className="col">
                    {this.articulo != null ? <img className={"art_img_" + this.articulo.id} /> : <></>}
                </div>

                <div className="col">
                    <div className="card-body">
                        <h5 className="card-title">{this.articulo == null ? '' : this.articulo.name}</h5>
                        <h6 className="card-subtitle mb-2 text-muted">{this.articulo == null ? '' : this.cant}</h6>
                    </div>
                </div>
            </div>
        </div>;
    }
};

export default RobotFormSlotInventario;
