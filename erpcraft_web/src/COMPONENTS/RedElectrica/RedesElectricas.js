import { Component } from "react";
import ReactDOM from 'react-dom';
import RedElectricaForm from "./RedElectricaForm";

import electricoIco from './../../IMG/electrico.svg';

import './../../CSS/RedElectrica.css';

class RedesElectricas extends Component {
    constructor({ handleRedesElectricas, handleAddRedElectrica, handleBuscar }) {
        super();

        this.handleRedesElectricas = handleRedesElectricas;
        this.handleAddRedElectrica = handleAddRedElectrica;
        this.handleBuscar = handleBuscar;

        this.addRedElectrica = this.addRedElectrica.bind(this);
        this.buscar = this.buscar.bind(this);
    }

    addRedElectrica() {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderTab'));
        ReactDOM.render(<RedElectricaForm
            handleRedesElectricas={this.handleRedesElectricas}
            handleAddRedElectrica={this.handleAddRedElectrica}
        />, document.getElementById('renderTab'));
    }

    buscar() {
        this.handleBuscar(this.refs.bus.value);
    }

    render() {
        return <div id="tabRedElectrica">
            <h3><img src={electricoIco} />Redes El&eacute;ctricas</h3>
            <div className="input-group busqueda">
                <input type="text" className="form-control" ref="bus" aria-label="" onChange={this.buscar} />
                <div className="input-group-append">
                    <button type="button" className="btn btn-outline-success" onClick={this.buscar}>Buscar</button>
                </div>
            </div>
            <button type="button" className="btn btn-primary" onClick={this.addRedElectrica}>A&ntilde;adir red el&eacute;ctrica</button>
            <table className="table table-dark" id="tableTabRedElectrica">
                <thead>
                    <tr>
                        <th scope="col">#</th>
                        <th scope="col">Nombre</th>
                        <th scope="col">Carga</th>
                        <th scope="col">Capacidad</th>
                        <th scope="col"></th>
                    </tr>
                </thead>
                <tbody id="renderRedElectrica">
                </tbody>
            </table>
        </div>
    }
};

export default RedesElectricas;
