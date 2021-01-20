import { Component } from "react";
import ReactDOM from 'react-dom';

class ArticuloLocalizador extends Component {
    constructor({ getArticulos, handleSelect }) {
        super();

        this.getArticulos = getArticulos;
        this.handleSelect = handleSelect;
        this.articulos = [];

        this.select = this.select.bind(this);
        this.cancelar = this.cancelar.bind(this);
        this.filtrar = this.filtrar.bind(this);
    }

    componentDidMount() {
        window.$('#articuloLocalizador').modal({ show: true });
        this.renderArticulos();
    }

    async renderArticulos() {
        this.articulos = await this.getArticulos();

        this.printArticulos();
    }

    async printArticulos() {
        await ReactDOM.unmountComponentAtNode(document.getElementById("renderArticulosLocalizador"));
        ReactDOM.render(this.articulos.map((element, i) => {
            return <ArticuloLocalizadorArticulo
                key={i}

                id={element.id}
                name={element.name}
                minecraftID={element.minecraftID}

                handleSelect={this.select}
            />
        }), document.getElementById("renderArticulosLocalizador"));
    }

    select(id, name) {
        this.handleSelect(id, name);
        window.$('#articuloLocalizador').modal('hide');
    }

    cancelar() {
        this.handleSelect(0, '');
        window.$('#articuloLocalizador').modal('hide');
    }

    async filtrar() {
        // texto de la b�squeda como stirng
        const txt = this.refs.txt.value;
        // no hacer b�squeda por defecto
        if (txt === '')
            return this.printArticulos();
        // C = C�digo, N = Nombre, U = UUID
        const tipoFiltro = this.refs.fil.value;
        // si se busca por c�digo pero no es un n�mero, no buscar
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
                    return element.minecraftID.startsWith(txt);
                };
                break;
            }
        }

        await ReactDOM.unmountComponentAtNode(document.getElementById("renderArticulosLocalizador"));
        ReactDOM.render(this.articulos.filter(callback).map((element, i) => {
            return <ArticuloLocalizadorArticulo
                key={i}

                id={element.id}
                name={element.name}
                minecraftID={element.minecraftID}

                handleSelect={this.select}
            />
        }), document.getElementById("renderArticulosLocalizador"));
    }

    render() {
        return <div className="modal fade bd-example-modal-lg" tabIndex="-1" role="dialog" aria-labelledby="articuloLocalizadorLabel" id="articuloLocalizador" aria-hidden="true">
            <div className="modal-dialog modal-lg" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="articuloLocalizadorLabel">Localizar Articulo</h5>
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
                                    <option value="M">Minecraft ID</option>
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
                                    <th scope="col">Minecraft ID</th>
                                </tr>
                            </thead>
                            <tbody id="renderArticulosLocalizador">
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

class ArticuloLocalizadorArticulo extends Component {
    constructor({ id, name, minecraftID, handleSelect }) {
        super();

        this.id = id;
        this.name = name;
        this.minecraftID = minecraftID;

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
            <td>{this.minecraftID}</td>
        </tr>
    }
};

export default ArticuloLocalizador;


