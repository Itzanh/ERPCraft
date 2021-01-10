import { Component } from "react";
import ReactDOM from 'react-dom';

class RobotTerminal extends Component {

    constructor({ handleEnviar }) {
        super();

        this.logs = [];
        this.handleEnviar = handleEnviar;

        this.enviar = this.enviar.bind(this);
    }

    componentDidMount() {
        window.$('#robotTerminalModal').modal({ show: true });
    }

    showAlert(txt) {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderRobotFormModalError'));
        ReactDOM.render(<RobotFormAlert
            txt={txt}
        />, document.getElementById('renderRobotFormModalError'));
    }

    enviar() {
        if (this.refs.comando.value == "")
            return;

        this.handleEnviar(this.refs.comando.value).then(() => {
            this.logs.unshift({
                date: new Date(),
                command: this.refs.comando.value
            });
            ReactDOM.unmountComponentAtNode(this.refs.logs);
            ReactDOM.render(this.logs.map((element, i) => {
                return <RobotTerminalLog
                    key={i}

                    date={element.date}
                    command={element.command}
                />
            }), this.refs.logs);
            this.refs.comando.value = "";

        }, () => {
            this.showAlert("No se puede enviar el comando, asegurate de que el robot sigue online");
        });
    }

    render() {
        return <div className="modal fade bd-example-modal-xl" tabIndex="-1" role="dialog" aria-labelledby="robotTerminalModalLabel"
            id="robotTerminalModal" aria-hidden="true">
            <div className="modal-dialog modal-xl" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="robotTerminalModalLabel">Terminal</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <table className="table table-dark">
                            <thead>
                                <tr>
                                    <th scope="col">#</th>
                                    <th scope="col">Comando</th>
                                </tr>
                            </thead>
                            <tbody ref="logs">
                            </tbody>
                        </table>

                        <div className="input-group">
                            <input type="text" className="form-control" ref="comando" />
                            <div className="input-group-append">
                                <button type="button" className="btn btn-outline-success" onClick={this.enviar}>Enviar</button>
                            </div>
                        </div>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div >
    }
};

class RobotTerminalLog extends Component {
    constructor({ date, command }) {
        super();

        this.date = date;
        this.command = command;
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
        return <tr>
            <th scope="col">{this.formatearFechaTiempo(this.date)}</th>
            <td>{this.command}</td>
        </tr>
    }
};

class RobotFormAlert extends Component {
    constructor({ txt }) {
        super();

        this.txt = txt;
    }

    componentDidMount() {
        window.$('#robotFormAlert').modal({ show: true });
    }

    render() {
        return <div class="modal fade" tabIndex="-1" role="robotFormAlert" id="robotFormAlert" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="robotFormDeleteConfirmLabel">Error</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <p>{this.txt}</p>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

export default RobotTerminal;


