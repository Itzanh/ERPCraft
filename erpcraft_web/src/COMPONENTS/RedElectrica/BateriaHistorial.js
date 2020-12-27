import { Component } from "react";

class BateriaHistorial extends Component {
    constructor({ id, tiempo, cargaActual }) {
        super();

        this.id = id;
        this.tiempo = this.formatDate(new Date(tiempo));
        this.cargaActual = cargaActual;
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

    render() {
        return <tr>
            <th scope="row">{this.id}</th>
            <td>{this.tiempo}</td>
            <td>{this.cargaActual}</td>
        </tr>
    }
};

export default BateriaHistorial;


