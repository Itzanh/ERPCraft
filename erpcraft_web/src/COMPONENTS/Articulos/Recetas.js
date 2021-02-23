import { Component } from "react";
import ReactDOM from 'react-dom';

import deleteIco from './../../IMG/delete.svg';

import ArticuloLocalizador from "./ArticuloLocalizador";

class Recetas extends Component {
    constructor({ getArticulos, getArticuloName, getArticuloImg, getCrafting, addCrafting, updateCrafting, deleteCrafting,
        getSmelting, addSmelting, updateSmelting, deleteSmelting }) {
        super();

        this.getArticulos = getArticulos;
        this.getArticuloName = getArticuloName;
        this.getArticuloImg = getArticuloImg;

        this.getCrafting = getCrafting;
        this.addCrafting = addCrafting;
        this.updateCrafting = updateCrafting;
        this.deleteCrafting = deleteCrafting;

        this.getSmelting = getSmelting;
        this.addSmelting = addSmelting;
        this.updateSmelting = updateSmelting;
        this.deleteSmelting = deleteSmelting;

        this.renderCrafteos = this.renderCrafteos.bind(this);
        this.renderSmelting = this.renderSmelting.bind(this);
    }

    componentDidMount() {
        window.$('#recetasModal').modal({ show: true });
        this.renderMenu(0);
        this.renderCrafteos();
    }

    renderMenu(pos = 0) {
        ReactDOM.render(<ul class="nav nav-tabs">
            <li class="nav-item">
                <a class={pos == 0 ? "nav-link active" : "nav-link"} onClick={this.renderCrafteos}>Crafteos</a>
            </li>
            <li class="nav-item">
                <a class={pos == 1 ? "nav-link active" : "nav-link"} onClick={this.renderSmelting}>Smelting</a>
            </li>
        </ul>, this.refs.renderMenu);
    }

    renderCrafteos() {
        this.renderMenu(0);
        ReactDOM.unmountComponentAtNode(this.refs.renderContenido);
        ReactDOM.render(<Crafteos
            getArticulos={this.getArticulos}
            getArticuloName={this.getArticuloName}
            getArticuloImg={this.getArticuloImg}
            getCrafting={this.getCrafting}
            addCrafting={this.addCrafting}
            updateCrafting={this.updateCrafting}
            deleteCrafting={this.deleteCrafting}
        />, this.refs.renderContenido);
    }

    renderSmelting() {
        this.renderMenu(1);
        ReactDOM.unmountComponentAtNode(this.refs.renderContenido);
        ReactDOM.render(<Smeltings
            getArticulos={this.getArticulos}
            getArticuloName={this.getArticuloName}
            getArticuloImg={this.getArticuloImg}
            getSmelting={this.getSmelting}
            addSmelting={this.addSmelting}
            updateSmelting={this.updateSmelting}
            deleteSmelting={this.deleteSmelting}
        />, this.refs.renderContenido);
    }

    render() {
        return <div class="modal fade bd-example-modal-xl" id="recetasModal" tabindex="-1" role="dialog" aria-labelledby="recetasModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-xl" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="recetasModalLabel">Recetas</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <div ref="renderMenu"></div>
                        <div ref="renderContenido" id="renderContenido"></div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-dismiss="modal">Aceptar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class Crafteos extends Component {
    constructor({ getArticulos, getArticuloName, getArticuloImg, getCrafting, addCrafting, updateCrafting, deleteCrafting }) {
        super();

        this.getArticulos = getArticulos;
        this.getArticuloName = getArticuloName;
        this.getArticuloImg = getArticuloImg;
        this.getCrafting = getCrafting;
        this.addCrafting = addCrafting;
        this.updateCrafting = updateCrafting;
        this.deleteCrafting = deleteCrafting;

        this.nuevo = this.nuevo.bind(this);
        this.editar = this.editar.bind(this);
        this.abrir = this.abrir.bind(this);
        this.eliminar = this.eliminar.bind(this);
    }

    componentDidMount() {
        this.renderCrafting();
    }

    async renderCrafting() {
        const limit = 50;
        var offset = 0;
        var continuar = true;
        var crafteos = [];
        do {
            const lista = await this.getCrafting(offset, limit);
            continuar = lista.length > 0;
            offset += lista.length;
            crafteos = crafteos.concat(lista);
        } while (continuar);

        const nameCache = {};
        const imgCache = {};

        for (let i = 0; i < crafteos.length; i++) {
            if (nameCache[crafteos[i].idArticuloResultado] != null) {
                crafteos[i].artName = nameCache[crafteos[i].idArticuloResultado];
            } else {
                const name = await this.getArticuloName(crafteos[i].idArticuloResultado);
                crafteos[i].artName = name;
                nameCache[crafteos[i].idArticuloResultado] = name;
            }
        }

        for (let i = 0; i < crafteos.length; i++) {
            if (imgCache[crafteos[i].idArticuloResultado] != null) {
                crafteos[i].artImg = imgCache[crafteos[i].idArticuloResultado];
            } else {
                const img = await this.getArticuloImg(crafteos[i].idArticuloResultado);
                crafteos[i].artImg = img;
                imgCache[crafteos[i].idArticuloResultado] = img;
            }
        }

        ReactDOM.render(crafteos.map((element, i) => {
            return <tr key={i} onClick={() => { this.editar(element); }}>
                <th scope="row">{element.id}</th>
                <td>{element.name}</td>
                <td><img src={element.artImg} /></td>
                <td>{element.artName}</td>
                <td>{element.cantidadResultado}</td>
                <td
                    onClick={(e) => {
                        e.stopPropagation();
                        this.eliminar(element.id);
                    }}
                ><img src={deleteIco}
                    /></td>
            </tr>
        }), this.refs.crafteos);
    }

    nuevo() {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderContenido"));
        ReactDOM.render(<AddCrafteo
            getArticulos={this.getArticulos}
            getArticuloImg={this.getArticuloImg}
            abrir={this.abrir}
            addCrafting={this.addCrafting}
        />, document.getElementById("renderContenido"));
    }

    editar(crafteo) {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderContenido"));
        ReactDOM.render(<EditCrafteo
            crafteo={crafteo}
            getArticulos={this.getArticulos}
            getArticuloImg={this.getArticuloImg}
            abrir={this.abrir}
            updateCrafting={this.updateCrafting}
        />, document.getElementById("renderContenido"));
    }

    async eliminar(idCrafting) {
        await this.deleteCrafting(idCrafting);
        this.renderCrafting();
    }

    abrir() {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderContenido"));
        ReactDOM.render(<Crafteos
            getArticulos={this.getArticulos}
            getArticuloName={this.getArticuloName}
            getArticuloImg={this.getArticuloImg}
            getCrafting={this.getCrafting}
            addCrafting={this.addCrafting}
            updateCrafting={this.updateCrafting}
            deleteCrafting={this.deleteCrafting}
        />, document.getElementById("renderContenido"));
    }

    render() {
        return <div id="crafteosList">
            <button type="button" className="btn btn-primary" onClick={this.nuevo}>Nuevo</button>
            <table class="table table-dark">
                <thead>
                    <tr>
                        <th scope="col">#</th>
                        <th scope="col">Nombre</th>
                        <th scope="col"></th>
                        <th scope="col">Art&iacute;culo resultado</th>
                        <th scope="col">Cantidad</th>
                        <th scope="col"></th>
                    </tr>
                </thead>
                <tbody ref="crafteos"></tbody>
            </table>
        </div>;
    }
};

class AddCrafteo extends Component {
    constructor({ getArticulos, getArticuloImg, abrir, addCrafting }) {
        super();

        this.getArticulos = getArticulos;
        this.getArticuloImg = getArticuloImg;
        this.abrir = abrir;
        this.addCrafting = addCrafting;

        this.crafteo = {};

        this.localizarArticulo = this.localizarArticulo.bind(this);
        this.crearCrafteo = this.crearCrafteo.bind(this);
        this.cancelar = this.cancelar.bind(this);
    }

    localizarArticulo(i) {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderArticulosModalAlert"));
        ReactDOM.render(<ArticuloLocalizador
            getArticulos={this.getArticulos}
            handleSelect={async (id, name) => {
                if (i == 0) {
                    if (id == 0) {
                        this.crafteo.idArticuloResultado = null;
                        this.refs.slot0.src = "";
                        this.refs.name.value = "";
                    } else {
                        this.crafteo.idArticuloResultado = id;
                        this.refs.slot0.src = await this.getArticuloImg(id);
                        this.refs.name.value = name;
                    }
                } else {
                    if (id == 0) {
                        this.crafteo["idArticuloSlot" + i] = null;
                        this.refs["slot" + i].src = "";
                        this.refs["cant" + i].value = "0";
                    } else {
                        this.crafteo["idArticuloSlot" + i] = id;
                        this.refs["slot" + i].src = await this.getArticuloImg(id);
                        this.refs["cant" + i].value = "1";
                    }
                }

            }}
        />, document.getElementById("renderArticulosModalAlert"));
    }

    cancelar() {
        this.abrir();
    }

    crearCrafteo() {
        this.crafteo.name = this.refs.name.value;
        this.crafteo.cantidadResultado = parseInt(this.refs.cant0.value);
        for (let i = 1; i <= 9; i++) {
            this.crafteo["cantidadArticuloSlot" + i] = parseInt(this.refs["cant" + i].value);
        }
        this.addCrafting(this.crafteo).then(() => {
            this.abrir();
        });
    }

    render() {
        return <div>
            <div className="form-row" id="creafteoPanel">
                <div className="col">
                    <table>
                        <tbody>
                            <tr>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(1); }}>
                                        <img ref="slot1" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant1" defaultValue="0" />
                                </td>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(2); }}>
                                        <img ref="slot2" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant2" defaultValue="0" />
                                </td>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(3); }}>
                                        <img ref="slot3" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant3" defaultValue="0" />
                                </td>
                            </tr>
                            <tr>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(4); }}>
                                        <img ref="slot4" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant4" defaultValue="0" />
                                </td>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(5); }}>
                                        <img ref="slot5" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant5" defaultValue="0" />
                                </td>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(6); }}>
                                        <img ref="slot6" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant6" defaultValue="0" />
                                </td>
                            </tr>
                            <tr>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(7); }}>
                                        <img ref="slot7" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant7" defaultValue="0" />
                                </td>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(8); }}>
                                        <img ref="slot8" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant8" defaultValue="0" />
                                </td>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(9); }}>
                                        <img ref="slot9" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant9" defaultValue="0" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div className="col">
                    <label>Nombre</label>
                    <input type="text" className="form-control" placeholder="Nombre" ref="name" />
                    <br />
                    <table>
                        <tbody>
                            <tr>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(0); }}>
                                        <img ref="slot0" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="1" max="64" ref="cant0" defaultValue="1" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    <br />
                    <button type="button" class="btn btn-primary" onClick={this.crearCrafteo}>Crear crafteo</button>
                    <button type="button" class="btn btn-warning" onClick={this.cancelar}>Cancelar</button>
                </div>
            </div>
        </div>;
    }
};

class EditCrafteo extends Component {
    constructor({ crafteo, getArticulos, getArticuloImg, abrir, updateCrafting }) {
        super();

        this.crafteo = crafteo;
        this.getArticulos = getArticulos;
        this.getArticuloImg = getArticuloImg;
        this.abrir = abrir;
        this.updateCrafting = updateCrafting;

        this.localizarArticulo = this.localizarArticulo.bind(this);
        this.editarCrafteo = this.editarCrafteo.bind(this);
        this.cancelar = this.cancelar.bind(this);
    }

    componentDidMount() {
        this.printCrafteo();
    }

    async printCrafteo() {
        const artImg = await this.getArticuloImg(this.crafteo.idArticuloResultado);
        this.refs.slot0.src = artImg;

        for (let i = 1; i <= 9; i++) {
            if (this.crafteo["idArticuloSlot" + i] != null) {
                const artImg = await this.getArticuloImg(this.crafteo["idArticuloSlot" + i]);
                this.refs["slot" + i].src = artImg;
            }
        }
    }

    localizarArticulo(i) {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderArticulosModalAlert"));
        ReactDOM.render(<ArticuloLocalizador
            getArticulos={this.getArticulos}
            handleSelect={async (id, name) => {
                if (i == 0) {
                    if (id == 0) {
                        this.crafteo.idArticuloResultado = null;
                        this.refs.slot0.src = "";
                        this.refs.name.value = "";
                    } else {
                        this.crafteo.idArticuloResultado = id;
                        this.refs.slot0.src = await this.getArticuloImg(id);
                        this.refs.name.value = name;
                    }
                } else {
                    if (id == 0) {
                        this.crafteo["idArticuloSlot" + i] = null;
                        this.refs["slot" + i].src = "";
                        this.refs["cant" + i].value = "0";
                    } else {
                        this.crafteo["idArticuloSlot" + i] = id;
                        this.refs["slot" + i].src = await this.getArticuloImg(id);
                        this.refs["cant" + i].value = "1";
                    }
                }

            }}
        />, document.getElementById("renderArticulosModalAlert"));
    }

    cancelar() {
        this.abrir();
    }

    editarCrafteo() {
        this.crafteo.name = this.refs.name.value;
        this.crafteo.cantidadResultado = parseInt(this.refs.cant0.value);
        for (let i = 1; i <= 9; i++) {
            this.crafteo["cantidadArticuloSlot" + i] = parseInt(this.refs["cant" + i].value);
        }
        this.crafteo.off = this.refs.off.checked;
        this.updateCrafting(this.crafteo).then(() => {
            this.abrir();
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

    render() {
        return <div>
            <div className="form-row" id="creafteoPanel">
                <div className="col">
                    <table>
                        <tbody>
                            <tr>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(1); }}>
                                        <img ref="slot1" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant1" defaultValue={this.crafteo.cantidadArticuloSlot1} />
                                </td>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(2); }}>
                                        <img ref="slot2" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant2" defaultValue={this.crafteo.cantidadArticuloSlot2} />
                                </td>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(3); }}>
                                        <img ref="slot3" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant3" defaultValue={this.crafteo.cantidadArticuloSlot3} />
                                </td>
                            </tr>
                            <tr>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(4); }}>
                                        <img ref="slot4" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant4" defaultValue={this.crafteo.cantidadArticuloSlot4} />
                                </td>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(5); }}>
                                        <img ref="slot5" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant5" defaultValue={this.crafteo.cantidadArticuloSlot5} />
                                </td>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(6); }}>
                                        <img ref="slot6" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant6" defaultValue={this.crafteo.cantidadArticuloSlot6} />
                                </td>
                            </tr>
                            <tr>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(7); }}>
                                        <img ref="slot7" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant7" defaultValue={this.crafteo.cantidadArticuloSlot7} />
                                </td>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(8); }}>
                                        <img ref="slot8" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant8" defaultValue={this.crafteo.cantidadArticuloSlot8} />
                                </td>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(9); }}>
                                        <img ref="slot9" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant9" defaultValue={this.crafteo.cantidadArticuloSlot9} />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div className="col">
                    <label>Nombre</label>
                    <input type="text" className="form-control" placeholder="Nombre" ref="name" defaultValue={this.crafteo.name} />
                    <br />
                    <table>
                        <tbody>
                            <tr>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(0); }}>
                                        <img ref="slot0" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="1" max="64" ref="cant0" defaultValue={this.crafteo.cantidadResultado} />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    <br />
                    <button type="button" class="btn btn-primary" onClick={this.editarCrafteo}>Guardar crafteo</button>
                    <button type="button" class="btn btn-warning" onClick={this.cancelar}>Cancelar</button>
                    <br />
                    <div className="form-row">
                        <div className="col">
                            <p>Creado {this.formatearFechaTiempo(this.crafteo.dateAdd)}</p>
                        </div>
                        <div className="col">
                            <p>Modificado {this.formatearFechaTiempo(this.crafteo.dateUpdate)}</p>
                        </div>
                        <div className="col">
                            <input type="checkbox" defaultChecked={this.crafteo.off} ref="off" />
                            <label>&iquest;Desactivado?</label>
                        </div>
                    </div>
                </div>
            </div>
        </div>;
    }
};

class Smeltings extends Component {
    constructor({ getArticulos, getArticuloName, getArticuloImg, getSmelting, addSmelting, updateSmelting, deleteSmelting }) {
        super();

        this.getArticulos = getArticulos;
        this.getArticuloName = getArticuloName;
        this.getArticuloImg = getArticuloImg;

        this.getSmelting = getSmelting;
        this.addSmelting = addSmelting;
        this.updateSmelting = updateSmelting;
        this.deleteSmelting = deleteSmelting;

        this.nuevo = this.nuevo.bind(this);
        this.abrir = this.abrir.bind(this);
    }

    componentDidMount() {
        this.renderSmelting();
    }

    async renderSmelting() {
        const smelting = await this.getSmelting();

        const nameCache = {};
        const imgCache = {};

        for (let i = 0; i < smelting.length; i++) {
            if (nameCache[smelting[i].idArticuloResultado] != null) {
                smelting[i].artName = nameCache[smelting[i].idArticuloResultado];
            } else {
                const name = await this.getArticuloName(smelting[i].idArticuloResultado);
                smelting[i].artName = name;
                nameCache[smelting[i].idArticuloResultado] = name;
            }
        }

        for (let i = 0; i < smelting.length; i++) {
            if (imgCache[smelting[i].idArticuloResultado] != null) {
                smelting[i].artImg = imgCache[smelting[i].idArticuloResultado];
            } else {
                const img = await this.getArticuloImg(smelting[i].idArticuloResultado);
                smelting[i].artImg = img;
                imgCache[smelting[i].idArticuloResultado] = img;
            }
        }

        ReactDOM.render(smelting.map((element, i) => {
            return <tr key={i} onClick={() => { this.editar(element); }}>
                <th scope="row">{element.id}</th>
                <td>{element.name}</td>
                <td><img src={element.artImg} /></td>
                <td>{element.artName}</td>
                <td>{element.cantidadResultado}</td>
                <td
                    onClick={(e) => {
                        e.stopPropagation();
                        this.eliminar(element.id);
                    }}
                ><img src={deleteIco}
                    /></td>
            </tr>
        }), this.refs.crafteos);
    }

    nuevo() {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderContenido"));
        ReactDOM.render(<AddSmelting
            getArticulos={this.getArticulos}
            getArticuloImg={this.getArticuloImg}
            abrir={this.abrir}
            addSmelting={this.addSmelting}
        />, document.getElementById("renderContenido"));
    }

    editar(smelting) {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderContenido"));
        ReactDOM.render(<EditSmelting
            smelting={smelting}
            getArticulos={this.getArticulos}
            getArticuloImg={this.getArticuloImg}
            abrir={this.abrir}
            updateSmelting={this.updateSmelting}
        />, document.getElementById("renderContenido"));
    }

    async eliminar(idSmelting) {
        await this.deleteSmelting(idSmelting);
        this.renderSmelting();
    }

    abrir() {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderContenido"));
        ReactDOM.render(<Smeltings
            getArticulos={this.getArticulos}
            getArticuloName={this.getArticuloName}
            getArticuloImg={this.getArticuloImg}
            getSmelting={this.getSmelting}
            addSmelting={this.addSmelting}
            updateSmelting={this.updateSmelting}
            deleteSmelting={this.deleteSmelting}
        />, document.getElementById("renderContenido"));
    }

    render() {
        return <div id="crafteosList">
            <button type="button" className="btn btn-primary" onClick={this.nuevo}>Nuevo</button>
            <table class="table table-dark">
                <thead>
                    <tr>
                        <th scope="col">#</th>
                        <th scope="col">Nombre</th>
                        <th scope="col"></th>
                        <th scope="col">Art&iacute;culo resultado</th>
                        <th scope="col">Cantidad</th>
                        <th scope="col"></th>
                    </tr>
                </thead>
                <tbody ref="crafteos"></tbody>
            </table>
        </div>;
    }
};

class AddSmelting extends Component {
    constructor({ getArticulos, getArticuloImg, abrir, addSmelting }) {
        super();

        this.getArticulos = getArticulos;
        this.getArticuloImg = getArticuloImg;
        this.abrir = abrir;
        this.addSmelting = addSmelting;

        this.smelting = {};

        this.localizarArticulo = this.localizarArticulo.bind(this);
        this.crearSmelting = this.crearSmelting.bind(this);
        this.cancelar = this.cancelar.bind(this);
    }

    localizarArticulo(i) {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderArticulosModalAlert"));
        ReactDOM.render(<ArticuloLocalizador
            getArticulos={this.getArticulos}
            handleSelect={async (id, name) => {
                if (i == 0) {
                    if (id == 0) {
                        this.smelting.idArticuloResultado = null;
                        this.refs.slot0.src = "";
                        this.refs.name.value = "";
                    } else {
                        this.smelting.idArticuloResultado = id;
                        this.refs.slot0.src = await this.getArticuloImg(id);
                        this.refs.name.value = name;
                    }
                } else if (i == 1) {
                    if (id == 0) {
                        this.smelting.idArticuloEntrada = null;
                        this.refs.slo1.src = "";
                        this.refs.cant1.value = "0";
                    } else {
                        this.smelting.idArticuloEntrada = id;
                        this.refs.slot1.src = await this.getArticuloImg(id);
                        this.refs.cant1.value = "1";
                    }
                }

            }}
        />, document.getElementById("renderArticulosModalAlert"));
    }

    crearSmelting() {
        this.smelting.name = this.refs.name.value;
        this.smelting.cantidadResultado = parseInt(this.refs.cant0.value);
        this.smelting.cantidadEntrada = parseInt(this.refs.cant1.value);
        this.addSmelting(this.smelting).then(() => {
            this.abrir();
        });
    }

    cancelar() {
        this.abrir();
    }

    render() {
        return <div>
            <div className="form-row" id="smeltingPanel">
                <div className="col">
                    <table>
                        <tbody>
                            <tr>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(1); }}>
                                        <img ref="slot1" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="1" max="64" ref="cant1" defaultValue="0" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div className="col">
                    <label>Nombre</label>
                    <input type="text" className="form-control" placeholder="Nombre" ref="name" />
                    <br />
                    <table>
                        <tbody>
                            <tr>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(0); }}>
                                        <img ref="slot0" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="1" max="64" ref="cant0" defaultValue="1" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    <br />
                    <button type="button" class="btn btn-primary" onClick={this.crearSmelting}>Crear horneo</button>
                    <button type="button" class="btn btn-warning" onClick={this.cancelar}>Cancelar</button>
                </div>
            </div>
        </div>
    }
};

class EditSmelting extends Component {
    constructor({ smelting, getArticulos, getArticuloImg, abrir, updateSmelting }) {
        super();

        this.getArticulos = getArticulos;
        this.getArticuloImg = getArticuloImg;
        this.abrir = abrir;
        this.updateSmelting = updateSmelting;

        this.smelting = smelting;

        this.localizarArticulo = this.localizarArticulo.bind(this);
        this.editarSmelting = this.editarSmelting.bind(this);
        this.cancelar = this.cancelar.bind(this);
    }

    componentDidMount() {
        this.printSmelting();
    }

    async printSmelting() {
        var artImg = await this.getArticuloImg(this.smelting.idArticuloResultado);
        this.refs.slot0.src = artImg;
        artImg = await this.getArticuloImg(this.smelting.idArticuloEntrada);
        this.refs.slot1.src = artImg;
    }

    localizarArticulo(i) {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderArticulosModalAlert"));
        ReactDOM.render(<ArticuloLocalizador
            getArticulos={this.getArticulos}
            handleSelect={async (id, name) => {
                if (i == 0) {
                    if (id == 0) {
                        this.smelting.idArticuloResultado = null;
                        this.refs.slot0.src = "";
                        this.refs.name.value = "";
                    } else {
                        this.smelting.idArticuloResultado = id;
                        this.refs.slot0.src = await this.getArticuloImg(id);
                        this.refs.name.value = name;
                    }
                } else if (i == 1) {
                    if (id == 0) {
                        this.smelting.idArticuloEntrada = null;
                        this.refs.slo1.src = "";
                        this.refs.cant1.value = "0";
                    } else {
                        this.smelting.idArticuloEntrada = id;
                        this.refs.slot1.src = await this.getArticuloImg(id);
                        this.refs.cant1.value = "1";
                    }
                }

            }}
        />, document.getElementById("renderArticulosModalAlert"));
    }

    editarSmelting() {
        this.smelting.name = this.refs.name.value;
        this.smelting.cantidadResultado = parseInt(this.refs.cant0.value);
        this.smelting.cantidadEntrada = parseInt(this.refs.cant1.value);
        this.smelting.off = this.refs.off.checked;
        this.updateSmelting(this.smelting).then(() => {
            this.abrir();
        });
    }

    cancelar() {
        this.abrir();
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
        return <div>
            <div className="form-row" id="smeltingPanel">
                <div className="col">
                    <table>
                        <tbody>
                            <tr>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(1); }}>
                                        <img ref="slot1" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="1" max="64" ref="cant1" defaultValue={this.smelting.cantidadEntrada} />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div className="col">
                    <label>Nombre</label>
                    <input type="text" className="form-control" placeholder="Nombre" ref="name" defaultValue={this.smelting.name} />
                    <br />
                    <table>
                        <tbody>
                            <tr>
                                <td className="slot">
                                    <div onClick={() => { this.localizarArticulo(0); }}>
                                        <img ref="slot0" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="1" max="64" ref="cant0" defaultValue={this.smelting.cantidadResultado} />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    <br />
                    <button type="button" class="btn btn-primary" onClick={this.editarSmelting}>Crear horneo</button>
                    <button type="button" class="btn btn-warning" onClick={this.cancelar}>Cancelar</button>
                    <br />
                    <div className="form-row">
                        <div className="col">
                            <p>Creado {this.formatearFechaTiempo(this.smelting.dateAdd)}</p>
                        </div>
                        <div className="col">
                            <p>Modificado {this.formatearFechaTiempo(this.smelting.dateUpdate)}</p>
                        </div>
                        <div className="col">
                            <input type="checkbox" defaultChecked={this.smelting.off} ref="off" />
                            <label>&iquest;Desactivado?</label>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    }
};



export default Recetas;
