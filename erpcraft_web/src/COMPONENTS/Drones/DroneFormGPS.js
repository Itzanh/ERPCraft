import { Component } from "react";

class DroneFormGPS extends Component {
    constructor({ tiempo, posX, posY, posZ }) {
        super();

        this.tiempo = this.formatearFechaTiempo(new Date(tiempo));
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;
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
        return <tr>
            <th scope="row">{this.tiempo}</th>
            <td>{this.posX}</td>
            <td>{this.posY}</td>
            <td>{this.posZ}</td>
        </tr>;
    }
}

export default DroneFormGPS;
