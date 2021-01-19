import { Component } from "react";
import ReactDOM from 'react-dom';

import FormAlert from "../FormAlert";
import RobotLocalizador from "../Robots/RobotLocalizador";

// IMG

import queueIco from './../../IMG/orden_minado_estado/queue.svg';
import readyIco from './../../IMG/orden_minado_estado/ready.svg';
import runningIco from './../../IMG/orden_minado_estado/running.svg';
import doneIco from './../../IMG/orden_minado_estado/done.svg';
import flashlightIco from './../../IMG/flashlight.svg';

import stonePickaxeIco from './../../IMG/stone_pickaxe.png';
import ironPickaxeIco from './../../IMG/iron_pickaxe.png';

import ecoIco from './../../IMG/orden_minado/eco.png';
import optimoIco from './../../IMG/orden_minado/optimo.png';


const ordenesMinadoEstado = {
    "Q": "En cola",
    "R": "Preparado",
    "E": "En curso",
    "O": "Realizado"
};

const ordenesMinadoEstadoImg = {
    "Q": queueIco,
    "R": readyIco,
    "E": runningIco,
    "O": doneIco
};

class OrdenMinadoForm extends Component {
    constructor({ orden, localizarRobots, getRobotName, handleAdd, handleEdit, handleDelete, handleInventario, ordenMinadoInventarioPush, getOrdenMinadoInventarioArticuloImg }) {
        super();

        this.orden = orden;
        this.localizarRobots = localizarRobots;
        this.getRobotName = getRobotName;
        this.handleAdd = handleAdd;
        this.handleEdit = handleEdit;
        this.handleDelete = handleDelete;
        this.handleInventario = handleInventario;
        this.ordenMinadoInventarioPush = ordenMinadoInventarioPush;
        this.getOrdenMinadoInventarioArticuloImg = getOrdenMinadoInventarioArticuloImg;

        this.getInventario = this.getInventario.bind(this);
        this.localizarRobot = this.localizarRobot.bind(this);
        this.busquedaRobot = this.busquedaRobot.bind(this);
        this.robotName = this.robotName.bind(this);
        this.eliminar = this.eliminar.bind(this);
        this.aceptar = this.aceptar.bind(this);
    }

    componentDidMount() {
        window.$('#ordenMinadoModal').modal({ show: true });
        window.$(function () {
            window.$('[data-toggle="tooltip"]').tooltip()
        });
        this.robotName();
        this.getInventario();
    };

    showAlert(txt) {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderOrdenesMinadoModalAlert'));
        ReactDOM.render(<FormAlert
            txt={txt}
        />, document.getElementById('renderOrdenesMinadoModalAlert'));
    }

    formatearFechaTiempo(fechaTiempo) {
        const fecha = new Date(fechaTiempo);
        return fecha.getDate() + '/'
            + (fecha.getMonth() + 1) + '/'
            + fecha.getFullYear() + ' '
            + fecha.getHours() + ':'
            + fecha.getMinutes() + ':'
            + fecha.getSeconds();
    };

    async localizarRobot() {
        await ReactDOM.unmountComponentAtNode(document.getElementById("renderOrdenesMinadoLocalizarRobot"));
        ReactDOM.render(<RobotLocalizador
            getRobots={this.localizarRobots}
            handleSelect={this.busquedaRobot}
        />, document.getElementById("renderOrdenesMinadoLocalizarRobot"));
    };

    busquedaRobot(id, name) {
        this.refs.robId.value = id;
        this.refs.robName.value = name;
    };

    async robotName() {
        if (this.orden && this.orden.robot > 0) {
            const name = await this.getRobotName(this.orden.robot);
            if (name != null) {
                this.refs.robName.value = name;
            }
        }
    };

    async eliminar() {
        if (this.orden != null) {
            await ReactDOM.unmountComponentAtNode(document.getElementById("renderOrdenesMinadoLocalizarRobot"));
            ReactDOM.render(<OrdenMinadoFormDeleteConfirm
                ordenId={this.orden.id}
                ordenName={this.orden.name}
                handleEliminar={() => {
                    this.handleDelete(this.orden.id);
                    window.$('#ordenMinadoModal').modal('hide');
                }}
            />, document.getElementById("renderOrdenesMinadoLocalizarRobot"));
        }
    };

    aceptar() {
        if (this.orden == null) {
            this.add();
        } else {
            this.update();
        }
    };

    add() {
        const orden = {};
        orden.name = this.refs.name.value;
        orden.size = parseInt(this.refs.size.value);
        orden.robot = parseInt(this.refs.robId.value);
        if (orden.robot == 0) {
            orden.robot = null;
        }
        orden.gpsX = parseInt(this.refs.gpsX.value);
        orden.gpsY = parseInt(this.refs.gpsY.value);
        orden.gpsZ = parseInt(this.refs.gpsZ.value);
        orden.descripcion = this.refs.dsc.value;
        if (this.refs.unidadRecargaPor.checked) {
            orden.unidadRecarga = "%";
        } else if (this.refs.unidadRecargaIgu.checked) {
            orden.unidadRecarga = "=";
        } else {
            orden.unidadRecarga = "%";
        }
        orden.energiaRecarga = parseInt(this.refs.energiaRecarga.value);
        if (this.refs.modoMinadoO.checked) {
            orden.modoMinado = "O";
        } else if (this.refs.modoMinadoE.checked) {
            orden.modoMinado = "E";
        } else {
            orden.modoMinado = "O";
        }
        orden.shutdown = this.refs.shutdown.checked;

        if (orden.name == null || orden.name.length == 0) {
            this.showAlert("El nombre no puede estar vacío.");
            return;
        }
        if (orden.size <= 0) {
            this.showAlert("El tamaño de la minería debe de ser superior a 0.");
            return;
        }
        if (orden.energiaRecarga <= 0) {
            this.showAlert("La energía de recarga debe de ser mayor a 0.");
            return;
        }
        orden.notificacion = this.refs.notificacion.checked;

        this.handleAdd(orden).then(() => {
            window.$('#ordenMinadoModal').modal('hide');
        }, () => {
            this.showAlert("No se ha podido guardar la orden de minado.");
        });
    };

    update() {
        const orden = {};
        orden.id = this.orden.id;
        orden.name = this.refs.name.value;
        orden.size = parseInt(this.refs.size.value);
        orden.robot = parseInt(this.refs.robId.value);
        orden.gpsX = parseInt(this.refs.gpsX.value);
        orden.gpsY = parseInt(this.refs.gpsY.value);
        orden.gpsZ = parseInt(this.refs.gpsZ.value);
        orden.descripcion = this.refs.dsc.value;
        if (this.refs.unidadRecargaPor.checked) {
            orden.unidadRecarga = "%";
        } else if (this.refs.unidadRecargaIgu.checked) {
            orden.unidadRecarga = "=";
        } else {
            orden.unidadRecarga = this.orden.unidadRecarga;
        }
        orden.energiaRecarga = parseInt(this.refs.energiaRecarga.value);
        if (this.refs.modoMinadoO.checked) {
            orden.modoMinado = "O";
        } else if (this.refs.modoMinadoE.checked) {
            orden.modoMinado = "E";
        } else {
            orden.modoMinado = this.orden.modoMinado;
        }
        orden.shutdown = this.refs.shutdown.checked;

        if (orden.name == null || orden.name.length == 0) {
            this.showAlert("El nombre no puede estar vacío.");
            return;
        }
        if (orden.size <= 0) {
            this.showAlert("El tamaño de la minería debe de ser superior a 0.");
            return;
        }
        if (orden.energiaRecarga <= 0) {
            this.showAlert("La energía de recarga debe de ser mayor a 0.");
            return;
        }
        orden.notificacion = this.refs.notificacion.checked;

        this.handleEdit(orden).then(() => {
            window.$('#ordenMinadoModal').modal('hide');
        }, () => {
            this.showAlert("No se ha podido guardar la orden de minado.");
        });
    };

    getInventario() {
        if (this.orden == null) {
            return;
        }
        this.handleInventario(this.orden.id).then((inventario) => {
            this.renderInventario(inventario);
        });
        this.ordenMinadoInventarioPush(this.orden.id, (inventario) => {
            this.renderInventario(inventario);
        });
    };

    async renderInventario(inventario) {
        await ReactDOM.unmountComponentAtNode(document.getElementById("inventarioOrdenMinadoContenido"));
        await ReactDOM.render(inventario.map((element, i) => {
            return <OrdenMinadoFormInventarioSlot
                key={i}

                numeroSlot={element.numeroSlot}
                cant={element.cant}
                articulo={element.articulo}
                img={element.img}
            />
        }), document.getElementById("inventarioOrdenMinadoContenido"));

        for (let i = 0; i < inventario.length; i++) {
            const articulo = inventario[i].articulo;

            if (articulo != null && articulo.id != 0) {
                const img = await this.getOrdenMinadoInventarioArticuloImg(articulo.id);
                inventario[i].img = img;
            }

        }

        await ReactDOM.unmountComponentAtNode(document.getElementById("inventarioOrdenMinadoContenido"));
        await ReactDOM.render(inventario.map((element, i) => {
            return <OrdenMinadoFormInventarioSlot
                key={i}

                numeroSlot={element.numeroSlot}
                cant={element.cant}
                articulo={element.articulo}
                img={element.img}
            />
        }), document.getElementById("inventarioOrdenMinadoContenido"));
    };

    render() {
        return <div className="modal fade bd-example-modal-lg" tabIndex="-1" role="dialog" aria-labelledby="ordenMinadoModalLabel" id="ordenMinadoModal" aria-hidden="true">
            <div className="modal-dialog modal-lg" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="ordenMinadoModalLabel">Orden de minado</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <div className="form-row" id="detallesBasicos">
                            <div className="col">
                                <label>ID</label>
                                <input type="number" className="form-control" defaultValue={this.orden != null ? this.orden.id : 0} readOnly={true} />
                            </div>
                            <div className="col">
                                <label>Nombre</label>
                                <input type="text" className="form-control" placeholder="Nombre" ref="name" defaultValue={this.orden != null ? this.orden.name : ''} />
                            </div>
                            <div className="col">
                                <label>Tama&ntilde;o</label>
                                <input type="number" className="form-control" placeholder="Tama&ntilde;o" ref="size" defaultValue={this.orden != null ? this.orden.size : 0} />
                            </div>
                        </div>
                        <div className="form-row" id="detallesRobot">
                            <div className="col">
                                <label>Robot</label>
                                <img src={flashlightIco} onClick={this.localizarRobot} />
                                <input type="number" ref="robId" className="form-control" readOnly={true} defaultValue={this.orden != null ? this.orden.robot : 0} />
                                <input type="text" ref="robName" className="form-control" readOnly={true} />
                            </div>
                            <div className="col">
                                <label>Estado</label>
                                <p><img src={ordenesMinadoEstadoImg[this.orden != null ? this.orden.estado : 'Q']} />
                                    {ordenesMinadoEstado[this.orden != null ? this.orden.estado : 'Q']}
                                </p>
                            </div>
                        </div>
                        <div className="form-row">
                            <div className="col">
                                <label>Posici&oacute;n X</label>
                                <input type="number" className="form-control" defaultValue={this.orden != null ? this.orden.posX : 0} readOnly={true} />
                            </div>
                            <div className="col">
                                <label>Posici&oacute;n Y</label>
                                <input type="number" className="form-control" defaultValue={this.orden != null ? this.orden.posY : 0} readOnly={true} />
                            </div>
                            <div className="col">
                                <label>Posici&oacute;n Z</label>
                                <input type="number" className="form-control" defaultValue={this.orden != null ? this.orden.posZ : 0} readOnly={true} />
                            </div>
                            <div className="col">
                                <label>Posici&oacute;n F</label>
                                <input type="number" className="form-control" defaultValue={this.orden != null ? this.orden.facing : 0} readOnly={true} />
                            </div>
                        </div>
                        <div className="form-row">
                            <div className="col">
                                <label>GPS Posici&oacute;n X</label>
                                <input type="number" className="form-control" ref="gpsX" defaultValue={this.orden != null ? this.orden.gpsX : 0} />
                            </div>
                            <div className="col">
                                <label>GPS Posici&oacute;n Y</label>
                                <input type="number" className="form-control" ref="gpsY" defaultValue={this.orden != null ? this.orden.gpsY : 0} />
                            </div>
                            <div className="col">
                                <label>GPS Posici&oacute;n Z</label>
                                <input type="number" className="form-control" ref="gpsZ" defaultValue={this.orden != null ? this.orden.gpsZ : 0} />
                            </div>
                            <div className="col" id="shutdown">
                                <input type="checkbox" ref="shutdown" defaultChecked={this.orden != null ? this.orden.shutdown : false} />
                                <label>&iquest;Apagar?</label>
                            </div>
                        </div>
                        <div className="form-row">
                            <div className="col">
                                <label>Fecha de creaci&oacute;n</label>
                                <input type="text" className="form-control" readOnly={true}
                                    defaultValue={this.orden != null ? this.formatearFechaTiempo(this.orden.dateAdd) : ''} />
                            </div>
                            <div className="col">
                                <label>Fecha de &uacute;ltima modificaci&oacute;n</label>
                                <input type="text" className="form-control" readOnly={true}
                                    defaultValue={this.orden != null ? this.formatearFechaTiempo(this.orden.dateUpd) : ''} />
                            </div>
                        </div>
                        <div className="form-row">
                            <div className="col">
                                <label>Fecha de inicio</label>
                                <input type="text" className="form-control" readOnly={true} defaultValue=
                                    {this.orden != null && this.orden.dateInicio != null ? this.formatearFechaTiempo(new Date(this.orden.dateInicio)) : ''} />
                            </div>
                            <div className="col">
                                <label>Fecha de finalizaci&oacute;n</label>
                                <input type="text" className="form-control" readOnly={true}
                                    defaultValue={this.orden != null && this.orden.dateFin != null ? this.formatearFechaTiempo(new Date(this.orden.dateFin)) : ''} />
                            </div>
                        </div>
                        <label>Descripci&oacute;n</label>
                        <textarea className="form-control" ref="dsc" defaultValue={this.orden != null ? this.orden.descripcion : ''} rows="5"></textarea>

                        <div className="form-row" id="ordenRobotParams">
                            <div className="col">
                                <div className="btn-group btn-group-toggle" data-toggle="buttons">
                                    <label className={"btn btn-secondary" + (this.orden != null ? (this.orden.unidadRecarga == "%" ? " active" : "") : " active")}>
                                        <input type="radio" name="unidadRecarga" id="%" ref="unidadRecargaPor" />%
                                    </label>
                                    <label className={"btn btn-secondary" + (this.orden != null && this.orden.unidadRecarga == "=" ? " active" : "")}>
                                        <input type="radio" name="unidadRecarga" id="=" ref="unidadRecargaIgu" />=
                                    </label>
                                </div>
                                <input type="number" className="form-control" ref="energiaRecarga" min="0" defaultValue={this.orden != null ? this.orden.energiaRecarga : 10} />
                                <input type="checkbox" ref="notificacion" defaultChecked={this.orden != null ? this.orden.notificacion : false} />
                                <label>&iquest;Notificaci&oacute;n?</label>
                            </div>
                            <div className="col">
                                <div className="btn-group btn-group-toggle" data-toggle="buttons">
                                    <label className={"btn btn-primary" + (this.orden != null ? (this.orden.modoMinado == "O" ? " active" : "") : " active")}>
                                        <input type="radio" name="modoMinado" id="opti" data-toggle="tooltip" data-placement="top" data-html="true" title={"<div className='tooltipOrdenMinado'><img src=" + optimoIco + " />Estrategia de miner&iacute;a m&aacute;s r&aacute;pida aunque menos eficiente.<ul><li>Todos los picos del robot deben de ser de hierro o superior, para poder romper todos los elementos que se esperen minar sin considerar nada m&aacute;s. El robot volver&aacute; al origen y conseguir&aacute; un nuevo pico si se le termina.</li><li>El robot no recargar&aacute; tanto el pico.</li><li>Dependiendo de los materiales que se encuentren, puede ser contraproducente.</li></ul></div>"}
                                            ref="modoMinadoO" />
                                        <img src={ironPickaxeIco} />Modo optimizado
                                    </label>
                                    <label className={"btn btn-primary" + (this.orden != null && this.orden.modoMinado == "E" ? " active" : "")}>
                                        <input type="radio" name="modoMinado" id="eco"
                                            data-toggle="tooltip" data-placement="top" data-html="true" title={"<div className='tooltipOrdenMinado'><img src=" + ecoIco + " />Estategia de miner&iacute;a m&aacute;s lenta aunque m&aacute;s barata.<ul><li>El robot ocupar&aacute; dos posiciones en el inventario para picos, uno ser&iacute;a de piedra (m&aacute;s econ&oacute;mico), y otro de un material m&aacute;s &oacute;ptimo, como el herro. El robot elegir&aacute; el tipo de pico adecuado cada vez, usando solo el &oacute;ptimo cuando sea necesario.</li><li>La miner&iacute;a ser&aacute; mucho m&aacute;s lenta y el robot perder&aacute; mucho m&aacute;s tiempo recargando.</li><li>Minado mucho m&aacute;s f&aacute;cil de rentabilizar.</li></ul></div>"}
                                            ref="modoMinadoE" defaultChecked={this.orden != null ? this.orden.modoMinado == "E" : false} />
                                        <img src={stonePickaxeIco} />Modo econ&oacute;mico
                                    </label>
                                </div>
                            </div>
                        </div>

                        <h5>&Iacute;tems obtenidos</h5>
                        <div className="row row-cols-1 row-cols-md-4" id="inventarioOrdenMinadoContenido"></div>

                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-danger" onClick={this.eliminar}>Eliminar</button>
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                        <button type="button" className="btn btn-success" onClick={this.aceptar}>Guardar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class OrdenMinadoFormInventarioSlot extends Component {
    constructor({ numeroSlot, cant, articulo, img }) {
        super();

        this.numeroSlot = numeroSlot;
        this.cant = cant;
        this.articulo = articulo;
        this.img = img;
    }

    render() {
        return <div className="card">
            <div className="form-row">
                <div className="col">
                    {this.img != null ? <img src={this.img} /> : <img />}
                </div>

                <div className="col">
                    <div className="card-body">
                        <h5 className="card-title">{this.articulo == null ? '' : this.articulo.name}</h5>
                        <h6 className="card-subtitle mb-2 text-muted">{this.articulo == null ? '' : this.cant}</h6>
                    </div>
                </div>
            </div>
        </div>;
    }
};

class OrdenMinadoFormDeleteConfirm extends Component {
    constructor({ ordenName, ordenId, handleEliminar }) {
        super();

        this.ordenName = ordenName;
        this.ordenId = ordenId;

        this.handleEliminar = handleEliminar;

        this.eliminar = this.eliminar.bind(this);
    }

    componentDidMount() {
        window.$('#ordenMinadoFormDeleteConfirm').modal({ show: true });
    }

    eliminar() {
        window.$('#ordenMinadoFormDeleteConfirm').modal('hide');
        this.handleEliminar();
    }

    render() {
        return <div className="modal fade" id="ordenMinadoFormDeleteConfirm" tabIndex="-1" role="dialog"
            aria-labelledby="ordenMinadoFormDeleteConfirm" aria-hidden="true">
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="exampleModalLabel">Confirmar eliminaci&oacute;n</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <p>&iquest;Est&aacute;s seguro de que quieres eliminar la orden de minado {this.ordenName}#{this.ordenId}?</p>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                        <button type="button" className="btn btn-danger" onClick={this.eliminar}>Eliminar</button>
                    </div>
                </div>
            </div>
        </div>
    }
}

export default OrdenMinadoForm;


