import { Component } from "react";
import ReactDOM from 'react-dom';

import keyIco from './../../IMG/key.svg';
import ApiKey from "./ApiKey";

class ApiKeys extends Component {
    constructor({ getKeys, handleApiKeysChange, addKey, resetKey, deleteKey }) {
        super();

        this.getKeys = getKeys;
        this.handleApiKeysChange = handleApiKeysChange;
        this.addKey = addKey;
        this.resetKey = resetKey;
        this.deleteKey = deleteKey;

        this.crear = this.crear.bind(this);
        this.agregar = this.agregar.bind(this);
        this.reset = this.reset.bind(this);
        this.delete = this.delete.bind(this);
    }

    componentDidMount() {
        this.showKeys();
    }

    async showKeys() {
        const apiKeys = await this.getKeys();

        await ReactDOM.unmountComponentAtNode(document.getElementById("renderApiKeys"));
        ReactDOM.render(apiKeys.map((element, i) => {
            return <ApiKey
                key={i}

                id={element.id}
                name={element.name}
                uuid={element.uuid}
                ultimaConexion={element.ultimaConexion}

                handleReset={this.reset}
                handleDelete={this.delete}
            />
        }), document.getElementById("renderApiKeys"));

        this.handleApiKeysChange(async (_, topicName, changeType, pos, value) => {
            var newApiKey;
            if (changeType != 2) {
                newApiKey = JSON.parse(value);
            }
            console.log('Ha passat algo ', changeType, pos, newApiKey);
            switch (changeType) {
                case 0: { // add
                    apiKeys.push(newApiKey);

                    break;
                }
                case 1: { // update
                    for (var i = 0; i < apiKeys.length; i++) {
                        if (apiKeys[i].id === pos) {
                            apiKeys[i] = newApiKey;
                            break;
                        }
                    }

                    break;
                }
                case 2: { // delete
                    for (var i = 0; i < apiKeys.length; i++) {
                        if (apiKeys[i].id === pos) {
                            apiKeys.splice(i, 1);
                            break;
                        }
                    }

                    break;
                }
            }

            await ReactDOM.unmountComponentAtNode(document.getElementById("renderApiKeys"));
            ReactDOM.render(apiKeys.map((element, i) => {
                return <ApiKey
                    key={i}

                    id={element.id}
                    name={element.name}
                    uuid={element.uuid}
                    ultimaConexion={element.ultimaConexion}

                    handleReset={this.reset}
                    handleDelete={this.delete}
                />
            }), document.getElementById("renderApiKeys"));
        });
    }

    crear() {
        ReactDOM.unmountComponentAtNode(document.getElementById("renderApiKeysModal"));
        ReactDOM.render(<ApiKeyPrompt
            handleAceptar={this.agregar}
        />, document.getElementById("renderApiKeysModal"));
    }

    async agregar(name) {
        await this.addKey(name);
        this.showKeys();
    }

    async reset(uuid) {
        await this.resetKey(uuid);
        this.showKeys();
    }

    async delete(uuid) {
        await this.deleteKey(uuid);
        this.showKeys();
    }

    render() {
        return <div id="tabApiKeys">
            <div id="renderApiKeysModal"></div>
            <h3><img src={keyIco} />API Keys</h3>
            <button type="button" className="btn btn-primary" onClick={this.crear}>Crear</button>
            <div className="form-row">
                <div className="col">
                    <table className="table table-dark">
                        <thead>
                            <tr>
                                <th scope="col">#</th>
                                <th scope="col">Nombre</th>
                                <th scope="col">UUID</th>
                                <th scope="col">&Uacute;ltima conexi&oacute;n</th>
                                <th scope="col">Reset</th>
                                <th scope="col">Delete</th>
                            </tr>
                        </thead>
                        <tbody id="renderApiKeys">
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    }
};

class ApiKeyPrompt extends Component {
    constructor({ handleAceptar }) {
        super();

        this.handleAceptar = handleAceptar;

        this.aceptar = this.aceptar.bind(this);
    }

    componentDidMount() {
        window.$('#apiKeyPromptModal').modal({ show: true });
    }

    aceptar() {
        window.$('#apiKeyPromptModal').modal('hide');
        this.handleAceptar(this.refs.name.value);
    }

    render() {
        return <div className="modal fade" tabIndex="-1" role="dialog" aria-labelledby="apiKeyPromptModalLabel" id="apiKeyPromptModal" aria-hidden="true">
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <h5 className="modal-title" id="apiKeyPromptModalLabel">Introducir dato</h5>
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <p>Introduce el nombre de la API Key.</p>
                        <input type="text" className="form-control" placeholder="Nombre" ref="name" />
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancelar</button>
                        <button type="button" className="btn btn-primary" onClick={this.aceptar}>Aceptar</button>
                    </div>
                </div>
            </div>
        </div>
    }
};

export default ApiKeys;


