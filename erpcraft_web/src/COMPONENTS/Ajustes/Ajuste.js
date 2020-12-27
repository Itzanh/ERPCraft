import { Component } from "react";

class Ajuste extends Component {
    constructor({ id, name, activado, ajusteSeleccionado }) {
        super();

        this.id = id;
        this.name = name;
        this.activado = activado;

        this.ajusteSeleccionado = ajusteSeleccionado;
    }

    render() {
        return <tr onClick={this.ajusteSeleccionado}>
            <th scope="row">{this.id}</th>
            <td>{this.name}</td>
            <td>{this.activado ? "Si" : "No"}</td>
        </tr>
    }
};

export default Ajuste;


