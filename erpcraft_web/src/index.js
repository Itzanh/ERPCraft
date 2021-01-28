import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';
import Loading from './Loading';

// NETEVENTS.IO
import { NetEventIO_Client, NetEventIOOptions } from './netevent.io.js';
import Login from './COMPONENTS/Login';
import Menu from './COMPONENTS/Menu';
import Usuarios from './COMPONENTS/Usuarios/Usuarios';
import Usuario from './COMPONENTS/Usuarios/Usuario';
import Robots from './COMPONENTS/Robots/Robots';
import RobotForm from './COMPONENTS/Robots/RobotForm';
import RobotFormSlotInventario from './COMPONENTS/Robots/RobotFormSlotInventario';
import RobotFormGPS from './COMPONENTS/Robots/RobotFormGPS';
import Articulos from './COMPONENTS/Articulos/Articulos';
import Articulo from './COMPONENTS/Articulos/Articulo';
import EditArticulo from './COMPONENTS/Articulos/EditArticulo';
import RobotLog from './COMPONENTS/Robots/RobotLog';
import RedesElectricas from './COMPONENTS/RedElectrica/RedesElectricas';
import RedElectrica from './COMPONENTS/RedElectrica/RedElectrica';
import RedElectricaForm from './COMPONENTS/RedElectrica/RedElectricaForm';
import BateriaHistorial from './COMPONENTS/RedElectrica/BateriaHistorial';
import Ajustes from './COMPONENTS/Ajustes/Ajustes';
import ApiKeys from './COMPONENTS/Ajustes/ApiKeys';
import Servers from './COMPONENTS/Ajustes/Servers';
import OrdenesMinado from './COMPONENTS/Minado/OrdenesMinado';
import RobotsTablaAvanzada from './COMPONENTS/Robots/RobotsTablaAvanzada';
import RobotsTabla from './COMPONENTS/Robots/RobotsTabla';
import Inicio from './COMPONENTS/Inicio';
import Almacenes from './COMPONENTS/Almacen/Almacenes';
import UsuarioEditForm from './COMPONENTS/Usuarios/UsuarioEditForm';
import Drones from './COMPONENTS/Drones/Drones';
import Drone from './COMPONENTS/Drones/Drone';
import DroneForm from './COMPONENTS/Drones/DroneForm';
import DroneFormSlotInventario from './COMPONENTS/Drones/DroneFormSlotInventario';
import DroneFormGPS from './COMPONENTS/Drones/DroneFormGPS';
import DroneLog from './COMPONENTS/Drones/DroneLog';
import MovimientosAlmacen from './COMPONENTS/Almacen/MovimientosAlmacen';
import MovimientoAlmacen from './COMPONENTS/Almacen/MovimientoAlmacen';
import MovimientosAlmacenFormDetails from './COMPONENTS/Almacen/MovimientosAlmacenFormDetails';
import Notificaciones from './COMPONENTS/Notificaciones';

/**
 * Object that represents the WebSocket data connection with the server
 * */
var client;

ReactDOM.render(
    <React.StrictMode>
        <Loading />
    </React.StrictMode>,
    document.getElementById('root')
);

async function main() {
    const options = new NetEventIOOptions(onPasswordLogin, onTokenLogin);
    var addr;
    var port;
    if (window.location.search === "") {
        addr = window.location.hostname;
        port = 32324;
    } else {
        addr = await getServerAddr();
        port = await getServerPort();
    }

    var ws;
    try {
        ws = new WebSocket('ws://' + addr + ':' + port + '/');
    } catch (e) {
        console.error(e);
    }
    client = new NetEventIO_Client(ws, options);
    // wait for the connection to open
    const connected = await client.connect();
    if (connected[0]) {
        console.log(client);
        window.$('#loginScreenModal').modal('hide');
        printMenu();

        ws.onclose = (e) => {
            console.log('Disconnected', e);
            ws.close();
            ReactDOM.render(<App />, document.getElementById('root'));
            setTimeout(() => {
                main();
            }, 1000);
        };

    } else {
        console.log('No connectat. ' + client.state);
        ReactDOM.render(
            <React.StrictMode>
                <App />
            </React.StrictMode>,
            document.getElementById('root')
        );
    }
};

function getServerAddr() {
    return new Promise((resolve) => {
        const parametres = window.location.search.substring(1).split('&');
        parametres.forEach((element) => {
            const info = element.split('=');
            if (info[0] === 'host') {
                resolve(info[1]);
            }
        });
        resolve(window.location.hostname);
    });
};

function getServerPort() {
    return new Promise((resolve) => {
        const parametres = window.location.search.substring(1).split('&');
        parametres.forEach((element) => {
            const info = element.split('=');
            if (info[0] === 'port') {
                resolve(info[1]);
            }
        });
        resolve(32324);
    });
};

function onPasswordLogin() {
    return new Promise((resolve) => {
        ReactDOM.render(<Login
            handleLogin={(name, pwd) => {
                resolve(JSON.stringify({ name, pwd }));
            }}
        />, document.getElementById('root'));
    });
};

function onTokenLogin() {
    return new Promise((resolve) => {
        resolve(null);
    });
};

function printMenu() {
    ReactDOM.render(<Menu
        handleInicio={inicio}
        handleRobots={tabRobots}
        handleArticulos={tabArticulos}
        handleMovimientosAlmacen={tabMovimientoAlmacen}
        handleUsuarios={tabUsuarios}
        handleRedesElectricas={tabRedElectrica}
        handleOrdenesMinado={tabOrdenesMinado}
        handleConfiguraciones={tabAjustes}
        handleApiKey={tabApiKey}
        handleServidores={tabServidores}
        handleAlmacen={tabAlmacen}
        handleDrones={tabDrones}
        handleNotificaciones={tabNotificaciones}
    />, document.getElementById('root'));

    inicio();
    countNotificaciones();
};

function inicio() {
    client.unsubscribeAll();
    ReactDOM.unmountComponentAtNode(document.getElementById('renderTab'));
    ReactDOM.render(<Inicio

    />, document.getElementById('renderTab'));
};

function countNotificaciones() {
    setTimeout(() => {
        client.emit('notificacion', 'count', '', (_, response) => {
            document.getElementById("notificacionesCount").innerText = response;
        });
    }, 200);

    setInterval(() => {
        client.emit('notificacion', 'count', '', (_, response) => {
            document.getElementById("notificacionesCount").innerText = response;
        });
    }, 1000);
};

/* ROBOTS */

async function tabRobots() {
    client.unsubscribeAll();

    var robots = [];
    ReactDOM.unmountComponentAtNode(document.getElementById('renderTab'));
    await ReactDOM.render(<Robots
        handleRobots={tabRobots}
        handleBuscar={buscarRobots}
        handleAddRobot={addRobot}
        handleEditRobot={updateRobot}
        handleEliminar={deleteRobot}
        handleLogs={getRobotLogs}
        handleRender={(view) => {
            renderRobots(robots, view);
        }}
        getServidores={getServidores}
    />, document.getElementById('renderTab'));

    client.emit('robot', 'get', '', (_, message) => {
        robots = JSON.parse(message);

        renderRobots(robots);
    });

    client.subscribe('robots', (_, topicName, changeType, pos, value) => {
        var newRobot;
        if (changeType != 2) {
            newRobot = JSON.parse(value);
        }
        console.log('Ha passat algo ', changeType, pos, newRobot);
        switch (changeType) {
            case 0: { // add
                robots.push(newRobot);

                break;
            }
            case 1: { // update
                for (var i = 0; i < robots.length; i++) {
                    if (robots[i].id === pos) {
                        newRobot.robotChange = robots[i].robotChange;
                        if (robots[i].robotChange != null) {
                            robots[i].robotChange(newRobot);
                        }

                        robots[i] = newRobot;
                        break;
                    }
                }

                break;
            }
            case 2: { // delete
                for (var i = 0; i < robots.length; i++) {
                    if (robots[i].id === pos) {
                        if (robots[i].robotChange != null) {
                            robots[i].robotChange(null);
                        }

                        robots.splice(i, 1);
                        break;
                    }
                }

                break;
            }
        }

        renderRobots(robots);
    });
};

function buscarRobots(query) {
    client.emit('robot', 'get', JSON.stringify(query), (_, message) => {
        const robots = JSON.parse(message);

        renderRobots(robots);
    });
}

function renderRobots(robots, view = 1) {
    if (!document.getElementById('robotTabla')) {
        return;
    }

    if (view == 1) {
        ReactDOM.unmountComponentAtNode(document.getElementById('robotTabla'));
        ReactDOM.render(<RobotsTabla
            robots={robots}
            editarRobot={(robot, i) => {
                editarRobot(robot, robots, i);
            }}
        />, document.getElementById('robotTabla'));
    } else if (view == 2) {
        ReactDOM.unmountComponentAtNode(document.getElementById('robotTabla'));
        ReactDOM.render(<RobotsTablaAvanzada
            robots={robots}
            editarRobot={editarRobot}
        />, document.getElementById('robotTabla'));
    }
};

async function editarRobot(robot, robots, i) {
    ReactDOM.unmountComponentAtNode(document.getElementById('renderTab'));
    await ReactDOM.render(<RobotForm
        robot={robot}
        robotChange={(callback) => {
            robots[i].robotChange = callback;
        }}
        handleCancelar={tabRobots}
        handleEditRobot={updateRobot}
        handleEliminar={deleteRobot}
        handleLogs={getRobotLogs}
        handleDeleteLogs={deleteRobotLogs}
        handleClearLogs={clearRobotLogs}
        handleComando={robotCommand}
    />, document.getElementById('renderTab'));

    await getRobotInventario(robot);
    renderRobotGPS(robot);
};

// ROBOT - INVENTARIO

function getRobotInventario(robot) {
    return new Promise((resolve) => {
        client.emit('robot', 'getInventario', '' + robot.id, async (_, response) => {
            const inventario = JSON.parse(response);

            await renderRobotInventario(inventario);
            resolve();
        });

        client.subscribe('robotInv#' + robot.id, (_, topicName, changeType, pos, value) => {
            if (changeType != 1) {
                return;
            }

            const inventario = JSON.parse(value);
            renderRobotInventario(inventario);
        });
    });
};

function renderRobotInventario(inventario) {
    return new Promise(async (resolve) => {
        await new ReactDOM.unmountComponentAtNode(document.getElementById("inventarioContenido"));
        await ReactDOM.render(inventario.map((element, i) => {
            return <RobotFormSlotInventario
                key={i}

                numeroSlot={element.numeroSlot}
                cant={element.cant}
                articulo={element.articulo}
            />
        }), document.getElementById('inventarioContenido'));

        const inventarioImg = inventario.filter((value, index, self) => {
            return self.indexOf(value) === index;
        });
        for (let i = 0; i < inventarioImg.length; i++) {
            const articulo = inventarioImg[i].articulo;

            if (articulo != null) {
                await new Promise((resolve) => {
                    client.on("artImg", (_, response) => {
                        if (response.message.size) {
                            const img = document.getElementsByClassName("art_img_" + articulo.id);
                            if (img.length > 0) {
                                const src = URL.createObjectURL(response.message);

                                for (let j = 0; j < img.length; j++) {
                                    img[j].src = src;
                                }
                            }

                        }

                        resolve();
                    });
                    client.emit('articulos', 'getImg', '' + articulo.id);
                });
            }
        }

        resolve();
    });
};

function addRobot(robot) {
    console.log(robot);
    return new Promise((resolve, reject) => {
        client.emit('robot', 'add', JSON.stringify(robot), (_, response) => {
            if (response === "ERR") {
                reject();
            } else {
                resolve(parseInt(response));
            }
        });
    });
};

function updateRobot(robot) {
    return new Promise((resolve, reject) => {
        client.emit('robot', 'update', JSON.stringify(robot), (_, response) => {
            if (response === "ERR") {
                reject();
            } else {
                resolve();
            }
        });
    });
};

function deleteRobot(idRobot) {
    return new Promise((resolve, reject) => {
        client.emit('robot', 'delete', '' + idRobot, (_, response) => {
            if (response === "ERR") {
                reject();
            } else {
                resolve();
            }
        });
    });
};

// ROBOT - GPS

async function renderRobotGPS(robot) {
    var listaGPS = [];
    var continuar = true;
    const limit = 10;
    var offset = 0;

    do {
        await new Promise((resolve) => {
            client.emit('robot', 'getGPS', JSON.stringify({ "robotId": robot.id, "offset": offset, "limit": limit }), async (_, response) => {
                const gps = JSON.parse(response);
                if (gps.length == 0) {
                    continuar = false;
                    resolve();
                    return;
                }
                listaGPS = listaGPS.concat(gps);

                ReactDOM.unmountComponentAtNode(document.getElementById('robotGPS'));
                ReactDOM.render(listaGPS.map((element, i) => {
                    return <RobotFormGPS
                        key={i}

                        tiempo={element.tiempo}
                        posX={element.posX}
                        posY={element.posY}
                        posZ={element.posZ}
                    />
                }), document.getElementById('robotGPS'));

                offset += limit;
                resolve();
            });
        });
    }
    while (continuar);

    client.subscribe('robotGPS#' + robot.id, async (_, topicName, changeType, pos, value) => {
        if (changeType != 0) {
            return;
        }

        const gps = JSON.parse(value);
        listaGPS.unshift(gps);

        await ReactDOM.unmountComponentAtNode(document.getElementById('robotGPS'));
        ReactDOM.render(listaGPS.map((element, i) => {
            return <RobotFormGPS
                key={i}

                tiempo={element.tiempo}
                posX={element.posX}
                posY={element.posY}
                posZ={element.posZ}
            />
        }), document.getElementById('robotGPS'));
    });
};

// ROBOT - LOGS

async function getRobotLogs(idRobot, start, end) {
    var logs = [];
    const limit = 10;
    var offset = 0;
    var continueDownloading = true;
    do {
        await new Promise((resolve) => {
            client.emit('robot', 'getLog', JSON.stringify({
                idRobot,
                start,
                end,
                limit,
                offset
            }), (_, response) => {

                const logsPart = JSON.parse(response);
                logs = logs.concat(logsPart);
                continueDownloading = (logsPart.length == limit);

                ReactDOM.render(logs.map((element, i) => {
                    return <RobotLog
                        key={i}

                        id={element.id}
                        name={element.name}
                        mensaje={element.mensaje}
                    />
                }), document.getElementById("renderRobotLogs"));

                resolve();
            });
        });

        offset += limit;
    } while (continueDownloading);

    client.subscribe('robotLog#' + idRobot, async (_, topicName, changeType, pos, value) => {
        var newRobotLog;
        if (changeType != 2) {
            newRobotLog = JSON.parse(value);
        }

        switch (changeType) {
            case 0: {// add
                logs.unshift(newRobotLog);

                await ReactDOM.unmountComponentAtNode(document.getElementById('renderRobotLogs'));
                ReactDOM.render(logs.map((element, i) => {
                    return <RobotLog
                        key={i}

                        id={element.id}
                        name={element.name}
                        mensaje={element.mensaje}
                    />
                }), document.getElementById("renderRobotLogs"));

                break;
            }
            case 2: {// delete
                getRobotLogs(idRobot, start, end);

                break;
            }
        }

    });
};

function clearRobotLogs(idRobot) {
    client.emit('robot', 'clearLogs', '' + idRobot, (_, response) => {

    });
};

function deleteRobotLogs(idRobot, start, end) {
    client.emit('robot', 'clearLogsBetween', JSON.stringify({ idRobot, start, end }), (_, response) => {

    });
};

function robotCommand(robotId, command) {
    return new Promise((resolve, reject) => {
        client.emit('robot', 'command', JSON.stringify({ robotId, command }), (_, response) => {
            if (response == "OK") {
                resolve();
            } else if (response == "ERR") {
                reject();
            }
        });
    });
};

/* USUARIOS */

async function tabUsuarios() {
    ReactDOM.unmountComponentAtNode(document.getElementById('renderTab'));
    await ReactDOM.render(<Usuarios
        searchUsuarios={searchUsuarios}
        handleAdd={addUsuario}
    />, document.getElementById('renderTab'));

    getUsuarios();
};

function getUsuarios() {
    client.emit('usuarios', 'get', '', (_, response) => {
        const usuarios = JSON.parse(response);
        renderUsuarios(usuarios);

        client.unsubscribeAll();
        client.subscribe('usuarios', (_, topicName, changeType, pos, value) => {
            var newUsuario;
            if (changeType != 2) {
                newUsuario = JSON.parse(value);
            }
            console.log('Ha passat algo ', changeType, pos, newUsuario);
            switch (changeType) {
                case 0: { // add
                    usuarios.push(newUsuario);

                    break;
                }
                case 1: { // update
                    for (var i = 0; i < usuarios.length; i++) {
                        if (usuarios[i].id === pos) {
                            usuarios[i] = newUsuario;
                            break;
                        }
                    }

                    break;
                }
                case 2: { // delete
                    for (var i = 0; i < usuarios.length; i++) {
                        if (usuarios[i].id === pos) {
                            usuarios.splice(i, 1);
                            break;
                        }
                    }

                    break;
                }
            }

            renderUsuarios(usuarios);
        });
    });
};

function searchUsuarios(query) {
    if ((query.text && query.text.length > 0) || query.off) {
        client.emit('usuarios', 'search', JSON.stringify(query), (_, response) => {
            const usuarios = JSON.parse(response);
            renderUsuarios(usuarios);
        });
    } else {
        getUsuarios();
    }
};

async function renderUsuarios(usuarios) {
    await ReactDOM.unmountComponentAtNode(document.getElementById('renderUsuarios'));
    ReactDOM.render(
        usuarios.map((element, i) => {
            return <Usuario
                key={i}

                id={element.id}
                name={element.name}
                ultima_con={element.ultima_con}
                dateAdd={element.dateAdd}
                off={element.off}
                iteraciones={element.iteraciones}

                handleEdit={() => {
                    editarUsuario(element);
                }}
            />
        })
        , document.getElementById('renderUsuarios'));
};

function editarUsuario(usuario) {
    ReactDOM.unmountComponentAtNode(document.getElementById('renderUsuarioModal'));
    ReactDOM.render(<UsuarioEditForm
        usuario={usuario}
        handleEdit={editUsuario}
        handleDelete={deleteUsuario}
        handlePwd={pwdUsuario}
    />, document.getElementById('renderUsuarioModal'));

};

function addUsuario(name, pwd) {
    return new Promise((resolve) => {
        client.emit('usuarios', 'add', JSON.stringify({ name, pwd }), (_, response) => {
            resolve();
        });
    });
};

function editUsuario(usuario) {
    return new Promise((resolve) => {
        client.emit('usuarios', 'edit', JSON.stringify(usuario), (_, response) => {
            resolve();
        });
    });
};

function deleteUsuario(id) {
    return new Promise((resolve) => {
        client.emit('usuarios', 'delete', '' + id, (_, response) => {
            resolve();
        });
    });
};

function pwdUsuario(id, pwdActual, pwdNuevo) {
    return new Promise((resolve, reject) => {
        client.emit('usuarios', 'pwd', JSON.stringify({ id, pwdActual, pwdNuevo }), (_, response) => {
            if (response == 'OK') {
                resolve();
            } else {
                reject();
            }
        });
    });
};

/* ARTÍCULOS */

async function tabArticulos() {
    ReactDOM.unmountComponentAtNode(document.getElementById('renderTab'));
    await ReactDOM.render(<Articulos
        handleAddArticulo={addArticulo}
        handleSearch={getArticulos}
    />, document.getElementById('renderTab'));

    getArticulos();
};

function getArticulos(busqueda) {
    if (busqueda && busqueda.length > 0) {
        client.emit('articulos', 'search', busqueda, async (_, response) => {
            const articulos = JSON.parse(response);

            renderArticulos(articulos);
        });
    } else {
        client.emit('articulos', 'get', '', async (_, response) => {
            const articulos = JSON.parse(response);

            renderArticulos(articulos);

            client.unsubscribeAll();
            client.subscribe('articulos', (_, topicName, changeType, pos, value) => {
                var newArticulo;
                if (changeType != 2) {
                    newArticulo = JSON.parse(value);
                }
                console.log('Ha passat algo ', changeType, pos, newArticulo);
                switch (changeType) {
                    case 0: { // add
                        articulos.push(newArticulo);

                        break;
                    }
                    case 1: { // update
                        for (var i = 0; i < articulos.length; i++) {
                            if (articulos[i].id === pos) {
                                articulos[i] = newArticulo;
                                break;
                            }
                        }

                        break;
                    }
                    case 2: { // delete
                        for (var i = 0; i < articulos.length; i++) {
                            if (articulos[i].id === pos) {
                                articulos.splice(i, 1);
                                break;
                            }
                        }

                        break;
                    }
                }

                renderArticulos(articulos);
            });

            client.subscribe('articulosImg', (_, topicName, changeType, pos, value) => {
                switch (changeType) {
                    case 1: { // update
                        client.on("artImg", (_, response) => {
                            if (response.message.size) {
                                const img = document.getElementById("art_img_" + pos);
                                if (img) {
                                    img.src = URL.createObjectURL(response.message);
                                }

                                for (var i = 0; i < articulos.length; i++) {
                                    if (articulos[i].id === pos) {
                                        articulos[i].img = response.message;
                                        break;
                                    }
                                }

                            }

                        });
                        client.emit('articulos', 'getImg', '' + pos);

                        break;
                    }
                    case 2: { // delete
                        const img = document.getElementById("art_img_" + pos);
                        img.attributes.removeNamedItem("src");

                        break;
                    }
                }
            });
        });
    }
};

async function renderArticulos(articulos) {
    await ReactDOM.unmountComponentAtNode(document.getElementById('renderArticulos'));
    ReactDOM.render(
        articulos.map((element, i) => {
            return <Articulo
                key={i}

                id={element.id}
                name={element.name}
                minecraftID={element.minecraftID}
                cantidad={element.cantidad}

                editarArticulo={() => { editArticulo(element) }}
            />
        })
        , document.getElementById('renderArticulos'));

    for (let i = 0; i < articulos.length; i++) {
        const articulo = articulos[i];

        await new Promise((resolve) => {
            client.on("artImg", (_, response) => {
                if (response.message.size) {
                    const img = document.getElementById("art_img_" + articulo.id);
                    if (img) {
                        img.src = URL.createObjectURL(response.message);
                    }
                    articulos[i].img = response.message;
                }

                resolve();
            });
            client.emit('articulos', 'getImg', '' + articulo.id);
        });
    }
};

function addArticulo(articulo) {
    return new Promise((resolve, reject) => {
        client.emit('articulos', 'add', JSON.stringify(articulo), (_, response) => {
            if (response === "OK") {
                resolve();
            } else {
                reject();
            }
        });
    });
};

function editArticulo(articulo) {
    ReactDOM.unmountComponentAtNode(document.getElementById('renderArticulosModal'));
    ReactDOM.render(<EditArticulo

        id={articulo.id}
        name={articulo.name}
        minecraftID={articulo.minecraftID}
        cant={articulo.cantidad}
        descripcion={articulo.descripcion}
        img={articulo.img}

        handleEdit={updateArticulo}
        handleSubirImagen={articuloSetImagen}
        handleQuitarImagen={articuloQuitarImagen}
        handleEliminar={deleteArticulo}

    />, document.getElementById('renderArticulosModal'));
};

function articuloSetImagen(id, imagen) {
    client.emitBinary('articuloSetImg', '' + id, imagen);

    const img = document.getElementById("art_img_" + id);
    if (img) {
        img.src = URL.createObjectURL(imagen);
    }
};

function articuloQuitarImagen(id) {
    client.emit('articulos', 'deleteImg', '' + id, (_, response) => {
        const img = document.getElementById("art_img_" + id);
        if (img) {
            img.src = "";
        }
    });
};

function updateArticulo(articulo) {
    return new Promise((resolve, reject) => {
        client.emit('articulos', 'edit', JSON.stringify(articulo), (_, response) => {
            if (response === 'OK') {
                resolve();
            } else {
                reject();
            }
        });
    });
};

function deleteArticulo(id) {
    return new Promise((resolve) => {
        client.emit('articulos', 'delete', '' + id, (_, response) => {
            resolve();
        });
    });
};

function getArticuloImg(artImg) {
    return new Promise((resolve) => {
        client.on("artImg", (_, response) => {
            if (response.message.size) {
                resolve(URL.createObjectURL(response.message));
                return;
            }

            resolve();
        });
        client.emit('articulos', 'getImg', '' + artImg);
    });
};

/* RED ELECTRICA */

async function tabRedElectrica() {
    ReactDOM.unmountComponentAtNode(document.getElementById('renderTab'));
    await ReactDOM.render(<RedesElectricas
        handleAddRedElectrica={addRedElectrica}
        handleRedesElectricas={tabRedElectrica}
        handleBuscar={searchRedElectrica}
    />, document.getElementById('renderTab'));

    getRedElectrica();
};

function searchRedElectrica(text) {
    if (text && text.length > 0) {
        client.emit('electrico', 'search', text, (_, message) => {
            const redes = JSON.parse(message);
            renderRedElectrica(redes);
        });
    } else {
        getRedElectrica();
    }
};

function getRedElectrica() {
    client.emit('electrico', 'get', '', (_, message) => {
        const redes = JSON.parse(message);
        renderRedElectrica(redes);

        client.unsubscribeAll();
        client.subscribe('electrico', (_, topicName, changeType, pos, value) => {
            var newRedElectrica;
            if (changeType != 2) {
                newRedElectrica = JSON.parse(value);
            }
            console.log('Ha passat algo ', changeType, pos, newRedElectrica);
            switch (changeType) {
                case 0: { // add
                    redes.push(newRedElectrica);

                    break;
                }
                case 1: { // update
                    for (var i = 0; i < redes.length; i++) {
                        if (redes[i].id === pos) {
                            newRedElectrica.generadoresChange = redes[i].generadoresChange;
                            newRedElectrica.bateriasChange = redes[i].bateriasChange;

                            newRedElectrica.redChange = redes[i].redChange;
                            if (redes[i].redChange != null) {
                                redes[i].redChange(newRedElectrica);
                            }

                            redes[i] = newRedElectrica;
                            break;
                        }
                    }

                    break;
                }
                case 2: { // delete
                    for (var i = 0; i < redes.length; i++) {
                        if (redes[i].id === pos) {

                            if (redes[i].redChange != null) {
                                redes[i].redChange(null);
                            }

                            redes.splice(i, 1);
                            break;
                        }
                    }

                    break;
                }
            }

            renderRedElectrica(redes);
        });

        client.subscribe('generador', (_, topicName, changeType, pos, value) => {
            const newGenerador = JSON.parse(value);

            console.log('Ha passat algo ', changeType, pos, newGenerador);
            switch (changeType) {
                case 0: { // add
                    for (var i = 0; i < redes.length; i++) {
                        if (redes[i].id === pos) {
                            redes[i].generadores.push(newGenerador);
                            if (redes[i].generadoresChange) {
                                redes[i].generadoresChange(redes[i].generadores);
                            }
                            break;
                        }
                    }

                    break;
                }
                case 1: { // update
                    for (var i = 0; i < redes.length; i++) {
                        if (redes[i].id === pos) {
                            for (var j = 0; j < redes[i].generadores.length; j++) {
                                if (redes[i].generadores[j].id === newGenerador.id) {
                                    redes[i].generadores[j] = newGenerador;
                                    break;
                                }
                            }
                            if (redes[i].generadoresChange) {
                                redes[i].generadoresChange(redes[i].generadores);
                            }
                            break;
                        }
                    }

                    break;
                }
                case 2: { // delete
                    for (var i = 0; i < redes.length; i++) {
                        if (redes[i].id === pos) {
                            for (var j = 0; j < redes[i].generadores.length; j++) {
                                if (redes[i].generadores[j].id === newGenerador.id) {
                                    redes[i].generadores.splice(j, 1);
                                    break;
                                }
                            }
                            if (redes[i].generadoresChange) {
                                redes[i].generadoresChange(redes[i].generadores);
                            }
                            break;
                        }
                    }

                    break;
                }
            }

        });

        client.subscribe('bateria', (_, topicName, changeType, pos, value) => {
            const newBateria = JSON.parse(value);

            console.log('Ha passat algo ', changeType, pos, newBateria);
            switch (changeType) {
                case 0: { // add
                    for (var i = 0; i < redes.length; i++) {
                        if (redes[i].id === pos) {
                            var alreadyExists = false;
                            for (var j = 0; j < redes[i].baterias.length; j++) {
                                if (redes[i].baterias[j].id === newBateria.id) {
                                    alreadyExists = true;
                                    break;
                                }
                            }

                            if (!alreadyExists) {
                                redes[i].baterias.push(newBateria);
                            }

                            if (redes[i].bateriasChange) {
                                redes[i].bateriasChange(redes[i].baterias);
                            }
                            break;
                        }
                    }

                    break;
                }
                case 1: { // update
                    for (var i = 0; i < redes.length; i++) {
                        if (redes[i].id === pos) {
                            for (var j = 0; j < redes[i].baterias.length; j++) {
                                if (redes[i].baterias[j].id === newBateria.id) {
                                    redes[i].baterias[j] = newBateria;
                                    break;
                                }
                            }
                            if (redes[i].bateriasChange) {
                                redes[i].bateriasChange(redes[i].baterias);
                            }
                            break;
                        }
                    }

                    break;
                }
                case 2: { // delete
                    for (var i = 0; i < redes.length; i++) {
                        if (redes[i].id === pos) {
                            for (var j = 0; j < redes[i].baterias.length; j++) {
                                if (redes[i].baterias[j].id === newBateria.id) {
                                    redes[i].baterias.splice(j, 1);
                                    break;
                                }
                            }
                            if (redes[i].bateriasChange) {
                                redes[i].bateriasChange(redes[i].baterias);
                            }
                            break;
                        }
                    }

                    break;
                }
            }

        });
    });
};

async function renderRedElectrica(redes) {
    if (!document.getElementById('renderRedElectrica')) {
        return;
    }

    await ReactDOM.unmountComponentAtNode(document.getElementById('renderRedElectrica'));
    ReactDOM.render(
        redes.map((element, i) => {
            return <RedElectrica
                key={i}

                id={element.id}
                name={element.name}
                capacidadElectrica={element.capacidadElectrica}
                energiaActual={element.energiaActual}

                editarRed={() => {
                    editarRedElectrica(element);
                }}
            />
        })
        , document.getElementById('renderRedElectrica'));
};

function editarRedElectrica(red) {
    ReactDOM.unmountComponentAtNode(document.getElementById('renderTab'));
    ReactDOM.render(<RedElectricaForm
        // red
        red={red}
        redChange={(callback) => {
            red.redChange = callback;
        }}
        generadoresChange={(callback) => {
            red.generadoresChange = callback;
        }}
        bateriasChange={(callback) => {
            red.bateriasChange = callback;
        }}
        handleRedesElectricas={tabRedElectrica}
        handleEditRedElectrica={updateRedElectrica}
        handleDeleteRedElectrica={deleteRedElectrica}
        // generador
        handleAddGeneradorRedElectrica={addGeneradorRedElectrica}
        handleUpdateGeneradorRedElectrica={updateGeneradorRedElectrica}
        handleDeleteGeneradorRedElectrica={deleteGeneradorRedElectrica}
        // bateria
        handleAddBateriaRedElectrica={addBateriaRedElectrica}
        handleUpdateBateriaRedElectrica={updateBateriaRedElectrica}
        handleDeleteBateriaRedElectrica={deleteBateriaRedElectrica}
        // bateria - historial
        handleGetBateriaHistorial={getBateriaHistorial}
    />, document.getElementById('renderTab'));
}

function addRedElectrica(red) {
    return new Promise((resolve) => {
        client.emit('electrico', 'add', JSON.stringify(red), (_, response) => {
            resolve();
        });
    });
};

function updateRedElectrica(red) {
    return new Promise((resolve) => {
        client.emit('electrico', 'update', JSON.stringify(red), (_, response) => {
            resolve();
        });
    });
};

function deleteRedElectrica(id) {
    return new Promise((resolve) => {
        client.emit('electrico', 'delete', '' + id, (_, response) => {
            resolve();
        });
    });
};

// generador

function addGeneradorRedElectrica(generador) {
    return new Promise((resolve) => {
        client.emit('electrico', 'addGenerador', JSON.stringify(generador), (_, response) => {
            resolve(parseInt(response));
        });
    });
};

function updateGeneradorRedElectrica(generador) {
    return new Promise((resolve, reject) => {
        client.emit('electrico', 'updateGenerador', JSON.stringify(generador), (_, response) => {
            if (response == "OK") {
                resolve();
            } else {
                reject();
            }
        });
    });
};

function deleteGeneradorRedElectrica(idRed, idGenerador) {
    return new Promise((resolve) => {
        client.emit('electrico', 'deleteGenerador', JSON.stringify({ redElectrica: idRed, id: idGenerador }), (_, response) => {
            resolve(response == "OK");
        });
    });
};

// bateria

function addBateriaRedElectrica(bateria) {
    return new Promise((resolve) => {
        client.emit('electrico', 'addBateria', JSON.stringify(bateria), (_, response) => {
            resolve(parseInt(response));
        });
    });
};

function updateBateriaRedElectrica(bateria) {
    return new Promise((resolve, reject) => {
        client.emit('electrico', 'updateBateria', JSON.stringify(bateria), (_, response) => {
            if (response == "OK") {
                resolve();
            } else {
                reject();
            }
        });
    });
};

function deleteBateriaRedElectrica(idRed, idBateria) {
    return new Promise((resolve) => {
        client.emit('electrico', 'deleteBateria', JSON.stringify({ redElectrica: idRed, id: idBateria }), (_, response) => {
            resolve(response == "OK");
        });
    });
};

// bateria - historial

function getBateriaHistorial(idRed, idBateria) {
    var historial = [];
    var continuar = true;
    const limit = 10;

    return new Promise(async (finish) => {
        do {
            await new Promise((resolve) => {
                client.emit('electrico', 'getBateriaHist', JSON.stringify({ "redElectrica": idRed, "bateria": idBateria, "offset": historial.length, "limit": limit }), async (_, response) => {
                    const chunk = JSON.parse(response);
                    if (chunk.length != limit)
                        continuar = false;

                    historial = historial.concat(chunk);
                    await ReactDOM.render(historial.map((element, i) => {
                        return <BateriaHistorial
                            key={i}

                            id={element.id}
                            tiempo={element.tiempo}
                            cargaActual={element.cargaActual}
                        />
                    }), document.getElementById("renderBateriaHistorial"));



                    resolve();
                });
            });

        }
        while (continuar);

        finish(historial);
    });
};

/* ORDENES DE MINADO */

async function tabOrdenesMinado() {
    client.unsubscribeAll();

    ReactDOM.unmountComponentAtNode(document.getElementById('renderTab'));
    ReactDOM.render(<OrdenesMinado
        getOrdenes={getOrdenesMinado}
        localizarRobots={localizarRobots}
        getRobotName={getRobotName}

        handleAdd={addOrdenMinado}
        addOrdenesMinadoArray={addOrdenesMinadoArray}
        handleEdit={updateOrdenesMinado}
        handleDelete={deleteOrdenesMinado}
        handleInventario={getOrdenMinadoInventario}
        ordenMinadoInventarioPush={ordenMinadoInventarioPush}
        getOrdenMinadoInventarioArticuloImg={getArticuloImg}
        tabOrdenesMinadoPush={tabOrdenesMinadoPush}
    />, document.getElementById('renderTab'));
};

function tabOrdenesMinadoPush(callback) {
    client.unsubscribeAll();
    client.subscribe('ordenMinado', (_, topicName, changeType, pos, value) => {
        var newOrdenMinado;
        if (changeType != 2) {
            newOrdenMinado = JSON.parse(value);
        }

        callback(changeType, pos, newOrdenMinado);
    });
};

function getOrdenesMinado(query) {
    return new Promise((resolve) => {
        client.emit('ordenMinado', 'get', JSON.stringify(query), (_, response) => {
            resolve(JSON.parse(response));
        });
    });
};

function addOrdenMinado(orden) {
    return new Promise((resolve, reject) => {
        client.emit('ordenMinado', 'add', JSON.stringify(orden), (_, response) => {
            if (response == "OK") {
                resolve();
            } else {
                reject();
            }
        });
    });
};

function addOrdenesMinadoArray(array) {
    return new Promise((resolve, reject) => {
        client.emit('ordenMinado', 'addArray', JSON.stringify(array), (_, response) => {
            if (response == "OK") {
                resolve();
            } else {
                reject();
            }
        });
    });
};

function updateOrdenesMinado(orden) {
    return new Promise((resolve, reject) => {
        client.emit('ordenMinado', 'update', JSON.stringify(orden), (_, response) => {
            if (response == "OK") {
                resolve();
            } else {
                reject();
            }
        });
    });
};

function deleteOrdenesMinado(ordenId) {
    return new Promise((resolve) => {
        client.emit('ordenMinado', 'delete', '' + ordenId, (_, response) => {
            resolve();
        });
    });
};

function localizarRobots() {
    return new Promise((resolve) => {
        client.emit('robot', 'localizar', '', (_, response) => {
            resolve(JSON.parse(response));
        });
    });
};

function getRobotName(robotId) {
    return new Promise((resolve) => {
        client.emit('robot', 'name', '' + robotId, (_, response) => {
            resolve(response);
        });
    });
};

function getOrdenMinadoInventario(ordenId) {
    return new Promise((resolve, reject) => {
        client.emit('ordenMinado', 'getInventario', '' + ordenId, (_, response) => {
            if (response === "ERR") {
                reject();
            } else {
                resolve(JSON.parse(response));
            }
        });
    });
};

function ordenMinadoInventarioPush(ordenId, callback) {
    client.unsubscribe("ordenMinadoInventario");
    client.subscribe('ordenMinadoInventario', (_, topicName, changeType, pos, value) => {
        if (changeType != 1 || pos !== ordenId) {
            return;
        }

        callback(JSON.parse(value));
    });
};

/* AJUSTES */

async function tabAjustes() {
    await ReactDOM.unmountComponentAtNode(document.getElementById('renderTab'));
    ReactDOM.render(<Ajustes
        handleGet={getAjustes}
        handleAdd={addAjustes}
        handleUpdate={updateAjustes}
        handleActivar={activarAjustes}
        handleLimpiar={ajustesEjecutarLimpieza}
        handleEliminar={deleteAjustes}
        tabAjustesPush={tabAjustesPush}
        pwdAjuste={pwdAjuste}
    />, document.getElementById('renderTab'));

};

function ajustesEjecutarLimpieza() {
    return new Promise((resolve) => {
        client.emit('ajustes', 'limpiar', '', (_, response) => {
            resolve();
        });
    });
};

function getAjustes() {
    return new Promise((resolve) => {
        client.emit('ajustes', 'get', '', (_, response) => {
            resolve(JSON.parse(response));
        });
    });
};

function tabAjustesPush(callback) {
    client.unsubscribeAll();
    client.subscribe('config', (_, topicName, changeType, pos, value) => {
        var newAjuste;
        if (changeType != 2) {
            newAjuste = JSON.parse(value);
        }

        callback(changeType, pos, newAjuste);
    });
};

function addAjustes(ajuste) {
    return new Promise((resolve) => {
        client.emit('ajustes', 'add', JSON.stringify(ajuste), (_, response) => {
            resolve(parseInt(response));
        });
    });
};

function updateAjustes(ajuste) {
    return new Promise((resolve) => {
        client.emit('ajustes', 'update', JSON.stringify(ajuste), (_, response) => {
            resolve();
        });
    });
};

function deleteAjustes(ajusteId) {
    return new Promise((resolve) => {
        client.emit('ajustes', 'delete', '' + ajusteId, (_, response) => {
            resolve();
        });
    });
};

function activarAjustes(ajusteId) {
    return new Promise((resolve) => {
        client.emit('ajustes', 'activar', '' + ajusteId, (_, response) => {
            resolve();
        });
    });
};

function pwdAjuste(idAjuste, pwd) {
    return new Promise((resolve) => {
        client.emit('ajustes', 'pwd', JSON.stringify({
            id: idAjuste,
            pwd: pwd
        }), (_, response) => {
            resolve();
        });
    });
};

// API KEY

function tabApiKey() {
    ReactDOM.unmountComponentAtNode(document.getElementById('renderTab'));
    ReactDOM.render(<ApiKeys
        getKeys={getApiKeys}
        handleApiKeysChange={subscribeApiKeys}
        addKey={addApiKey}
        resetKey={resetApiKey}
        deleteKey={deleteApiKey}
    />, document.getElementById('renderTab'));
};

function getApiKeys() {
    return new Promise((resolve) => {
        client.emit('apiKey', 'get', '', (_, response) => {
            const apiKeys = JSON.parse(response);
            resolve(apiKeys);

        });
    });
};

function subscribeApiKeys(callback) {
    client.unsubscribeAll();
    client.subscribe('apiKeys', callback);
};

function addApiKey(name) {
    return new Promise((resolve) => {
        client.emit('apiKey', 'add', name, (_, response) => {
            resolve();
        });
    });
};

function resetApiKey(uuid) {
    return new Promise((resolve) => {
        client.emit('apiKey', 'reset', uuid, (_, response) => {
            resolve();
        });
    });
};

function deleteApiKey(uuid) {
    return new Promise((resolve) => {
        client.emit('apiKey', 'delete', uuid, (_, response) => {
            resolve();
        });
    });
};

// SERVERS

function tabServidores() {
    ReactDOM.unmountComponentAtNode(document.getElementById('renderTab'));
    ReactDOM.render(<Servers
        getServers={getServidores}
        handleApiKeysChange={subscribeServers}
        handleAdd={addServer}
        handleUpdate={updateServer}
        handleDelete={deleteServer}
        handlePwd={pwdAutoregServer}
        getAjustes={getAjustes}
    />, document.getElementById('renderTab'));
};

function getServidores() {
    return new Promise((resolve) => {
        client.emit('server', 'get', '', (_, response) => {
            resolve(JSON.parse(response));
        });
    });
};

function subscribeServers(callback) {
    client.unsubscribeAll();
    client.subscribe('servers', callback);
};

function addServer(server) {
    return new Promise((resolve) => {
        client.emit('server', 'add', JSON.stringify(server), (_, response) => {
            resolve();
        });
    });
};

function updateServer(server) {
    return new Promise((resolve) => {
        client.emit('server', 'update', JSON.stringify(server), (_, response) => {
            resolve();
        });
    });
};

function deleteServer(uuid) {
    return new Promise((resolve) => {
        client.emit('server', 'delete', uuid, (_, response) => {
            resolve();
        });
    });
};

function pwdAutoregServer(uuid, pwd) {
    client.emit('server', 'pwd', JSON.stringify({ uuid, pwd }), (_, response) => {
        console.log(response);
    });
};

/* ALMACÉN */

function tabAlmacen() {
    ReactDOM.unmountComponentAtNode(document.getElementById('renderTab'));
    ReactDOM.render(<Almacenes
        getAlmacenes={getAlmacenes}
        getAlmacenInventario={getAlmacenInventario}
        almacenInventarioPush={almacenInventarioPush}
        getArticuloImg={getArticuloImg}
        tabAlmacenPush={tabAlmacenPush}

        handleAdd={addAlmacen}
        handleEdit={editAlmacen}
        handleDelete={deleteAlmacen}

        getAlmacenNotificaciones={getAlmacenNotificaciones}
        addAlmacenNotificaciones={addAlmacenNotificaciones}
        deleteAlmacenNotificaciones={deleteAlmacenNotificaciones}
        getArticulos={localizarArticulos}

        getAlmacenStorageCells={getAlmacenStorageCells}
        addAlmacenStorageCell={addAlmacenStorageCell}
        deleteAlmacenStorageCell={deleteAlmacenStorageCell}
    />, document.getElementById('renderTab'));
};

function tabAlmacenPush(callback) {
    client.unsubscribeAll();
    client.subscribe('almacen', (_, topicName, changeType, pos, value) => {
        var newAlmacen;
        if (changeType != 2) {
            newAlmacen = JSON.parse(value);
        }

        callback(changeType, pos, newAlmacen);
    });
};

function getAlmacenes() {
    return new Promise((resolve) => {
        client.emit('almacen', 'get', '', (_, response) => {
            resolve(JSON.parse(response));
        });
    });
};

function addAlmacen(almacen) {
    return new Promise((resolve) => {
        client.emit('almacen', 'add', JSON.stringify(almacen), (_, response) => {
            resolve(response);
        });
    });
};

function editAlmacen(almacen) {
    return new Promise((resolve) => {
        client.emit('almacen', 'edit', JSON.stringify(almacen), (_, response) => {
            resolve(response);
        });
    });
};

function deleteAlmacen(idAlmacen) {
    return new Promise((resolve) => {
        client.emit('almacen', 'delete', '' + idAlmacen, (_, response) => {
            resolve(response);
        });
    });
};

function getAlmacenInventario(idAlmacen) {
    return new Promise((resolve) => {
        client.emit('almacen', 'inventario', '' + idAlmacen, (_, response) => {
            resolve(JSON.parse(response));
        });
    });
};

function almacenInventarioPush(id, callback) {
    client.subscribe('almacenInv#' + id, async (_, topicName, changeType, pos, value) => {
        if (changeType != 1) {
            return;
        }

        callback(JSON.parse(value));
    });
};

function getAlmacenNotificaciones(id) {
    return new Promise((resolve) => {
        client.emit('almacen', 'getNotificaciones', '' + id, (_, response) => {
            resolve(JSON.parse(response));
        });
    });
};

function addAlmacenNotificaciones(notificacion) {
    return new Promise((resolve, reject) => {
        client.emit('almacen', 'addNotificacion', JSON.stringify(notificacion), (_, response) => {
            if (response == "OK") {
                resolve();
            } else {
                reject();
            }
        });
    });
};

function deleteAlmacenNotificaciones(idAlmacen, id) {
    return new Promise((resolve, reject) => {
        client.emit('almacen', 'deleteNotificacion', JSON.stringify({ idAlmacen, id }), (_, response) => {
            if (response == "OK") {
                resolve();
            } else {
                reject();
            }
        });
    });
};

function getAlmacenStorageCells(idAlmacen) {
    return new Promise((resolve) => {
        client.emit('almacen', 'getAE2StorageCells', '' + idAlmacen, (_, response) => {
            resolve(JSON.parse(response));
        });
    });
};

function addAlmacenStorageCell(storageCell) {
    return new Promise((resolve, reject) => {
        client.emit('almacen', 'addAE2StorageCell', JSON.stringify(storageCell), (_, response) => {
            if (response == "OK") {
                resolve();
            } else {
                reject();
            }
        });
    });
};

function deleteAlmacenStorageCell(idAlmacen, id) {
    return new Promise((resolve, reject) => {
        client.emit('almacen', 'deleteAE2StorageCell', JSON.stringify({ idAlmacen, id }), (_, response) => {
            if (response == "OK") {
                resolve();
            } else {
                reject();
            }
        });
    });
};

/* DRONES */

async function tabDrones() {
    await ReactDOM.unmountComponentAtNode(document.getElementById('renderTab'));
    ReactDOM.render(<Drones
        handleAddDrone={nuevoDrone}
        handleSearch={searchDrone}
    />, document.getElementById('renderTab'));

    getDrones();
};

function getDrones() {
    client.emit('drone', 'get', '', (_, response) => {
        const drones = JSON.parse(response);
        renderDrones(drones);

        client.unsubscribeAll();
        client.subscribe('drones', (_, topicName, changeType, pos, value) => {
            var newDrone;
            if (changeType != 2) {
                newDrone = JSON.parse(value);
            }
            console.log('Ha passat algo ', changeType, pos, newDrone);
            switch (changeType) {
                case 0: { // add
                    drones.push(newDrone);

                    break;
                }
                case 1: { // update
                    for (var i = 0; i < drones.length; i++) {
                        if (drones[i].id === pos) {
                            newDrone.droneChange = drones[i].droneChange;
                            if (drones[i].droneChange != null) {
                                drones[i].droneChange(newDrone);
                            }

                            drones[i] = newDrone;
                            break;
                        }
                    }

                    break;
                }
                case 2: { // delete
                    for (var i = 0; i < drones.length; i++) {
                        if (drones[i].id === pos) {
                            if (drones[i].droneChange != null) {
                                drones[i].droneChange(null);
                            }

                            drones.splice(i, 1);
                            break;
                        }
                    }

                    break;
                }
            }

            renderDrones(drones);
        });
    });
};

function searchDrone(text) {
    if (text === '')
        return getDrones();
    client.emit('drone', 'search', text, (_, response) => {
        renderDrones(JSON.parse(response));
    });
};

function renderDrones(drones) {
    if (!document.getElementById('renderDrones')) {
        return;
    }

    ReactDOM.unmountComponentAtNode(document.getElementById('renderDrones'));
    ReactDOM.render(
        drones.map((element, i) => {
            return <Drone
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
                    editarDrone(element);
                }}
            />
        })
        , document.getElementById('renderDrones'));
};

async function nuevoDrone() {
    await ReactDOM.unmountComponentAtNode(document.getElementById('renderTab'));
    ReactDOM.render(<DroneForm
        handleCancelar={tabDrones}
        handleAddDrone={addDrone}
        handleEditDrone={updateDrone}
    />, document.getElementById('renderTab'));
}

async function editarDrone(drone) {
    ReactDOM.unmountComponentAtNode(document.getElementById('renderTab'));
    ReactDOM.render(<DroneForm
        drone={drone}

        droneChange={(callback) => {
            drone.droneChange = callback;
        }}
        handleCancelar={tabDrones}
        handleAddDrone={addDrone}
        handleEditDrone={updateDrone}
        handleEliminar={deleteDrone}
        handleLogs={getDroneLogs}
        handleDeleteLogs={deleteDroneLogs}
        handleClearLogs={clearDroneLogs}
    />, document.getElementById('renderTab'));

    await getDroneInventario(drone);
    renderDroneGPS(drone);
};

function getDroneInventario(drone) {
    return new Promise((resolve) => {
        client.emit('drone', 'getInventario', '' + drone.id, async (_, response) => {
            const inventario = JSON.parse(response);
            await renderDroneInventario(inventario);

            resolve();
        });

        client.subscribe('droneInv#' + drone.id, (_, topicName, changeType, pos, value) => {
            if (changeType != 1) {
                return;
            }

            const inventario = JSON.parse(value);
            renderDroneInventario(inventario);
        });
    });
};

function renderDroneInventario(inventario) {
    return new Promise(async (resolve) => {
        await ReactDOM.render(inventario.map((element, i) => {
            return <DroneFormSlotInventario
                key={i}

                numeroSlot={element.numeroSlot}
                cant={element.cant}
                articulo={element.articulo}
            />
        }), document.getElementById('inventarioContenido'));

        const inventarioImg = inventario.filter((value, index, self) => {
            return self.indexOf(value) === index;
        });
        for (let i = 0; i < inventarioImg.length; i++) {
            const articulo = inventarioImg[i].articulo;

            if (articulo != null) {
                await new Promise((resolve) => {
                    client.on("artImg", (_, response) => {
                        if (response.message.size) {
                            const img = document.getElementsByClassName("art_img_" + articulo.id);
                            if (img.length > 0) {
                                const src = URL.createObjectURL(response.message);

                                for (let j = 0; j < img.length; j++) {
                                    img[j].src = src;
                                }
                            }

                        }

                        resolve();
                    });
                    client.emit('articulos', 'getImg', '' + articulo.id);
                });
            }
        }

        resolve();
    });
};

async function renderDroneGPS(drone) {
    var listaGPS = [];
    var continuar = true;
    const limit = 10;
    var offset = 0;

    do {
        await new Promise((resolve) => {
            client.emit('drone', 'getGPS', JSON.stringify({ "droneId": drone.id, "offset": offset, "limit": limit }), async (_, response) => {
                const gps = JSON.parse(response);
                if (gps.length == 0) {
                    continuar = false;
                    resolve();
                    return;
                }
                listaGPS = listaGPS.concat(gps);

                ReactDOM.unmountComponentAtNode(document.getElementById('droneGPS'));
                ReactDOM.render(listaGPS.map((element, i) => {
                    return <DroneFormGPS
                        key={i}

                        tiempo={element.tiempo}
                        posX={element.posX}
                        posY={element.posY}
                        posZ={element.posZ}
                    />
                }), document.getElementById('droneGPS'));

                offset += limit;
                resolve();
            });
        });
    }
    while (continuar);

    client.subscribe('droneGPS#' + drone.id, async (_, topicName, changeType, pos, value) => {
        if (changeType != 0) {
            return;
        }

        const gps = JSON.parse(value);
        listaGPS.unshift(gps);

        await ReactDOM.unmountComponentAtNode(document.getElementById('droneGPS'));
        ReactDOM.render(listaGPS.map((element, i) => {
            return <DroneFormGPS
                key={i}

                tiempo={element.tiempo}
                posX={element.posX}
                posY={element.posY}
                posZ={element.posZ}
            />
        }), document.getElementById('droneGPS'));
    });
};

function addDrone(drone) {
    return new Promise((resolve, reject) => {
        client.emit('drone', 'add', JSON.stringify(drone), (_, response) => {
            if (response === "ERR") {
                reject();
            } else {
                resolve(parseInt(response));
            }
        });
    });
};

function updateDrone(drone) {
    return new Promise((resolve, reject) => {
        client.emit('drone', 'update', JSON.stringify(drone), (_, response) => {
            if (response === "ERR") {
                reject();
            } else {
                resolve();
            }
        });
    });
};

function deleteDrone(idDrone) {
    return new Promise((resolve, reject) => {
        client.emit('drone', 'delete', '' + idDrone, (_, response) => {
            if (response === "ERR") {
                reject();
            } else {
                resolve();
            }
        });
    });
};

async function getDroneLogs(idDrone, start, end) {
    var logs = [];
    const limit = 10;
    var offset = 0;
    var continueDownloading = true;
    console.log({
        idDrone,
        start,
        end,
        limit,
        offset
    });

    do {
        await new Promise((resolve) => {
            client.emit('drone', 'getLog', JSON.stringify({
                idDrone,
                start,
                end,
                limit,
                offset
            }), (_, response) => {

                const logsPart = JSON.parse(response);
                logs = logs.concat(logsPart);
                continueDownloading = (logsPart.length == limit);

                ReactDOM.render(logs.map((element, i) => {
                    return <DroneLog
                        key={i}

                        id={element.id}
                        name={element.name}
                        mensaje={element.mensaje}
                    />
                }), document.getElementById("renderDroneLogs"));

                resolve();
            });
        });

        offset += limit;
    } while (continueDownloading);

    client.subscribe('droneLog#' + idDrone, async (_, topicName, changeType, pos, value) => {
        var newDroneLog;
        if (changeType != 2) {
            newDroneLog = JSON.parse(value);
        }

        switch (changeType) {
            case 0: {// add
                logs.unshift(newDroneLog);

                await ReactDOM.unmountComponentAtNode(document.getElementById('renderDroneLogs'));
                ReactDOM.render(logs.map((element, i) => {
                    return <DroneLog
                        key={i}

                        id={element.id}
                        name={element.name}
                        mensaje={element.mensaje}
                    />
                }), document.getElementById("renderDroneLogs"));

                break;
            }
            case 2: {// delete
                getDroneLogs(idDrone, start, end);

                break;
            }
        }

    });
};

function clearDroneLogs(idDrone) {
    client.emit('drone', 'clearLogs', '' + idDrone, (_, response) => {

    });
};

function deleteDroneLogs(idDrone, start, end) {
    client.emit('drone', 'clearLogsBetween', JSON.stringify({ idDrone, start, end }), (_, response) => {

    });
};

/* MOVIMIENTOS DE ALMACÉN */

async function tabMovimientoAlmacen() {
    client.unsubscribeAll();

    await ReactDOM.unmountComponentAtNode(document.getElementById('renderTab'));
    await ReactDOM.render(<MovimientosAlmacen
        getAlmacenes={localizarAlmacenes}
        getArticulos={localizarArticulos}
        handleBuscar={searchMovimientosAlmacen}
        addMovimientoAlmacen={addMovimientoAlmacen}
    />, document.getElementById('renderTab'));

    getMovimientosAlmacen();
};

function getMovimientosAlmacen() {
    client.emit('movAlmacen', 'get', '', (_, response) => {
        renderMovimientosAlmacen(JSON.parse(response));
    });
};

function searchMovimientosAlmacen(query) {
    client.emit('movAlmacen', 'search', JSON.stringify(query), (_, response) => {
        renderMovimientosAlmacen(JSON.parse(response));
    });
};

async function renderMovimientosAlmacen(movimientos) {
    ReactDOM.unmountComponentAtNode(document.getElementById('renderMovimientos'));
    await ReactDOM.render(movimientos.map((element, i) => {
        return <MovimientoAlmacen
            key={i}

            movimiento={element}
            handleSelect={() => {
                editMovimientoAlmacen(element);
            }}
        />
    }), document.getElementById('renderMovimientos'));

    const almNameCache = {};
    for (let i = 0; i < movimientos.length; i++) {
        if (almNameCache[movimientos[i].almacen]) {
            movimientos[i].almacenName = almNameCache[movimientos[i].almacen];
        } else {
            const name = await getAlmacenName(movimientos[i].almacen);
            almNameCache[movimientos[i].almacen] = name;
            movimientos[i].almacenName = name;
        }
    }

    const artNameCache = {};
    for (let i = 0; i < movimientos.length; i++) {
        if (artNameCache[movimientos[i].articulo]) {
            movimientos[i].articuloName = artNameCache[movimientos[i].articulo];
        } else {
            const name = await getArticuloName(movimientos[i].articulo);
            artNameCache[movimientos[i].articulo] = name;
            movimientos[i].articuloName = name;
        }
    }

    ReactDOM.render(movimientos.map((element, i) => {
        return <MovimientoAlmacen
            key={i}

            movimiento={element}
            handleSelect={() => {
                editMovimientoAlmacen(element);
            }}
        />
    }), document.getElementById('renderMovimientos'));
};

function editMovimientoAlmacen(movimiento) {
    ReactDOM.unmountComponentAtNode(document.getElementById('renderMovimientoModal'));
    ReactDOM.render(<MovimientosAlmacenFormDetails
        movimiento={movimiento}
        updateMovimientoAlmacen={updateMovimientoAlmacen}
    />, document.getElementById('renderMovimientoModal'));
};

function updateMovimientoAlmacen(movimiento) {
    return new Promise((resolve) => {
        client.emit('movAlmacen', 'update', JSON.stringify(movimiento), (_, response) => {
            resolve(response);
        });
    });
};

function getArticuloName(id) {
    return new Promise((resolve) => {
        client.emit('articulos', 'name', '' + id, (_, response) => {
            resolve(response);
        });
    });
};

function getAlmacenName(id) {
    return new Promise((resolve) => {
        client.emit('almacen', 'name', '' + id, (_, response) => {
            resolve(response);
        });
    });
};

function localizarArticulos() {
    return new Promise((resolve) => {
        client.emit('articulos', 'localizar', '', (_, response) => {
            resolve(JSON.parse(response));
        });
    });
};

function localizarAlmacenes() {
    return new Promise((resolve) => {
        client.emit('almacen', 'localizar', '', (_, response) => {
            resolve(JSON.parse(response));
        });
    });
};

function addMovimientoAlmacen(movAlmacen) {
    return new Promise((resolve, reject) => {
        client.emit('movAlmacen', 'add', JSON.stringify(movAlmacen), (_, response) => {
            if (response == "OK") {
                resolve();
            } else {
                reject();
            }
        });
    });
};

/* NOTIFICACIONES */

async function tabNotificaciones() {
    await ReactDOM.unmountComponentAtNode(document.getElementById('renderNotificacionesModal'));
    await ReactDOM.render(<Notificaciones
        getNotificaciones={getNotificaciones}
        notificacionesLeidas={notificacionesLeidas}
        onNotificaciones={onNotificaciones}
    />, document.getElementById('renderNotificacionesModal'));
};

function getNotificaciones() {
    return new Promise((resolve) => {
        client.emit('notificacion', 'get', '', (_, response) => {
            resolve(JSON.parse(response));
        });
    });
};

function notificacionesLeidas() {
    return new Promise((resolve) => {
        client.emit('notificacion', 'leidas', '', (_, response) => {
            resolve();
        });
    });
};

function onNotificaciones(callback) {
    client.subscribe('notificaciones', (_, topicName, changeType, pos, value) => {
        if (pos == 0) {
            callback(value);
        }
    });
};



main();


