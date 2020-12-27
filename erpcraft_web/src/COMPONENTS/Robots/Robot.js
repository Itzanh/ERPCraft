import { Component } from "react";

// IMG
import onlineIco from './../../IMG/robot_estado/online.svg';
import minandoIco from './../../IMG/robot_estado/minando.png';
import offlineIco from './../../IMG/robot_estado/offline.svg';
import conPerdidaIco from './../../IMG/robot_estado/con_perdida.svg';
import sinBateriaIco from './../../IMG/robot_estado/sin_bateria.svg';

const estadosRobot = {
    "O": "Online",
    "M": "Minando",
    "F": "Offline",
    "L": "Conexión perdida",
    "B": "Sin batería"
};

const imagenEstadosRobot = {
    "O": onlineIco,
    "M": minandoIco,
    "F": offlineIco,
    "L": conPerdidaIco,
    "B": sinBateriaIco
};

class Robot extends Component {
    constructor({ id, name, uuid, numeroSlots, numeroStacks, numeroItems, estado, totalEnergia, energiaActual, handleEdit }) {
        super();

        this.id = id;
        this.name = name;
        this.uuid = uuid;
        this.numeroSlots = numeroSlots;
        this.numeroStacks = numeroStacks;
        this.numeroItems = numeroItems;
        this.estado = estadosRobot[estado];
        this.estadoImagen = imagenEstadosRobot[estado];
        this.totalEnergia = totalEnergia;
        this.energiaActual = energiaActual;
        this.porcentajeBateria = Math.floor((energiaActual / totalEnergia) * 100);

        this.handleEdit = handleEdit;

        this.editarRobot = this.editarRobot.bind(this);
    }

    editarRobot() {
        this.handleEdit();
    }

    render() {
        return <tr onClick={this.editarRobot}>
            <th scope="row">{this.id}</th>
            <td>{this.name}</td>
            <td>{this.uuid}</td>
            <td>{this.numeroItems} &iacute;tems ({this.numeroStacks} stacks / {this.numeroSlots} slots)</td>
            <td>
                <img src={this.estadoImagen} />
                {this.estado}
            </td>
            <td>{this.energiaActual} / {this.totalEnergia}</td>
            <td>
                <div className="progress">
                    <div
                        className={"progress-bar " + (this.porcentajeBateria <= 10 ? 'bg-danger' : (this.porcentajeBateria > 10 && this.porcentajeBateria <= 20 ? 'bg-warning' : ''))}
                        role="progressbar" style={{ width: this.porcentajeBateria + '%' }} aria-valuemin="0" aria-valuemax="100">
                        {this.porcentajeBateria}%
                    </div>
                </div>
            </td>
        </tr>;
    }
};

export default Robot;
