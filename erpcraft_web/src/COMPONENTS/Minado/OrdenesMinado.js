import { Component } from "react";
import ReactDOM from 'react-dom';

import minandoIco from './../../IMG/robot_estado/minando.png';
import queueIco from './../../IMG/orden_minado_estado/queue.svg';
import readyIco from './../../IMG/orden_minado_estado/ready.svg';
import runningIco from './../../IMG/orden_minado_estado/running.svg';
import doneIco from './../../IMG/orden_minado_estado/done.svg';
import flashlightIco from './../../IMG/flashlight.svg';

import './../../CSS/OrdenesMinado.css';
import OrdenMinado from "./OrdenMinado";
import OrdenMinadoForm from "./OrdenMinadoForm";
import RobotLocalizador from "../Robots/RobotLocalizador";
import GestionPasillo from "./GestionPasillo";

class OrdenesMinado extends Component {
    constructor({ getOrdenes, localizarRobots, getRobotName, handleAdd, handleEdit, handleDelete, handleInventario, ordenMinadoInventarioPush, getOrdenMinadoInventarioArticuloImg, tabOrdenesMinadoPush, addOrdenesMinadoArray }) {
        super();

        this.getOrdenes = getOrdenes;
        this.localizarRobots = localizarRobots;
        this.getRobotName = getRobotName;
        this.handleAdd = handleAdd;
        this.handleEdit = handleEdit;
        this.handleDelete = handleDelete;
        this.handleInventario = handleInventario;
        this.ordenMinadoInventarioPush = ordenMinadoInventarioPush;
        this.getOrdenMinadoInventarioArticuloImg = getOrdenMinadoInventarioArticuloImg;
        this.tabOrdenesMinadoPush = tabOrdenesMinadoPush;
        this.addOrdenesMinadoArray = addOrdenesMinadoArray;

        this.localizarRobot = this.localizarRobot.bind(this);
        this.busquedaRobot = this.busquedaRobot.bind(this);
        this.renderOrdenes = this.renderOrdenes.bind(this);
        this.editar = this.editar.bind(this);
        this.nuevaOrden = this.nuevaOrden.bind(this);
        this.gestionPasillo = this.gestionPasillo.bind(this);
    }

    componentDidMount() {
        this.renderOrdenes();
    }

    async renderOrdenes() {
        const ordenes = await this.getOrdenes(this.busqueda());

        await ReactDOM.unmountComponentAtNode(document.getElementById("renderOrdenesMinado"));
        await ReactDOM.render(ordenes.map((element, i) => {
            return <OrdenMinado
                key={i}

                ordenMinado={element}
                handleEdit={() => {
                    this.editar(element);
                }}
            />
        }), document.getElementById("renderOrdenesMinado"));

        for (var i = 0; i < ordenes.length; i++) {
            if (ordenes[i].robot == null || ordenes[i].robot == 0) {
                continue;
            }

            const name = await this.getRobotName(ordenes[i].robot);
            ordenes[i].robotName = name;
            ReactDOM.render(ordenes.map((element, i) => {
                return <OrdenMinado
                    key={i}

                    ordenMinado={element}
                    handleEdit={() => {
                        this.editar(element);
                    }}
                />
            }), document.getElementById("renderOrdenesMinado"));
        }

        this.tabOrdenesMinadoPush(async (changeType, pos, newOrdenMinado) => {

            switch (changeType) {
                case 0: { // add
                    // / pedir el nombre del nuevo robot
                    const name = await this.getRobotName(newOrdenMinado.robot);
                    newOrdenMinado.robotName = name;
                    // guardar y renderizar
                    ordenes.push(newOrdenMinado);
                    await ReactDOM.unmountComponentAtNode(document.getElementById("renderOrdenesMinado"));
                    ReactDOM.render(ordenes.map((element, i) => {
                        return <OrdenMinado
                            key={i}

                            ordenMinado={element}
                            handleEdit={() => {
                                this.editar(element);
                            }}
                        />
                    }), document.getElementById("renderOrdenesMinado"));

                    break;
                }
                case 1: { // update
                    // ¿el nombre del robot se ha de vovler a cargar?
                    var robotChanged = 0;
                    // buscar la orden para substituirla
                    for (var i = 0; i < ordenes.length; i++) {
                        if (ordenes[i].id === newOrdenMinado.id) {
                            if (ordenes[i].robot != newOrdenMinado.robot) {
                                robotChanged = i;
                            } else {
                                newOrdenMinado.robotName = ordenes[i].robotName;
                            }
                            ordenes[i] = newOrdenMinado;
                            break;
                        }
                    }
                    // pedir el nombre del nuevo robot si se ha cambiado (guardar en posición encontrada)
                    if (robotChanged > 0) {
                        const name = await this.getRobotName(newOrdenMinado.robot);
                        ordenes[robotChanged].robotName = name;
                    }
                    // refrescar
                    await ReactDOM.unmountComponentAtNode(document.getElementById("renderOrdenesMinado"));
                    ReactDOM.render(ordenes.map((element, i) => {
                        return <OrdenMinado
                            key={i}

                            ordenMinado={element}
                            handleEdit={() => {
                                this.editar(element);
                            }}
                        />
                    }), document.getElementById("renderOrdenesMinado"));

                    break;
                }
                case 2: { // delete
                    // buscar la orden para borrarla
                    for (var i = 0; i < ordenes.length; i++) {
                        if (ordenes[i].id === pos) {
                            ordenes.splice(i, 1);
                            break;
                        }
                    }
                    // refrescar
                    await ReactDOM.unmountComponentAtNode(document.getElementById("renderOrdenesMinado"));
                    ReactDOM.render(ordenes.map((element, i) => {
                        return <OrdenMinado
                            key={i}

                            ordenMinado={element}
                            handleEdit={() => {
                                this.editar(element);
                            }}
                        />
                    }), document.getElementById("renderOrdenesMinado"));

                    break;
                }
            }
        });

    }

    async localizarRobot() {
        await ReactDOM.unmountComponentAtNode(document.getElementById("renderOrdenesMinadoLocalizarRobot"));
        ReactDOM.render(<RobotLocalizador
            getRobots={this.localizarRobots}
            handleSelect={this.busquedaRobot}
        />, document.getElementById("renderOrdenesMinadoLocalizarRobot"));
    }

    busquedaRobot(id, name) {
        this.refs.robId.value = id;
        this.refs.robName.value = name;
    }

    async editar(orden) {
        await ReactDOM.unmountComponentAtNode(document.getElementById("renderOrdenesMinadoModal"));
        ReactDOM.render(<OrdenMinadoForm
            orden={orden}

            localizarRobots={this.localizarRobots}
            getRobotName={this.getRobotName}
            handleEdit={this.handleEdit}
            handleDelete={this.handleDelete}
            handleInventario={this.handleInventario}
            ordenMinadoInventarioPush={this.ordenMinadoInventarioPush}
            getOrdenMinadoInventarioArticuloImg={this.getOrdenMinadoInventarioArticuloImg}

        />, document.getElementById("renderOrdenesMinadoModal"));
    }

    busqueda() {
        const bus = {
            estado: [],
            robot: 0
        }
        if (this.refs.q.checked) {
            bus.estado.push("Q");
        }
        if (this.refs.r.checked) {
            bus.estado.push("R");
        }
        if (this.refs.e.checked) {
            bus.estado.push("E");
        }
        if (this.refs.o.checked) {
            bus.estado.push("O");
        }
        bus.robot = parseInt(this.refs.robId.value);

        return bus;
    }

    nuevaOrden() {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderOrdenesMinadoModal"));
        ReactDOM.render(<OrdenMinadoForm

            localizarRobots={this.localizarRobots}
            handleAdd={this.handleAdd}

        />, document.getElementById("renderOrdenesMinadoModal"));
    }

    gestionPasillo() {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderOrdenesMinadoModal"));
        ReactDOM.render(<GestionPasillo

            localizarRobots={this.localizarRobots}
            getOrdenes={this.getOrdenes}
            addOrdenesMinadoArray={this.addOrdenesMinadoArray}

        />, document.getElementById("renderOrdenesMinadoModal"));
    }

    render() {
        return <div id="tabOrdenesMinado">
            <div id="renderOrdenesMinadoModal"></div>
            <div id="renderOrdenesMinadoLocalizarRobot"></div>
            <div id="renderOrdenesMinadoModalAlert"></div>
            <h3><img src={minandoIco} />&Oacute;rdenes de minado</h3>
            <div className="form-row" id="OrdenesMinadoBusqueda">
                <div className="col">
                    <input type="checkbox" ref="q" className="form-control" defaultChecked={true} />
                    <label><img src={queueIco} />En cola</label>
                </div>
                <div className="col">
                    <input type="checkbox" ref="r" className="form-control" defaultChecked={true} />
                    <label><img src={readyIco} />Preparado</label>
                </div>
                <div className="col">
                    <input type="checkbox" ref="e" className="form-control" defaultChecked={true} />
                    <label><img src={runningIco} />En curso</label>
                </div>
                <div className="col">
                    <input type="checkbox" ref="o" className="form-control" />
                    <label><img src={doneIco} />Realizado</label>
                </div>
                <div className="col">
                    <label>Robot</label>
                    <img src={flashlightIco} onClick={this.localizarRobot} />
                    <input type="number" ref="robId" className="form-control" readOnly={true} defaultValue={0} />
                    <input type="text" ref="robName" className="form-control" readOnly={true} />
                </div>
                <div className="col">
                    <button type="button" className="btn btn-outline-success" onClick={this.renderOrdenes}>Buscar</button>
                </div>
            </div>
            <div id="ordenesMinadoOpciones">
                <button type="button" className="btn btn-primary" onClick={this.nuevaOrden}>A&ntilde;adir orden de minado</button>
                <button type="button" className="btn btn-warning" onClick={this.gestionPasillo}>Gesti&oacute;n del pasillo</button>
            </div>
            <table className="table table-dark" id="tableTabOrdenesMinado">
                <thead>
                    <tr>
                        <th scope="col">#</th>
                        <th scope="col">Nombre</th>
                        <th scope="col">Tama&ntilde;o</th>
                        <th scope="col">Robot</th>
                        <th scope="col">Posici&oacute;n (X,Y,Z,F)</th>
                        <th scope="col">Estado</th>
                        <th scope="col">Fecha creaci&oacute;n</th>
                        <th scope="col">Items minados</th>
                    </tr>
                </thead>
                <tbody id="renderOrdenesMinado">
                </tbody>
            </table>
        </div>;
    }
};



export default OrdenesMinado;


