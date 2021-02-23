import { Component } from "react";
import ReactDOM from 'react-dom';

import FormAlert from "../FormAlert";
import photoICO from './../../IMG/photo.svg';

class EditArticulo extends Component {
    constructor({ articulo, handleEdit, handleSubirImagen, handleQuitarImagen, handleEliminar, getInventarioArticulo, getAlmacenName, getArticuloImg,
        getCrafteosArticuloUso, getCrafteosArticuloResult, getSmeltingArticuloUso, getSmeltingArticuloResult }) {
        super();

        this.articulo = articulo;

        this.handleEdit = handleEdit;
        this.handleSubirImagen = handleSubirImagen;
        this.handleQuitarImagen = handleQuitarImagen;
        this.handleEliminar = handleEliminar;

        this.getInventarioArticulo = getInventarioArticulo;
        this.getAlmacenName = getAlmacenName;
        this.getArticuloImg = getArticuloImg;

        this.getCrafteosArticuloUso = getCrafteosArticuloUso;
        this.getCrafteosArticuloResult = getCrafteosArticuloResult;
        this.getSmeltingArticuloUso = getSmeltingArticuloUso;
        this.getSmeltingArticuloResult = getSmeltingArticuloResult;

        this.renderTab = this.renderTab.bind(this);
    }

    componentDidMount() {
        window.$('#editArticuloModal').modal({ show: true });
        this.renderTab();
    }

    renderTab(tab = 1) {
        ReactDOM.unmountComponentAtNode(this.refs.renderMenu);
        ReactDOM.render(<EditArticuloMenu
            tab={tab}
            renderTab={this.renderTab}
        />, this.refs.renderMenu);

        switch (tab) {
            case 1: {
                ReactDOM.render(<EditArticuloDetalles
                    articulo={this.articulo}
                    handleEdit={this.handleEdit}
                    handleSubirImagen={this.handleSubirImagen}
                    handleQuitarImagen={this.handleQuitarImagen}
                    handleEliminar={this.handleEliminar}
                />, this.refs.renderTab);
                break;
            }
            case 2: {
                ReactDOM.render(<EditArticuloExistencias
                    articuloId={this.articulo.id}
                    getArticuloExistencias={this.getInventarioArticulo}
                    getAlmacenName={this.getAlmacenName}
                />, this.refs.renderTab);
                break;
            }
            case 3: {
                ReactDOM.render(<EditArticuloRecetasCrafteos
                    articuloId={this.articulo.id}
                    getArticuloImg={this.getArticuloImg}
                    getCrafteosArticuloResult={this.getCrafteosArticuloResult}
                />, this.refs.renderTab);
                break;
            }
            case 4: {
                ReactDOM.render(<EditArticuloUsoCrafteos
                    articuloId={this.articulo.id}
                    getArticuloImg={this.getArticuloImg}
                    getCrafteosArticuloUso={this.getCrafteosArticuloUso}
                />, this.refs.renderTab);
                break;
            }
            case 5: {
                ReactDOM.render(<EditArticuloRecetasHorneos
                    articuloId={this.articulo.id}
                    getArticuloImg={this.getArticuloImg}
                    getSmeltingArticuloResult={this.getSmeltingArticuloResult}
                />, this.refs.renderTab);
                break;
            }
            case 6: {
                ReactDOM.render(<EditArticuloUsoHorneos
                    articuloId={this.articulo.id}
                    getArticuloImg={this.getArticuloImg}
                    getSmeltingArticuloUso={this.getSmeltingArticuloUso}
                />, this.refs.renderTab);
                break;
            }
        }
    }

    render() {
        return <div id="editArticuloModal" className="modal fade bd-example-modal-xl" tabIndex="-1" role="dialog" aria-labelledby="editArticuloModalLabel" aria-hidden="true">
            <div className="modal-dialog modal-xl" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="editArticuloModalLabel">Editar art&iacute;culo {this.articulo.id}</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <div ref="renderMenu"></div>
                        <div ref="renderTab"></div>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class EditArticuloMenu extends Component {
    constructor({ tab, renderTab }) {
        super();

        this.tab = tab;
        this.renderTab = renderTab;
    }

    render() {
        return <ul class="nav nav-tabs">
            <li class="nav-item">
                <a class={this.tab == 1 ? "nav-link active" : "nav-link"} onClick={() => { this.renderTab(1) }}>Detalles</a>
            </li>
            <li class="nav-item">
                <a class={this.tab == 2 ? "nav-link active" : "nav-link"} onClick={() => { this.renderTab(2) }}>Existencias por almac&eacute;n</a>
            </li>
            <li class="nav-item">
                <a class={this.tab == 3 ? "nav-link active" : "nav-link"} onClick={() => { this.renderTab(3) }}>Recetas: Crafteos</a>
            </li>
            <li class="nav-item">
                <a class={this.tab == 4 ? "nav-link active" : "nav-link"} onClick={() => { this.renderTab(4) }}>Usos: Crafteos</a>
            </li>
            <li class="nav-item">
                <a class={this.tab == 5 ? "nav-link active" : "nav-link"} onClick={() => { this.renderTab(5) }}>Recetas: Horneos</a>
            </li>
            <li class="nav-item">
                <a class={this.tab == 6 ? "nav-link active" : "nav-link"} onClick={() => { this.renderTab(6) }}>Usos: Horneros</a>
            </li>
        </ul>
    }
};

class EditArticuloDetalles extends Component {
    constructor({ articulo, handleEdit, handleSubirImagen, handleQuitarImagen, handleEliminar }) {
        super();

        this.articulo = articulo;

        this.handleEdit = handleEdit;
        this.handleSubirImagen = handleSubirImagen;
        this.handleQuitarImagen = handleQuitarImagen;
        this.handleEliminar = handleEliminar;

        this.save = this.save.bind(this);
        this.subirImagen = this.subirImagen.bind(this);
        this.quitarImagen = this.quitarImagen.bind(this);
        this.eliminarArticulo = this.eliminarArticulo.bind(this);
    }

    showAlert(txt) {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderArticulosModalAlert'));
        ReactDOM.render(<FormAlert
            txt={txt}
        />, document.getElementById('renderArticulosModalAlert'));
    }

    seleccionarImagen() {
        document.getElementById("artImgFile").click();
    }

    subirImagen() {
        const files = document.getElementById("artImgFile").files;
        if (files.length == 0)
            return;
        const file = files[0];
        if (file.size == 0)
            return;
        if (file.size >= 32767) {
            alert("El tamaño máximo de los iconos es de 32Kb");
            return;
        }

        this.handleSubirImagen(this.articulo.id, file);
        this.refs.img.src = URL.createObjectURL(file);
    }

    save() {
        const articulo = {};
        articulo.id = this.articulo.id;
        articulo.name = this.refs.name.value;
        articulo.minecraftID = this.refs.minecraftID.value;
        articulo.descripcion = this.refs.descripcion.value;

        if (articulo.name == null || articulo.name.length == "") {
            this.showAlert("El nombre del articulo no puede estar vacío.");
            return;
        }
        if (articulo.minecraftID == null || articulo.minecraftID.length == "") {
            this.showAlert("El ID de Minecraft del articulo no puede estar vacío.");
            return;
        }

        this.handleEdit(articulo).then(() => {
            window.$('#editArticuloModal').modal('hide');
        }, () => {
            this.showAlert("No se ha podido guardar el articulo.");
        });
    }

    quitarImagen() {
        this.handleQuitarImagen(this.articulo.id);
        this.refs.img.src = photoICO;
    }

    eliminarArticulo() {
        this.handleEliminar(this.articulo.id);
        window.$('#editArticuloModal').modal('hide');
    }

    render() {
        return <div>
            <input type="file" id="artImgFile" ref="img" onChange={this.subirImagen} />
            <div className="form-row" id="editArtMainForm">
                <div className="col">
                    <img src={this.articulo.img != null ? URL.createObjectURL(this.articulo.img) : photoICO} />
                    <button type="button" className="btn btn-danger" onClick={this.quitarImagen}>Quitar</button>
                    <button type="button" className="btn btn-primary" onClick={this.seleccionarImagen}>Subir</button>
                </div>
                <div className="col" id="editArtDetails">
                    <form>
                        <div className="form-row">
                            <div className="col">
                                <label>Nombre</label>
                                <input type="text" className="form-control" placeholder="Nombre" ref="name" defaultValue={this.articulo.name} />
                            </div>
                            <div className="col">
                                <label>ID de Minecraft</label>
                                <input type="text" className="form-control" placeholder="ID de Minecraft" ref="minecraftID" defaultValue={this.articulo.minecraftID} />
                            </div>
                            <div className="col">
                                <label>Cantidad</label>
                                <input type="number" className="form-control" placeholder="Cantidad" readOnly defaultValue={this.articulo.cantidad} />
                            </div>
                        </div>
                    </form>
                    <label>Descripcion</label>
                    <textarea className="form-control" ref="descripcion" defaultValue={this.articulo.descripcion}></textarea>
                </div>
            </div>
            <div id="editArticuloDetallesButtons">
                <button type="button" className="btn btn-danger" onClick={this.eliminarArticulo}>Delete</button>
                <button type="button" className="btn btn-primary" onClick={this.save}>Guardar</button>
            </div>
        </div>
    }
};

class EditArticuloExistencias extends Component {
    constructor({ articuloId, getArticuloExistencias, getAlmacenName }) {
        super();

        this.articuloId = articuloId;
        this.getArticuloExistencias = getArticuloExistencias;
        this.getAlmacenName = getAlmacenName;
    }

    componentDidMount() {
        this.renderExistencias();
    }

    async renderExistencias() {
        // obtener existencias
        var inventario = await this.getArticuloExistencias(this.articuloId);

        await ReactDOM.render(inventario.map((element, i) => {
            return <tr key={i}>
                <th scope="row">{element.id}</th>
                <td>{element.almacen}</td>
                <td>{element.cantidad}</td>
                <td>{element.cantidadDisponible}</td>
            </tr>
        }), this.refs.renderInventario);

        // obtener nombres de almacén
        for (let i = 0; i < inventario.length; i++) {
            inventario[i].almacen = await this.getAlmacenName(inventario[i].almacen);
        }

        await ReactDOM.render(inventario.map((element, i) => {
            return <tr key={i}>
                <th scope="row">{element.id}</th>
                <td>{element.almacen}</td>
                <td>{element.cantidad}</td>
                <td>{element.cantidadDisponible}</td>
            </tr>
        }), this.refs.renderInventario);

        // generar footer
        var cantidadTotal = 0;
        var cantidadDisponibleTotal = 0;
        for (let i = 0; i < inventario.length; i++) {
            cantidadTotal += inventario[i].cantidad;
            cantidadDisponibleTotal += inventario[i].cantidadDisponible;
        }

        const inventarioComponents = inventario.map((element, i) => {
            return <tr key={i}>
                <th scope="row">{element.id}</th>
                <td>{element.almacen}</td>
                <td>{element.cantidad}</td>
                <td>{element.cantidadDisponible}</td>
            </tr>
        });

        inventarioComponents.push(<tr className="tableFooter">
            <th scope="row">{inventario.length}</th>
            <td></td>
            <td>{cantidadTotal}</td>
            <td>{cantidadDisponibleTotal}</td>
        </tr>);
        await ReactDOM.render(inventarioComponents, this.refs.renderInventario);
    }

    render() {
        return <div>
            <table class="table table-dark" id="editArticulosInventarioAlmacen">
                <thead>
                    <tr>
                        <th scope="col">#</th>
                        <th scope="col">Almac&eacute;n</th>
                        <th scope="col">Cantidad</th>
                        <th scope="col">Disponible</th>
                    </tr>
                </thead>
                <tbody ref="renderInventario"></tbody>
            </table>
        </div>
    }
};

class EditArticuloRecetasCrafteos extends Component {
    constructor({ articuloId, getCrafteosArticuloResult, getArticuloImg }) {
        super();

        this.articuloId = articuloId;
        this.getCrafteosArticuloResult = getCrafteosArticuloResult;
        this.getArticuloImg = getArticuloImg;
    }

    componentDidMount() {
        this.renderCrafteos();
    }

    async renderCrafteos() {
        const crafteos = await this.getCrafteosArticuloResult(this.articuloId);

        const artImg = await this.getArticuloImg(this.articuloId);
        const imgCache = {};
        for (let i = 0; i < crafteos.length; i++) {
            crafteos[i].imgArticuloResultado = artImg;

            const crafteo = crafteos[i];
            for (let j = 1; j <= 9; j++) {
                if (crafteo["idArticuloSlot" + j] != null) {
                    if (imgCache[crafteo["idArticuloSlot" + j]] != null) {
                        crafteo["imgArticuloSlot" + j] = imgCache[crafteo["idArticuloSlot" + j]];
                    } else {
                        imgCache[crafteo["idArticuloSlot" + j]] = await this.getArticuloImg(crafteo["idArticuloSlot" + j]);
                        crafteo["imgArticuloSlot" + j] = imgCache[crafteo["idArticuloSlot" + j]];
                    }
                }
            }
        }

        ReactDOM.render(crafteos.map((element, i) => {
            return <ComponenteCrafteo
                key={i}
                crafteo={element}
            />
        }), this.refs.renderHorneos);
    }

    render() {
        return <div ref="renderHorneos" className="renderCrafteosArticulos">
        </div>
    }
};

class EditArticuloUsoCrafteos extends Component {
    constructor({ articuloId, getCrafteosArticuloUso, getArticuloImg }) {
        super();

        this.articuloId = articuloId;
        this.getCrafteosArticuloUso = getCrafteosArticuloUso;
        this.getArticuloImg = getArticuloImg;
    }

    componentDidMount() {
        this.renderCrafteos();
    }

    async renderCrafteos() {
        const crafteos = await this.getCrafteosArticuloUso(this.articuloId);

        const imgCache = {};
        for (let i = 0; i < crafteos.length; i++) {
            crafteos[i].imgArticuloResultado = await this.getArticuloImg(crafteos[i].idArticuloResultado);

            const crafteo = crafteos[i];
            for (let j = 1; j <= 9; j++) {
                if (crafteo["idArticuloSlot" + j] != null) {
                    if (imgCache[crafteo["idArticuloSlot" + j]] != null) {
                        crafteo["imgArticuloSlot" + j] = imgCache[crafteo["idArticuloSlot" + j]];
                    } else {
                        imgCache[crafteo["idArticuloSlot" + j]] = await this.getArticuloImg(crafteo["idArticuloSlot" + j]);
                        crafteo["imgArticuloSlot" + j] = imgCache[crafteo["idArticuloSlot" + j]];
                    }
                }
            }
        }

        ReactDOM.render(crafteos.map((element, i) => {
            return <ComponenteCrafteo
                key={i}
                crafteo={element}
                getArticuloImg={this.getArticuloImg}
            />
        }), this.refs.renderHorneos);
    }

    render() {
        return <div ref="renderHorneos" className="renderCrafteosArticulos">
        </div>
    }
};

class ComponenteCrafteo extends Component {
    constructor({ crafteo }) {
        super();

        this.crafteo = crafteo;
    }

    componentDidMount() {
        this.printCrafteo();
    }

    async printCrafteo() {
        this.refs.slot0.src = this.crafteo.imgArticuloResultado;

        for (let i = 1; i <= 9; i++) {
            if (this.crafteo["idArticuloSlot" + i] != null) {
                this.refs["slot" + i].src = this.crafteo["imgArticuloSlot" + i];
            }
        }
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
            <div className="form-row creafteoPanel">
                <div className="col">
                    <table>
                        <tbody>
                            <tr>
                                <td className="slot">
                                    <div>
                                        <img ref="slot1" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant1" defaultValue={this.crafteo.cantidadArticuloSlot1} readOnly={true} />
                                </td>
                                <td className="slot">
                                    <div>
                                        <img ref="slot2" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant2" defaultValue={this.crafteo.cantidadArticuloSlot2} readOnly={true} />
                                </td>
                                <td className="slot">
                                    <div>
                                        <img ref="slot3" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant3" defaultValue={this.crafteo.cantidadArticuloSlot3} readOnly={true} />
                                </td>
                            </tr>
                            <tr>
                                <td className="slot">
                                    <div>
                                        <img ref="slot4" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant4" defaultValue={this.crafteo.cantidadArticuloSlot4} readOnly={true} />
                                </td>
                                <td className="slot">
                                    <div>
                                        <img ref="slot5" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant5" defaultValue={this.crafteo.cantidadArticuloSlot5} readOnly={true} />
                                </td>
                                <td className="slot">
                                    <div>
                                        <img ref="slot6" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant6" defaultValue={this.crafteo.cantidadArticuloSlot6} readOnly={true} />
                                </td>
                            </tr>
                            <tr>
                                <td className="slot">
                                    <div>
                                        <img ref="slot7" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant7" defaultValue={this.crafteo.cantidadArticuloSlot7} readOnly={true} />
                                </td>
                                <td className="slot">
                                    <div>
                                        <img ref="slot8" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant8" defaultValue={this.crafteo.cantidadArticuloSlot8} readOnly={true} />
                                </td>
                                <td className="slot">
                                    <div>
                                        <img ref="slot9" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="0" max="64" ref="cant9" defaultValue={this.crafteo.cantidadArticuloSlot9} readOnly={true} />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div className="col">
                    <label>Nombre</label>
                    <input type="text" className="form-control" placeholder="Nombre" ref="name" defaultValue={this.crafteo.name} readOnly={true} />
                    <br />
                    <table>
                        <tbody>
                            <tr>
                                <td className="slot">
                                    <div>
                                        <img ref="slot0" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="1" max="64" ref="cant0" defaultValue={this.crafteo.cantidadResultado} readOnly={true} />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    <br />
                    <div className="form-row">
                        <div className="col">
                            <p>Creado {this.formatearFechaTiempo(this.crafteo.dateAdd)}</p>
                        </div>
                        <div className="col">
                            <p>Modificado {this.formatearFechaTiempo(this.crafteo.dateUpdate)}</p>
                        </div>
                        <div className="col">
                            <input type="checkbox" defaultChecked={this.crafteo.off} ref="off" readOnly={true} />
                            <label>&iquest;Desactivado?</label>
                        </div>
                    </div>
                </div>
            </div>
        </div>;
    }
};

class EditArticuloRecetasHorneos extends Component {
    constructor({ articuloId, getSmeltingArticuloResult, getArticuloImg }) {
        super();

        this.articuloId = articuloId;
        this.getSmeltingArticuloResult = getSmeltingArticuloResult;
        this.getArticuloImg = getArticuloImg;
    }

    componentDidMount() {
        this.renderHorneos();
    }

    async renderHorneos() {
        const horneos = await this.getSmeltingArticuloResult(this.articuloId);

        const artImg = await this.getArticuloImg(this.articuloId);
        for (let i = 0; i < horneos.length; i++) {
            horneos[i].imgArticuloEntrada = await await this.getArticuloImg(horneos[i].idArticuloEntrada);
            horneos[i].imgArticuloResultado = artImg;
        }

        ReactDOM.render(horneos.map((element, i) => {
            return <ComponenteHorneo
                key={i}
                smelting={element}
            />
        }), this.refs.renderHorneos);
    }

    render() {
        return <div ref="renderHorneos" className="renderHornosArticulos">
        </div>
    }
};

class EditArticuloUsoHorneos extends Component {
    constructor({ articuloId, getSmeltingArticuloUso, getArticuloImg }) {
        super();

        this.articuloId = articuloId;
        this.getSmeltingArticuloUso = getSmeltingArticuloUso;
        this.getArticuloImg = getArticuloImg;
    }

    componentDidMount() {
        this.renderHorneos();
    }

    async renderHorneos() {
        const horneos = await this.getSmeltingArticuloUso(this.articuloId);

        const artImg = await this.getArticuloImg(this.articuloId);
        for (let i = 0; i < horneos.length; i++) {
            horneos[i].imgArticuloResultado = await await this.getArticuloImg(horneos[i].idArticuloResultado);
            horneos[i].imgArticuloEntrada = artImg;
        }

        ReactDOM.render(horneos.map((element, i) => {
            return <ComponenteHorneo
                key={i}
                smelting={element}
            />
        }), this.refs.renderHorneos);
    }

    render() {
        return <div ref="renderHorneos" className="renderHornosArticulos">
        </div>
    }
};

class ComponenteHorneo extends Component {
    constructor({ smelting }) {
        super();

        this.smelting = smelting;
    }

    componentDidMount() {
        this.printSmelting();
    }

    async printSmelting() {
        //var artImg = await this.getArticuloImg(this.smelting.idArticuloResultado);
        this.refs.slot0.src = this.smelting.imgArticuloResultado;
        //artImg = await this.getArticuloImg(this.smelting.idArticuloEntrada);
        this.refs.slot1.src = this.smelting.imgArticuloEntrada;
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
            <div className="form-row smeltingPanel">
                <div className="col">
                    <table>
                        <tbody>
                            <tr>
                                <td className="slot">
                                    <div>
                                        <img ref="slot1" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="1" max="64" ref="cant1" defaultValue={this.smelting.cantidadEntrada} readOnly={true} />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div className="col">
                    <label>Nombre</label>
                    <input type="text" className="form-control" placeholder="Nombre" ref="name" defaultValue={this.smelting.name} readOnly={true} />
                    <br />
                    <table>
                        <tbody>
                            <tr>
                                <td className="slot">
                                    <div>
                                        <img ref="slot0" />
                                    </div>
                                    <input type="number" className="form-control" placeholder="Cantidad" min="1" max="64" ref="cant0" defaultValue={this.smelting.cantidadResultado} readOnly={true} />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    <br />
                    <div className="form-row">
                        <div className="col">
                            <p>Creado {this.formatearFechaTiempo(this.smelting.dateAdd)}</p>
                        </div>
                        <div className="col">
                            <p>Modificado {this.formatearFechaTiempo(this.smelting.dateUpdate)}</p>
                        </div>
                        <div className="col">
                            <input type="checkbox" defaultChecked={this.smelting.off} ref="off" readOnly={true} />
                            <label>&iquest;Desactivado?</label>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    }
};

export default EditArticulo;
