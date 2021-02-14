import { Component } from "react";
import ReactDOM from 'react-dom';

import FormAlert from "../FormAlert";

import robotIco from './../../IMG/robot.png';

import queueIco from './../../IMG/orden_minado_estado/queue.svg';
import readyIco from './../../IMG/orden_minado_estado/ready.svg';
import runningIco from './../../IMG/orden_minado_estado/running.svg';
import doneIco from './../../IMG/orden_minado_estado/done.svg';

import stonePickaxeIco from './../../IMG/stone_pickaxe.png';
import ironPickaxeIco from './../../IMG/iron_pickaxe.png';

import ecoIco from './../../IMG/orden_minado/eco.png';
import optimoIco from './../../IMG/orden_minado/optimo.png';


const ordenesMinadoEstadoImg = {
    "Q": queueIco,
    "R": readyIco,
    "E": runningIco,
    "O": doneIco
};

const ordenesMinadoModoImg = {
    "E": stonePickaxeIco,
    "O": ironPickaxeIco
};

class GestionPasillo extends Component {
    constructor({ localizarRobots, getOrdenes, addOrdenesMinadoArray, robotHasInventoryController, robotHasGeolyzer }) {
        super();

        this.localizarRobots = localizarRobots;
        this.getOrdenes = getOrdenes;
        this.addOrdenesMinadoArray = addOrdenesMinadoArray;
        this.robotHasInventoryController = robotHasInventoryController;
        this.robotHasGeolyzer = robotHasGeolyzer;

        this.robotSelId = 0;
        this.robots = [];
        this.ordenes = [];
        this.ordenesActuales = [];

        this.agregar = this.agregar.bind(this);
        this.generar = this.generar.bind(this);
        this.renombrarOrden = this.renombrarOrden.bind(this);
        this.onRobotSelected = this.onRobotSelected.bind(this);
        this.checkGeolyzerWarning = this.checkGeolyzerWarning.bind(this);
    }

    componentDidMount() {
        window.$('#gestionPasillo').modal({ show: true });
        window.$(function () {
            window.$('[data-toggle="tooltip"]').tooltip()
        });
        this.printRobots();
    }

    showAlert(txt) {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderOrdenesMinadoModalAlert'));
        ReactDOM.render(<FormAlert
            txt={txt}
        />, document.getElementById('renderOrdenesMinadoModalAlert'));
    }

    async printRobots() {
        this.robots = await this.localizarRobots();

        ReactDOM.render(this.robots.map((element, i) => {
            return <tr key={i} onClick={() => {
                this.onRobotSelected(element.id, element.name);
            }}>
                <th scope="row">{element.id}</th>
                <td>{element.name}</td>
            </tr>
        }), this.refs.renderRobots);
        this.printOrdenes();
    }

    onRobotSelected(id, name) {
        this.robotSelId = id;
        this.refs.robId.value = id;
        this.refs.robName.value = name;

        if (id != 0) {
            // inventory controller warning
            this.robotHasInventoryController(id).then((upgradeInentoryController) => {
                if (upgradeInentoryController) {
                    this.refs.robotInvConWarnings.style.visibility = 'hidden';
                    this.refs.robotInvConWarnings.style.height = '0';
                } else {
                    this.refs.robotInvConWarnings.style.visibility = 'visible';
                    this.refs.robotInvConWarnings.style.height = 'inherit';
                }
            });

            // modo economico warning
            if (this.refs.modoMinadoE.checked) {
                this.robotHasGeolyzer(id).then((upgradeGeolyzer) => {
                    if (upgradeGeolyzer) {
                        this.refs.robotEcoWarnings.style.visibility = 'hidden';
                        this.refs.robotEcoWarnings.style.height = '0';
                    } else {
                        this.refs.robotEcoWarnings.style.visibility = 'visible';
                        this.refs.robotEcoWarnings.style.height = 'inherit';
                    }
                });
            } else {
                this.refs.robotEcoWarnings.style.visibility = 'hidden';
                this.refs.robotEcoWarnings.style.height = '0';
            }
        }
    }

    checkGeolyzerWarning() {
        if (this.refs.robId.value == '0')
            return;

        if (this.refs.modoMinadoE.checked) {
            this.robotHasGeolyzer(parseInt(this.refs.robId.value)).then((upgradeGeolyzer) => {
                if (upgradeGeolyzer) {
                    this.refs.robotEcoWarnings.style.visibility = 'hidden';
                    this.refs.robotEcoWarnings.style.height = '0';
                } else {
                    this.refs.robotEcoWarnings.style.visibility = 'visible';
                    this.refs.robotEcoWarnings.style.height = 'inherit';
                }
            });
        } else {
            this.refs.robotEcoWarnings.style.visibility = 'hidden';
            this.refs.robotEcoWarnings.style.height = '0';
        }
    }

    async printOrdenes() {
        this.ordenesActuales = await this.getOrdenes({ estado: ["Q", "R", "E"], robot: 0 });
        this.ordenesActuales.forEach((element) => {
            for (let i = 0; i < this.robots.length; i++) {
                if (this.robots[i].id === element.robot) {
                    element.robotName = this.robots[i].name;
                    break;
                }
            }
            return element;
        });

        ReactDOM.render(this.ordenesActuales.map((element, i) => {
            return <tr key={i}>
                <th scope="row">{element.id}</th>
                <td>{element.robotName}</td>
                <td>{element.size}</td>
                <td>{element.posX}x{element.posY}x{element.posZ}:{element.facing}</td>
                <td>{element.gpsX}</td>
                <td>{element.gpsY}</td>
                <td>{element.gpsZ}</td>
                <td><img src={ordenesMinadoModoImg[element.modoMinado]} /></td>
                <td><img src={ordenesMinadoEstadoImg[element.estado]} /></td>
            </tr>
        }), this.refs.renderOrdenes);
    }

    /***
     * función que devuelve el callback de ordenación para buscar las ordenes de minado anteriores de las listas de ordenes
     * buscar por mayor/menor en función de la dirección y distancia +/-
     * */
    getFuncionBusqueda(distancia, direccion) {
        if (distancia > 0) {
            if (direccion == "X") {
                return ((a, b) => {
                    if (a.gpsX > b.gpsX) {
                        return -1
                    } else if (a.gpsX < b.gpsX) {
                        return 1;
                    }
                    return 0;
                });
            } else {
                return ((a, b) => {
                    if (a.gpsZ > b.gpsZ) {
                        return -1
                    } else if (a.gpsZ < b.gpsZ) {
                        return 1;
                    }
                    return 0;
                });
            }
        } else if (distancia < 0) {
            if (direccion == "X") {
                return ((a, b) => {
                    if (a.gpsX < b.gpsX) {
                        return -1
                    } else if (a.gpsX > b.gpsX) {
                        return 1;
                    }
                    return 0;
                });
            } else {
                return ((a, b) => {
                    if (a.gpsZ < b.gpsZ) {
                        return -1
                    } else if (a.gpsZ > b.gpsZ) {
                        return 1;
                    }
                    return 0;
                });
            }
        }
    }

    agregar() {
        // verificar el contenido del formulario
        if (this.robotSelId <= 0) {
            this.showAlert("Debes seleccionar un robot.");
            return;
        }
        if (this.refs.size.value == "") {
            this.showAlert("Introduce un tamaño del area a minar.");
            return;
        }
        const size = parseInt(this.refs.size.value);
        if (size <= 0) {
            this.showAlert("El tamaño debe de ser un número mayor que 0.");
            return;
        }
        if (this.refs.dist.value == "") {
            this.showAlert("Introduce la distancia que debe viajar el robot entre órdenes de minado.");
            return;
        }
        const distancia = parseInt(this.refs.dist.value);
        if (Math.abs(distancia) < size) {
            this.showAlert("Los bloques de distancia que debe viajar el robot entre órdenes de minado debe de ser mayor o igual al tamaño de la minería.");
            return;
        }
        if (this.refs.num.value == "") {
            this.showAlert("Introduce un numero de órdenes de minado a generar consecutivamente.");
            return;
        }
        const numero = parseInt(this.refs.num.value);
        if (numero <= 0) {
            this.showAlert("Introduce un numero de órdenes de minado a generar mayor a 0.");
            return;
        }
        const direccion = this.refs.dirX.checked ? "X" : "Z";

        // buscar la posición GPS anterior, si se encuentra una orden para ese robot
        var posX = 0;
        var posZ = 0;
        var ordenesRobot = this.ordenesActuales.filter((element) => {
            return (element.robot === this.robotSelId);
        }).sort(this.getFuncionBusqueda(distancia, direccion));
        if (ordenesRobot.length > 0) {
            if (direccion == "X") {
                posX = ordenesRobot[0].gpsX + distancia;
            } else {
                posZ = ordenesRobot[0].gpsZ + distancia;
            }
        }

        // buscar ordenes para este robot en el array de ordenes pendientes de generar
        ordenesRobot = this.ordenes.filter((element) => {
            return (element.robot === this.robotSelId);
        }).sort(this.getFuncionBusqueda(distancia, direccion));
        if (ordenesRobot.length > 0) {
            if (direccion == "X") {
                if (distancia > 0) {
                    if (ordenesRobot[0].gpsX + distancia > posX) {
                        posX = ordenesRobot[0].gpsX + distancia;
                    }
                } else if (distancia < 0) {
                    if (ordenesRobot[0].gpsX + distancia < posX) {
                        posX = ordenesRobot[0].gpsX + distancia;
                    }
                }
            } else {
                if (distancia > 0) {
                    if (ordenesRobot[0].gpsZ + distancia > posZ) {
                        posZ = ordenesRobot[0].gpsZ + distancia;
                    }
                } else if (distancia < 0) {
                    if (ordenesRobot[0].gpsZ + distancia < posZ) {
                        posZ = ordenesRobot[0].gpsZ + distancia;
                    }
                }
            }
        }

        // generar las ordenes
        for (let i = 0; i < numero; i++) {
            this.ordenes.push({
                id: this.ordenes.length + 1,
                name: "Orden de minado #" + (this.ordenes.length + 1) + " de " + this.refs.robName.value,
                robot: this.robotSelId,
                robotName: this.refs.robName.value,
                size: size,
                gpsX: posX,
                gpsY: 0,
                gpsZ: posZ,
                modoMinado: this.refs.modoMinadoO.checked ? "O" : "E"
            });

            if (direccion == "X") {
                posX = posX + distancia;
            } else {
                posZ = posZ + distancia;
            }
        }

        // imprimir las ordenes generadas
        this.renderOrdenes();
    }

    renderOrdenes() {
        ReactDOM.render(this.ordenes.map((element, i) => {
            return <tr key={i} onClick={() => {
                this.renombrarOrden(element);
            }}>
                <th scope="row">{element.id}</th>
                <td>{element.robotName}</td>
                <td>{element.name}</td>
                <td>{element.size}</td>
                <td>{element.gpsX}</td>
                <td>{element.gpsY}</td>
                <td>{element.gpsZ}</td>
                <td><img src={ordenesMinadoModoImg[element.modoMinado]} /></td>
            </tr>
        }), this.refs.nuevasOrdenes);
    }

    renombrarOrden(orden) {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderOrdenesMinadoModalAlert"));
        ReactDOM.render(<GestionPasilloPromptName
            name={orden.name}
            handleAceptar={(name) => {
                orden.name = name;
                this.renderOrdenes();
            }}
        />, document.getElementById("renderOrdenesMinadoModalAlert"));
    }

    generar() {
        this.ordenes.forEach((element) => {
            delete element.id;
            delete element.robotName;
            return element;
        });
        this.addOrdenesMinadoArray(this.ordenes).then(() => {
            window.$('#gestionPasillo').modal('hide');
        }, () => {
            this.showAlert("El servidor ha devuelto un error al guardar los datos");
        });
    }

    render() {
        return <div class="modal fade bd-example-modal-xl" tabindex="-1" role="dialog" id="gestionPasillo" aria-labelledby="gestionPasilloLabel" aria-hidden="true">
            <div class="modal-dialog modal-xl" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="gestionPasilloLabel">Gesti&oacute;n del pasillo</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <div className="form-row" id="gesPasilloMainForm">
                            <div className="col">
                                <div className="form-row" id="ordenesNuevasMenu">
                                    <div className="col">
                                        <label>Robots</label>
                                        <input type="number" ref="robId" className="form-control" readOnly={true} defaultValue={0} />
                                        <input type="text" ref="robName" className="form-control" readOnly={true} />
                                    </div>
                                    <div className="col">
                                        <input type="number" className="form-control" placeholder="Tama&ntilde;o" ref="size" />
                                    </div>
                                    <div className="col">
                                        <div className="btn-group btn-group-toggle" data-toggle="buttons">
                                            <label className="btn btn-primary" onClick={this.checkGeolyzerWarning}>
                                                <input type="radio" name="modoMinado" id="opti" data-toggle="tooltip" data-placement="top" data-html="true" title={"<div className='tooltipOrdenMinado'><img src=" + optimoIco + " />Estrategia de miner&iacute;a m&aacute;s r&aacute;pida aunque menos eficiente.<ul><li>Todos los picos del robot deben de ser de hierro o superior, para poder romper todos los elementos que se esperen minar sin considerar nada m&aacute;s. El robot volver&aacute; al origen y conseguir&aacute; un nuevo pico si se le termina.</li><li>El robot no recargar&aacute; tanto el pico.</li><li>Dependiendo de los materiales que se encuentren, puede ser contraproducente.</li></ul></div>"}
                                                    ref="modoMinadoO" />
                                                <img src={ironPickaxeIco} />Modo optimo
                                            </label>
                                            <label className="btn btn-primary active" onClick={this.checkGeolyzerWarning}>
                                                <input type="radio" name="modoMinado" id="eco"
                                                    data-toggle="tooltip" data-placement="top" data-html="true" title={"<div className='tooltipOrdenMinado'><img src=" + ecoIco + " />Estategia de miner&iacute;a m&aacute;s lenta aunque m&aacute;s barata.<ul><li>El robot ocupar&aacute; dos posiciones en el inventario para picos, uno ser&iacute;a de piedra (m&aacute;s econ&oacute;mico), y otro de un material m&aacute;s &oacute;ptimo, como el herro. El robot elegir&aacute; el tipo de pico adecuado cada vez, usando solo el &oacute;ptimo cuando sea necesario.</li><li>La miner&iacute;a ser&aacute; mucho m&aacute;s lenta y el robot perder&aacute; mucho m&aacute;s tiempo recargando.</li><li>Minado mucho m&aacute;s f&aacute;cil de rentabilizar.</li></ul></div>"}
                                                    ref="modoMinadoE" defaultChecked={true} />
                                                <img src={stonePickaxeIco} />Modo econ&oacute;mico
                                            </label>
                                        </div>
                                    </div>
                                    <div className="col">
                                        <input type="number" className="form-control" placeholder="Distancia" ref="dist" />
                                    </div>
                                    <div className="col">
                                        <div className="btn-group btn-group-toggle" data-toggle="buttons">
                                            <label className="btn btn-secondary active">
                                                <input type="radio" name="dir" id="dirX" defaultChecked={true} ref="dirX" />
                                                X
                                            </label>
                                            <label className="btn btn-secondary">
                                                <input type="radio" name="dir" id="dirZ" />
                                                Z
                                            </label>
                                        </div>
                                    </div>
                                    <div className="col">
                                        <input type="number" className="form-control" placeholder="Num. de &oacute;rdenes" ref="num" />
                                    </div>
                                    <div className="col">
                                        <button type="button" class="btn btn-warning" onClick={this.agregar}>A&ntilde;adir</button>
                                    </div>
                                </div>

                                <div className="form-row robotWarning" ref="robotInvConWarnings" style={{ 'visibility': 'hidden', 'height': '0' }}>
                                    <div className="col">
                                        <img src={robotIco} />
                                    </div>
                                    <div className="col">
                                        <h5>Robot incompatible</h5>
                                        <p>Este robot no es compatible con el minado porque en al ficha de este robot aparece que no dispone de un controlador de inventario como mejora. Si dispone de el en el juego, activa ese check, y si no, desensambla el robot e introduce la mejora.</p>
                                    </div>
                                </div>
                                <div className="form-row robotWarning" ref="robotEcoWarnings" style={{ 'visibility': 'hidden', 'height': '0' }}>
                                    <div className="col">
                                        <img src={stonePickaxeIco} />
                                    </div>
                                    <div className="col">
                                        <h5>Robot incompatible</h5>
                                        <p>Este robot no es compatible con el minado econ&oacute;mico porque en al ficha de este robot aparece que no dispone de un Geolyzer como mejora. Es necesario el Geolyzer para poder detectar los &iacute;tems y utilizar la herramienta de manera econ&oacute;mica.</p>
                                    </div>
                                </div>

                                <h6>Nuevas &oacute;rdenes</h6>
                                <table className="table table-dark" id="ordenesMinadoGenerador">
                                    <thead>
                                        <tr>
                                            <th scope="col">#</th>
                                            <th scope="col">Robot</th>
                                            <th scope="col">Nombre</th>
                                            <th scope="col">Tama&ntilde;o</th>
                                            <th scope="col">GPS X</th>
                                            <th scope="col">GPS Y</th>
                                            <th scope="col">GPS Z</th>
                                            <th scope="col">Modo</th>
                                        </tr>
                                    </thead>
                                    <tbody ref="nuevasOrdenes">
                                    </tbody>
                                </table>
                                <h6>&Oacute;rdenes registradas</h6>
                                <table className="table table-dark" id="ordenesMinadoActuales">
                                    <thead>
                                        <tr>
                                            <th scope="col">#</th>
                                            <th scope="col">Robot</th>
                                            <th scope="col">Tama&ntilde;o</th>
                                            <th scope="col">Posici&oacute;n</th>
                                            <th scope="col">GPS X</th>
                                            <th scope="col">GPS Y</th>
                                            <th scope="col">GPS Z</th>
                                            <th scope="col">Modo</th>
                                            <th scope="col">Estado</th>
                                        </tr>
                                    </thead>
                                    <tbody ref="renderOrdenes">
                                    </tbody>
                                </table>
                            </div>
                            <div className="col">
                                <table className="table table-dark">
                                    <thead>
                                        <tr>
                                            <th scope="col">#</th>
                                            <th scope="col">Nombre</th>
                                        </tr>
                                    </thead>
                                    <tbody ref="renderRobots">
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                        <button type="button" class="btn btn-primary" onClick={this.generar}>Generar &oacute;rdenes</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class GestionPasilloPromptName extends Component {
    constructor({ name, handleAceptar }) {
        super();

        this.name = name;
        this.handleAceptar = handleAceptar;

        this.aceptar = this.aceptar.bind(this);
    }

    componentDidMount() {
        window.$('#promptNameModal').modal({ show: true });
    }

    aceptar() {
        if (this.refs.name.value.length === 0) {
            return;
        }
        this.handleAceptar(this.refs.name.value);
        window.$('#promptNameModal').modal('hide');
    }

    render() {
        return <div class="modal fade" tabindex="-1" role="dialog" aria-labelledby="promptNameModalLabel" id="promptNameModal" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="promptNameModalLabel">Renombrar</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <label>Nombre</label>
                        <input type="text" className="form-control" placeholder="Nombre" ref="name" defaultValue={this.name} />
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                        <button type="button" class="btn btn-primary" onClick={this.aceptar}>Aceptar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

export default GestionPasillo;


