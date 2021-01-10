import { Component } from "react";
import ReactDOM from 'react-dom';

import FormAlert from "../FormAlert";

// IMG
import generadorIco from './../../IMG/generador_tipo/generador.png';
import geothermalIco from './../../IMG/generador_tipo/geothermal.png';
import semifluidIco from './../../IMG/generador_tipo/semifluid.png';
import solarIco from './../../IMG/generador_tipo/solar.png';

const tipoGeneradorIco = {
    "G": generadorIco,
    "S": semifluidIco,
    "R": solarIco,
    "T": geothermalIco
}

class GeneradorForm extends Component {
    constructor({ generador, idRedElectrica, handleAdd, handleUpdate, handleDelete }) {
        super();

        this.generador = generador;
        this.idRedElectrica = idRedElectrica;
        if (!generador) {
            this.generador = {};
            this.generador.tipo = "G";
            this.generador.euTick = 10;
        }
        this.tipoIco = tipoGeneradorIco[this.generador.tipo];

        this.handleAdd = handleAdd;
        this.handleUpdate = handleUpdate;
        this.handleDelete = handleDelete;

        this.cambioTipoGenerador = this.cambioTipoGenerador.bind(this);
        this.eliminar = this.eliminar.bind(this);
        this.aceptar = this.aceptar.bind(this);
    }

    componentDidMount() {
        window.$('#generadorModal').modal({ show: true });
    }

    showAlert(txt) {
        ReactDOM.unmountComponentAtNode(document.getElementById('renderRedElectricaModalAlert'));
        ReactDOM.render(<FormAlert
            txt={txt}
        />, document.getElementById('renderRedElectricaModalAlert'));
    }

    isValidUUID(uuid) {
        return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(uuid);
    }

    cambioTipoGenerador() {
        // mostrar imagen
        this.tipoIco = tipoGeneradorIco[this.refs.tipo.value];
        this.refs.tipoIco.src = this.tipoIco;

        // EU/tick por defecto
        switch (this.refs.tipo.value) {
            case "G": {
                this.refs.eu_t.value = "10";
                break;
            }
            case "S": {
                this.refs.eu_t.value = "16";
                break;
            }
            case "R": {
                this.refs.eu_t.value = "1";
                break;
            }
            case "T": {
                this.refs.eu_t.value = "20";
                break;
            }
            default: {
                this.refs.eu_t.value = "0";
            }
        }
    }

    aceptar() {
        if (this.generador && this.generador.id > 0) {
            this.updateGenerador();
        } else {
            this.addGenerador();
        }
    }

    addGenerador() {
        const generador = {};
        generador.redElectrica = this.idRedElectrica;
        generador.name = this.refs.name.value;
        generador.uuid = this.refs.uuid.value;
        generador.tipo = this.refs.tipo.value;
        generador.euTick = parseInt(this.refs.eu_t.value);
        generador.activado = this.refs.act.checked;
        generador.descripcion = this.refs.dsc.value;

        if (generador.name == null || generador.name.length == 0) {
            this.showAlert("El nombre no puede estar vacio.");
            return;
        }
        if (generador.uuid == null || generador.uuid.length == 0) {
            this.showAlert("Debes escribir un UUID valido.");
            return;
        }
        if (generador.euTick <= 0) {
            this.showAlert("El generador debe tener los EU/Tick que genera (superior a 0).");
            return;
        }

        this.handleAdd(generador).then((id) => {
            if (id > 0) {
                window.$('#generadorModal').modal('hide');
            } else {
                this.showAlert("No se ha podido guardar el generador.");
            }
        });
    }

    updateGenerador() {
        const generador = {};
        generador.redElectrica = this.generador.redElectrica;
        generador.id = this.generador.id;
        generador.name = this.refs.name.value;
        generador.uuid = this.refs.uuid.value;
        generador.tipo = this.refs.tipo.value;
        generador.euTick = parseInt(this.refs.eu_t.value);
        generador.activado = this.refs.act.checked;
        generador.descripcion = this.refs.dsc.value;

        if (generador.name == null || generador.name.length == 0) {
            this.showAlert("El nombre no puede estar vacio.");
            return;
        }
        if (generador.uuid == null || generador.uuid.length == 0) {
            this.showAlert("Debes escribir un UUID valido.");
            return;
        }
        if (generador.euTick <= 0) {
            this.showAlert("El generador debe tener los EU/Tick que genera (superior a 0).");
            return;
        }

        this.handleUpdate(generador).then(() => {
            window.$('#generadorModal').modal('hide');
        }, () => {
            this.showAlert("No se ha podido guardar el generador.");
        });
    }

    async eliminar() {
        window.$('#generadorModal').modal('hide');
        await ReactDOM.unmountComponentAtNode(document.getElementById('renderRedElectricaModal'));
        ReactDOM.render(<GeneradorFormDeleteConfirm
            generadorId={this.generador.id}
            generadorName={this.generador.name}
            handleEliminar={() => {
                this.handleDelete(this.idRedElectrica, this.generador.id);
            }}
        />, document.getElementById('renderRedElectricaModal'));
    }

    render() {
        return <div className="modal fade" id="generadorModal" tabIndex="-1" role="dialog" aria-labelledby="generadorModalLabel" aria-hidden="true">
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="generadorModalLabel"><img src={this.tipoIco} ref="tipoIco" /> Generador</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <div id="generadorDetallesBasicos">
                            <label>Nombre</label>
                            <input type="text" className="form-control" placeholder="Nombre" ref="name" defaultValue={this.generador != null ? this.generador.name : ''} />
                            <label>UUID</label>
                            <input type="text" className="form-control" placeholder="UUID" ref="uuid" defaultValue={this.generador != null ? this.generador.uuid : ''} />
                        </div>

                        <div className="form-row">
                            <div className="col">
                                <label>EU/Tick</label>
                                <input type="number" className="form-control" placeholder="EU/Tick" ref="eu_t" defaultValue={this.generador != null ? this.generador.euTick : '0'} />
                            </div>
                            <div className="col">
                                <input type="checkbox" name="act" ref="act" defaultChecked={this.generador != null && this.generador.activado} />
                                <label>&iquest;Activado?</label>
                            </div>
                            <div className="col">
                                <label>Tipo de generador</label>
                                <select name="tipo" ref="tipo" className="form-control" defaultValue={this.generador != null ? this.generador.tipo : 'G'} onChange={this.cambioTipoGenerador}>
                                    <option value="G">Generador convencional</option>
                                    <option value="S">Generador semifluidos</option>
                                    <option value="R">Energia solar</option>
                                    <option value="T">Generador geotermal</option>
                                    <option value="O">Otro</option>
                                </select>
                            </div>
                        </div>

                        <label>Descripcion</label><br />
                        <textarea className="form-control" rows="5" ref="dsc" defaultValue={this.generador != null ? this.generador.descripcion : ''} ></textarea>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-danger" onClick={this.eliminar}>Eliminar</button>
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                        <button type="button" className="btn btn-primary" onClick={this.aceptar}>Aceptar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class GeneradorFormDeleteConfirm extends Component {
    constructor({ generadorName, generadorId, handleEliminar }) {
        super();

        this.generadorName = generadorName;
        this.generadorId = generadorId;

        this.handleEliminar = handleEliminar;

        this.eliminar = this.eliminar.bind(this);
    }

    componentDidMount() {
        window.$('#generadorDeleteConfirm').modal({ show: true });
    }

    eliminar() {
        window.$('#generadorDeleteConfirm').modal('hide');
        this.handleEliminar();
    }

    render() {
        return <div className="modal fade" id="generadorDeleteConfirm" tabIndex="-1" role="dialog" aria-labelledby="generadorDeleteConfirmModalLabel" aria-hidden="true">
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="generadorDeleteConfirmModalLabel">Confirmar eliminaci&oacute;n</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <p>&iquest;Est&aacute;s seguro de que quieres eliminar el generador {this.generadorName}#{this.generadorId}?</p>
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

export default GeneradorForm;


