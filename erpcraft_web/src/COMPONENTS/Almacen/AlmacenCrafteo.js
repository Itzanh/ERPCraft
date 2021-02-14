import { Component } from "react";
import ReactDOM from 'react-dom';

import flashlightIco from './../../IMG/flashlight.svg';
import trashIco from './../../IMG/trash.svg';

import CrafteoLocalizador from "./CrafteoLocalizador";
import SmeltingLocalizador from "./SmeltingLocalizador";

const ordenMinadoEstado = {
    "Q": "En cola",
    "R": "En curso",
    "D": "Hecho",
    "E": "Error"
};

class AlmacenCrafteo extends Component {
    constructor({ idAlmacen, getFabricaciones, addFabricacion, editFabricacion, deleteFabricacion,
        localizarCrafteos, localizarSmeltings, addOrdenFabricacion, getOrdenFabricacion, searchOrdenesFabricacion, previewCrafteo, deleteOrdenFabricacion }) {
        super();

        this.idAlmacen = idAlmacen;

        // FABRICACION
        this.getFabricaciones = getFabricaciones;
        this.addFabricacion = addFabricacion;
        this.editFabricacion = editFabricacion;
        this.deleteFabricacion = deleteFabricacion;

        // ORDENES DE FABRICACION
        this.localizarCrafteos = localizarCrafteos;
        this.localizarSmeltings = localizarSmeltings;
        this.addOrdenFabricacion = addOrdenFabricacion;
        this.getOrdenFabricacion = getOrdenFabricacion;
        this.searchOrdenesFabricacion = searchOrdenesFabricacion;
        this.previewCrafteo = previewCrafteo;
        this.deleteOrdenFabricacion = deleteOrdenFabricacion;

        this.idReceta = 0;
        this.idRecetaBuscar = 0;

        this.fabricacion = this.fabricacion.bind(this);
        this.localizar = this.localizar.bind(this);
        this.clearLocalizar = this.clearLocalizar.bind(this);
        this.clearLocalizarBuscar = this.clearLocalizarBuscar.bind(this);
        this.add = this.add.bind(this);
        this.eliminar = this.eliminar.bind(this);
        this.localizarBusqueda = this.localizarBusqueda.bind(this);
        this.buscar = this.buscar.bind(this);
    }

    async componentDidMount() {
        window.$('#almacenCrafteo').modal({ show: true });
        await this.renderFabricaciones();
        this.renderOrdenesFabricacion();
    }

    renderFabricaciones() {
        return new Promise(async (resolve) => {
            const fabricaciones = await this.getFabricaciones(this.idAlmacen, true);

            ReactDOM.render(fabricaciones.map((element, i) => {
                return <option key={i} value={element.id}>{element.name}</option>;
            }), this.refs.fabricacion);
            resolve();
        });
    }

    formatearFechaTiempo(fechaTiempo) {
        const fecha = new Date(fechaTiempo);
        return fecha.getDate() + '/'
            + (fecha.getMonth() + 1) + '/'
            + fecha.getFullYear() + ' '
            + fecha.getHours() + ':'
            + fecha.getMinutes() + ':'
            + fecha.getSeconds();
    }

    async renderOrdenesFabricacion() {
        if (this.refs.fabricacion.value.length == 0) {
            return;
        }
        const idFabricacion = parseInt(this.refs.fabricacion.value);

        const ordenesFabricacion = await this.getOrdenFabricacion(this.idAlmacen, idFabricacion);
        this.printOrdenesFabricacion(ordenesFabricacion);
    }

    printOrdenesFabricacion(ordenesFabricacion) {
        ReactDOM.unmountComponentAtNode(this.refs.renderOrdenes);
        ReactDOM.render(ordenesFabricacion.map((element, i) => {
            return <tr key={i}>
                <th scope="row">{element.id}</th>
                <td>{element.name}</td>
                <td>{ordenMinadoEstado[element.estado]}</td>
                <td>{element.idCrafteo != null ? 'Craftear' : 'Hornear'}</td>
                <td>{element.idCrafteo != null ? element.idCrafteo : element.idSmelting}</td>
                <td>{element.cantidad}</td>
                <td>{this.formatearFechaTiempo(element.dateAdd)}</td>
                <td>{element.dateFinalizado == null ? '' : this.formatearFechaTiempo(element.dateFinalizado)}</td>
                <td>{element.cantidadFabricado}</td>
                <td>{element.errorCode == 0 ? "OK" : "ERR"}</td>
                <td><img src={trashIco} onClick={() => { this.eliminar(element.id); }} /></td>
            </tr>
        }), this.refs.renderOrdenes);
    }

    fabricacion() {
        window.$('#almacenCrafteo').modal('hide');
        ReactDOM.unmountComponentAtNode(document.getElementById("renderModalAlmacen"));
        ReactDOM.render(<AlmacenFabricacion
            idAlmacen={this.idAlmacen}
            // FABRICACION
            getFabricaciones={this.getFabricaciones}
            addFabricacion={this.addFabricacion}
            editFabricacion={this.editFabricacion}
            deleteFabricacion={this.deleteFabricacion}
        />, document.getElementById("renderModalAlmacen"));
    }

    clearLocalizar() {
        this.idReceta = 0;
        this.refs.recId.value = 0;
        this.refs.recName.value = '';
        this.refs.cant.value = '0';
        this.refs.cantMax.value = '0';
    }

    clearLocalizarBuscar() {
        this.idRecetaBuscar = 0;
        this.refs.busrecId.value = 0;
        this.refs.busrecName.value = '';
    }

    calcularName(name) {
        if (name == null || name == '') {
            return;
        }

        if (this.refs.name.value.length == 0) {
            name += ' ';
            const charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            for (let i = 0; i < 6; i++) {
                name += charset.charAt(Math.random() * charset.length);
            }

            this.refs.name.value = name;
        }

        if (this.idReceta != 0) {
            const tipo = this.refs.radioCraftC.checked ? 'C' : 'S';
            this.previewCrafteo(this.idAlmacen, this.idReceta, tipo).then((preview) => {
                this.refs.cant.value = preview.cantidadCrafteo;
                this.refs.cant.step = preview.cantidadCrafteo;
                this.refs.cant.min = preview.cantidadCrafteo;
                this.refs.cantMax.value = preview.maxCantidadCrafteo;
                this.refs.cant.max = preview.maxCantidadCrafteo;
            });
        }

    }

    localizar() {
        if (this.refs.radioCraftC.checked) {
            this.clearLocalizar();
            this.localizarCrafteo();
        } else if (this.refs.radioCraftS.checked) {
            this.clearLocalizar();
            this.localizarSmelting();
        }
    }

    localizarBusqueda() {
        this.clearLocalizarBuscar();
        if (this.refs.busCraftC.checked) {
            ReactDOM.unmountComponentAtNode(document.getElementById("renderAlmacenModalAlert"));
            ReactDOM.render(<CrafteoLocalizador
                getCrafteos={this.localizarCrafteos}
                handleSelect={(id, name) => {
                    this.idRecetaBuscar = id;
                    this.refs.busrecId.value = id;
                    this.refs.busrecName.value = name;
                }}
            />, document.getElementById("renderAlmacenModalAlert"));
        } else if (this.refs.busCraftS.checked) {
            ReactDOM.unmountComponentAtNode(document.getElementById("renderAlmacenModalAlert"));
            ReactDOM.render(<SmeltingLocalizador
                getSmeltings={this.localizarSmeltings}
                handleSelect={(id, name) => {
                    this.idRecetaBuscar = id;
                    this.refs.busrecId.value = id;
                    this.refs.busrecName.value = name;
                }}
            />, document.getElementById("renderAlmacenModalAlert"));
        }
    }

    localizarCrafteo() {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderAlmacenModalAlert"));
        ReactDOM.render(<CrafteoLocalizador
            getCrafteos={this.localizarCrafteos}
            handleSelect={(id, name) => {
                this.idReceta = id;
                this.refs.recId.value = id;
                this.refs.recName.value = name;
                this.calcularName(name);
            }}
        />, document.getElementById("renderAlmacenModalAlert"));
    };

    localizarSmelting() {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderAlmacenModalAlert"));
        ReactDOM.render(<SmeltingLocalizador
            getSmeltings={this.localizarSmeltings}
            handleSelect={(id, name) => {
                this.idReceta = id;
                this.refs.recId.value = id;
                this.refs.recName.value = name;
                this.calcularName(name);
            }}
        />, document.getElementById("renderAlmacenModalAlert"));
    };

    add() {
        const ordenFabricacion = {};
        ordenFabricacion.idAlmacen = this.idAlmacen;
        ordenFabricacion.name = this.refs.name.value;
        ordenFabricacion.idFabricacion = parseInt(this.refs.fabricacion.value);
        if (this.refs.radioCraftC.checked) {
            ordenFabricacion.idCrafteo = this.idReceta;
        } else if (this.refs.radioCraftS.checked) {
            ordenFabricacion.idSmelting = this.idReceta;
        } else {
            return;
        }
        ordenFabricacion.cantidad = parseInt(this.refs.cant.value);
        this.addOrdenFabricacion(ordenFabricacion).then(() => {
            this.renderOrdenesFabricacion();
        });
    }

    eliminar(id) {
        if (this.refs.fabricacion.value.length == 0) {
            return;
        }
        const idFabricacion = parseInt(this.refs.fabricacion.value);
        this.deleteOrdenFabricacion(id, this.idAlmacen, idFabricacion).then(() => {
            this.renderOrdenesFabricacion();
        });
    }

    buscar() {
        if (this.refs.fabricacion.value.length == 0) {
            return;
        }
        const idFabricacion = parseInt(this.refs.fabricacion.value);

        const busqueda = {};
        busqueda.idAlmacen = this.idAlmacen;
        busqueda.idFabricacion = idFabricacion;

        if (this.refs.busCraftC.checked) {
            busqueda.tipoReceta = "C";
        } else if (this.refs.busCraftS.checked) {
            busqueda.tipoReceta = "S";
        } else {
            busqueda.tipoReceta = "T";
        }

        var start;
        var end;
        if (this.refs.startdate.value != "") {
            start = new Date(this.refs.startdate.value + " " + this.refs.starttime.value);
            start.setHours(start.getHours() + 1);
        }
        if (this.refs.enddate.value != "") {
            end = new Date(this.refs.enddate.value + " " + this.refs.endtime.value);
            end.setHours(end.getHours() + 1);
            end.setSeconds(59);
            end.setMilliseconds(999);
        }

        busqueda.inicio = start;
        busqueda.fin = end;
        busqueda.idReceta = this.idRecetaBuscar;
        busqueda.estado = parseInt(this.refs.busEstado.value);

        console.log(busqueda);
        this.searchOrdenesFabricacion(busqueda).then((ordenesFabricacion) => {
            console.log(ordenesFabricacion);
            this.printOrdenesFabricacion(ordenesFabricacion);
        });
    }

    render() {
        return <div class="modal fade bd-example-modal-xl" id="almacenCrafteo" tabindex="-1" role="dialog" aria-labelledby="almacenCrafteoLabel" aria-hidden="true">
            <div class="modal-dialog modal-xl" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <select class="form-control form-control-lg" ref="fabricacion">
                        </select>
                        <button type="button" class="btn btn-primary" onClick={this.fabricacion}>Fabricaci&oacute;n</button>
                        <h5 class="modal-title" id="almacenCrafteoLabel">&Oacute;rdenes de fabricaci&oacute;n</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <div className="form-row" id="formAddOrdenFabricacion">
                            <div className="col">
                                <label>Nombre</label>
                                <input type="text" className="form-control" placeholder="Nombre" ref="name" />
                            </div>
                            <div className="col">
                                <div class="form-check form-check-inline">
                                    <input class="form-check-input" type="radio" name="tipoRadio" id="tipoRadio1" value="C" defaultChecked={true} ref="radioCraftC" onClick={this.clearLocalizar} />
                                    <label class="form-check-label" for="tipoRadio1">Craftear</label>
                                </div>
                                <div class="form-check form-check-inline">
                                    <input class="form-check-input" type="radio" name="tipoRadio" id="tipoRadio2" value="S" ref="radioCraftS" onClick={this.clearLocalizar} />
                                    <label class="form-check-label" for="tipoRadio2">Hornear</label>
                                </div>
                            </div>
                            <div className="col localizador">
                                <label>Receta</label>
                                <img src={flashlightIco} onClick={this.localizar} />
                                <input type="number" ref="recId" className="form-control" readOnly={true} defaultValue={0} />
                                <input type="text" ref="recName" className="form-control" readOnly={true} />
                            </div>
                            <div className="col">
                                <div className="form-row">
                                    <div className="col">
                                        <label>Cantidad</label>
                                        <input type="number" className="form-control" defaultValue="0" ref="cant" step="1" />
                                    </div>
                                    <div className="col">
                                        <label>Max. cant.</label>
                                        <input type="number" className="form-control" defaultValue="0" ref="cantMax" readOnly={true} />
                                    </div>
                                </div>
                            </div>
                            <div className="col">
                                <button type="button" class="btn btn-primary" onClick={this.add}>Agregar</button>
                            </div>
                        </div>
                        <div className="form-row" id="formBusOrdenFabricacion">
                            <div className="col">
                                <div class="form-check form-check-inline">
                                    <input class="form-check-input" type="radio" name="tipoRadioBus" id="tipoRadio3" value="C" ref="busCraftC" onClick={this.clearLocalizarBuscar} />
                                    <label class="form-check-label" for="tipoRadio3">Craftear</label>
                                </div>
                                <div class="form-check form-check-inline">
                                    <input class="form-check-input" type="radio" name="tipoRadioBus" id="tipoRadio4" value="S" ref="busCraftS" onClick={this.clearLocalizarBuscar} />
                                    <label class="form-check-label" for="tipoRadio4">Hornear</label>
                                </div>
                                <div class="form-check form-check-inline">
                                    <input class="form-check-input" type="radio" name="tipoRadioBus" id="tipoRadio5" value=" " ref="busCraftT" defaultChecked={true} onClick={this.clearLocalizarBuscar} />
                                    <label class="form-check-label" for="tipoRadio5">Todo</label>
                                </div>
                            </div>
                            <div className="col localizador">
                                <label>Receta</label>
                                <img src={flashlightIco} onClick={this.localizarBusqueda} />
                                <input type="number" ref="busrecId" className="form-control" readOnly={true} defaultValue={0} />
                                <input type="text" ref="busrecName" className="form-control" readOnly={true} />
                            </div>
                            <div className="col">
                                <div className="form-row">
                                    <div className="col">
                                        <label>Fecha inicio</label>
                                        <br />
                                        <input type="date" id="startdate" name="startdate" ref="startdate" />
                                        <input type="time" id="starttime" name="starttime" ref="starttime" />
                                    </div>
                                    <div className="col">
                                        <label>Fecha fin</label>
                                        <br />
                                        <input type="date" id="enddate" name="enddate" ref="enddate" />
                                        <input type="time" id="endtime" name="endtime" ref="endtime" />
                                    </div>
                                </div>
                            </div>
                            <div className="col">
                                <label for="busEstado">Estado</label>
                                <select class="form-control" id="busEstado" ref="busEstado" onChange={this.buscar}>
                                    <option value="0">En cola & pendiente</option>
                                    <option value="1">En cola</option>
                                    <option value="2">En curso</option>
                                    <option value="3">Finalizado</option>
                                    <option value="4">Todo</option>
                                </select>
                            </div>
                            <div className="col">
                                <button type="button" class="btn btn-success" onClick={this.buscar}>Buscar</button>
                            </div>
                        </div>
                        <table class="table table-dark" id="ordenesFabricacion">
                            <thead>
                                <tr>
                                    <th scope="col">#</th>
                                    <th scope="col">Nombre</th>
                                    <th scope="col">Estado</th>
                                    <th scope="col">Receta</th>
                                    <th scope="col">Receta</th>
                                    <th scope="col">Cantidad</th>
                                    <th scope="col">Fecha creado</th>
                                    <th scope="col">Fecha finalizado</th>
                                    <th scope="col">Cantidad resultado</th>
                                    <th scope="col">&iquest;OK?</th>
                                    <th scope="col"></th>
                                </tr>
                            </thead>
                            <tbody ref="renderOrdenes"></tbody>
                        </table>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-dismiss="modal">Aceptar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class AlmacenFabricacion extends Component {
    constructor({ fabricacion, idAlmacen, getFabricaciones, addFabricacion, editFabricacion, deleteFabricacion }) {
        super();

        this.fabricacion = fabricacion;
        this.idAlmacen = idAlmacen;

        // FABRICACION
        this.getFabricaciones = getFabricaciones;
        this.addFabricacion = addFabricacion;
        this.editFabricacion = editFabricacion;
        this.deleteFabricacion = deleteFabricacion;

        this.newForm = this.newForm.bind(this);
        this.editForm = this.editForm.bind(this);
    }

    componentDidMount() {
        window.$('#almFabricacionesModal').modal({ show: true });
        this.newForm();
        this.renderFabricaciones();
    }

    newForm() {
        ReactDOM.unmountComponentAtNode(this.refs.form);
        ReactDOM.render(<AlmacenFabricacionForm
            idAlmacen={this.idAlmacen}
            addFabricacion={(fabricacion) => { this.addFabricacion(fabricacion); this.renderFabricaciones(); }}
        />, this.refs.form);
    }

    editForm(fabricacion) {
        ReactDOM.unmountComponentAtNode(this.refs.form);
        ReactDOM.render(<AlmacenFabricacionForm
            idAlmacen={this.idAlmacen}
            fabricacion={fabricacion}
            editFabricacion={(fabricacion) => { this.editFabricacion(fabricacion); this.renderFabricaciones(); }}
            deleteFabricacion={(id, idAlmacen) => { this.deleteFabricacion(id, idAlmacen); this.renderFabricaciones(); }}
        />, this.refs.form);
    }

    async renderFabricaciones() {
        const fabricaciones = await this.getFabricaciones(this.idAlmacen);

        ReactDOM.unmountComponentAtNode(this.refs.fabricaciones);
        ReactDOM.render(fabricaciones.map((element, i) => {
            return <tr key={i} onClick={() => { this.editForm(element); }}>
                <th scope="row">{element.id}</th>
                <td>{element.name}</td>
            </tr>
        }), this.refs.fabricaciones);
    }

    render() {
        return <div class="modal fade bd-example-modal-lg" id="almFabricacionesModal" tabindex="-1" role="dialog" aria-labelledby="almFabricacionesModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-lg" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="almFabricacionesModalLabel">Fabricaciones del almac&eacute;n</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <div className="form-row">
                            <div className="col">
                                <button type="button" class="btn btn-primary" onClick={this.newForm}>Nuevo</button>
                                <table class="table table-dark">
                                    <thead>
                                        <tr>
                                            <th scope="col">#</th>
                                            <th scope="col">Nombre</th>
                                        </tr>
                                    </thead>
                                    <tbody ref="fabricaciones"></tbody>
                                </table>
                            </div>
                            <div className="col" ref="form">

                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class AlmacenFabricacionForm extends Component {
    constructor({ fabricacion, idAlmacen, addFabricacion, editFabricacion, deleteFabricacion }) {
        super();

        this.fabricacion = fabricacion;
        this.idAlmacen = idAlmacen;

        // FABRICACION
        this.addFabricacion = addFabricacion;
        this.editFabricacion = editFabricacion;
        this.deleteFabricacion = deleteFabricacion;

        this.aceptar = this.aceptar.bind(this);
    }

    async aceptar() {
        const fabricacion = {};
        fabricacion.idAlmacen = this.idAlmacen;
        fabricacion.name = this.refs.name.value;
        fabricacion.tipo = this.refs.tipo.value;
        fabricacion.uuid = this.refs.uuid.value;
        fabricacion.off = this.refs.off.checked;
        fabricacion.cofreSide = parseInt(this.refs.cofreSide.value);
        fabricacion.hornoSide = parseInt(this.refs.hornoSide.value);
        console.log(fabricacion);
        if (this.fabricacion == null) {
            const id = await this.addFabricacion(fabricacion);
            if (id > 0) {
                this.refs.id.value = id;
            }
        } else {
            fabricacion.id = this.fabricacion.id;
            this.editFabricacion(fabricacion);
        }
    }

    delete() {
        if (this.fabricacion == null) {
            return;
        }
        this.deleteFabricacion(this.fabricacion.id, this.idAlmacen);
    }

    render() {
        return <div>
            <div className="form-row">
                <div className="col">
                    <label>ID de fabricaci&oacute;n</label>
                    <input class="form-control" type="number" readOnly={true} defaultValue={this.fabricacion == null ? '' : this.fabricacion.id} ref="id" />
                </div>
                <div className="col">
                    <label>ID de almac&eacute;n</label>
                    <input class="form-control" type="number" readOnly={true} defaultValue={this.idAlmacen} />
                </div>
            </div>
            <label>Nombre</label>
            <input class="form-control" type="text" placeholder="Nombre" ref="name" defaultValue={this.fabricacion == null ? '' : this.fabricacion.name} />
            <label>UUID del controlador</label>
            <input class="form-control" type="text" placeholder="UUID" ref="uuid" defaultValue={this.fabricacion == null ? '' : this.fabricacion.uuid} />
            <label>Tipo</label>
            <select class="form-control" ref="tipo" defaultValue={this.fabricacion == null ? 'R' : this.fabricacion.tipo}>
                <option value="R">Robot con cofre</option>
                <option value="A">Controlador AE2</option>
            </select>
            <label>Lado del Robot en el que est&aacute; el cofre</label>
            <select class="form-control" ref="cofreSide" defaultValue={this.fabricacion == null ? '3' : this.fabricacion.cofreSide}>
                <option value="0">Abajo</option>
                <option value="1">Arriba</option>
                <option value="2">Atr&aacute;s</option>
                <option value="3">Delante</option>
                <option value="4">Derecha</option>
                <option value="5">Izquierda</option>
            </select>
            <label>Lado del Robot en el que est&aacute; el horno</label>
            <select class="form-control" ref="hornoSide" defaultValue={this.fabricacion == null ? '0' : this.fabricacion.hornoSide}>
                <option value="0">Izquierda</option>
                <option value="1">Derecha</option>
            </select>
            <label>Fecha de creci&oacute;n</label>
            <input class="form-control" type="text" readOnly={true} />
            <input type="checkbox" ref="off" defaultChecked={this.fabricacion == null ? false : this.fabricacion.off} />
            <label>&iquest;Desactivado?</label>
            <br />
            <button type="button" class="btn btn-info" onClick={this.aceptar}>Crear/modificar</button>
            <button type="button" class="btn btn-danger">Eliminar</button>
        </div>
    }
};



export default AlmacenCrafteo;
