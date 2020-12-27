import { Component } from "react";

class RedElectrica extends Component {
    constructor({ id, name, capacidadElectrica, energiaActual, editarRed }) {
        super();

        this.id = id;
        this.name = name;
        this.capacidadElectrica = capacidadElectrica;
        this.energiaActual = energiaActual;
        if (capacidadElectrica == 0) {
            this.porcentaje = 0;
        } else {
            this.porcentaje = Math.floor((energiaActual / capacidadElectrica) * 100);
        }

        this.editarRed = editarRed;
    }

    render() {
        return <tr onClick={this.editarRed}>
            <th scope="row">{this.id}</th>
            <td>{this.name}</td>
            <td>{this.energiaActual}</td>
            <td>{this.capacidadElectrica}</td>
            <td>
                <div className="progress">
                    <div className="progress-bar" role="progressbar" style={{ 'width': this.porcentaje + '%' }} aria-valuenow="25" aria-valuemin="0" aria-valuemax="100">
                        {this.porcentaje}%
                    </div>
                </div>
            </td>
        </tr>;
    }
};

export default RedElectrica;
