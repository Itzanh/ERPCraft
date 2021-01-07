import { Component } from "react";
import ReactDOM from 'react-dom';
import RobotForm from './RobotForm.js';
import RobotAutoregister from "./RobotAutoregister.js";

import robotIco from './../../IMG/robot.png';
import singularIco from './../../IMG/singular.svg';
import referenciaIco from './../../IMG/referencia.svg';
import otrasTablasIco from './../../IMG/otras_tablas.svg';

import './../../CSS/Robots.css';

class Robots extends Component {
    constructor({ handleRobots, handleBuscar, handleAddRobot, handleEditRobot, handleEliminar, handleLogs, handleRender, getServidores }) {
        super();

        this.handleRobots = handleRobots;
        this.handleBuscar = handleBuscar;
        this.handleAddRobot = handleAddRobot;
        this.handleEditRobot = handleEditRobot;
        this.handleEliminar = handleEliminar;
        this.handleLogs = handleLogs;
        this.handleRender = handleRender;
        this.getServidores = getServidores;

        this.nuevoRobot = this.nuevoRobot.bind(this);
        this.autoregistroRobot = this.autoregistroRobot.bind(this);
        this.buscar = this.buscar.bind(this);
    }

    nuevoRobot() {
        ReactDOM.render(<RobotForm
            handleCancelar={this.handleRobots}
            handleAddRobot={this.handleAddRobot}
            handleEditRobot={this.handleEditRobot}
            handleEliminar={this.handleEliminar}
            handleLogs={this.handleLogs}
        />, document.getElementById('renderTab'));
    }

    autoregistroRobot() {
        ReactDOM.unmountComponentAtNode(this.refs.renderRobotModal);
        ReactDOM.render(<RobotAutoregister
            getServidores={this.getServidores}
        />, this.refs.renderRobotModal);
    }

    buscar() {
        const query = {};
        query.text = this.refs.name.value;
        query.off = this.refs.off.checked;

        this.handleBuscar(query);
    }

    render() {
        return <div id="tabRobots">
            <div ref="renderRobotModal"></div>
            <h3><img src={robotIco} />Robots</h3>
            <div className="input-group busqueda">
                <input type="text" className="form-control" ref="name" />
                <div className="input-group-append">
                    <button type="button" className="btn btn-outline-success" onClick={this.buscar}>Buscar</button>
                </div>
                <input type="checkbox" className="form-control" ref="off" onChange={this.buscar} />
                <label>&iquest;Desactivado?</label>
            </div>
            <div className="dropdown">
                <button className="btn btn-success dropdown-toggle" type="button" id="dropdownMenuButton" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                    Vistas
                </button>
                <div className="dropdown-menu" aria-labelledby="dropdownMenuButton">
                    <a className="dropdown-item" href="#" onClick={() => { this.handleRender(1) }}>Lista</a>
                    <a className="dropdown-item" href="#" onClick={() => { this.handleRender(2) }}>Tabla (Todos los campos)</a>
                </div>
            </div>
            <div className="dropdown">
                <button className="btn btn-danger dropdown-toggle" type="button" id="dropdownMenuButton" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                    <img src={otrasTablasIco} />Otras tablas
                </button>
                <div className="dropdown-menu" aria-labelledby="dropdownMenuButton">
                    <a className="dropdown-item" href="#"><img src={singularIco} />Singulares</a>
                    <a className="dropdown-item" href="#"><img src={referenciaIco} />Referencias</a>
                </div>
            </div>
            <button type="button" className="btn btn-primary" onClick={this.nuevoRobot}>A&ntilde;adir robot</button>
            <button type="button" className="btn btn-primary" onClick={this.autoregistroRobot}>Registrar autom&aacute;ticamente</button>
            <div id="robotTabla"></div>
        </div>;
    }
};

export default Robots;


