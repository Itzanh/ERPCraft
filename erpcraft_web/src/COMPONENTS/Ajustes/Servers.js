import { Component } from "react";
import ReactDOM from 'react-dom';

import serverIco from './../../IMG/oc_server.png';
import Server from "./Server";
import ServerForm from "./ServerForm";

class Servers extends Component {
    constructor({ getServers, handleApiKeysChange, handleAdd, handleUpdate, handleDelete, handlePwd }) {
        super();

        this.getServers = getServers;
        this.handleApiKeysChange = handleApiKeysChange;
        this.handleAdd = handleAdd;
        this.handleUpdate = handleUpdate;
        this.handleDelete = handleDelete;
        this.handlePwd = handlePwd;

        this.crear = this.crear.bind(this);
        this.editar = this.editar.bind(this);
        this.update = this.update.bind(this);
        this.delete = this.delete.bind(this);
        this.add = this.add.bind(this);
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

                handleEdit={() => {
                    this.editar(element);
                }}
            />
        }), document.getElementById("renderServers"));

        this.handleApiKeysChange(async (_, topicName, changeType, pos, value) => {
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

    async add(server) {
        await this.handleAdd(server);
        this.renderServers();
    }

    render() {
        return <div id="tabServers">
            <div id="renderServersModal"></div>
            <h3><img src={serverIco} />Servers</h3>
            <button type="button" className="btn btn-primary" onClick={this.crear}>Crear</button>
            <div className="form-row">
                <div className="col">
                    <table className="table table-dark">
                        <thead>
                            <tr>
                                <th scope="col">UUID</th>
                                <th scope="col">Nombre</th>
                                <th scope="col">Online</th>
                                <th scope="col">&Uacute;ltima conexi&oacute;n</th>
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

export default Servers;


