import { Component } from "react";
import ReactDOM from 'react-dom';

class RobotLocalizador extends Component {
    constructor({ getRobots, handleSelect }) {
        super();

        this.getRobots = getRobots;
        this.handleSelect = handleSelect;
        this.robots = [];

        this.select = this.select.bind(this);
        this.cancelar = this.cancelar.bind(this);
        this.filtrar = this.filtrar.bind(this);
    }

    componentDidMount() {
        window.$('#robotLocalizador').modal({ show: true });
        this.renderRobots();
    }

    async renderRobots() {
        const robots = await this.getRobots();
        this.robots = robots;

        await this.printRobots();
    }

    async printRobots() {
        await ReactDOM.unmountComponentAtNode(document.getElementById("renderRobotsLocalizador"));
        ReactDOM.render(this.robots.map((element, i) => {
            return <RobotLocalizadorRobot
                key={i}
                id={element.id}
                name={element.name}
                uuid={element.uuid}
                handleSelect={this.select} />;
        }), document.getElementById("renderRobotsLocalizador"));
    }

    select(id, name) {
        this.handleSelect(id, name);
        window.$('#robotLocalizador').modal('hide');
    }

    cancelar() {
        this.handleSelect(0, '');
        window.$('#robotLocalizador').modal('hide');
    }

    async filtrar() {
        // texto de la búsqueda como stirng
        const txt = this.refs.txt.value;
        // no hacer búsqueda por defecto
        if (txt === '')
            return this.printRobots();
        // C = Código, N = Nombre, U = UUID
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
            case "U": {
                callback = (element) => {
                    return element.uuid.startsWith(txt);
                };
                break;
            }
        }

        await ReactDOM.unmountComponentAtNode(document.getElementById("renderRobotsLocalizador"));
        ReactDOM.render(this.robots.filter(callback).map((element, i) => {
            return <RobotLocalizadorRobot
                key={i}

                id={element.id}
                name={element.name}
                uuid={element.uuid}

                handleSelect={this.select}
            />
        }), document.getElementById("renderRobotsLocalizador"));
    }

    render() {
        return <div className="modal fade bd-example-modal-lg" tabIndex="-1" role="dialog" aria-labelledby="robotLocalizadorLabel" id="robotLocalizador" aria-hidden="true">
            <div className="modal-dialog modal-lg" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="robotLocalizadorLabel">Localizar Robot</h5>
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
                                    <option value="U">UUID</option>
                                </select>
                            </div>
                            <div className="col">
                                <input type="text" className="form-control" placeholder="Introducir dato" ref="txt" onChange={this.filtrar} />
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
                            <tbody id="renderRobotsLocalizador">
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

class RobotLocalizadorRobot extends Component {
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

export default RobotLocalizador;


