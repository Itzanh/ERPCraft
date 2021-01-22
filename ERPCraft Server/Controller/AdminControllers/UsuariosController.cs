using ERPCraft_Server.Models.DB;
using ERPCraft_Server.Storage;
using Newtonsoft.Json;
using System;

namespace ERPCraft_Server.Controller.AdminControllers
{
    public static class UsuariosController
    {
        public static string usuarioCommands(DBStorage db, string command, string message)
        {
            switch (command)
            {
                case "get":
                    {
                        return getUsuarios(db);
                    }
                case "search":
                    {
                        return searchUsuarios(db, message);
                    }
                case "add":
                    {
                        return addUsuario(db, message);
                    }
                case "edit":
                    {
                        return editUsuario(db, message);
                    }
                case "delete":
                    {
                        return deleteUsuario(db, message);
                    }
                case "pwd":
                    {
                        return passwordUsuario(db, message);
                    }
            }

            return "ERR";
        }

        private static string getUsuarios(DBStorage db)
        {
            return JsonConvert.SerializeObject(db.getUsuarios());
        }

        private static string searchUsuarios(DBStorage db, string message)
        {
            UsuarioQuery query;
            try
            {
                query = (UsuarioQuery)JsonConvert.DeserializeObject(message, typeof(UsuarioQuery));
            }
            catch (Exception) { return "ERR"; }
            if (query == null || query.text == null)
                return "ERR";

            return JsonConvert.SerializeObject(db.searchUsuarios(query.text, query.off));
        }

        private static string addUsuario(DBStorage db, string message)
        {
            UsuarioLogin usuario;
            try
            {
                usuario = (UsuarioLogin)JsonConvert.DeserializeObject(message, typeof(UsuarioLogin));
            }
            catch (Exception) { return "ERR"; }
            if (usuario == null || !usuario.isValid())
                return "ERR";

            string salt = Usuario.generateSalt();
            string hash = Usuario.hash(salt + usuario.pwd, Program.ajuste.hashIteraciones);

            Usuario usr = new Usuario(usuario.name, hash, salt, Program.ajuste.hashIteraciones);

            return db.addUsuario(usr) ? "OK" : "ERR";
        }

        private static string editUsuario(DBStorage db, string message)
        {
            UsuarioEdit usuario;
            try
            {
                usuario = (UsuarioEdit)JsonConvert.DeserializeObject(message, typeof(UsuarioEdit));
            }
            catch (Exception e) { Console.WriteLine(e); return "ERR"; }
            if (usuario == null || !usuario.isValid())
                return "ERR";

            return db.updateUsuario(usuario) ? "OK" : "ERR";
        }

        private static string deleteUsuario(DBStorage db, string message)
        {
            short usuarioId;
            try
            {
                usuarioId = Int16.Parse(message);
            }
            catch (Exception) { return "ERR"; }
            // no permitir borrar IDs incorrectos (0, -1, etc), y no permitir borrar el usuario 1
            if (usuarioId <= 1)
                return "ERR";

            return db.deleteUsuario(usuarioId) ? "OK" : "ERR";
        }

        private static string passwordUsuario(DBStorage db, string message)
        {
            UsuarioPwd usuario;
            try
            {
                usuario = (UsuarioPwd)JsonConvert.DeserializeObject(message, typeof(UsuarioPwd));
            }
            catch (Exception e) { Console.WriteLine(e); return "ERR"; }
            if (usuario == null || !usuario.isValid())
                return "ERR";

            Usuario usr = db.getUsuario(usuario.id);
            if (usr == null)
                return "ERR";

            bool loggedIn = Usuario.verifyHash(usr.pwd, (usr.salt + usuario.pwdActual), usr.iteraciones);
            if (!loggedIn)
                return "ERR";

            string salt = Usuario.generateSalt();
            string hash = Usuario.hash(salt + usuario.pwdNuevo, Program.ajuste.hashIteraciones);

            return db.updateUsuario(usuario.id, hash, salt, Program.ajuste.hashIteraciones) ? "OK" : "ERR";
        }

    }
}
