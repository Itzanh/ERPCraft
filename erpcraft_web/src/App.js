import logo from './logo.png';
import './App.css';

function App() {
  return (
    <div className="App">
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
        <p>
          No se ha podido conectar al servidor. Vuelve a cargar para reintentar.
        </p>
        <p className="App-subtitle">ERPCraft</p>
      </header>
    </div>
  );
}

export default App;
