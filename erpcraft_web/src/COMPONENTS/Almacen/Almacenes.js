import { Component } from "react";
import ReactDOM from 'react-dom';

// Componentes
import FormAlert from "../FormAlert";
import ArticuloLocalizador from "../Articulos/ArticuloLocalizador";

// CSS
import './../../CSS/Almacen.css';

// Iconos
import inventarioIco from './../../IMG/inventario.png';
import flashlightIco from './../../IMG/flashlight.svg';
import deleteIco from './../../IMG/delete.svg';

// Tipos de almacen
import chestIco from './../../IMG/almacen_tipo/chest.png';
import double_chestIco from './../../IMG/almacen_tipo/double_chest.gif';
import me_chestIco from './../../IMG/almacen_tipo/me_chest.png';
import ender_chestIco from './../../IMG/almacen_tipo/ender_chest.png';

// Tipos de almacenamiento de AE2
import storageCell1K from './../../IMG/ae2_storage/StorageCell.1k.png';
import storageCell4K from './../../IMG/ae2_storage/StorageCell.4k.png';
import storageCell16K from './../../IMG/ae2_storage/StorageCell.16k.png';
import storageCel64K from './../../IMG/ae2_storage/StorageCell.64k.png';
import AlmacenCrafteo from "./AlmacenCrafteo";

const almacenTipoImg = {
    "C": chestIco,
    "D": double_chestIco,
    "M": me_chestIco,
    "O": ender_chestIco
};

const storageCell = {
    1: "1k ME Storage Cell",
    2: "4k ME Storage Cell",
    3: "16k ME Storage Cell",
    4: "64k ME Storage Cell"
};

const storageCellImg = {
    1: storageCell1K,
    2: storageCell4K,
    3: storageCell16K,
    4: storageCel64K
};


class Almacenes extends Component {
    constructor({ getAlmacenes, getAlmacenInventario, almacenInventarioPush, getArticuloImg, tabAlmacenPush, handleAdd, handleEdit, handleDelete,
        getAlmacenNotificaciones, addAlmacenNotificaciones, deleteAlmacenNotificaciones, getArticulos,
        getAlmacenStorageCells, addAlmacenStorageCell, deleteAlmacenStorageCell,
        getFabricaciones, addFabricacion, editFabricacion, deleteFabricacion,
        localizarCrafteos, localizarSmeltings, addOrdenFabricacion, getOrdenFabricacion, searchOrdenesFabricacion, previewCrafteo, deleteOrdenFabricacion }) {
        super();
        this.almacen = null;
        this.imgCache = {};

        // ALMACENES
        this.getAlmacenes = getAlmacenes;
        this.getAlmacenInventario = getAlmacenInventario;
        this.almacenInventarioPush = almacenInventarioPush;
        this.getArticuloImg = getArticuloImg;
        this.tabAlmacenPush = tabAlmacenPush;

        this.handleAdd = handleAdd;
        this.handleEdit = handleEdit;
        this.handleDelete = handleDelete;

        // NOTIFICACIONES
        this.getAlmacenNotificaciones = getAlmacenNotificaciones;
        this.addAlmacenNotificaciones = addAlmacenNotificaciones;
        this.deleteAlmacenNotificaciones = deleteAlmacenNotificaciones;
        this.getArticulos = getArticulos;

        // STORAGE CELLS
        this.getAlmacenStorageCells = getAlmacenStorageCells;
        this.addAlmacenStorageCell = addAlmacenStorageCell;
        this.deleteAlmacenStorageCell = deleteAlmacenStorageCell;

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

        this.getInventario = this.getInventario.bind(this);
        this.crear = this.crear.bind(this);
        this.editar = this.editar.bind(this);
        this.borrar = this.borrar.bind(this);
        this.craft = this.craft.bind(this);
    }

    componentDidMount() {
        this.renderAlmacenes();
    }

    almacenPorcentajeOcupado(almacen) {
        switch (almacen.almacenamiento) {
            case "C": {
            }
            case "D": {
                return Math.floor((almacen.stacks / almacen.maximoStacks) * 100);
            }
            case "M": {
                if (almacen.maximoTipos == 0 || almacen.maximoItems == 0) {
                    return 0;
                }
                return Math.floor(Math.max(((almacen.tipos / almacen.maximoTipos) * 100), ((almacen.items / almacen.maximoItems) * 100)));
            }
            case "O": {
                var porcentajeUsado = 0;
                if (almacen.maximoStacks > 0) {
                    porcentajeUsado = (almacen.stacks / almacen.maximoStacks) * 100;
                }
                if (almacen.maximoTipos > 0) {
                    porcentajeUsado = Math.max(porcentajeUsado, ((almacen.tipos / almacen.maximoTipos) * 100));
                }
                if (almacen.maximoItems > 0) {
                    porcentajeUsado = Math.max(porcentajeUsado, ((almacen.items / almacen.maximoItems) * 100));
                }
                return Math.floor(porcentajeUsado);
            }
        }
    }

    async renderAlmacenes() {
        const almacenes = await this.getAlmacenes();
        await this.renderAlmacenesList(almacenes);

        this.tabAlmacenPush(async (changeType, pos, newAlmacen) => {

            switch (changeType) {
                case 0: { // add
                    almacenes.push(newAlmacen);

                    break;
                }
                case 1: { // update
                    for (var i = 0; i < almacenes.length; i++) {
                        if (almacenes[i].id === pos) {
                            almacenes[i] = newAlmacen;
                            break;
                        }
                    }

                    break;
                }
                case 2: { // delete
                    for (var i = 0; i < almacenes.length; i++) {
                        if (almacenes[i].id === pos) {
                            almacenes.splice(i, 1);
                            break;
                        }
                    }

                    break;
                }
            }

            this.renderAlmacenesList(almacenes);
        });
    };

    async renderAlmacenesList(almacenes) {
        await ReactDOM.unmountComponentAtNode(this.refs.renderAlmacenes);
        ReactDOM.render(almacenes.map((element, i) => {
            return <Almacen
                key={i}

                id={element.id}
                name={element.name}
                ocupado={this.almacenPorcentajeOcupado(element)}
                almacenamiento={element.almacenamiento}
                selectAlmacen={(idAlmacen) => {
                    const idAlmacenAnterior = this.almacen == null ? null : this.almacen.id;
                    this.almacen = element;
                    this.getInventario(idAlmacen, idAlmacenAnterior);
                }}
            />
        }), this.refs.renderAlmacenes);
    }

    async getInventario(idAlmacen, idAlmacenAnterior) {
        const inventario = await this.getAlmacenInventario(idAlmacen);

        this.renderInventario(inventario);

        this.almacenInventarioPush(idAlmacen, (inventario) => {
            this.renderInventario(inventario);
        }, idAlmacenAnterior);

    };

    getArticuloImgCache(idArticulo) {
        return new Promise(async (resolve) => {
            if (this.imgCache[idArticulo] != null) { // intentar obtener de cache local
                resolve(this.imgCache[idArticulo]);
            } else { // pedir al servidor y vovler a guardar en cache
                const img = await this.getArticuloImg(idArticulo);
                this.imgCache[idArticulo] = img;
                resolve(img);
            }
        });
    }

    async renderInventario(inventario) {
        // render inventario
        await ReactDOM.unmountComponentAtNode(this.refs.renderSlots);
        ReactDOM.render(inventario.map((element, i) => {
            return <AlmacenesFormInventarioSlot
                key={i}

                cant={element.cantidad}
                cantDisp={element.cantidadDisponible}
                articulo={element.articulo}
            />
        }), this.refs.renderSlots);

        // obtener imágenes de los artículos
        for (let i = 0; i < inventario.length; i++) {
            const articulo = inventario[i].articulo;

            if (articulo != null && articulo.id != 0) {
                const img = await this.getArticuloImgCache(articulo.id);
                inventario[i].img = img;
            }
        }

        // render inventario con imágenes
        ReactDOM.unmountComponentAtNode(this.refs.renderSlots);
        ReactDOM.render(inventario.map((element, i) => {
            return <AlmacenesFormInventarioSlot
                key={i}

                cant={element.cantidad}
                cantDisp={element.cantidadDisponible}
                articulo={element.articulo}
                img={element.img}
            />
        }), this.refs.renderSlots);
    };

    async crear() {
        await ReactDOM.unmountComponentAtNode(this.refs.renderModal);
        ReactDOM.render(<AlmacenForm
            handleAdd={this.handleAdd}
        />, this.refs.renderModal);
    }

    async editar() {
        if (this.almacen == null) {
            return;
        }
        await ReactDOM.unmountComponentAtNode(this.refs.renderModal);
        ReactDOM.render(<AlmacenForm
            almacen={this.almacen}
            handleEdit={this.handleEdit}
            handleDelete={this.handleDelete}

            getAlmacenNotificaciones={this.getAlmacenNotificaciones}
            addAlmacenNotificaciones={this.addAlmacenNotificaciones}
            deleteAlmacenNotificaciones={this.deleteAlmacenNotificaciones}
            getArticulos={this.getArticulos}

            getAlmacenStorageCells={this.getAlmacenStorageCells}
            addAlmacenStorageCell={this.addAlmacenStorageCell}
            deleteAlmacenStorageCell={this.deleteAlmacenStorageCell}
        />, this.refs.renderModal);
    };

    async borrar() {
        await ReactDOM.unmountComponentAtNode(this.refs.renderModal);
        ReactDOM.render(<AlmacenFormDeleteConfirm
            almacenName={this.almacen.name}
            almacenId={this.almacen.id}
            handleEliminar={() => {
                this.handleDelete(this.almacen.id);
            }}
        />, this.refs.renderModal);
    }

    async craft() {
        if (this.almacen == null) {
            return;
        }
        await ReactDOM.unmountComponentAtNode(this.refs.renderModal);
        ReactDOM.render(<AlmacenCrafteo
            idAlmacen={this.almacen.id}
            // FABRICACION
            getFabricaciones={this.getFabricaciones}
            addFabricacion={this.addFabricacion}
            editFabricacion={this.editFabricacion}
            deleteFabricacion={this.deleteFabricacion}
            // ORDENES DE FABRICACION
            localizarCrafteos={this.localizarCrafteos}
            localizarSmeltings={this.localizarSmeltings}
            addOrdenFabricacion={this.addOrdenFabricacion}
            getOrdenFabricacion={this.getOrdenFabricacion}
            searchOrdenesFabricacion={this.searchOrdenesFabricacion}
            previewCrafteo={this.previewCrafteo}
            deleteOrdenFabricacion={this.deleteOrdenFabricacion}
        />, this.refs.renderModal);
    }

    render() {
        return <div id="tabAlmacenes">
            <div ref="renderModal" id="renderModalAlmacen" />
            <div id="renderAlmacenModalAlert" />
            <h3><img src={inventarioIco} />Almac&eacute;n</h3>
            <div className="form-row" id="almColumnas">
                <div className="col">
                    <button type="button" className="btn btn-primary" onClick={this.crear}>Crear</button>
                    <button type="button" className="btn btn-success" onClick={this.editar}>Editar</button>
                    <button type="button" className="btn btn-danger" onClick={this.borrar}>Borrar</button>
                    <button type="button" class="btn btn-warning" onClick={this.craft}>Crafteo</button>
                    <table className="table table-dark">
                        <thead>
                            <tr>
                                <th scope="col">#</th>
                                <th scope="col">Nombre</th>
                                <th scope="col">%</th>
                            </tr>
                        </thead>
                        <tbody ref="renderAlmacenes"></tbody>
                    </table>
                </div>
                <div className="col">
                    <div className="row row-cols-1 row-cols-md-4" ref="renderSlots" id="renderSlotsAlmacen">
                    </div>
                </div>
            </div>
        </div>
    }
};

class Almacen extends Component {
    constructor({ id, name, ocupado, almacenamiento, selectAlmacen }) {
        super();

        this.id = id;
        this.name = name;
        this.ocupado = ocupado;
        this.almacenamiento = almacenamiento;
        this.selectAlmacen = selectAlmacen;

        this.select = this.select.bind(this);
    }

    select() {
        this.selectAlmacen(this.id);
    }

    render() {
        return <tr onClick={this.select}>
            <th scope="row">{this.id}</th>
            <td>{this.name}</td>
            <td>{this.ocupado}%</td>
            <td><img src={almacenTipoImg[this.almacenamiento]} /></td>
        </tr>
    }
};

class AlmacenForm extends Component {
    constructor({ almacen, handleAdd, handleEdit, handleDelete, getAlmacenNotificaciones, addAlmacenNotificaciones, deleteAlmacenNotificaciones, getArticulos,
        getAlmacenStorageCells, addAlmacenStorageCell, deleteAlmacenStorageCell }) {
        super();

        this.almacen = almacen;

        this.handleAdd = handleAdd;
        this.handleEdit = handleEdit;
        this.handleDelete = handleDelete;

        this.getAlmacenNotificaciones = getAlmacenNotificaciones;
        this.addAlmacenNotificaciones = addAlmacenNotificaciones;
        this.deleteAlmacenNotificaciones = deleteAlmacenNotificaciones;
        this.getArticulos = getArticulos;

        this.getAlmacenStorageCells = getAlmacenStorageCells;
        this.addAlmacenStorageCell = addAlmacenStorageCell;
        this.deleteAlmacenStorageCell = deleteAlmacenStorageCell;

        this.aceptar = this.aceptar.bind(this);
        this.notificaciones = this.notificaciones.bind(this);
        this.setAlmacenImg = this.setAlmacenImg.bind(this);
        this.storageCells = this.storageCells.bind(this);
    }

    componentDidMount() {
        window.$('#almacenModal').modal({ show: true });
    }

    componentWillUnmount() {
        window.$('#almacenModal').modal('hide');
    }

    showAlert(txt) {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderAlmacenModalAlert'));
        ReactDOM.render(<FormAlert
            txt={txt}
        />, document.getElementById('renderAlmacenModalAlert'));
    }

    isValidUUID(uuid) {
        return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(uuid);
    }

    async aceptar() {
        const almacen = {};
        almacen.name = this.refs.name.value;
        almacen.uuid = this.refs.uuid.value;
        almacen.descripcion = this.refs.descripcion.value;
        almacen.off = this.refs.off.checked;
        almacen.almacenamiento = this.refs.tipo.value;
        almacen.maximoStacks = parseInt(this.refs.maximoStacks.value);
        almacen.maximoTipos = parseInt(this.refs.maximoTipos.value);
        almacen.maximoItems = parseInt(this.refs.maximoItems.value);

        if (almacen.name == null || almacen.name.length == 0) {
            this.showAlert("El nombre no puede estar vacio.");
            return;
        }
        if (almacen.uuid == null || almacen.uuid.length == 0 || !this.isValidUUID(almacen.uuid)) {
            this.showAlert("Debes escribir un UUID valido.");
            return;
        }

        if (this.almacen == null) {
            await this.handleAdd(almacen);
        } else {
            almacen.id = this.almacen.id;
            await this.handleEdit(almacen);
        }
        window.$('#almacenModal').modal('hide');
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

    async notificaciones() {
        await ReactDOM.unmountComponentAtNode(document.getElementById("renderModalAlmacen"));
        ReactDOM.render(<AlmacenNotificaciones
            getAlmacenNotificaciones={() => {
                return this.getAlmacenNotificaciones(this.almacen.id);
            }}
            addAlmacenNotificaciones={(notificacion) => {
                notificacion.idAlmacen = this.almacen.id;
                return this.addAlmacenNotificaciones(notificacion);
            }}
            deleteAlmacenNotificaciones={(id) => {
                return this.deleteAlmacenNotificaciones(this.almacen.id, id);
            }}
            getArticulos={this.getArticulos}
        />, document.getElementById("renderModalAlmacen"));
    }

    async storageCells() {
        await ReactDOM.unmountComponentAtNode(document.getElementById("renderModalAlmacen"));
        ReactDOM.render(<AlmacenStorageCells
            idAlmacen={this.almacen.id}
            getStorageCells={this.getAlmacenStorageCells}
            addStorageCell={this.addAlmacenStorageCell}
            deleteStorageCell={this.deleteAlmacenStorageCell}
        />, document.getElementById("renderModalAlmacen"));
    }

    setAlmacenImg() {
        this.refs.tipoImg.src = almacenTipoImg[(this.refs.tipo.value)];

        switch (this.refs.tipo.value) {
            case "C": {
                this.refs.maximoStacks.value = 27;
                this.refs.maximoTipos.value = 0;
                this.refs.maximoItems.value = 0;
                break;
            }
            case "D": {
                this.refs.maximoStacks.value = 54;
                this.refs.maximoTipos.value = 0;
                this.refs.maximoItems.value = 0;
                break;
            }
            case "M": {

            }
            case "O": {
                this.refs.maximoStacks.value = 0;
                this.refs.maximoTipos.value = 0;
                this.refs.maximoItems.value = 0;
                break;
            }
        }
    }

    render() {
        return <div className="modal fade" id="almacenModal" tabIndex="-1" role="dialog" aria-labelledby="almacenModalLabel" aria-hidden="true">
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="almacenModalLabel">Almac&eacute;n</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <label>Nombre</label>
                        <input type="text" className="form-control" placeholder="Nombre" ref="name" defaultValue={this.almacen != null ? this.almacen.name : ''} />

                        <label>UUID del controlador</label>
                        <input type="text" className="form-control" placeholder="UUID" ref="uuid" defaultValue={this.almacen != null ? this.almacen.uuid : ''} />

                        <label>Descripci&oacute;n</label>
                        <textarea className="form-control" ref="descripcion" rows="5" defaultValue={this.almacen != null ? this.almacen.descripcion : ''}></textarea>

                        <div className="form-row">
                            <div className="col">
                                <label>Tipos</label>
                                <input type="number" className="form-control" defaultValue={this.almacen != null ? this.almacen.tipos : ''} readOnly={true} />
                            </div>
                            <div className="col">
                                <label>Stacks</label>
                                <input type="number" className="form-control" defaultValue={this.almacen != null ? this.almacen.stacks : ''} readOnly={true} />
                            </div>
                            <div className="col">
                                <label>&Iacute;tems</label>
                                <input type="number" className="form-control" defaultValue={this.almacen != null ? this.almacen.items : ''} readOnly={true} />
                            </div>
                        </div>

                        <div className="form-row" id="tipoAlmacenModal">
                            <div className="col">
                                <img src={almacenTipoImg[(this.almacen != null ? this.almacen.almacenamiento : 'C')]} ref="tipoImg" />
                            </div>
                            <div className="col">
                                <select name="tipo" ref="tipo" className="form-control" defaultValue={this.almacen != null ? this.almacen.almacenamiento : 'C'} onChange={this.setAlmacenImg}>
                                    <option value="C">Cofre</option>
                                    <option value="D">Cofre doble</option>
                                    <option value="M">Almacenamiento ME</option>
                                    <option value="O">Otro</option>
                                </select>
                            </div>
                            <div className="col">
                                <button type="button" className="btn btn-success" onClick={this.storageCells}>AE2</button>
                            </div>
                        </div>

                        <div className="form-row">
                            <div className="col">
                                <label>M&aacute;ximo de stacks</label>
                                <input type="number" className="form-control" ref="maximoStacks" defaultValue={this.almacen != null ? this.almacen.maximoStacks : '0'} />
                            </div>
                            <div className="col">
                                <label>M&aacute;ximo de tipos</label>
                                <input type="number" className="form-control" ref="maximoTipos" defaultValue={this.almacen != null ? this.almacen.maximoTipos : '0'} />
                            </div>
                            <div className="col">
                                <label>M&aacute;ximo de &iacute;tems</label>
                                <input type="number" className="form-control" ref="maximoItems" defaultValue={this.almacen != null ? this.almacen.maximoItems : '0'} />
                            </div>
                        </div>

                        <input type="checkbox" defaultChecked={this.almacen != null && this.almacen.off} ref="off" />
                        <label>&iquest;Desactivado?</label>

                        <div className="form-row">
                            <div className="col">
                                <label>Fecha de creaci&oacute;n</label>
                                <input type="text" className="form-control" defaultValue={this.almacen != null ? this.formatearFechaTiempo(this.almacen.dateAdd) : ''} readOnly={true} />
                            </div>
                            <div className="col">
                                <label>Fecha de &uacute;ltima actualizaci&oacute;n del inventario</label>
                                <input type="text" className="form-control" defaultValue={this.almacen != null ? this.formatearFechaTiempo(this.almacen.dateLastUpdate) : ''} readOnly={true} />
                            </div>
                        </div>

                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-primary" onClick={this.aceptar}>Aceptar</button>
                        <button type="button" className="btn btn-success" onClick={this.notificaciones}>Notificaciones</button>
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class AlmacenFormDeleteConfirm extends Component {
    constructor({ almacenName, almacenId, handleEliminar }) {
        super();

        this.almacenName = almacenName;
        this.almacenId = almacenId;

        this.handleEliminar = handleEliminar;

        this.eliminar = this.eliminar.bind(this);
    }

    componentDidMount() {
        window.$('#almacenFormDeleteConfirm').modal({ show: true });
    }

    eliminar() {
        window.$('#almacenFormDeleteConfirm').modal('hide');
        this.handleEliminar();
    }

    render() {
        return <div className="modal fade" id="almacenFormDeleteConfirm" tabIndex="-1" role="dialog" aria-labelledby="almacenFormDeleteConfirmLabel" aria-hidden="true">
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="almacenFormDeleteConfirmLabel">Confirmar eliminaci&oacute;n</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <p>&iquest;Est&aacute;s seguro de que quieres eliminar el almac&eacute;n {this.almacenName}#{this.almacenId}?</p>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                        <button type="button" className="btn btn-danger" onClick={this.eliminar}>Eliminar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class AlmacenesFormInventarioSlot extends Component {
    constructor({ cant, cantDisp, articulo, img }) {
        super();

        this.cant = cant;
        this.cantDisp = cantDisp;
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
                        <h6 className="card-subtitle mb-2 text-muted">Cantidad: {this.articulo == null ? '' : this.cant}</h6>
                        <h6 className="card-subtitle mb-2 text-muted">Disponible: {this.articulo == null ? '' : this.cantDisp}</h6>
                    </div>
                </div>
            </div>
        </div>;
    }
};

class AlmacenNotificaciones extends Component {
    constructor({ getAlmacenNotificaciones, addAlmacenNotificaciones, deleteAlmacenNotificaciones, getArticulos }) {
        super();

        this.getAlmacenNotificaciones = getAlmacenNotificaciones;
        this.addAlmacenNotificaciones = addAlmacenNotificaciones;
        this.deleteAlmacenNotificaciones = deleteAlmacenNotificaciones;
        this.getArticulos = getArticulos;

        this.localizarArticulo = this.localizarArticulo.bind(this);
        this.agregar = this.agregar.bind(this);
        this.borrar = this.borrar.bind(this);
    }

    componentDidMount() {
        window.$('#almNotificacionesModal').modal({ show: true });
        this.renderNotificaciones();
    }

    showAlert(txt) {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderAlmacenModalAlert'));
        ReactDOM.render(<FormAlert
            txt={txt}
        />, document.getElementById('renderAlmacenModalAlert'));
    }

    getModo(modo) {
        if (modo == '+') {
            return ">=";
        } else if (modo == '-') {
            return "<=";
        } else {
            return modo;
        }
    }

    async renderNotificaciones() {
        const notificaciones = await this.getAlmacenNotificaciones();
        ReactDOM.render(notificaciones.map((element, i) => {
            return <tr key={i}>
                <th scope="row">{element.id}</th>
                <td>{element.name}</td>
                <td>{element.idArticulo}</td>
                <td>{this.getModo(element.modo)}</td>
                <td>{element.cantidad}</td>
                <td><img src={deleteIco} onClick={() => {
                    this.borrar(element.id);
                }} /></td>
            </tr>
        }), this.refs.renderNotificacionesAlmacen);
    }

    localizarArticulo() {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderAlmacenModalAlert"));
        ReactDOM.render(<ArticuloLocalizador
            getArticulos={this.getArticulos}
            handleSelect={(id, name) => {
                this.refs.artId.value = id;
                this.refs.artName.value = name;
            }}
        />, document.getElementById("renderAlmacenModalAlert"));
    }

    agregar() {
        const notificacion = {};
        notificacion.name = this.refs.name.value;
        notificacion.idArticulo = parseInt(this.refs.artId.value);
        notificacion.modo = this.refs.modo.value;
        notificacion.cantidad = parseInt(this.refs.cantidad.value);

        this.addAlmacenNotificaciones(notificacion).then(() => {
            this.renderNotificaciones();
        }, () => {
            this.showAlert("No se ha podido añadir la notificacion. Comprueba que el nombre no está vacío y que se ha seleccionado un artículo.");
        });
    }

    borrar(id) {
        this.deleteAlmacenNotificaciones(id).then(() => {
            this.renderNotificaciones();
        }, () => {
            this.showAlert("El servidor ha devuelto un error al eliminar. Refresca y vuelve a intentarlo.");
        });

    }

    render() {
        return <div class="modal fade bd-example-modal-xl" tabindex="-1" role="dialog" aria-labelledby="almNotificacionesModalLabel" id="almNotificacionesModal" aria-hidden="true">
            <div class="modal-dialog modal-xl" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="almNotificacionesModalLabel">Notificaciones</h5>
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
                                    <th scope="col">Art&iacute;culo</th>
                                    <th scope="col">Operador</th>
                                    <th scope="col">Cantidad</th>
                                    <th scope="col"></th>
                                </tr>
                            </thead>
                            <tbody ref="renderNotificacionesAlmacen">
                            </tbody>
                        </table>

                        <div className="form-row">
                            <div className="col">
                                <label>Nombre</label>
                                <input type="text" className="form-control" ref="name" />
                            </div>
                            <div className="col">
                                <div className="localizador">
                                    <label>Art&iacute;culo</label>
                                    <br />
                                    <img src={flashlightIco} onClick={this.localizarArticulo} />
                                    <input type="number" ref="artId" className="form-control" readOnly={true} defaultValue={0} />
                                    <input type="text" ref="artName" className="form-control" readOnly={true} />
                                </div>
                            </div>
                            <div className="col">
                                <label for="operadorSelect">Operador</label>
                                <select class="form-control" id="operadorSelect" ref="modo">
                                    <option value="<">&lt;</option>
                                    <option value=">">></option>
                                    <option value="=">=</option>
                                    <option value="-">&lt;=</option>
                                    <option value="+">>=</option>
                                </select>
                            </div>
                            <div className="col">
                                <label>Cantidad</label>
                                <input type="number" className="form-control" defaultValue={0} ref="cantidad" min="0" />
                            </div>
                            <div className="col">
                                <button type="button" class="btn btn-primary" onClick={this.agregar}>A&ntilde;adir</button>
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

class AlmacenStorageCells extends Component {
    constructor({ idAlmacen, getStorageCells, addStorageCell, deleteStorageCell }) {
        super();

        this.idAlmacen = idAlmacen;
        this.getStorageCells = getStorageCells;
        this.addStorageCell = addStorageCell;
        this.deleteStorageCell = deleteStorageCell;

        this.addStorageCells = this.addStorageCells.bind(this);
        this.deleteStorageCells = this.deleteStorageCells.bind(this);
    }

    componentDidMount() {
        window.$('#almStorageCells').modal({ show: true });
        this.renderStorageCells();
    }

    async renderStorageCells() {
        const storageCells = await this.getStorageCells(this.idAlmacen);
        ReactDOM.unmountComponentAtNode(this.refs.cells);
        ReactDOM.render(storageCells.map((element, i) => {
            return <tr>
                <th scope="col">{element.id}</th>
                <td><img src={storageCellImg[element.tier]} /></td>
                <td>{storageCell[element.tier]}</td>
                <td>{this.formatearFechaTiempo(element.dateAdd)}</td>
                <td><img src={deleteIco} onClick={() => {
                    this.deleteStorageCells(element.id);
                }} /></td>
            </tr>
        }), this.refs.cells);
    }

    async addStorageCells() {
        const storageCell = {};
        storageCell.idAlmacen = this.idAlmacen;
        storageCell.tier = parseInt(this.refs.tipo.value);

        await this.addStorageCell(storageCell);
        this.renderStorageCells();
    }

    async deleteStorageCells(id) {
        await this.deleteStorageCell(this.idAlmacen, id);
        this.renderStorageCells();
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

    render() {
        return <div class="modal fade bd-example-modal-lg" id="almStorageCells" tabindex="-1" role="dialog" aria-labelledby="almStorageCellsLabel" aria-hidden="true">
            <div class="modal-dialog modal-lg" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="almStorageCellsLabel">AE2 Storage Cells</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <table class="table table-dark">
                            <thead>
                                <tr>
                                    <th scope="col">#</th>
                                    <th scope="col"></th>
                                    <th scope="col">Tier</th>
                                    <th scope="col">Fecha de creaci&oacute;n</th>
                                    <th scope="col">Borrar</th>
                                </tr>
                            </thead>
                            <tbody ref="cells">
                            </tbody>
                        </table>
                        <div className="form-row">
                            <div className="col">
                            </div>
                            <div className="col">
                                <select name="tipo" ref="tipo" className="form-control">
                                    <option value="1">1k ME Storage Cell</option>
                                    <option value="2">4k ME Storage Cell</option>
                                    <option value="3">16k ME Storage Cell</option>
                                    <option value="4">64k ME Storage Cell</option>
                                </select>
                            </div>
                            <div className="col">
                                <button type="button" class="btn btn-primary" onClick={this.addStorageCells}>Crear</button>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-dismiss="modal">OK</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

export default Almacenes;


