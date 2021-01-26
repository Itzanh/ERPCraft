import { Component } from "react";
import ReactDOM from 'react-dom';
import RobotForm from './RobotForm.js';
import RobotAutoregister from "./RobotAutoregister.js";

import robotIco from './../../IMG/robot.png';
import singularIco from './../../IMG/singular.svg';
import referenciaIco from './../../IMG/referencia.svg';
import otrasTablasIco from './../../IMG/otras_tablas.svg';

import inventarioIco from './../../IMG/inventario.png';
import mapaIco from './../../IMG/mapa.png';
import logIco from './../../IMG/log.svg';
import minandoIco from './../../IMG/robot_estado/minando.png';

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
        this.referencias = this.referencias.bind(this);
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

    referencias() {
        ReactDOM.unmountComponentAtNode(this.refs.renderRobotModal);
        ReactDOM.render(<RobotsReferenciasSelectModal

        />, this.refs.renderRobotModal);
    }

    render() {
        return <div id="tabRobots">
            <div ref="renderRobotModal" id="renderRobotModal"></div>
            <h3><img src={robotIco} />Robots</h3>
            <div className="input-group busqueda">
                <input type="text" className="form-control" ref="name" onChange={this.buscar} />
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
                    <a className="dropdown-item" href="#" onClick={this.referencias}><img src={referenciaIco} />Referencias</a>
                </div>
            </div>
            <button type="button" className="btn btn-primary" onClick={this.nuevoRobot}>A&ntilde;adir robot</button>
            <button type="button" className="btn btn-primary" onClick={this.autoregistroRobot}>Registrar autom&aacute;ticamente</button>
            <div id="robotTabla"></div>
        </div>;
    }
};

class RobotsReferenciasSelectModal extends Component {
    constructor({ }) {
        super();

        this.logs = this.logs.bind(this);
    }

    componentDidMount() {
        window.$('#robotsReferencias').modal({ show: true });
    }

    logs() {
        window.$('#robotsReferencias').modal('hide');
        ReactDOM.unmountComponentAtNode(document.getElementById("renderRobotModal"));
        ReactDOM.render(<RobotReferenciasResultsModal
            referencia={2}
        />, document.getElementById("renderRobotModal"));
    }

    render() {
        return <div class="modal fade" id="robotsReferencias" tabindex="-1" role="dialog" aria-labelledby="robotsReferenciasLabel" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="robotsReferenciasLabel">Seleccionar tabla</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <table class="table table-dark">
                            <thead>
                                <tr>
                                    <th scope="col">#</th>
                                    <th scope="col">Nombre</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <th scope="row"><img src={inventarioIco} /></th>
                                    <td>Inventario</td>
                                </tr>
                                <tr>
                                    <th scope="row"><img src={mapaIco} /></th>
                                    <td>Historial GPS</td>
                                </tr>
                                <tr onClick={this.logs}>
                                    <th scope="row"><img src={logIco} /></th>
                                    <td>Logs</td>
                                </tr>
                                <tr>
                                    <th scope="row"><img src={minandoIco} /></th>
                                    <td>&Oacute;rdenes de minado</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class RobotReferenciasResultsModal extends Component {
    constructor({ referencia }) {
        super();

        this.referencia = referencia;
    }

    componentDidMount() {
        window.$('#robotsReferenciasModal').modal({ show: true });

        switch (this.referencia) {
            case 2: {
                ReactDOM.render(<table class="table table-dark">
                    <thead>
                        <tr>
                            <th scope="col">Robot</th>
                            <th scope="col">#</th>
                            <th scope="col">T&iacute;tulo</th>
                            <th scope="col">Mensaje</th>
                        </tr>
                    </thead>
                    <tbody>
                    </tbody>
                </table>, this.refs.body);
                break;
            }
        }
    }

    render() {
        return <div class="modal fade bd-example-modal-xl" tabindex="-1" role="dialog" id="robotsReferenciasModal" aria-labelledby="robotsReferenciasModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-xl" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="robotsReferenciasModalLabel">Registros</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body" ref="body">

                    </div>
                </div>
            </div>
        </div>
    }
}

export default Robots;


