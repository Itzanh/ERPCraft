import { Component } from "react";
import ReactDOM from 'react-dom';

class ArticuloLocalizador extends Component {
    constructor({ getArticulos, handleSelect }) {
        super();

        this.getArticulos = getArticulos;
        this.handleSelect = handleSelect;

        this.select = this.select.bind(this);
        this.cancelar = this.cancelar.bind(this);
    }

    componentDidMount() {
        window.$('#articuloLocalizador').modal({ show: true });
        this.renderArticulos();
    }

    async renderArticulos() {
        const articulos = await this.getArticulos();

        await ReactDOM.unmountComponentAtNode(document.getElementById("renderArticulosLocalizador"));
        ReactDOM.render(articulos.map((element, i) => {
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
                                <select className="form-control">
                                    <option>C&oacute;digo</option>
                                    <option>Nombre</option>
                                    <option>Minecraft ID</option>
                                </select>
                            </div>
                            <div className="col">
                                <input type="text" className="form-control" placeholder="Introducir dato" />
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


