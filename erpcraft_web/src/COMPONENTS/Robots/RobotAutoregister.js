import { Component } from "react";
import ReactDOM from 'react-dom';

import robotIco from './../../IMG/robot.png';
import serverIco from './../../IMG/oc_server.png';

const autoRegisterScript = "--[[\nMIT License\n\nPermission is hereby granted, free of charge, to any person obtaining a copy\nof this software and associated documentation files (the \"Software\"), to deal\nin the Software without restriction, including without limitation the rights\nto use, copy, modify, merge, publish, distribute, sublicense, and\/or sell\ncopies of the Software, and to permit persons to whom the Software is\nfurnished to do so, subject to the following conditions:\n\nThe above copyright notice and this permission notice shall be included in all\ncopies or substantial portions of the Software.\n\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR\nIMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,\nFITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE\nAUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER\nLIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,\nOUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE\nSOFTWARE.\n]]--\n\n\n\nlocal component = require(\"component\")\nlocal computer = require(\"computer\")\nlocal robot = require(\"robot\")\n\nlocal m = component.modem\n\nSERVER_ADDR = \"$$UUID$$\"\nSERVER_PORT = 32325\n\n\n\nprint(\"Introduce la contrase\u00F1a de autoregistro del servidor\")\nserver_pwd = io.read()\n\nstr = server_pwd .. \";\" .. robot.name() .. \";\" .. math.floor(robot.inventorySize()) .. \";\" .. math.floor(computer.energy()) .. \";\" .. math.floor(computer.maxEnergy())\n\nif component.isAvailable(\"generator\") then\n  str = str .. \";1;\" .. math.floor(component.generator.count())\nelse\n  str = str .. \";0;0\"\nend\n\nif component.isAvailable(\"navigation\") then\n  local posX, posY, posZ = component.navigation.getPosition()\n  str = str .. \";1;\" .. math.floor(posX) .. \";\" .. math.floor(posY) .. \";\" .. math.floor(posZ)\nelse\n  str = str .. \";0;0;0;0\"\nend\n\nif component.isAvailable(\"inventory_controller\") then\nstr = str .. \";1\"\nelse\nstr = str .. \";0\"\nend\n\nif component.isAvailable(\"geolyzer\") then\nstr = str .. \";1\"\nelse\nstr = str .. \";0\"\nend\n\nm.send(SERVER_ADDR, SERVER_PORT, \"robRegister--\" .. str)";

class RobotAutoregister extends Component {
    constructor({ getServidores }) {
        super();

        this.getServidores = getServidores;

        this.onServerSelected = this.onServerSelected.bind(this);
    }

    componentDidMount() {
        window.$('#robotAutoregister').modal({ show: true });
        ReactDOM.render(<RobotAutoregisterSelectServer
            getServidores={this.getServidores}
            onServerSelected={this.onServerSelected}
        />, this.refs.content);
    }

    onServerSelected(uuid) {
        ReactDOM.render(<RobotAutoregisterResult
            uuid={uuid}
        />, this.refs.content);
    }

    render() {
        return <div class="modal fade bd-example-modal-lg" tabindex="-1" role="dialog" aria-labelledby="robotAutoregisterLabel" id="robotAutoregister" aria-hidden="true">
            <div class="modal-dialog modal-lg" role="document">
                <div class="modal-content" ref="content">

                </div>
            </div>
        </div>
    }
};

class RobotAutoregisterSelectServer extends Component {
    constructor({ getServidores, onServerSelected }) {
        super();

        this.getServidores = getServidores;
        this.onServerSelected = onServerSelected;
    }

    componentDidMount() {
        this.renderServidores();
    }

    async renderServidores() {
        const servidores = await this.getServidores();

        ReactDOM.render(
            <div id="autoregisterServersEmpty">
                <div className="form-row">
                    <div className="col">
                        <img src={serverIco} />
                    </div>
                    <div className="col">
                        <h4>No hay servidores con la funci&oacute;n de autoregistro habilitada.</h4>
                    </div>
                </div>
            </div>, this.refs.renderServidores);

        ReactDOM.render(servidores.filter((element) => {
            return element.permitirAutoregistro;
        }).map((element, i) => {
            return <tr key={i} onClick={() => {
                this.onServerSelected(element.uuid);
            }}>
                <th scope="row">{element.uuid}</th>
                <td>{element.name}</td>
            </tr>
        }), this.refs.renderServidores);
    }

    render() {
        return <div>
            <div class="modal-header">
                <h5 class="modal-title" id="robotAutoregisterLabel">Autoregistrar robot</h5>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>
            <div class="modal-body">
                <div className="form-row" id="pantallaPrincipal">
                    <div className="col">
                        <h5>Registrar robot autom&aacute;ticamente</h5>
                        <br />
                        <p>En este asistente podr&aacute;s crear un nuevo robot a la lista sin tener que introducir sus datos manualmente, a trav&eacute;s de la funcionalidad de autoregistro que puede habilitarse en la configuraci&oacute;n del servidor, marcando el check de "permitir autoregistrarse", y estableciendo una contrase&ntilde;a de registro.</p>
                        <br />
                        <p>Selecciona un servidor con autoregistro de la lista para continuar:</p>
                        <table class="table table-dark">
                            <thead>
                                <tr>
                                    <th scope="col">#</th>
                                    <th scope="col">Nombre</th>
                                </tr>
                            </thead>
                            <tbody ref="renderServidores">
                            </tbody>
                        </table>
                    </div>
                    <div className="col">
                        <img src={robotIco} />
                    </div>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancelar</button>
            </div>
        </div>
    }
};

class RobotAutoregisterResult extends Component {
    constructor({ uuid }) {
        super();

        this.uuid = uuid;
    }

    render() {
        return <div>
            <div class="modal-header">
                <h5 class="modal-title" id="robotAutoregisterLabel">Autoregistrar robot</h5>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>
            <div class="modal-body" id="robotRegResult">
                <h5>Registrar robot autom&aacute;ticamente</h5>
                <p>OK! El &uacute;ltimo paso es crear un script que conectar&aacute; el robot con el servidor para transferir los datos del robot por primera vez, arrancarlo y listo.</p>
                <code>
                    $ edit autoRegister.lua
                </code>
                <br />
                <p>Copia y pega el c&oacute;digo que se ve abajo en el script:</p>
                <code>
                    {autoRegisterScript.replace("$$UUID$$", this.uuid).split("\n").map((element, i) => {
                        return <div>{element}<br /></div>
                    })}
                </code>
                <br />
                <p>Arranca el script, e introduce la contrase&ntilde;a de autoregistro del servidor.</p>
                <code>
                    $ autoRegister
                <br />
                    Introduce la contrase&ntilde;a de autoregistro del servidor
                <br />
                    ****
                <br />
                    $
                </code>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-primary" data-dismiss="modal">OK</button>
            </div>
        </div>
    }
};

export default RobotAutoregister;


