import { Component } from "react";

// IMG
import automaticoIco from './../../IMG/mov_almacen/automatico.png';
import manualIco from './../../IMG/mov_almacen/manual.png';

const movimientoAlmacenOrigenImg = {
    "A": automaticoIco,
    "M": manualIco
};

const movimientoAlmacenOrigen = {
    "A": "Automatico",
    "M": "Manual"
};

class MovimientoAlmacen extends Component {
    constructor({ movimiento, handleSelect }) {
        super();

        this.movimiento = movimiento;
        this.handleSelect = handleSelect;
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
        return <tr onClick={this.handleSelect}>
            <td>{this.movimiento.almacenName == null ? this.movimiento.almacen : this.movimiento.almacenName}</td>
            <th scope="row">{this.movimiento.id}</th>
            <td>{this.movimiento.articuloName == null ? this.movimiento.articulo : this.movimiento.articuloName}</td>
            <td>{this.movimiento.cantidad}</td>
            <td><img src={movimientoAlmacenOrigenImg[this.movimiento.origen]} />{movimientoAlmacenOrigen[this.movimiento.origen]}</td>
            <td>{this.formatearFechaTiempo(this.movimiento.dateAdd)}</td>
        </tr>;
    }
};



export default MovimientoAlmacen;
