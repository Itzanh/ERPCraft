import { Component } from "react";
import ReactDOM from 'react-dom';

class Notificaciones extends Component {
    constructor({ getNotificaciones, notificacionesLeidas, onNotificaciones }) {
        super();

        this.getNotificaciones = getNotificaciones;
        this.notificacionesLeidas = notificacionesLeidas;
        this.onNotificaciones = onNotificaciones;

        this.nuevaNotificacion = this.nuevaNotificacion.bind(this);
    }

    componentDidMount() {
        window.$('#notificacionesModal').modal({ show: true });
        this.renderNotificaciones();
    }

    async renderNotificaciones() {
        const notificaciones = await this.getNotificaciones();

        await ReactDOM.render(notificaciones.map((element, i) => {
            return <Notificacion
                key={i}

                id={element.id}
                name={element.name}
                descripcion={element.descripcion}
                leido={element.leido}
            />
        }), this.refs.renderNotificaciones);

        this.onNotificaciones(this.nuevaNotificacion);
        this.notificacionesLeidas();
    }

    async nuevaNotificacion(notificaciones, value) {
        if (value == "") {
            notificaciones.forEach((element) => {
                element.leido = true;
                return element;
            });
        } else {
            try {
                notificaciones.unshift(JSON.parse(value));
            }
            catch (_) { return; }
        }

        await ReactDOM.unmountComponentAtNode(this.refs.renderNotificaciones);
        ReactDOM.render(notificaciones.map((element, i) => {
            return <Notificacion
                key={i}

                id={element.id}
                name={element.name}
                descripcion={element.descripcion}
                leido={element.leido}
            />
        }), this.refs.renderNotificaciones);
    }

    render() {
        return <div class="modal fade bd-example-modal-xl" tabindex="-1" role="dialog" id="notificacionesModal" aria-labelledby="notificacionesModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-xl" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="notificacionesModalLabel">Notificaciones</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <table class="table table-dark">
                            <thead>
                                <tr>
                                    <th scope="col">#</th>
                                    <th scope="col">Titulo</th>
                                    <th scope="col">Mensaje</th>
                                </tr>
                            </thead>
                            <tbody ref="renderNotificaciones">
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    }
};

class Notificacion extends Component {
    constructor({ id, name, descripcion, leido }) {
        super();

        this.id = id;
        this.name = name;
        this.descripcion = descripcion;
        this.leido = leido;
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
        return <tr className={this.leido ? '' : 'noLeido'}>
            <th scope="row">{this.formatearFechaTiempo(this.id)}</th>
            <td>{this.name}</td>
            <td>{this.descripcion}</td>
        </tr>
    }
}

export default Notificaciones;


