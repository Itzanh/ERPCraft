import { Component } from "react";
import ReactDOM from 'react-dom';

const estadosRobot = {
    "O": "Online",
    "M": "Minando",
    "F": "Offline",
    "L": "Conexión perdida",
    "B": "Sin batería"
};

class RobotsTablaAvanzada extends Component {
    constructor({ robots, editarRobot }) {
        super();

        this.robots = robots;
        this.editarRobot = editarRobot;
    }

    componentDidMount() {
        this.renderRobots();
    }

    renderRobots() {
        ReactDOM.render(
            this.robots.map((element, i) => {
                return <RobotTablaAvanzada
                    key={i}

                    robot={element}

                    handleEdit={() => {
                        this.editarRobot(element);
                    }}
                />
            })
            , document.getElementById('renderRobots'));
    }

    render() {
        return <div className="tablaAvanzada">
            <table className="table table-dark" id="tableTabRobots">
                <thead>
                    <tr>
                        <th scope="col">#</th>
                        <th scope="col">Nombre</th>
                        <th scope="col">UUID</th>
                        <th scope="col">Tier</th>
                        <th scope="col">N&uacute;mero de slots</th>
                        <th scope="col">N&uacute;mero de stacks</th>
                        <th scope="col">N&uacute;mero de &iacute;tems</th>
                        <th scope="col">Estado</th>
                        <th scope="col">Total energ&iacute;a</th>
                        <th scope="col">Energ&iacute;a actual</th>
                        <th scope="col">&iquest;Upgrade de generador?</th>
                        <th scope="col">N&uacute;mero de &iacute;tems en el generador</th>
                        <th scope="col">Fecha de conexi&oacute;n</th>
                        <th scope="col">Fecha de desconexi&oacute;n</th>
                        <th scope="col">Descripci&oacute;n</th>
                        <th scope="col">&iquest;Upgrade de GPS?</th>
                        <th scope="col">Posici&oacute;n X</th>
                        <th scope="col">Posici&oacute;n Y</th>
                        <th scope="col">Posici&oacute;n Z</th>
                        <th scope="col">Complejidad</th>
                        <th scope="col">Fecha de creaci&oacute;n</th>
                        <th scope="col">Fecha de &uacute;ltima modificaci&oacute;n</th>
                        <th scope="col">&iquest;Desactivado?</th>
                    </tr>
                </thead>
                <tbody id="renderRobots">
                </tbody>
            </table>
        </div>
    }
};

class RobotTablaAvanzada extends Component {
    constructor({ robot, handleEdit }) {
        super();

        this.robot = robot;
        this.handleEdit = handleEdit;

        this.editarRobot = this.editarRobot.bind(this);
    }

    editarRobot() {
        this.handleEdit();
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
        return <tr onClick={this.editarRobot}>
            <th scope="row">{this.robot.id}</th>
            <td>{this.robot.name}</td>
            <td>{this.robot.uuid}</td>
            <td>{this.robot.tier}</td>
            <td>{this.robot.numeroSlots}</td>
            <td>{this.robot.numeroStacks}</td>
            <td>{this.robot.numeroItems}</td>
            <td>{estadosRobot[this.robot.estado]}</td>
            <td>{this.robot.totalEnergia}</td>
            <td>{this.robot.energiaActual}</td>
            <td>{this.robot.upgradeGenerador ? "Si" : "No"}</td>
            <td>{this.robot.itemsGenerador}</td>
            <td>{this.formatearFechaTiempo(this.robot.fechaConexion)}</td>
            <td>{this.formatearFechaTiempo(this.robot.fechaDesconexion)}</td>
            <td>{this.robot.descripcion}</td>
            <td>{this.robot.upgradeGps ? "Si" : "No"}</td>
            <td>{this.robot.posX}</td>
            <td>{this.robot.posY}</td>
            <td>{this.robot.posZ}</td>
            <td>{this.robot.complejidad}</td>
            <td>{this.formatearFechaTiempo(this.robot.dateAdd)}</td>
            <td>{this.formatearFechaTiempo(this.robot.dateUpd)}</td>
            <td>{this.robot.off ? "Si" : "No"}</td>
        </tr>;
    }
};

export default RobotsTablaAvanzada;


