import { Component } from "react";
import ReactDOM from 'react-dom';

class RobotLogs extends Component {
    constructor({ idRobot, handleLogs, handleDeleteLogs, handleClearLogs }) {
        super();

        this.idRobot = idRobot;
        this.handleLogs = handleLogs;
        this.handleDeleteLogs = handleDeleteLogs;
        this.handleClearLogs = handleClearLogs;

        this.buscar = this.buscar.bind(this);
        this.eliminar = this.eliminar.bind(this);
        this.limpiar = this.limpiar.bind(this);
    }

    componentDidMount() {
        window.$('#robotLogs').modal({ show: true });

        this.handleLogs(this.idRobot);
    }

    buscar() {
        var start;
        var end;
        if (this.refs.startdate.value != "") {
            start = new Date(this.refs.startdate.value + " " + this.refs.starttime.value);
            start.setHours(start.getHours() + 1);
        }
        if (this.refs.enddate.value != "") {
            end = new Date(this.refs.enddate.value + " " + this.refs.endtime.value);
            end.setHours(end.getHours() + 1);
            end.setSeconds(59);
            end.setMilliseconds(999);
        }
        ReactDOM.unmountComponentAtNode(document.getElementById("renderRobotLogs"));
        this.handleLogs(this.idRobot, start, end);
    }

    eliminar() {
        var start;
        var end;
        if (this.refs.startdate.value != "") {
            start = new Date(this.refs.startdate.value + " " + this.refs.starttime.value);
            start.setHours(start.getHours() + 1);
        }
        if (this.refs.enddate.value != "") {
            end = new Date(this.refs.enddate.value + " " + this.refs.endtime.value);
            end.setHours(end.getHours() + 1);
            end.setSeconds(59);
            end.setMilliseconds(999);
        }
        ReactDOM.unmountComponentAtNode(document.getElementById("renderRobotLogs"));
        this.handleDeleteLogs(this.idRobot, start, end);
    }

    limpiar() {
        this.handleClearLogs(this.idRobot);
        ReactDOM.unmountComponentAtNode(document.getElementById("renderRobotLogs"));
    }

    render() {
        return <div className="modal fade bd-example-modal-xl" tabIndex="-1" role="dialog" aria-labelledby="robotLogsLabel" id="robotLogs" aria-hidden="true">
            <div className="modal-dialog modal-xl" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="robotLogsLabel">Logs del robot</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <div>
                            <label>Fecha inicio</label>
                            <input type="date" id="startdate" name="startdate" ref="startdate" />
                            <input type="time" id="starttime" name="starttime" ref="starttime" />
                            <label>Fecha fin</label>
                            <input type="date" id="enddate" name="enddate" ref="enddate" />
                            <input type="time" id="endtime" name="endtime" ref="endtime" />
                            <button type="button" className="btn btn-primary" onClick={this.buscar}>Buscar</button>
                            <br />
                            <table className="table table-dark">
                                <thead>
                                    <tr>
                                        <th scope="col">#</th>
                                        <th scope="col">T&iacute;tulo</th>
                                        <th scope="col">Mensaje</th>
                                    </tr>
                                </thead>
                                <tbody id="renderRobotLogs">
                                </tbody>
                            </table>
                        </div>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-danger" onClick={this.eliminar}>Eliminar logs buscados</button>
                        <button type="button" className="btn btn-danger" onClick={this.limpiar}>Limpiar los logs</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

export default RobotLogs;


