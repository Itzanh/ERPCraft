import { Component } from "react";

// IMG
import generadorIco from './../../IMG/generador_tipo/generador.png';
import geothermalIco from './../../IMG/generador_tipo/geothermal.png';
import semifluidIco from './../../IMG/generador_tipo/semifluid.png';
import solarIco from './../../IMG/generador_tipo/solar.png';

const tipoGenerador = {
    "G": "Generador convencional",
    "S": "Generador semifluidos",
    "R": "Energia solar",
    "T": "Generador geotermal",
    "O": "Otro"
}

const tipoGeneradorIco = {
    "G": generadorIco,
    "S": semifluidIco,
    "R": solarIco,
    "T": geothermalIco
}

class Generador extends Component {
    constructor({ id, name, uuid, euTick, activado, tipo, handleEdit }) {
        super();

        this.id = id;
        this.name = name;
        this.uuid = uuid;
        this.euTick = euTick;
        this.activado = activado;
        this.tipo = tipoGenerador[tipo];
        this.tipoIco = tipoGeneradorIco[tipo];

        this.handleEdit = handleEdit;
    }

    render() {
        return <tr onClick={this.handleEdit}>
            <th scope="row">{this.id}</th>
            <td>{this.name}</td>
            <td>{this.uuid}</td>
            <td>{this.euTick}</td>
            <td>{this.activado ? "Si" : "No"}</td>
            <td>
                <img src={this.tipoIco} />{this.tipo}
            </td>
        </tr>
    }
};

export default Generador;


