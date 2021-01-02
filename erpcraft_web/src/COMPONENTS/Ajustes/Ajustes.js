import { Component } from "react";
import ReactDOM from 'react-dom';

import settingsIco from './../../IMG/settings.svg';

import './../../CSS/Ajustes.css';
import Ajuste from "./Ajuste";
import AjusteForm from "./AjusteForm";

class Ajustes extends Component {
    constructor({ handleGet, handleAdd, handleUpdate, handleActivar, handleLimpiar, handleEliminar, tabAjustesPush }) {
        super();

        this.handleGet = handleGet;
        this.handleAdd = handleAdd;
        this.handleUpdate = handleUpdate;
        this.handleActivar = handleActivar;
        this.handleLimpiar = handleLimpiar;
        this.handleEliminar = handleEliminar;
        this.tabAjustesPush = tabAjustesPush;

        this.ajusteSel = null;
        this.ajusteSeleccionado = this.ajusteSeleccionado.bind(this);
        this.crearAjuste = this.crearAjuste.bind(this);
    }

    componentDidMount() {
        this.renderAjustes();
    }

    async renderAjustes() {
        const ajustes = await this.handleGet();

        ReactDOM.render(ajustes.map((element, i) => {
            return <Ajuste
                key={i}

                id={element.id}
                name={element.name}
                activado={element.activado}

                ajusteSeleccionado={() => {
                    this.ajusteSeleccionado(element);
                }}
            />
        }), document.getElementById("renderAjustes"));

        this.tabAjustesPush(async (changeType, pos, newAjuste) => {

            switch (changeType) {
                case 0: { // add
                    ajustes.push(newAjuste);

                    break;
                }
                case 1: { // update
                    for (var i = 0; i < ajustes.length; i++) {
                        if (ajustes[i].id === pos) {
                            ajustes[i] = newAjuste;
                            break;
                        }
                    }

                    // si es el seleccionado
                    if (this.ajusteSel != null && this.ajusteSel.id == pos) {
                        this.ajusteSeleccionado(newAjuste);
                    }

                    break;
                }
                case 2: { // delete
                    for (var i = 0; i < ajustes.length; i++) {
                        if (ajustes[i].id === pos) {
                            ajustes.splice(i, 1);
                            break;
                        }
                    }

                    // si es el seleccionado
                    if (this.ajusteSel != null && this.ajusteSel.id == pos) {
                        ReactDOM.unmountComponentAtNode(document.getElementById("renderAjuste"));
                        this.ajusteSel = null;
                    }

                    break;
                }
            }

            await ReactDOM.unmountComponentAtNode(document.getElementById("renderAjustes"));
            ReactDOM.render(ajustes.map((element, i) => {
                return <Ajuste
                    key={i}

                    id={element.id}
                    name={element.name}
                    activado={element.activado}

                    ajusteSeleccionado={() => {
                        this.ajusteSeleccionado(element);
                    }}
                />
            }), document.getElementById("renderAjustes"));
        });
    }

    ajusteSeleccionado(ajuste) {
        this.ajusteSel = ajuste;
        ReactDOM.unmountComponentAtNode(document.getElementById("renderAjuste"));
        ReactDOM.render(<AjusteForm
            ajuste={ajuste}
            handleUpdate={this.handleUpdate}
            handleActivar={this.handleActivar}
            handleLimpiar={this.handleLimpiar}
            handleEliminar={this.handleEliminar}
        />, document.getElementById("renderAjuste"));
    }

    crearAjuste() {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderAjuste"));
        ReactDOM.render(<AjusteForm
            handleAdd={this.handleAdd}
            handleUpdate={this.handleUpdate}
            handleActivar={this.handleActivar}
            handleEliminar={this.handleEliminar}
        />, document.getElementById("renderAjuste"));
    }

    render() {
        return <div id="tabAjustes">
            <h3><img src={settingsIco} />Ajustes</h3>
            <div className="form-row">
                <div className="col">
                    <button type="button" className="btn btn-primary" onClick={this.crearAjuste}>Agregar</button>
                    <table className="table table-dark">
                        <thead>
                            <tr>
                                <th scope="col">#</th>
                                <th scope="col">Nombre</th>
                                <th scope="col">Activo</th>
                            </tr>
                        </thead>
                        <tbody id="renderAjustes">
                        </tbody>
                    </table>
                </div>
                <div className="col" id="renderAjuste">
                </div>
            </div>
        </div>
    }
};

export default Ajustes;


