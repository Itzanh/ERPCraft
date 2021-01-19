import { Component } from "react";

class Server extends Component {
    constructor({ uuid, name, online, ultimaConexion, dateAdd, handleEdit }) {
        super();

        this.uuid = uuid;
        this.name = name;
        this.online = online;
        this.ultimaConexion = this.formatearFechaTiempo(new Date(ultimaConexion));
        this.dateAdd = this.formatearFechaTiempo(new Date(dateAdd));

        this.handleEdit = handleEdit;
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

    render() {
        return <tr onClick={this.handleEdit}>
            <th scope="col">{this.uuid}</th>
            <td>{this.name}</td>
            <td>{this.online ? "Si" : "No"}</td>
            <td>{this.ultimaConexion}</td>
            <td>{this.dateAdd}</td>
        </tr>
    }
};

export default Server;


