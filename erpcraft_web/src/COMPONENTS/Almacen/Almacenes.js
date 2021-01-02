import { Component } from "react";
import ReactDOM from 'react-dom';

import './../../CSS/Almacen.css';

import inventarioIco from './../../IMG/inventario.png';

class Almacenes extends Component {
    constructor({ getAlmacenes, getAlmacenInventario, almacenInventarioPush, getArticuloImg, tabAlmacenPush, handleAdd, handleEdit, handleDelete }) {
        super();
        this.almacen = null;

        this.getAlmacenes = getAlmacenes;
        this.getAlmacenInventario = getAlmacenInventario;
        this.almacenInventarioPush = almacenInventarioPush;
        this.getArticuloImg = getArticuloImg;
        this.tabAlmacenPush = tabAlmacenPush;

        this.handleAdd = handleAdd;
        this.handleEdit = handleEdit;
        this.handleDelete = handleDelete;

        this.getInventario = this.getInventario.bind(this);
        this.crear = this.crear.bind(this);
        this.editar = this.editar.bind(this);
        this.borrar = this.borrar.bind(this);
    }

    componentDidMount() {
        this.renderAlmacenes();
    }

    async renderAlmacenes() {
        const almacenes = await this.getAlmacenes();

        await ReactDOM.unmountComponentAtNode(this.refs.renderAlmacenes);
        ReactDOM.render(almacenes.map((element, i) => {
            return <Almacen
                key={i}

                id={element.id}
                name={element.name}
                selectAlmacen={(idAlmacen) => {
                    this.almacen = element;
                    this.getInventario(idAlmacen);
                }}
            />
        }), this.refs.renderAlmacenes);

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

            await ReactDOM.unmountComponentAtNode(this.refs.renderAlmacenes);
            ReactDOM.render(almacenes.map((element, i) => {
                return <Almacen
                    key={i}

                    id={element.id}
                    name={element.name}
                    selectAlmacen={(idAlmacen) => {
                        this.almacen = element;
                        this.getInventario(idAlmacen);
                    }}
                />
            }), this.refs.renderAlmacenes);
        });
    };

    async getInventario(idAlmacen) {
        const inventario = await this.getAlmacenInventario(idAlmacen);

        this.renderInventario(inventario);

        this.almacenInventarioPush(idAlmacen, (inventario) => {
            this.renderInventario(inventario);
        });

    };

    async renderInventario(inventario) {
        await ReactDOM.unmountComponentAtNode(this.refs.renderSlots);
        ReactDOM.render(inventario.map((element, i) => {
            return <AlmacenesFormInventarioSlot
                key={i}

                cant={element.cantidad}
                articulo={element.articulo}
            />
        }), this.refs.renderSlots);

        for (let i = 0; i < inventario.length; i++) {
            const articulo = inventario[i].articulo;

            if (articulo != null && articulo.id != 0) {
                const img = await this.getArticuloImg(articulo.id);
                inventario[i].img = img;
            }

            ReactDOM.unmountComponentAtNode(this.refs.renderSlots);
            ReactDOM.render(inventario.map((element, i) => {
                return <AlmacenesFormInventarioSlot
                    key={i}

                    cant={element.cantidad}
                    articulo={element.articulo}
                    img={element.img}
                />
            }), this.refs.renderSlots);
        }
    };

    async crear() {
        await ReactDOM.unmountComponentAtNode(this.refs.renderModal);
        ReactDOM.render(<AlmacenForm
            handleAdd={this.handleAdd}
        />, this.refs.renderModal);
    }

    async editar() {
        await ReactDOM.unmountComponentAtNode(this.refs.renderModal);
        ReactDOM.render(<AlmacenForm
            almacen={this.almacen}
            handleEdit={this.handleEdit}
            handleDelete={this.handleDelete}
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

    render() {
        return <div id="tabAlmacenes">
            <div ref="renderModal" />
            <h3><img src={inventarioIco} />Almac&eacute;n</h3>
            <div className="form-row" id="almColumnas">
                <div className="col">
                    <button type="button" className="btn btn-primary" onClick={this.crear}>Crear</button>
                    <button type="button" className="btn btn-success" onClick={this.editar}>Editar</button>
                    <button type="button" className="btn btn-danger" onClick={this.borrar}>Borrar</button>
                    <table className="table table-dark">
                        <thead>
                            <tr>
                                <th scope="col">#</th>
                                <th scope="col">Nombre</th>
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
    constructor({ id, name, selectAlmacen }) {
        super();

        this.id = id;
        this.name = name;
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
        </tr>
    }
};

class AlmacenForm extends Component {
    constructor({ almacen, handleAdd, handleEdit, handleDelete }) {
        super();

        this.almacen = almacen;

        this.handleAdd = handleAdd;
        this.handleEdit = handleEdit;
        this.handleDelete = handleDelete;

        this.aceptar = this.aceptar.bind(this);
    }

    componentDidMount() {
        window.$('#almacenModal').modal({ show: true });
    }

    async aceptar() {
        const almacen = {};
        almacen.name = this.refs.name.value;
        almacen.uuid = this.refs.uuid.value;
        almacen.descripcion = this.refs.descripcion.value;
        almacen.off = this.refs.off.checked;

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
                                <label>Slots usados</label>
                                <input type="number" className="form-control" defaultValue={this.almacen != null ? this.almacen.slots : ''} readOnly={true} />
                            </div>
                            <div className="col">
                                <label>&Iacute;tems usados</label>
                                <input type="number" className="form-control" defaultValue={this.almacen != null ? this.almacen.items : ''} readOnly={true} />
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
    constructor({ cant, articulo, img }) {
        super();

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

export default Almacenes;


