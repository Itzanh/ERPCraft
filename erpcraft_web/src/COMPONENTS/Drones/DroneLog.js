import { Component } from "react";

class DroneLog extends Component {
    constructor({ id, name, mensaje }) {
        super();

        this.id = this.formatDate(id);
        this.name = name;
        this.mensaje = mensaje;
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
            <td>{this.name}</td>
            <td>{this.mensaje}</td>
        </tr>
    }
};

export default DroneLog;


