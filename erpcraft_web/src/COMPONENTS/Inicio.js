import { Component } from "react";

import logo from './../logo.png';

class Inicio extends Component {
    constructor({ }) {
        super();

    }

    render() {
        return <div id="inicio">
            <header className="App-header">
                <img src={logo} className="App-logo" alt="logo" />
                <p className="App-subtitle">ERPCraft</p>
                <p>
                    ERPCraft es un software de gesti&oacute;n que te permite controlar tu mundo de Minecraft desde el mundo real.
                </p>
            </header>
        </div>
    }
};

export default Inicio;

