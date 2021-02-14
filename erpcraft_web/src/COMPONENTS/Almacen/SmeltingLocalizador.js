import { Component } from "react";
import ReactDOM from 'react-dom';

class SmeltingLocalizador extends Component {
    constructor({ getSmeltings, handleSelect }) {
        super();

        this.getSmeltings = getSmeltings;
        this.handleSelect = handleSelect;
        this.smeltings = [];

        this.select = this.select.bind(this);
        this.cancelar = this.cancelar.bind(this);
        this.filtrar = this.filtrar.bind(this);
    }

    componentDidMount() {
        window.$('#smeltingLocalizador').modal({ show: true });
        this.renderArticulos();
    }

    async renderArticulos() {
        this.smeltings = await this.getSmeltings();

        this.printSmeltings();
    }

    async printSmeltings() {
        await ReactDOM.unmountComponentAtNode(document.getElementById("renderSmeltingLocalizador"));
        ReactDOM.render(this.smeltings.map((element, i) => {
            return <ArticuloLocalizadorArticulo
                key={i}

                id={element.id}
                name={element.name}
                articuloResultadoName={element.articuloResultadoName}

                handleSelect={this.select}
            />
        }), document.getElementById("renderSmeltingLocalizador"));
    }

    select(id, name) {
        this.handleSelect(id, name);
        window.$('#smeltingLocalizador').modal('hide');
    }

    cancelar() {
        this.handleSelect(0, '');
        window.$('#smeltingLocalizador').modal('hide');
    }

    async filtrar() {
        // texto de la búsqueda como stirng
        const txt = this.refs.txt.value;
        // no hacer búsqueda por defecto
        if (txt === '')
            return this.printSmeltings();
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

        await ReactDOM.unmountComponentAtNode(document.getElementById("renderSmeltingLocalizador"));
        ReactDOM.render(this.smeltings.filter(callback).map((element, i) => {
            return <ArticuloLocalizadorArticulo
                key={i}

                id={element.id}
                name={element.name}
                articuloResultadoName={element.articuloResultadoName}

                handleSelect={this.select}
            />
        }), document.getElementById("renderSmeltingLocalizador"));
    }

    render() {
        return <div className="modal fade bd-example-modal-lg" tabIndex="-1" role="dialog" aria-labelledby="smeltingLocalizadorLabel" id="smeltingLocalizador" aria-hidden="true">
            <div className="modal-dialog modal-lg" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="smeltingLocalizadorLabel">Localizar Horneo</h5>
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
                            <tbody id="renderSmeltingLocalizador">
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

export default SmeltingLocalizador;


