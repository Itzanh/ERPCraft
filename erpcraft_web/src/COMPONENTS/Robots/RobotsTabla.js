import { Component } from "react";
import ReactDOM from 'react-dom';
import Robot from "./Robot";

class RobotsTabla extends Component {
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
                return <Robot
                    key={i}

                    id={element.id}
                    name={element.name}
                    uuid={element.uuid}
                    numeroSlots={element.numeroSlots}
                    numeroStacks={element.numeroStacks}
                    numeroItems={element.numeroItems}
                    estado={element.estado}
                    totalEnergia={element.totalEnergia}
                    energiaActual={element.energiaActual}

                    handleEdit={() => {
                        this.editarRobot(element, i);
                    }}
                />
            })
            , document.getElementById('renderRobots'));
    }

    render() {
        return <table className="table table-dark" id="tableTabRobots">
            <thead>
                <tr>
                    <th scope="col">#</th>
                    <th scope="col">Nombre</th>
                    <th scope="col">UUID</th>
                    <th scope="col">Inventario</th>
                    <th scope="col">Estado</th>
                    <th scope="col">Energia</th>
                    <th scope="col">Bater&iacute;a</th>
                </tr>
            </thead>
            <tbody id="renderRobots">
            </tbody>
        </table>
    }
};

export default RobotsTabla;


