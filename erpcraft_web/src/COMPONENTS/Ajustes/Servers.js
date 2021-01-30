import { Component } from "react";
import ReactDOM from 'react-dom';

import serverIco from './../../IMG/oc_server.png';
import Server from "./Server";
import ServerForm from "./ServerForm";

class Servers extends Component {
    constructor({ getServers, handleServersChange, handleAdd, handleUpdate, handleDelete, handlePwd, getAjustes }) {
        super();

        this.getServers = getServers;
        this.handleServersChange = handleServersChange;
        this.handleAdd = handleAdd;
        this.handleUpdate = handleUpdate;
        this.handleDelete = handleDelete;
        this.handlePwd = handlePwd;
        this.getAjustes = getAjustes;

        this.crear = this.crear.bind(this);
        this.editar = this.editar.bind(this);
        this.update = this.update.bind(this);
        this.delete = this.delete.bind(this);
        this.add = this.add.bind(this);
        this.autoReg = this.autoReg.bind(this);
    }

    componentDidMount() {
        this.renderServers();
    }

    async renderServers() {
        const servers = await this.getServers();

        await ReactDOM.unmountComponentAtNode(document.getElementById("renderServers"));
        ReactDOM.render(servers.map((element, i) => {
            return <Server
                key={i}

                uuid={element.uuid}
                name={element.name}
                online={element.online}
                ultimaConexion={element.ultimaConexion}
                dateAdd={element.dateAdd}

                handleEdit={() => {
                    this.editar(element);
                }}
            />
        }), document.getElementById("renderServers"));

        this.handleServersChange(async (_, topicName, changeType, pos, value) => {
            var newServer;
            if (changeType != 2) {
                newServer = JSON.parse(value);
            }
            console.log('Ha passat algo ', changeType, pos, newServer);
            switch (changeType) {
                case 0: { // add
                    servers.push(newServer);

                    break;
                }
                case 1: { // update
                    for (var i = 0; i < servers.length; i++) {
                        if (servers[i].uuid === newServer.uuid) {
                            servers[i] = newServer;
                            break;
                        }
                    }

                    break;
                }
                case 2: { // delete
                    for (var i = 0; i < servers.length; i++) {
                        if (servers[i].uuid === value) {
                            servers.splice(i, 1);
                            break;
                        }
                    }

                    break;
                }
            }

            await ReactDOM.unmountComponentAtNode(document.getElementById("renderServers"));
            ReactDOM.render(servers.map((element, i) => {
                return <Server
                    key={i}

                    uuid={element.uuid}
                    name={element.name}
                    online={element.online}
                    ultimaConexion={element.ultimaConexion}
                    dateAdd={element.dateAdd}

                    handleEdit={() => {
                        this.editar(element);
                    }}
                />
            }), document.getElementById("renderServers"));
        });
    }

    async editar(server) {
        await ReactDOM.unmountComponentAtNode(document.getElementById("renderServersModal"));
        ReactDOM.render(<ServerForm
            server={server}
            handleUpdate={this.update}
            handleDelete={this.delete}
            handlePwd={this.handlePwd}
        />, document.getElementById("renderServersModal"));
    }

    async update(server) {
        await this.handleUpdate(server);
        this.renderServers();
    }

    async delete(uuid) {
        await this.handleDelete(uuid);
        this.renderServers();
    }

    async crear() {
        await ReactDOM.unmountComponentAtNode(document.getElementById("renderServersModal"));
        ReactDOM.render(<ServerForm
            handleAdd={this.add}
        />, document.getElementById("renderServersModal"));
    }

    async autoReg() {
        var autoregistro = false;
        var puerto = 32325;
        const ajustes = await this.getAjustes();
        for (let i = 0; i < ajustes.length; i++) {
            if (ajustes[i].activado) {
                autoregistro = ajustes[i].permitirAutoregistrar;
                puerto = ajustes[i].puertoOC;
                break;
            }
        }

        if (autoregistro == true) {
            await ReactDOM.unmountComponentAtNode(document.getElementById("renderServersModal"));
            ReactDOM.render(<ServerAutoregistrarModal
                puerto={puerto}
            />, document.getElementById("renderServersModal"));
        } else {
            await ReactDOM.unmountComponentAtNode(document.getElementById("renderServersModal"));
            ReactDOM.render(<ServerAutoregistrarError

            />, document.getElementById("renderServersModal"));
        }

    }

    async add(server) {
        await this.handleAdd(server);
        this.renderServers();
    }

    render() {
        return <div id="tabServers">
            <div id="renderServersModal"></div>
            <div id="renderServersPwdModal"></div>
            <div id="renderServersModalAlert"></div>
            <h3><img src={serverIco} />Servers</h3>
            <button type="button" className="btn btn-primary" onClick={this.crear}>Crear</button>
            <button type="button" className="btn btn-primary" onClick={this.autoReg}>Registrar autom&aacute;ticamente</button>
            <div className="form-row">
                <div className="col">
                    <table className="table table-dark">
                        <thead>
                            <tr>
                                <th scope="col">UUID</th>
                                <th scope="col">Nombre</th>
                                <th scope="col">Online</th>
                                <th scope="col">&Uacute;ltima conexi&oacute;n</th>
                                <th scope="col">Fecha de creaci&oacute;n</th>
                            </tr>
                        </thead>
                        <tbody id="renderServers">
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    }
};

class ServerAutoregistrarError extends Component {

    componentDidMount() {
        window.$('#autoregistrarSrvError').modal({ show: true });
    }

    componentWillUnmount() {
        window.$('#autoregistrarSrvError').modal('hide');
    }

    render() {
        return <div class="modal fade" id="autoregistrarSrvError" tabindex="-1" role="dialog" aria-labelledby="autoregistrarSrvErrorLabel" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="autoregistrarSrvErrorLabel">Error</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <p>La funcionalidad de registro autom&aacute;tico de servidores no est&aacute; dispoinble porque no est&aacute; habilitada en la configuraci&oacute;n actualmente activa del servidor ERPCraft.</p>
                        <p>Ve a los ajustes del software, habilita el autoregistro y establece una contrase&ntilde;a segura para continuar.</p>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class ServerAutoregistrarModal extends Component {
    constructor({ puerto }) {
        super();

        this.puerto = puerto;

        this.ok = this.ok.bind(this);
    }

    componentDidMount() {
        window.$('#autoregistrarSrv').modal({ show: true });
    }

    componentWillUnmount() {
        window.$('#autoregistrarSrv').modal('hide');
    }

    async ok() {
        const host = this.refs.host.value;
        const port = this.refs.port.value;

        setTimeout(() => {
            ReactDOM.unmountComponentAtNode(document.getElementById("renderServersModal"));
            ReactDOM.render(<ServerAutoregistrarScript
                host={host}
                port={port}
            />, document.getElementById("renderServersModal"));
        }, 400);
    }

    render() {
        return <div class="modal fade" id="autoregistrarSrv" tabindex="-1" role="dialog" aria-labelledby="autoregistrarSrvLabel" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="autoregistrarSrvLabel">Asistente de registro</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <p>La funcionalidad de autoregistro permite registrar un nuevo servidor de OpenComputers en el sistema sin tener que introducir los datos manualmente.</p>
                        <p>Esto es posible gracias a una contrase&ntilde;a segura establecida en la configuraci&oacute;n del servidor ERPCraft, con la que, con el script de autoregistro y la contrase&ntilde;a se puede crear un servidor en cualquier momento.</p>
                        <label>Puerto</label>
                        <input type="number" className="form-control" ref="port" placeholder="Puerto" min="0" max="65535" defaultValue={this.puerto} />
                        <label>Host</label>
                        <input type="text" className="form-control" ref="host" placeholder="Host" defaultValue={"127.0.0.1"} />
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                        <button type="button" class="btn btn-primary" data-dismiss="modal" onClick={this.ok}>OK</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

class ServerAutoregistrarScript extends Component {
    constructor({ host, port }) {
        super();

        this.host = host;
        this.port = port;
    }

    componentDidMount() {
        window.$('#autoregistrarSrvScript').modal({ show: true });
    }

    componentWillUnmount() {
        window.$('#autoregistrarSrvScript').modal('hide');
    }

    render() {
        return <div class="modal fade bd-example-modal-lg" id="autoregistrarSrvScript" tabindex="-1" role="dialog" aria-labelledby="autoregistrarSrvScriptLabel" aria-hidden="true">
            <div class="modal-dialog modal-lg" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="autoregistrarSrvScriptLabel">Asistente de registro</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <p>Crea el script que registrar&aacute; el servidor de OpenComputers en el ordenador o servidor:</p>
                        <code>
                            $ edit registerServer.lua
                        </code>
                        <p>Copia y pega el c&oacute;digo que se ve abajo en el script:</p>
                        <code>
                            --[[
                            <br />
                            MIT License
                            <br />
                            <br />
                            Permission is hereby granted, free of charge, to any person obtaining a copy
                            <br />
                            of this software and associated documentation files (the "Software"), to deal
                            <br />
                            in the Software without restriction, including without limitation the rights
                            <br />
                            to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
                            <br />
                            copies of the Software, and to permit persons to whom the Software is
                            <br />
                            furnished to do so, subject to the following conditions:
                            <br />
                            <br />
                            The above copyright notice and this permission notice shall be included in all
                            <br />
                            copies or substantial portions of the Software.
                            <br />
                            <br />
                            THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
                            <br />
                            IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
                            <br />
                            FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
                            <br />
                            AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
                            <br />
                            LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
                            <br />
                            OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
                            <br />
                            SOFTWARE.
                            <br />
                            ]]--
                            <br />
                            <br />
                            -- Este script permitir&aacute; al servidor real de ERPCraft registrar un nuevo servidor de OpenComputers sin introducir datos manualmente
                            <br />
                            <br />
                            <br />
                            -- Includes
                            <br />
                            local inet = require("internet")
                            <br />
                            local component = require("component")
                            <br />
                            local m = component.modem
                            <br />
                            <br />
                            print("Introduce la contrase&nilde;a de autoregistro de ERPCraft")
                            <br />
                            server_pwd = io.read()
                            <br />
                            <br />
                            if server_pwd == "" then
                            <br />
                            return;
                            <br />
                            end
                            <br />
                            <br />
                            for i=string.len(server_pwd),35,1 do
                            <br />
                            server_pwd = server_pwd .. " "
                            <br />
                            end
                            <br />
                            <br />
                            -- Conectarse al servidor real
                            <br />
                            local handle = inet.open("{this.host}", {this.port})
                            <br />
                            handle:setTimeout(0.1)
                            <br />
                            handle:write("AUTOREGISTER                        " .. m.address .. server_pwd)
                            <br />
                            handle:flush()
                        </code>
                        <p>Ejecuta el script, e introduce la contrase&ntilde;a de autoregistro del servidor.</p>
                        <code>
                            <br />
                            $ registerServer
                            <br />
                            Introduce la contrase&ntilde;a de autoregistro de ERPCraft
                            <br />
                            ****
                            <br />
                            $
                        </code>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-dismiss="modal">OK</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

export default Servers;


