import { Component } from "react";
import ReactDOM from 'react-dom';
import BateriaHistorial from './BateriaHistorial'
import { Line } from 'react-chartjs-2';

// IMG
import batboxIco from './../../IMG/bateria_tipo/batbox.png';
import cesuIco from './../../IMG/bateria_tipo/cesu.png';
import mfeIco from './../../IMG/bateria_tipo/mfe.png';
import mfsuIco from './../../IMG/bateria_tipo/mfsu.png';

const tipoBateriaImg = {
    "B": batboxIco,
    "C": cesuIco,
    "M": mfeIco,
    "F": mfsuIco
}

class BateriaForm extends Component {
    constructor({ bateria, handleGetBateriaHistorial, idRedElectrica, handleAdd, handleUpdate, handleDelete }) {
        super();

        this.bateria = bateria;
        if (bateria) {
            this.tipoIco = tipoBateriaImg[bateria.tipo];
            if (bateria.capacidadElectrica == 0) {
                this.porcentaje = 0;
            } else {
                this.porcentaje = Math.floor((bateria.cargaActual / bateria.capacidadElectrica) * 100);
            }
        } else {
            this.tipoIco = batboxIco;
        }

        this.handleGetBateriaHistorial = handleGetBateriaHistorial;
        this.idRedElectrica = idRedElectrica;

        this.handleAdd = handleAdd;
        this.handleUpdate = handleUpdate;
        this.handleDelete = handleDelete;

        this.cambioTipoBateria = this.cambioTipoBateria.bind(this);
        this.mostrarBateria = this.mostrarBateria.bind(this);
        this.historial = this.historial.bind(this);
        this.eliminar = this.eliminar.bind(this);
        this.aceptar = this.aceptar.bind(this);
    }

    componentDidMount() {
        window.$('#bateriaModal').modal({ show: true });
        this.mostrarBateria();
    }

    formatDate(date) {
        const time = new Date(date);
        return time.getDate() + '/'
            + (time.getMonth() + 1) + '/'
            + time.getFullYear() + ' '
            + time.getHours() + ':'
            + time.getMinutes() + ':'
            + time.getSeconds();
    }

    cambioTipoBateria() {
        // mostrar la imagen de la batería
        this.tipoIco = tipoBateriaImg[this.refs.tipo.value];
        this.refs.tipoIco.src = this.tipoIco;

        // capacidad por defecto
        switch (this.refs.tipo.value) {
            case "B": {
                this.refs.cap_ele.value = "40000";
                break;
            }
            case "C": {
                this.refs.cap_ele.value = "300000";
                break;
            }
            case "M": {
                this.refs.cap_ele.value = "4000000";
                break;
            }
            case "F": {
                this.refs.cap_ele.value = "40000000";
                break;
            }
            default: {
                this.refs.cap_ele.value = "0";
                break;
            }
        }

        this.mostrarBateria();
    }

    mostrarBateria() {
        // mostrar la barra de la batería
        if (!this.bateria)
            return;
        this.bateria.cargaActual = parseInt(this.refs.carga_act.value);
        this.bateria.capacidadElectrica = parseInt(this.refs.cap_ele.value);
        if (this.bateria.capacidadElectrica == 0) {
            this.porcentaje = 0;
        } else {
            this.porcentaje = Math.floor((this.bateria.cargaActual / this.bateria.capacidadElectrica) * 100);
        }

        this.refs.progress.style.width = this.porcentaje + '%';
        this.refs.progress.innerText = this.porcentaje + '%';
    }

    async historial() {
        var data;
        await window.$('#bateriaModal').modal('hide');
        await ReactDOM.unmountComponentAtNode(document.getElementById('renderRedElectricaModal'));
        await ReactDOM.render(<BateriaHistorialModal
            handleTabla={() => { this.historiaTabla(data) }}
            handleGrafico={() => { this.historialGrafico(data) }}
        />, document.getElementById('renderRedElectricaModal'));
        data = await this.handleGetBateriaHistorial(this.bateria.redElectrica, this.bateria.id);
    }

    historiaTabla(historial) {
        ReactDOM.render(historial.map((element, i) => {
            return <BateriaHistorial
                key={i}

                id={element.id}
                tiempo={element.tiempo}
                cargaActual={element.cargaActual}
            />
        }), document.getElementById("renderBateriaHistorial"));
    }

    historialGrafico(data) {
        const datasets = [];
        data.forEach((element) => {
            datasets.unshift(element.cargaActual);
        });
        const labels = [];
        data.forEach((element) => {
            labels.unshift(this.formatDate(new Date(element.tiempo)));
        });

        const state = {
            labels: labels,
            datasets: [
                {
                    fill: false,
                    lineTension: 0.5,
                    backgroundColor: 'rgba(75,192,192,1)',
                    borderColor: 'rgba(0,0,0,1)',
                    borderWidth: 2,
                    data: datasets
                }
            ]
        }

        ReactDOM.render(<Line
            data={state}
            options={{
                title: {
                    display: false
                },
                legend: {
                    display: false
                }
            }}

            width={1100}
            height={600}
            options={{ maintainAspectRatio: false }}

        />, document.getElementById('historialBateriaContent'));
    }

    aceptar() {
        if (this.bateria && this.bateria.id > 0) {
            this.updateBateria();
        } else {
            this.addBateria();
        }
    }

    addBateria() {
        const bateria = {};
        bateria.redElectrica = this.idRedElectrica;
        bateria.name = this.refs.name.value;
        bateria.uuid = this.refs.uuid.value;
        bateria.cargaActual = parseInt(this.refs.carga_act.value);
        bateria.capacidadElectrica = parseInt(this.refs.cap_ele.value);
        bateria.tipo = this.refs.tipo.value;
        bateria.descripcion = this.refs.dsc.value;

        this.handleAdd(bateria).then(() => {
            window.$('#bateriaModal').modal('hide');
        });
    }

    updateBateria() {
        const bateria = {};
        bateria.redElectrica = this.idRedElectrica;
        bateria.id = this.bateria.id;
        bateria.name = this.refs.name.value;
        bateria.uuid = this.refs.uuid.value;
        bateria.cargaActual = parseInt(this.refs.carga_act.value);
        bateria.capacidadElectrica = parseInt(this.refs.cap_ele.value);
        bateria.tipo = this.refs.tipo.value;
        bateria.descripcion = this.refs.dsc.value;

        this.handleUpdate(bateria).then(() => {
            window.$('#bateriaModal').modal('hide');
        });
    }

    async eliminar() {
        window.$('#bateriaModal').modal('hide');
        await ReactDOM.unmountComponentAtNode(document.getElementById('renderRedElectricaModal'));
        ReactDOM.render(<BateriaFormDeleteConfirm
            bateriaId={this.bateria.id}
            bateriaName={this.bateria.name}
            handleEliminar={() => {
                this.handleDelete(this.idRedElectrica, this.bateria.id);
            }}
        />, document.getElementById('renderRedElectricaModal'));
    }

    render() {
        return <div className="modal fade" id="bateriaModal" tabIndex="-1" role="dialog" aria-labelledby="bateriaModalLabel" aria-hidden="true">
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="bateriaModalLabel"><img src={this.tipoIco} ref="tipoIco" /> Bateria</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>

                    <div className="progress">
                        <div className="progress-bar" role="progressbar" ref="progress" style={{ width: '0%' }} aria-valuemin="0" aria-valuemax="100">0%</div>
                    </div>

                    <div className="modal-body">
                        <div id="bateriaDetallesBasicos">
                            <label>Nombre</label>
                            <input type="text" className="form-control" placeholder="Nombre" ref="name" defaultValue={this.bateria != null ? this.bateria.name : ''} />
                            <label>UUID</label>
                            <input type="text" className="form-control" placeholder="UUID" ref="uuid" defaultValue={this.bateria != null ? this.bateria.uuid : ''} />
                        </div>

                        <div className="form-row">
                            <div className="col">
                                <label>Carga actual</label>
                                <input type="number" className="form-control" placeholder="Carga actual" ref="carga_act" defaultValue={this.bateria != null ? this.bateria.cargaActual : '0'} onChange={this.mostrarBateria} />
                            </div>
                            <div className="col">
                                <label>Capacidad</label>
                                <input type="number" className="form-control" placeholder="Capacidad" ref="cap_ele" defaultValue={this.bateria != null ? this.bateria.capacidadElectrica : '40000'} onChange={this.mostrarBateria} />
                            </div>
                            <div className="col">
                                <label>Tipo de bater&iacute;a</label>
                                <select name="tipo" ref="tipo" className="form-control" defaultValue={this.bateria != null ? this.bateria.tipo : 'B'} onChange={this.cambioTipoBateria}>
                                    <option value="B">BatBox</option>
                                    <option value="C">CESU</option>
                                    <option value="M">MFE</option>
                                    <option value="F">MFSU</option>
                                    <option value="O">Otro</option>
                                </select>
                            </div>
                        </div>

                        <label>Descripcion</label><br />
                        <textarea className="form-control" rows="5" ref="dsc" defaultValue={this.bateria != null ? this.bateria.descripcion : ''} ></textarea>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-success" onClick={this.historial}>Historial</button>
                        <button type="button" className="btn btn-danger" onClick={this.eliminar}>Eliminar</button>
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                        <button type="button" className="btn btn-primary" onClick={this.aceptar}>Aceptar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class BateriaHistorialModal extends Component {
    constructor({ handleTabla, handleGrafico }) {
        super();

        this.handleTabla = handleTabla;
        this.handleGrafico = handleGrafico;

        this.onDetalles = this.onDetalles.bind(this);
        this.onGrafico = this.onGrafico.bind(this);
    }

    componentDidMount() {
        ReactDOM.unmountComponentAtNode(document.getElementById('historialBateriaContent'));
        ReactDOM.render(<this.Tabla

        />, document.getElementById('historialBateriaContent'));
        window.$('#bateriaHistorialModal').modal({ show: true });
    }

    Tabla() {
        return (<table className="table table-dark">
            <thead>
                <tr>
                    <th scope="col">#</th>
                    <th scope="col">Tiempo</th>
                    <th scope="col">Carga acual</th>
                </tr>
            </thead>
            <tbody id="renderBateriaHistorial">
            </tbody>
        </table>);
    }

    async onDetalles() {
        await ReactDOM.unmountComponentAtNode(document.getElementById('historialBateriaContent'));
        await ReactDOM.render(<this.Tabla

        />, document.getElementById('historialBateriaContent'));
        this.handleTabla();
    }

    async onGrafico() {
        await ReactDOM.unmountComponentAtNode(document.getElementById('historialBateriaContent'));
        this.handleGrafico();
    }

    render() {
        return <div className="modal fade bd-example-modal-xl" tabIndex="-1" role="dialog" aria-labelledby="bateriaHistorialModalLabel" id="bateriaHistorialModal" aria-hidden="true">
            <div className="modal-dialog modal-xl" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="bateriaHistorialModalLabel">Historial de energ&iacute;a</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <label>Fecha inicio</label>
                        <input type="date" id="startdate" name="startdate" ref="startdate" />
                        <input type="time" id="starttime" name="starttime" ref="starttime" />
                        <label>Fecha fin</label>
                        <input type="date" id="enddate" name="enddate" ref="enddate" />
                        <input type="time" id="endtime" name="endtime" ref="endtime" />
                        <button type="button" className="btn btn-primary" onClick={this.buscar}>Buscar</button>
                        <br />
                        <ul className="nav nav-tabs" id="myTab" role="tablist">
                            <li className="nav-item">
                                <a className="nav-link active" id="home-tab" data-toggle="tab" href="#home" role="tab" aria-controls="home" aria-selected="true" onClick={this.onDetalles}>Detalles</a>
                            </li>
                            <li className="nav-item">
                                <a className="nav-link" id="profile-tab" data-toggle="tab" href="#profile" role="tab" aria-controls="profile" aria-selected="false" onClick={this.onGrafico}>Gr&aacute;fico</a>
                            </li>
                        </ul>
                        <div id="historialBateriaContent"></div>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Close</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class BateriaFormDeleteConfirm extends Component {
    constructor({ bateriaName, bateriaId, handleEliminar }) {
        super();

        this.bateriaName = bateriaName;
        this.bateriaId = bateriaId;

        this.handleEliminar = handleEliminar;

        this.eliminar = this.eliminar.bind(this);
    }

    componentDidMount() {
        window.$('#bateriaDeleteConfirm').modal({ show: true });
    }

    eliminar() {
        window.$('#bateriaDeleteConfirm').modal('hide');
        this.handleEliminar();
    }

    render() {
        return <div className="modal fade" id="bateriaDeleteConfirm" tabIndex="-1" role="dialog" aria-labelledby="bateriaDeleteConfirmModalLabel" aria-hidden="true">
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="bateriaDeleteConfirmModalLabel">Confirmar eliminaci&oacute;n</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <p>&iquest;Est&aacute;s seguro de que quieres eliminar el generador {this.bateriaName}#{this.bateriaId}?</p>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                        <button type="button" className="btn btn-danger" onClick={this.eliminar}>Eliminar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

export default BateriaForm;


