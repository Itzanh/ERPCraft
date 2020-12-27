import { Component } from "react";

class Usuario extends Component {
    constructor({ id, name, ultima_con, iteraciones, off, handleEdit }) {
        super();

        this.id = id;
        this.name = name;
        this.ultima_con = this.formatearFechaTiempo(new Date(ultima_con));
        this.iteraciones = iteraciones;
        this.off = off;
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
            <th scope="row">{this.id}</th>
            <td>{this.name}</td>
            <td>{this.ultima_con}</td>
            <td className={ this.off ? "off" : "on" }>{this.off ? "Desactivado" : "Activado"}</td>
            <td>{this.iteraciones}</td>
        </tr>;
    }
};

export default Usuario;


