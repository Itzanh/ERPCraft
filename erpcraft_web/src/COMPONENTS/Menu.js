import { Component } from "react";

import robotIco from './../IMG/robot.png';
import articulosIco from './../IMG/articulos.png';
import inventarioIco from './../IMG/inventario.png';
import electricoIco from './../IMG/electrico.svg';
import minandoIco from './../IMG/robot_estado/minando.png';
import movimientosIco from './../IMG/movimientos.png';
import droneIco from './../IMG/drone.png';
import usersIco from './../IMG/users.svg';
import settingsIco from './../IMG/settings.svg';
import keyIco from './../IMG/key.svg';
import serverIco from './../IMG/oc_server.png';
import logo from './../logo.png';

import './../CSS/Menu.css';

class Menu extends Component {
    constructor({ handleInicio, handleRobots, handleArticulos, handleMovimientosAlmacen, handleUsuarios, handleRedesElectricas, handleOrdenesMinado, handleConfiguraciones, handleApiKey, handleServidores, handleAlmacen, handleDrones }) {
        super();

        this.handleInicio = handleInicio;
        this.handleRobots = handleRobots;
        this.handleArticulos = handleArticulos;
        this.handleMovimientosAlmacen = handleMovimientosAlmacen;
        this.handleUsuarios = handleUsuarios;
        this.handleRedesElectricas = handleRedesElectricas;
        this.handleOrdenesMinado = handleOrdenesMinado;
        this.handleConfiguraciones = handleConfiguraciones;
        this.handleApiKey = handleApiKey;
        this.handleServidores = handleServidores;
        this.handleAlmacen = handleAlmacen;
        this.handleDrones = handleDrones;
    }

    render() {
        return <div id="mainScreen">
            <nav className="navbar navbar-expand-lg navbar-dark bg-dark">
                <a className="navbar-brand" href="#" onClick={this.handleInicio}><img src={logo} className="d-inline-block align-top" alt="" />ERPCraft</a>
                <div className="collapse navbar-collapse" id="navbarNav">
                    <ul className="navbar-nav">
                        <li className="nav-item active">
                            <a className="nav-link" href="#" onClick={this.handleRobots}><img src={robotIco} />Robots <span className="sr-only">(current)</span></a>
                        </li>
                        <li className="nav-item">
                            <a className="nav-link" href="#" onClick={this.handleArticulos}><img src={articulosIco} />Articulos</a>
                        </li>
                        <li className="nav-item">
                            <a className="nav-link" href="#" onClick={this.handleMovimientosAlmacen}><img src={movimientosIco} />Movimientos de almac&eacute;n</a>
                        </li>
                        <li className="nav-item">
                            <a className="nav-link" href="#" onClick={this.handleRedesElectricas}><img src={electricoIco} />Redes el&eacute;ctricas</a>
                        </li>
                        <li className="nav-item">
                            <a className="nav-link" href="#" onClick={this.handleOrdenesMinado}><img src={minandoIco} />&Oacute;rdenes de minado</a>
                        </li>
                        <li className="nav-item">
                            <a className="nav-link" href="#" onClick={this.handleAlmacen}><img src={inventarioIco} />Almac&eacute;n</a>
                        </li>
                        <li className="nav-item">
                            <a className="nav-link" href="#" onClick={this.handleDrones}><img src={droneIco} />Drones</a>
                        </li>
                        <li className="nav-item">
                            <div className="dropdown">
                                <button className="btn btn-secondary dropdown-toggle" type="button" id="settingsDropdown" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                    Ajustes
                                </button>
                                <div className="dropdown-menu" aria-labelledby="dropdownMenuButton">
                                    <a className="dropdown-item" href="#" onClick={this.handleUsuarios}><img src={usersIco} />Usuarios</a>
                                    <a className="dropdown-item" href="#" onClick={this.handleConfiguraciones}><img src={settingsIco} />Configuraciones</a>
                                    <a className="dropdown-item" href="#" onClick={this.handleApiKey}><img src={keyIco} />Claves de API</a>
                                    <a className="dropdown-item" href="#" onClick={this.handleServidores}><img src={serverIco} />Servidores</a>
                                </div>
                            </div>
                        </li>
                    </ul>
                </div>
            </nav>

            <div id="renderTab"></div>

        </div>
    }
};

export default Menu;


