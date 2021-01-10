import { Component } from "react";
import ReactDOM from 'react-dom';

import articulosIco from './../../IMG/articulos.png';

import './../../CSS/Articulos.css';
import AddArticulo from "./AddArticulo";

class Articulos extends Component {
    constructor({ handleAddArticulo, handleSearch }) {
        super();

        this.handleAddArticulo = handleAddArticulo;
        this.handleSearch = handleSearch;

        this.busTimer = null;

        this.addArticulo = this.addArticulo.bind(this);
        this.buscar = this.buscar.bind(this);
        this.buscarAuto = this.buscarAuto.bind(this);
    }

    addArticulo() {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderArticulosModal'));
        ReactDOM.render(<AddArticulo
            handleAddArticulo={this.handleAddArticulo}
        />, document.getElementById('renderArticulosModal'));
    }

    buscar() {
        if (this.busTimer != null) {
            clearTimeout(this.busTimer);
            this.busTimer = null;
        }

        this.handleSearch(this.refs.bus.value);
    }

    buscarAuto() {
        if (this.busTimer != null) {
            clearTimeout(this.busTimer);
            this.busTimer = null;
        }

        this.busTimer = setTimeout(() => {
            this.buscar();
        }, 400);
    }

    render() {
        return <div id="tabArticulos">
            <div id="renderArticulosModal"></div>
            <div id="renderArticulosModalAlert"></div>
            <h3><img src={articulosIco} />Articulos</h3>
            <div className="input-group busqueda">
                <input type="text" className="form-control" ref="bus" onChange={this.buscarAuto} />
                <div className="input-group-append">
                    <button type="button" className="btn btn-outline-success" onClick={this.buscar}>Buscar</button>
                </div>
            </div>
            <button type="button" className="btn btn-primary" onClick={this.addArticulo}>A&ntilde;adir art&iacute;culo</button>
            <table className="table table-dark" id="tableTabArticulos">
                <thead>
                    <tr>
                        <th scope="col">#</th>
                        <th scope="col"></th>
                        <th scope="col">Nombre</th>
                        <th scope="col">ID Minecraft</th>
                        <th scope="col">Cantidad</th>
                    </tr>
                </thead>
                <tbody id="renderArticulos">
                </tbody>
            </table>
        </div>
    }
};

export default Articulos;
