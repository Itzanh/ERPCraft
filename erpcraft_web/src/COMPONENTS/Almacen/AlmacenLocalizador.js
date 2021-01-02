import { Component } from "react";
import ReactDOM from 'react-dom';

class AlmacenLocalizador extends Component {
    constructor({ getAlmacenes, handleSelect }) {
        super();

        this.getAlmacenes = getAlmacenes;
        this.handleSelect = handleSelect;

        this.select = this.select.bind(this);
        this.cancelar = this.cancelar.bind(this);
    }

    componentDidMount() {
        window.$('#almacenLocalizador').modal({ show: true });
        this.renderAlmacenes();
    }

    async renderAlmacenes() {
        const almacenes = await this.getAlmacenes();

        await ReactDOM.unmountComponentAtNode(document.getElementById("renderAlmacenesLocalizador"));
        ReactDOM.render(almacenes.map((element, i) => {
            return <AlmacenLocalizadorAlmacen
                key={i}

                id={element.id}
                name={element.name}
                uuid={element.uuid}

                handleSelect={this.select}
            />
        }), document.getElementById("renderAlmacenesLocalizador"));
    }

    select(id, name) {
        this.handleSelect(id, name);
        window.$('#almacenLocalizador').modal('hide');
    }

    cancelar() {
        this.handleSelect(0, '');
        window.$('#almacenLocalizador').modal('hide');
    }

    render() {
        return <div className="modal fade bd-example-modal-lg" tabIndex="-1" role="dialog" aria-labelledby="almacenLocalizadorLabel" id="almacenLocalizador" aria-hidden="true">
            <div className="modal-dialog modal-lg" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="almacenLocalizadorLabel">Localizar Almac&eacute;n</h5>
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
                                    <option>UUID</option>
                                </select>
                            </div>
                            <div className="col">
                                <input type="text" className="form-control" placeholder="Introducir dato" />
                            </div>
                        </div>

                        <table className="table table-dark">
                            <thead>
                                <tr>
                                    <th scope="col">#</th>
                                    <th scope="col">Nombre</th>
                                    <th scope="col">UUID</th>
                                </tr>
                            </thead>
                            <tbody id="renderAlmacenesLocalizador">
                            </tbody>
                        </table>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" onClick={this.cancelar}>Cancelar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class AlmacenLocalizadorAlmacen extends Component {
    constructor({ id, name, uuid, handleSelect }) {
        super();

        this.id = id;
        this.name = name;
        this.uuid = uuid;

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
            <td>{this.uuid}</td>
        </tr>
    }
};

export default AlmacenLocalizador;


