import { Component } from "react";

import trashIco from './../../IMG/trash.svg';
import resetIco from './../../IMG/reset.svg';

class ApiKey extends Component {
    constructor({ id, name, uuid, ultimaConexion, handleReset, handleDelete }) {
        super();

        this.id = id;
        this.name = name;
        this.uuid = uuid;
        this.ultimaConexion = this.formatearFechaTiempo(new Date(ultimaConexion));

        this.handleReset = handleReset;
        this.handleDelete = handleDelete;

        this.reset = this.reset.bind(this);
        this.delete = this.delete.bind(this);
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

    reset() {
        this.handleReset(this.id);
    }

    delete() {
        this.handleDelete(this.id);
    }

    render() {
        return <tr>
            <th scope="row">{this.id}</th>
            <td>{this.name}</td>
            <td>{this.uuid}</td>
            <td>{this.ultimaConexion}</td>
            <td><img src={resetIco} onClick={this.reset} /></td>
            <td><img src={trashIco} onClick={this.delete} /></td>
        </tr>
    }
};

export default ApiKey;


