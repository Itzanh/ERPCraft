import { Component } from "react";
import ReactDOM from 'react-dom';

class CrafteoLocalizador extends Component {
    constructor({ getCrafteos, handleSelect }) {
        super();

        this.getCrafteos = getCrafteos;
        this.handleSelect = handleSelect;
        this.crafteos = [];

        this.select = this.select.bind(this);
        this.cancelar = this.cancelar.bind(this);
        this.filtrar = this.filtrar.bind(this);
    }

    componentDidMount() {
        window.$('#crafteoLocalizador').modal({ show: true });
        this.renderArticulos();
    }

    async renderArticulos() {
        this.crafteos = await this.getCrafteos();

        this.printCrafteos();
    }

    async printCrafteos() {
        await ReactDOM.unmountComponentAtNode(document.getElementById("renderCrafteosLocalizador"));
        ReactDOM.render(this.crafteos.map((element, i) => {
            return <CrafteoLocalizadorCrafteo
                key={i}

                id={element.id}
                name={element.name}
                articuloResultadoName={element.articuloResultadoName}

                handleSelect={this.select}
            />
        }), document.getElementById("renderCrafteosLocalizador"));
    }

    select(id, name) {
        this.handleSelect(id, name);
        window.$('#crafteoLocalizador').modal('hide');
    }

    cancelar() {
        this.handleSelect(0, '');
        window.$('#crafteoLocalizador').modal('hide');
    }

    async filtrar() {
        // texto de la búsqueda como stirng
        const txt = this.refs.txt.value;
        // no hacer búsqueda por defecto
        if (txt === '')
            return this.printCrafteos();
        // C = Código, N = Nombre, M = Nombre del artículo
        const tipoFiltro = this.refs.fil.value;
        // si se busca por código pero no es un número, no buscar
        if (tipoFiltro === 'C' && isNaN(txt))
            return;

        // establecer el callback dependiendo del filtro
        var callback;
        switch (tipoFiltro) {
            case "C": {
                const id = parseInt(txt);
                callback = (element) => {
                    return element.id === id;
                };
                break;
            }
            case "N": {
                callback = (element) => {
                    return element.name.startsWith(txt);
                };
                break;
            }
            case "M": {
                callback = (element) => {
                    return element.articuloResultadoName.startsWith(txt);
                };
                break;
            }
        }

        await ReactDOM.unmountComponentAtNode(document.getElementById("renderCrafteosLocalizador"));
        ReactDOM.render(this.crafteos.filter(callback).map((element, i) => {
            return <CrafteoLocalizadorCrafteo
                key={i}

                id={element.id}
                name={element.name}
                articuloResultadoName={element.articuloResultadoName}

                handleSelect={this.select}
            />
        }), document.getElementById("renderCrafteosLocalizador"));
    }

    render() {
        return <div className="modal fade bd-example-modal-lg" tabIndex="-1" role="dialog" aria-labelledby="crafteoLocalizadorLabel" id="crafteoLocalizador" aria-hidden="true">
            <div className="modal-dialog modal-lg" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="crafteoLocalizadorLabel">Localizar Crafteo</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <div className="form-row">
                            <div className="col">
                                <select className="form-control" onChange={this.filtrar} ref="fil">
                                    <option value="C">C&oacute;digo</option>
                                    <option value="N">Nombre</option>
                                    <option value="M">Nombre del art&iacute;culo</option>
                                </select>
                            </div>
                            <div className="col">
                                <input type="text" className="form-control" placeholder="Introducir dato" ref="txt" onChange={this.filtrar} />
                            </div>
                            <div className="col">
                                <button type="button" className="btn btn-secondary" onClick={this.cancelar}>Cancelar</button>
                            </div>
                        </div>

                        <table className="table table-dark">
                            <thead>
                                <tr>
                                    <th scope="col">#</th>
                                    <th scope="col">Nombre</th>
                                    <th scope="col">Nombre del art&iacute;culo</th>
                                </tr>
                            </thead>
                            <tbody id="renderCrafteosLocalizador">
                            </tbody>
                        </table>
                    </div>
                    <div className="modal-footer">
                    </div>
                </div>
            </div>
        </div>
    }
};

class CrafteoLocalizadorCrafteo extends Component {
    constructor({ id, name, articuloResultadoName, handleSelect }) {
        super();

        this.id = id;
        this.name = name;
        this.articuloResultadoName = articuloResultadoName;

        this.handleSelect = handleSelect;

        this.select = this.select.bind(this);
    }

    select() {
        this.handleSelect(this.id, this.name);
    }

    render() {
        return <tr onClick={this.select}>
            <th scope="row">{this.id}</th>
            <td>{this.name}</td>
            <td>{this.articuloResultadoName}</td>
        </tr>
    }
};

export default CrafteoLocalizador;


