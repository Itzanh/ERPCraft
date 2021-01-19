using ERPCraft_Server.Controller.AdminControllers;
using serverHashes;

namespace ERPCraft_Server.Controller
{
    /// <summary>
    /// Clase que añade los escuchadores de eventos por NetEvent.IO a los clientes de la página web.
    /// Estas funciones permiten hacer CRUD a datos o ejecutar acciones de manera concurrente.
    /// </summary>
    public static class AdminController
    {
        public static void addEventListeners(NetEventIO clientIO)
        {
            clientIO.on("usuarios", (NetEventIO sender, OnMessageEventArgs msg, NetEventIO.Callback callback) =>
            {
                callback(UsuariosController.usuarioCommands(sender.db, msg.message.command, msg.message.message));
            });

            clientIO.on("robot", (NetEventIO sender, OnMessageEventArgs msg, NetEventIO.Callback callback) =>
            {
                callback(RobotController.robotCommands(sender.db, msg.message.command, msg.message.message));
            });

            clientIO.on("ordenMinado", (NetEventIO sender, OnMessageEventArgs msg, NetEventIO.Callback callback) =>
            {
                callback(OrdenMinadoController.ordenesCommand(sender.db, msg.message.command, msg.message.message));
            });

            clientIO.on("articulos", (NetEventIO sender, OnMessageEventArgs msg, NetEventIO.Callback callback) =>
            {
                callback(ArticulosController.articulosCommand(sender.db, msg.message.command, msg.message.message));
            });

            clientIO.on("articulos", (NetEventIO sender, OnMessageEventArgs msg) =>
            {
                switch (msg.message.command)
                {
                    case "getImg":
                        {
                            ArticulosController.getArticuloImg(sender, msg.message.message);
                            break;
                        }
                }
            });

            clientIO.on("articuloSetImg", (NetEventIO sender, BinaryMessage msg) =>
            {
                ArticulosController.setArticuloImg(sender.db, msg);
            });

            clientIO.on("electrico", (NetEventIO sender, OnMessageEventArgs msg, NetEventIO.Callback callback) =>
            {
                callback(RedElectricaController.redElectricaCommand(sender.db, msg.message.command, msg.message.message));
            });

            clientIO.on("almacen", (NetEventIO sender, OnMessageEventArgs msg, NetEventIO.Callback callback) =>
            {
                callback(AlmacenController.almacenCommand(sender.db, msg.message.command, msg.message.message));
            });

            clientIO.on("ajustes", (NetEventIO sender, OnMessageEventArgs msg, NetEventIO.Callback callback) =>
            {
                callback(AjustesController.ajustesCommand(sender.db, msg.message.command, msg.message.message));
            });

            clientIO.on("apiKey", (NetEventIO sender, OnMessageEventArgs msg, NetEventIO.Callback callback) =>
            {
                callback(AjustesController.apiKeyCommand(sender.db, msg.message.command, msg.message.message));
            });

            clientIO.on("server", (NetEventIO sender, OnMessageEventArgs msg, NetEventIO.Callback callback) =>
            {
                callback(AjustesController.serverCommand(sender.db, msg.message.command, msg.message.message));
            });

            clientIO.on("drone", (NetEventIO sender, OnMessageEventArgs msg, NetEventIO.Callback callback) =>
            {
                callback(DroneController.droneCommands(sender.db, msg.message.command, msg.message.message));
            });

            clientIO.on("movAlmacen", (NetEventIO sender, OnMessageEventArgs msg, NetEventIO.Callback callback) =>
            {
                callback(MovimientosAlmacenController.movimientosAlmacenCommands(sender.db, msg.message.command, msg.message.message));
            });

            clientIO.on("notificacion", (NetEventIO sender, OnMessageEventArgs msg, NetEventIO.Callback callback) =>
            {
                callback(NotificacionesController.notificacionesCommands(sender.db, msg.message.command, msg.message.message));
            });

        }

        

    }
}
