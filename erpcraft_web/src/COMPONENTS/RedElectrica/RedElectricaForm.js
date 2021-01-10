import { Component } from "react";
import ReactDOM from 'react-dom';

import electricoIco from './../../IMG/electrico.svg';

import Generador from "./Generador";
import Bateria from "./Bateria";

import GeneradorForm from "./GeneradorForm";
import BateriaForm from "./BateriaForm";

import FormAlert from "../FormAlert";


class RedElectricaForm extends Component {
    constructor({ red, redChange, generadoresChange, bateriasChange, handleRedesElectricas, handleAddRedElectrica, handleEditRedElectrica, handleDeleteRedElectrica,
        handleGetBateriaHistorial, handleAddGeneradorRedElectrica, handleUpdateGeneradorRedElectrica, handleDeleteGeneradorRedElectrica,
        handleAddBateriaRedElectrica, handleUpdateBateriaRedElectrica, handleDeleteBateriaRedElectrica }) {
        super();

        if (red) {
            this.red = red;
            if (red.capacidadElectrica == 0) {
                this.porcentaje = 0;
            } else {
                this.porcentaje = Math.floor((red.energiaActual / red.capacidadElectrica) * 100);
            }
        }

        // main menu
        this.handleRedesElectricas = handleRedesElectricas;

        // red electrica
        this.handleAddRedElectrica = handleAddRedElectrica;
        this.handleEditRedElectrica = handleEditRedElectrica;
        this.handleDeleteRedElectrica = handleDeleteRedElectrica;

        // generador
        this.handleAddGeneradorRedElectrica = handleAddGeneradorRedElectrica;
        this.handleUpdateGeneradorRedElectrica = handleUpdateGeneradorRedElectrica;
        this.handleDeleteGeneradorRedElectrica = handleDeleteGeneradorRedElectrica;

        // bateria
        this.handleAddBateriaRedElectrica = handleAddBateriaRedElectrica;
        this.handleUpdateBateriaRedElectrica = handleUpdateBateriaRedElectrica;
        this.handleDeleteBateriaRedElectrica = handleDeleteBateriaRedElectrica;

        // bateria - historial
        this.handleGetBateriaHistorial = handleGetBateriaHistorial;

        // tiempo real
        if (red) {
            this.redChange = this.redChange.bind(this);
            redChange(this.redChange);
            this.handleRedChange = redChange;

            this.generadoresChange = this.generadoresChange.bind(this);
            generadoresChange(this.generadoresChange);
            this.handleGeneradoresChange = generadoresChange;

            this.bateriasChange = this.bateriasChange.bind(this);
            bateriasChange(this.bateriasChange);
            this.handleBateriasChange = bateriasChange;
        }

        this.aceptar = this.aceptar.bind(this);
        this.handleAddGenerador = this.handleAddGenerador.bind(this);
        this.handleAddBateria = this.handleAddBateria.bind(this);
        this.eliminarRedElectrica = this.eliminarRedElectrica.bind(this);
    }

    componentWillUnmount() {
        this.handleRedChange(null);
        this.handleGeneradoresChange(null);
        this.handleBateriasChange(null);
    }

    showAlert(txt) {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderRedElectricaModalAlert'));
        ReactDOM.render(<FormAlert
            txt={txt}
        />, document.getElementById('renderRedElectricaModalAlert'));
    }

    redChange(redElectrica) {
        if (redElectrica == null) {
            this.handleRedesElectricas();
            return;
        }
        this.red = redElectrica;
        this.refs.name.value = redElectrica.name;
        this.refs.energiaActual.value = redElectrica.energiaActual;
        this.refs.capacidadElectrica.value = redElectrica.capacidadElectrica;
        this.refs.pb.style.width = Math.floor((redElectrica.energiaActual / redElectrica.capacidadElectrica) * 100) + "%";
    }

    generadoresChange(generadores) {
        this.red.generadores = generadores;
        this.renderGeneradores();
    }

    bateriasChange(baterias) {
        this.red.baterias = baterias;
        this.renderBaterias();
    }

    componentDidMount() {
        this.renderGeneradores();
        this.renderBaterias();
    }

    renderGeneradores() {
        ReactDOM.unmountComponentAtNode(this.refs.gen);
        ReactDOM.render(
            this.red == null ? [] :
                this.red.generadores.map((element, i) => {
                    return <Generador
                        key={i}

                        id={element.id}
                        name={element.name}
                        uuid={element.uuid}
                        euTick={element.euTick}
                        activado={element.activado}
                        tipo={element.tipo}

                        handleEdit={() => {
                            this.handleEditGenerador(element);
                        }}
                    />
                })
            , this.refs.gen);
    }

    renderBaterias() {
        ReactDOM.unmountComponentAtNode(this.refs.bat);
        ReactDOM.render(
            this.red == null ? [] :
                this.red.baterias.map((element, i) => {
                    return <Bateria
                        key={i}

                        id={element.id}
                        name={element.name}
                        uuid={element.uuid}
                        capacidadElectrica={element.capacidadElectrica}
                        cargaActual={element.cargaActual}
                        tipo={element.tipo}

                        handleEdit={() => {
                            this.handleEditBateria(element);
                        }}
                    />
                })
            , this.refs.bat);
    }

    handleAddGenerador() {
        if (!this.red) {
            this.showAlert("Crea la red eléctrica antes de poder crear un generador.");
            return;
        }

        ReactDOM.unmountComponentAtNode(document.getElementById('renderRedElectricaModal'));
        ReactDOM.render(<GeneradorForm
            idRedElectrica={this.red.id}
            handleAdd={this.handleAddGeneradorRedElectrica}
        />, document.getElementById('renderRedElectricaModal'));
    }

    handleEditGenerador(generador) {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderRedElectricaModal'));
        ReactDOM.render(<GeneradorForm
            generador={generador}
            handleUpdate={this.handleUpdateGeneradorRedElectrica}
            handleDelete={this.handleDeleteGeneradorRedElectrica}
            idRedElectrica={this.red.id}
        />, document.getElementById('renderRedElectricaModal'));
    }

    handleAddBateria() {
        if (!this.red) {
            this.showAlert("Crea la red eléctrica antes de poder crear una batería.");
            return;
        }

        ReactDOM.unmountComponentAtNode(document.getElementById('renderRedElectricaModal'));
        ReactDOM.render(<BateriaForm
            idRedElectrica={this.red.id}
            handleAdd={this.handleAddBateriaRedElectrica}
        />, document.getElementById('renderRedElectricaModal'));
    }

    handleEditBateria(bateria) {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderRedElectricaModal'));
        ReactDOM.render(<BateriaForm
            bateria={bateria}
            handleGetBateriaHistorial={this.handleGetBateriaHistorial}
            idRedElectrica={this.red.id}
            handleUpdate={this.handleUpdateBateriaRedElectrica}
            handleDelete={this.handleDeleteBateriaRedElectrica}
        />, document.getElementById('renderRedElectricaModal'));
    }

    aceptar() {
        if (this.red && this.red.id > 0) {
            this.updateRedElectrica();
        } else {
            this.addRedElectrica();
        }

    }

    addRedElectrica() {
        const red = {};
        red.name = this.refs.name.value;
        red.descripcion = "";

        if (red.name == null || red.name.length == 0) {
            this.showAlert("El nombre de la red eléctrica no puede estar vacío.");
            return;
        }

        this.handleAddRedElectrica(red).then(() => {
            this.handleRedesElectricas();
        });
    }

    updateRedElectrica() {
        const red = {};
        red.id = this.red.id;
        red.name = this.refs.name.value;
        red.descripcion = "";

        if (red.name == null || red.name.length == 0) {
            this.showAlert("El nombre de la red eléctrica no puede estar vacío.");
            return;
        }

        this.handleEditRedElectrica(red).then(() => {
            this.handleRedesElectricas();
        });
    }

    eliminarRedElectrica() {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderRedElectricaModal'));
        ReactDOM.render(<RedElectricaFormDeleteConfirm
            redName={this.red.name}
            redId={this.red.id}
            handleEliminar={() => {
                this.handleDeleteRedElectrica(this.red.id).then(() => {
                    this.handleRedesElectricas();
                });
            }}
        />, document.getElementById('renderRedElectricaModal'));

    }

    render() {
        return <div id="redElectricaForm">
            <div id="renderRedElectricaModal"></div>
            <div id="renderRedElectricaModalAlert"></div>
            <div id="redElectricaTitle">
                <img src={electricoIco} />
                <h3>Red el&eacute;ctrica</h3>
                <p ref="id">ID: {this.red != null ? this.red.id : 0}</p>
            </div>

            <div className="form-row" id="detallesBasicos">
                <div className="col">
                    <label>Nombre</label>
                    <input type="text" className="form-control" placeholder="Nombre" ref="name" defaultValue={this.red != null ? this.red.name : ''} />
                </div>
                <div className="col">
                    <label>Energ&iacute;a actual</label>
                    <input type="number" className="form-control" placeholder="Energ&iacute;a actual" ref="energiaActual" defaultValue={this.red != null ? this.red.energiaActual : '0'} readOnly />
                </div>
                <div className="col">
                    <label>Capacidad el&eacute;ctrica</label>
                    <input type="number" className="form-control" placeholder="Capacidad el&eacute;ctrica" ref="capacidadElectrica" defaultValue={this.red != null ? this.red.capacidadElectrica : '0'} readOnly />
                </div>
                <div className="col">
                    <div className="progress">
                        <div className="progress-bar" role="progressbar" ref="pb" style={{ 'width': this.porcentaje + '%' }} aria-valuenow="25" aria-valuemin="0" aria-valuemax="100">
                            {this.porcentaje}%
                        </div>
                    </div>
                </div>
            </div>

            <div id="redElectricaContenido">
                <div className="form-row">
                    <div className="col">
                        <h3>Generadores</h3>
                        <button type="button" className="btn btn-primary" onClick={this.handleAddGenerador}>A&ntilde;adir</button>
                        <table className="table table-dark" id="redElectricaGeneradores">
                            <thead>
                                <tr>
                                    <th scope="col">#</th>
                                    <th scope="col">Nombre</th>
                                    <th scope="col">UUID</th>
                                    <th scope="col">EU/tick</th>
                                    <th scope="col">Activado</th>
                                    <th scope="col">Tipo de generador</th>
                                </tr>
                            </thead>
                            <tbody ref="gen">

                            </tbody>
                        </table>

                    </div>
                    <div className="col">
                        <h3>Bater&iacute;as</h3>
                        <button type="button" className="btn btn-primary" onClick={this.handleAddBateria}>A&ntilde;adir</button>
                        <table className="table table-dark" id="redElectricaBaterias">
                            <thead>
                                <tr>
                                    <th scope="col">#</th>
                                    <th scope="col">Nombre</th>
                                    <th scope="col">UUID</th>
                                    <th scope="col">Carga actual</th>
                                    <th scope="col">Capacidad</th>
                                    <th scope="col">Tipo de bater&iacute;a</th>
                                </tr>
                            </thead>
                            <tbody ref="bat">

                            </tbody>
                        </table>
                    </div>
                </div>
            </div>

            <div id="botonesInferiores">
                <button type="button" className="btn btn-danger" onClick={this.eliminarRedElectrica}>Borrar</button>
                <button type="button" className="btn btn-success" onClick={this.aceptar}>Aceptar</button>
                <button type="button" className="btn btn-light" onClick={this.handleRedesElectricas}>Cancelar</button>
            </div>

        </div>
    }
};

class RedElectricaFormDeleteConfirm extends Component {
    constructor({ redName, redId, handleEliminar }) {
        super();

        this.redName = redName;
        this.redId = redId;

        this.handleEliminar = handleEliminar;

        this.eliminar = this.eliminar.bind(this);
    }

    componentDidMount() {
        window.$('#redElectricaDeleteConfirm').modal({ show: true });
    }

    eliminar() {
        window.$('#redElectricaDeleteConfirm').modal('hide');
        this.handleEliminar();
    }

    render() {
        return <div className="modal fade" id="redElectricaDeleteConfirm" tabIndex="-1" role="dialog" aria-labelledby="redElectricaDeleteConfirmModalLabel" aria-hidden="true">
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="redElectricaDeleteConfirmModalLabel">Confirmar eliminaci&oacute;n</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <p>&iquest;Est&aacute;s seguro de que quieres eliminar la red el&eacute;ctrica {this.redName}#{this.redId}?</p>
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

export default RedElectricaForm;


