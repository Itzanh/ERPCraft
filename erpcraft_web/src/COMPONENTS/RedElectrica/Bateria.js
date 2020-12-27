import { Component } from "react";

// IMG
import batboxIco from './../../IMG/bateria_tipo/batbox.png';
import cesuIco from './../../IMG/bateria_tipo/cesu.png';
import mfeIco from './../../IMG/bateria_tipo/mfe.png';
import mfsuIco from './../../IMG/bateria_tipo/mfsu.png';

const tipoBateria = {
    "B": "BatBox",
    "C": "CESU",
    "M": "MFE",
    "F": "MFSU",
    "O": "Otro"
}

const tipoBateriaImg = {
    "B": batboxIco,
    "C": cesuIco,
    "M": mfeIco,
    "F": mfsuIco
}

class Bateria extends Component {
    constructor({ id, name, uuid, capacidadElectrica, cargaActual, tipo, handleEdit }) {
        super();

        this.id = id;
        this.name = name;
        this.uuid = uuid;
        this.capacidadElectrica = capacidadElectrica;
        this.cargaActual = cargaActual;
        this.tipo = tipoBateria[tipo];
        this.tipoImg = tipoBateriaImg[tipo];
        this.handleEdit = handleEdit;
    }

    render() {
        return <tr onClick={this.handleEdit}>
            <th scope="row">{this.id}</th>
            <td>{this.name}</td>
            <td>{this.uuid}</td>
            <td>{this.cargaActual}</td>
            <td>{this.capacidadElectrica}</td>
            <td>
                <img src={this.tipoImg} />{this.tipo}
            </td>
        </tr>
    }
};

export default Bateria;


