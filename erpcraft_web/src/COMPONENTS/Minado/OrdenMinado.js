import { Component } from "react";

import queueIco from './../../IMG/orden_minado_estado/queue.svg';
import readyIco from './../../IMG/orden_minado_estado/ready.svg';
import runningIco from './../../IMG/orden_minado_estado/running.svg';
import doneIco from './../../IMG/orden_minado_estado/done.svg';


const ordenesMinadoEstado = {
    "Q": "En cola",
    "R": "Preparado",
    "E": "En curso",
    "O": "Realizado"
};

const ordenesMinadoEstadoImg = {
    "Q": queueIco,
    "R": readyIco,
    "E": runningIco,
    "O": doneIco
};

class OrdenMinado extends Component {
    constructor({ ordenMinado, handleEdit }) {
        super();

        this.ordenMinado = ordenMinado;
        this.handleEdit = handleEdit;
        this.dateAdd = this.formatearFechaTiempo(new Date(ordenMinado.dateAdd));

        this.estado = ordenesMinadoEstado[this.ordenMinado.estado];
        this.estadoImg = ordenesMinadoEstadoImg[this.ordenMinado.estado];
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
            <th scope="row">{this.ordenMinado.id}</th>
            <td>{this.ordenMinado.name}</td>
            <td>{this.ordenMinado.size}</td>
            <td style={this.ordenMinado.robot == null || this.ordenMinado.robot == 0 ? { 'color': '#dc3545' } : {}}>
                {this.ordenMinado.robot == null || this.ordenMinado.robot == 0 ? 'El primer robot disponible.' :
                (this.ordenMinado.robotName == null ? this.ordenMinado.robot : this.ordenMinado.robotName)}</td>
            <td>{this.ordenMinado.posX}x{this.ordenMinado.posY}x{this.ordenMinado.posZ}:{this.ordenMinado.facing}</td>
            <td><img src={this.estadoImg} />{this.estado}</td>
            <td>{this.dateAdd}</td>
            <td>{this.ordenMinado.numeroItems}</td>
        </tr>;
    }
};

export default OrdenMinado;


