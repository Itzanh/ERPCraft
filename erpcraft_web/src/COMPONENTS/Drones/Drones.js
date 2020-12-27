import { Component } from "react";

import droneIco from './../../IMG/drone.png';

import './../../CSS/Drones.css';

class Drones extends Component {
    constructor({ handleAddDrone, handleSearch }) {
        super();

        this.handleAddDrone = handleAddDrone;
        this.handleSearch = handleSearch;

        this.buscar = this.buscar.bind(this);
    }

    buscar() {
        this.handleSearch(this.refs.bus.value);
    }

    render() {
        return <div id="tabDrones">
            <h3><img src={droneIco} />Drones</h3>
            <div className="input-group busqueda">
                <input type="text" className="form-control" ref="bus" />
                <div className="input-group-append">
                    <button type="button" className="btn btn-outline-success" onClick={this.buscar}>Buscar</button>
                </div>
            </div>
            <button type="button" className="btn btn-primary" onClick={this.handleAddDrone}>A&ntilde;adir drone</button>
            <table className="table table-dark" id="tableTabDrones">
                <thead>
                    <tr>
                        <th scope="col">#</th>
                        <th scope="col">Nombre</th>
                        <th scope="col">UUID</th>
                        <th scope="col">Inventario</th>
                        <th scope="col">Estado</th>
                        <th scope="col">Energia</th>
                        <th scope="col">Bater&iacute;a</th>
                    </tr>
                </thead>
                <tbody id="renderDrones">
                </tbody>
            </table>
        </div>
    }
};

export default Drones;
