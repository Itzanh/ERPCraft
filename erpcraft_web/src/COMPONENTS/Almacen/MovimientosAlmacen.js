import { Component } from "react";
import ReactDOM from 'react-dom';

import movimientosIco from './../../IMG/movimientos.png';
import flashlightIco from './../../IMG/flashlight.svg';

import './../../CSS/MovimientosAlmacen.css';
import ArticuloLocalizador from "../Articulos/ArticuloLocalizador";
import AlmacenLocalizador from "./AlmacenLocalizador";

class MovimientosAlmacen extends Component {
    constructor({ getAlmacenes, getArticulos, handleBuscar, addMovimientoAlmacen }) {
        super();

        this.getAlmacenes = getAlmacenes;
        this.getArticulos = getArticulos;
        this.handleBuscar = handleBuscar;
        this.addMovimientoAlmacen = addMovimientoAlmacen;

        this.query = {};

        this.nuevoMovimientoAlmacen = this.nuevoMovimientoAlmacen.bind(this);
        this.buscar = this.buscar.bind(this);
        this.localizarAlmacen = this.localizarAlmacen.bind(this);
        this.localizarArticulo = this.localizarArticulo.bind(this);
    }

    nuevoMovimientoAlmacen() {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderMovimientoModal"));
        ReactDOM.render(<MovimientosAlmacenForm
            getAlmacenes={this.getAlmacenes}
            getArticulos={this.getArticulos}
            addMovimientoAlmacen={this.addMovimientoAlmacen}
        />, document.getElementById("renderMovimientoModal"));
    }

    buscar() {
        var start;
        var end;
        if (this.refs.startdate.value != "") {
            start = new Date(this.refs.startdate.value + " " + this.refs.starttime.value);
            start.setHours(start.getHours() + 1);
        }
        if (this.refs.enddate.value != "") {
            end = new Date(this.refs.enddate.value + " " + this.refs.endtime.value);
            end.setHours(end.getHours() + 1);
            end.setSeconds(59);
            end.setMilliseconds(999);
        }

        this.query.dateInicio = start;
        this.query.dateFin = end;
        this.handleBuscar(this.query);
    }

    localizarAlmacen() {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderMovimientoLocalizador"));
        ReactDOM.render(<AlmacenLocalizador
            getAlmacenes={this.getAlmacenes}
            handleSelect={(id, name) => {
                this.query.almacen = id;
                this.refs.almId.value = id;
                this.refs.almName.value = name;
            }}
        />, document.getElementById("renderMovimientoLocalizador"));
    }

    localizarArticulo() {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderMovimientoLocalizador"));
        ReactDOM.render(<ArticuloLocalizador
            getArticulos={this.getArticulos}
            handleSelect={(id, name) => {
                this.query.articulo = id;
                this.refs.artId.value = id;
                this.refs.artName.value = name;
            }}
        />, document.getElementById("renderMovimientoLocalizador"));
    }

    render() {
        return <div id="tabMovimientoAlmacen">
            <div id="renderMovimientoModal"></div>
            <div id="renderMovimientoLocalizador"></div>
            <h3><img src={movimientosIco} />Movimientos de almac&eacute;n</h3>
            <div className="form-row" id="OrdenesMinadoBusqueda">
                <div className="col">
                    <div className="localizador">
                        <label>Almac&eacute;n</label>
                        <img src={flashlightIco} onClick={this.localizarAlmacen} />
                        <input type="number" ref="almId" className="form-control" readOnly={true} defaultValue={0} />
                        <input type="text" ref="almName" className="form-control" readOnly={true} />
                    </div>
                </div>
                <div className="col">
                    <div className="localizador">
                        <label>Art&iacute;culo</label>
                        <img src={flashlightIco} onClick={this.localizarArticulo} />
                        <input type="number" ref="artId" className="form-control" readOnly={true} defaultValue={0} />
                        <input type="text" ref="artName" className="form-control" readOnly={true} />
                    </div>
                </div>
                <div className="col">
                    <label>Fecha inicio</label>
                    <input type="date" id="startdate" name="startdate" ref="startdate" />
                    <input type="time" id="starttime" name="starttime" ref="starttime" />
                </div>
                <div className="col">
                    <label>Fecha fin</label>
                    <input type="date" id="enddate" name="enddate" ref="enddate" />
                    <input type="time" id="endtime" name="endtime" ref="endtime" />
                </div>
                <div className="col">
                    <button type="button" className="btn btn-outline-success" onClick={this.buscar}>Buscar</button>
                </div>
            </div>
            <button type="button" className="btn btn-primary" onClick={this.nuevoMovimientoAlmacen}>A&ntilde;adir movimiento de almac&eacute;n</button>
            <table class="table table-dark">
                <thead>
                    <tr>
                        <th scope="col">Almac&eacute;n</th>
                        <th scope="col">#</th>
                        <th scope="col">Art&iacute;culo</th>
                        <th scope="col">Cantidad</th>
                        <th scope="col">Origen</th>
                        <th scope="col">Tiempo</th>
                    </tr>
                </thead>
                <tbody id="renderMovimientos"></tbody>
            </table>
        </div>;
    }
};

class MovimientosAlmacenForm extends Component {
    constructor({ getAlmacenes, getArticulos, addMovimientoAlmacen }) {
        super();

        this.getAlmacenes = getAlmacenes;
        this.getArticulos = getArticulos;
        this.addMovimientoAlmacen = addMovimientoAlmacen;

        this.almacen = 0;
        this.articulo = 0;

        this.localizarAlmacen = this.localizarAlmacen.bind(this);
        this.localizarArticulo = this.localizarArticulo.bind(this);
        this.aceptar = this.aceptar.bind(this);
    }

    componentDidMount() {
        window.$('#movimientoAlmacenForm').modal({ show: true });
    }

    localizarAlmacen() {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderMovimientoLocalizador"));
        ReactDOM.render(<AlmacenLocalizador
            getAlmacenes={this.getAlmacenes}
            handleSelect={(id, name) => {
                this.almacen = id;
                this.refs.almId.value = id;
                this.refs.almName.value = name;
            }}
        />, document.getElementById("renderMovimientoLocalizador"));
    }

    localizarArticulo() {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderMovimientoLocalizador"));
        ReactDOM.render(<ArticuloLocalizador
            getArticulos={this.getArticulos}
            handleSelect={(id, name) => {
                this.articulo = id;
                this.refs.artId.value = id;
                this.refs.artName.value = name;
            }}
        />, document.getElementById("renderMovimientoLocalizador"));
    }

    aceptar() {
        const movimientoAlmacen = {};
        movimientoAlmacen.almacen = this.almacen;
        movimientoAlmacen.articulo = this.articulo;
        movimientoAlmacen.cantidad = parseInt(this.refs.cant.value);
        movimientoAlmacen.descripcion = this.refs.dsc.value;

        this.addMovimientoAlmacen(movimientoAlmacen);
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
                            <img src={flashlightIco} onClick={this.localizarAlmacen} />
                            <input type="number" ref="almId" className="form-control" readOnly={true} defaultValue={0} />
                            <input type="text" ref="almName" className="form-control" readOnly={true} />
                        </div>
                        <div className="localizador">
                            <label>Art&iacute;culo</label>
                            <img src={flashlightIco} onClick={this.localizarArticulo} />
                            <input type="number" ref="artId" className="form-control" readOnly={true} defaultValue={0} />
                            <input type="text" ref="artName" className="form-control" readOnly={true} />
                        </div>
                        <label>Cantidad</label>
                        <input type="number" ref="cant" className="form-control" />
                        <label>Descripci&oacute;n</label>
                        <textarea rows="5" ref="dsc" className="form-control"></textarea>
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

export default MovimientosAlmacen;


