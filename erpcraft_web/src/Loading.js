import logo from './IMG/End_Crystal_JE2.gif';
import './App.css';

function Loading() {
    return (
        <div className="App">
            <header className="App-header">
                <img src={logo} alt="logo" />
                <p className="App-subtitle">ERPCraft</p>
                <p>
                    Cargando...
                </p>
            </header>
        </div>
    );
}

export default Loading;
