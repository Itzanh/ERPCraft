import { Component } from "react";
import ReactDOM from 'react-dom';

// IMG
import articulosIco from './../../IMG/articulos.png';

// CSS
import './../../CSS/Articulos.css';

// COMPONENTES
import AddArticulo from "./AddArticulo";
import Recetas from "./Recetas";



class Articulos extends Component {
    constructor({ handleAddArticulo, handleSearch, localizarArticulos, getArticuloName, getArticuloImg, getCrafting, addCrafting, updateCrafting, deleteCrafting,
        getSmelting, addSmelting, updateSmelting, deleteSmelting }) {
        super();

        this.handleAddArticulo = handleAddArticulo;
        this.handleSearch = handleSearch;
        this.localizarArticulos = localizarArticulos;
        this.getArticuloName = getArticuloName;
        this.getArticuloImg = getArticuloImg;

        this.getCrafting = getCrafting;
        this.addCrafting = addCrafting;
        this.updateCrafting = updateCrafting;
        this.deleteCrafting = deleteCrafting;

        this.getSmelting = getSmelting;
        this.addSmelting = addSmelting;
        this.updateSmelting = updateSmelting;
        this.deleteSmelting = deleteSmelting;

        this.busTimer = null;

        this.addArticulo = this.addArticulo.bind(this);
        this.buscar = this.buscar.bind(this);
        this.buscarAuto = this.buscarAuto.bind(this);
        this.recetas = this.recetas.bind(this);
    }

    addArticulo() {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderArticulosModal'));
        ReactDOM.render(<AddArticulo
            handleAddArticulo={this.handleAddArticulo}
        />, document.getElementById('renderArticulosModal'));
    }

    recetas() {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderArticulosModal'));
        ReactDOM.render(<Recetas
            getArticulos={this.localizarArticulos}
            getArticuloName={this.getArticuloName}
            getArticuloImg={this.getArticuloImg}

            // CRAFTING
            getCrafting={this.getCrafting}
            addCrafting={this.addCrafting}
            updateCrafting={this.updateCrafting}
            deleteCrafting={this.deleteCrafting}

            // SMELTING
            getSmelting={this.getSmelting}
            addSmelting={this.addSmelting}
            updateSmelting={this.updateSmelting}
            deleteSmelting={this.deleteSmelting}
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
            <button type="button" className="btn btn-success" onClick={this.recetas}>Recetas</button>
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
