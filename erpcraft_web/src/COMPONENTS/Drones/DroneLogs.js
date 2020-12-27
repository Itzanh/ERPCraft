import { Component } from "react";
import ReactDOM from 'react-dom';

class DroneLogs extends Component {
    constructor({ idDrone, handleLogs, handleDeleteLogs, handleClearLogs }) {
        super();

        this.idDrone = idDrone;
        this.handleLogs = handleLogs;
        this.handleDeleteLogs = handleDeleteLogs;
        this.handleClearLogs = handleClearLogs;

        this.buscar = this.buscar.bind(this);
        this.eliminar = this.eliminar.bind(this);
        this.limpiar = this.limpiar.bind(this);
    }

    componentDidMount() {
        window.$('#droneLogs').modal({ show: true });

        this.handleLogs(this.idDrone);
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
        ReactDOM.unmountComponentAtNode(document.getElementById("renderDroneLogs"));
        this.handleLogs(this.idDrone, start, end);
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
        ReactDOM.unmountComponentAtNode(document.getElementById("renderDroneLogs"));
        this.handleDeleteLogs(this.idDrone, start, end);
    }

    limpiar() {
        this.handleClearLogs(this.idDrone);
        ReactDOM.unmountComponentAtNode(document.getElementById("renderDroneLogs"));
    }

    render() {
        return <div className="modal fade bd-example-modal-xl" tabIndex="-1" role="dialog" aria-labelledby="droneLogsLabel" id="droneLogs" aria-hidden="true">
            <div className="modal-dialog modal-xl" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="droneLogsLabel">Logs del drone</h5>
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
                                <tbody id="renderDroneLogs">
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

export default DroneLogs;


