using ERPCraft_Server.Models.DB;
using ERPCraft_Server.Models.DB.Robots;
using ERPCraft_Server.Models.Server;
using System;
using System.Collections.Generic;
using Npgsql;
using ERPCraft_Server.Models.DB.Electrico;
using Newtonsoft.Json;
using ERPCraft_Server.Models.DB.Almacen;
using ERPCraft_Server.Models.DB.Drones;
using System.IO;
using System.Reflection;
using System.Text;

namespace ERPCraft_Server.Storage
{
    public class DBStorage : IDisposable
    {
        private NpgsqlConnection conn;

        public DBStorage(Configuracion config)
        {
            this.conn = new NpgsqlConnection("Host=" + config.dbhost + ";Username=" + config.dbuser + ";Database=" + config.dbname + ";Password=" + config.dbpassword);
            conn.Open();
            conn.ChangeDatabase(config.dbname);
        }

        public void Dispose()
        {
            conn.Close();
            conn.Dispose();
        }

        public void updateAllOffline()
        {
            string sql = "UPDATE robots SET estado = 'F'";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.ExecuteNonQuery();

            sql = "UPDATE servers SET online = false";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Makes sure there is always an user with the user ID 1 created to work as an admin.
        /// If it doesn't exist, create it with the default parameters.
        /// </summary>
        public void initUsers()
        {
            string sql = "SELECT id, off FROM usuarios WHERE id = 1";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (rdr.HasRows)
            {
                rdr.Read();
                bool off = rdr.GetBoolean(1);
                if (off) // of the default user (ID 1) is off, set it to ON again.
                {
                    rdr.Close();

                    sql = "UPDATE usuarios SET off = false WHERE id = 1";
                    cmd = new NpgsqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    return;
                }
                else
                {
                    rdr.Close();
                    return;
                }
            }
            rdr.Close();
            // the default user is not set up, create the user ID 1
            string salt = Usuario.generateSalt();
            string hash = Usuario.hash(salt + "admin1234", Program.ajuste.hashIteraciones);
            sql = "INSERT INTO public.usuarios(id, name, pwd, salt, iteraciones, ultima_con, dsc, off) VALUES (1, 'admin', @pwd, @salt, @iteraciones, CURRENT_TIMESTAMP(3), '', false)";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pwd", hash);
            cmd.Parameters.AddWithValue("@salt", salt);
            cmd.Parameters.AddWithValue("@iteraciones", Program.ajuste.hashIteraciones);
            if (cmd.ExecuteNonQuery() > 0)
            {
                Console.WriteLine("Created the default system user 'admin' for web access. Check the 'How to install' page to learn more.");
            }
        }

        /// <summary>
        /// Imports the database schema into a white Postgresql database.
        /// </summary>
        public void createDB()
        {
            string sql = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public'";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            int rows = rdr.GetInt16(0);
            rdr.Close();

            // if the tables exist, don't import the schema
            if (rows > 0)
                return;

            //  read the SQL file that creates the schema from an embedded file
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ERPCraft_Server.Embedded.db_create.sql");
            TextReader tr = new StreamReader(stream);
            string schema = tr.ReadToEnd();
            rdr.Close();
            stream.Close();

            Console.WriteLine("*** THE DATABASE IS EMPTY. CREATING THE SCHEMA INTO THE DATABASE. ***");
            cmd = new NpgsqlCommand(schema, conn);
            if (cmd.ExecuteNonQuery() == 0)
            {
                Console.WriteLine("THERE WAS AN ERROR CREATING THE DATABASE.");
                return;
            }

            Console.WriteLine("*** DB CREATED OK ***");

            sql = "SET search_path TO public";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        public void limpiar()
        {
            DateTime time = DateTime.Now;
            // Robot
            if (Program.ajuste.limpiarRobotGps)
            {
                string sql = "DELETE FROM public.rob_gps WHERE id < @tim";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tim", time.AddHours(-Program.ajuste.horasRobotGps));
                cmd.ExecuteNonQuery();

            }
            if (Program.ajuste.limpiarRobotLogs)
            {
                string sql = "DELETE FROM public.rob_logs WHERE id < @tim";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tim", time.AddHours(-Program.ajuste.horasRobotLogs));
                cmd.ExecuteNonQuery();
            }
            // Drone
            if (Program.ajuste.limpiarDroneGps)
            {
                string sql = "DELETE FROM public.drn_gps WHERE id < @tim";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tim", time.AddHours(-Program.ajuste.horasDroneGps));
                cmd.ExecuteNonQuery();

            }
            if (Program.ajuste.limpiarDroneLogs)
            {
                string sql = "DELETE FROM public.drn_logs WHERE id < @tim";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tim", time.AddHours(-Program.ajuste.horasDroneLogs));
                cmd.ExecuteNonQuery();
            }
            // Red electrica
            if (Program.ajuste.limpiarBateriaHistorial)
            {
                string sql = "DELETE FROM public.bat_historial WHERE tim < @tim";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tim", time.AddHours(-Program.ajuste.horasBateriaHistorial));
                cmd.ExecuteNonQuery();
            }
            // VACUUM
            if (Program.ajuste.vacuumLimpiar)
            {
                string sql = "VACUUM FULL";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            // REINDEX
            if (Program.ajuste.reindexLimpiar)
            {
                string sql = "REINDEX DATABASE \"" + Program.config.dbname + "\"";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
        }

        /* AJUSTES */

        public List<Ajuste> getAjustes()
        {
            List<Ajuste> ajustes = new List<Ajuste>();
            string sql = "SELECT id,name,act,lim_rob_gps,horas_rob_gps,lim_robot_log,horas_rob_log,lim_drn_gps,horas_drn_gps,lim_drn_log,horas_drn_log,lim_bat_hist,horas_bat_hist,vacuum_lim,reindex_lim,ping_int,timeout,web_port,oc_port,hash_rounds FROM public.config ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                ajustes.Add(new Ajuste(rdr));
            }
            rdr.Close();
            return ajustes;
        }

        public Ajuste getAjuste()
        {
            string sql = "SELECT id,name,act,lim_rob_gps,horas_rob_gps,lim_robot_log,horas_rob_log,lim_drn_gps,horas_drn_gps,lim_drn_log,horas_drn_log,lim_bat_hist,horas_bat_hist,vacuum_lim,reindex_lim,ping_int,timeout,web_port,oc_port,hash_rounds FROM public.config WHERE act = true";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }
            rdr.Read();
            Ajuste ajuste = new Ajuste(rdr);
            rdr.Close();
            return ajuste;
        }

        public Ajuste getAjuste(short id)
        {
            string sql = "SELECT id,name,act,lim_rob_gps,horas_rob_gps,lim_robot_log,horas_rob_log,lim_drn_gps,horas_drn_gps,lim_drn_log,horas_drn_log,lim_bat_hist,horas_bat_hist,vacuum_lim,reindex_lim,ping_int,timeout,web_port,oc_port,hash_rounds FROM public.config WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }
            rdr.Read();
            Ajuste ajuste = new Ajuste(rdr);
            rdr.Close();
            return ajuste;
        }

        public short addAjuste(Ajuste ajuste)
        {
            string sql = "INSERT INTO public.config(name,act,lim_rob_gps,horas_rob_gps,lim_robot_log,horas_rob_log,lim_drn_gps,horas_drn_gps,lim_drn_log,horas_drn_log,lim_bat_hist,horas_bat_hist,vacuum_lim,reindex_lim,ping_int,timeout,web_port,oc_port,hash_rounds) VALUES (@name,false,@lim_rob_gps,@horas_rob_gps,@lim_robot_log,@horas_rob_log,@lim_drn_gps,@horas_drn_gps,@lim_drn_log,@horas_drn_log,@lim_bat_hist,@horas_bat_hist,@vacuum_lim,@reindex_lim,@ping_int,@timeout,@web_port,@oc_port,@hash_rounds) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", ajuste.name);
            cmd.Parameters.AddWithValue("@lim_rob_gps", ajuste.limpiarRobotGps);
            cmd.Parameters.AddWithValue("@horas_rob_gps", ajuste.horasRobotGps);
            cmd.Parameters.AddWithValue("@lim_robot_log", ajuste.limpiarRobotLogs);
            cmd.Parameters.AddWithValue("@horas_rob_log", ajuste.horasRobotLogs);
            cmd.Parameters.AddWithValue("@lim_drn_gps", ajuste.limpiarDroneGps);
            cmd.Parameters.AddWithValue("@horas_drn_gps", ajuste.horasDroneGps);
            cmd.Parameters.AddWithValue("@lim_drn_log", ajuste.limpiarDroneLogs);
            cmd.Parameters.AddWithValue("@horas_drn_log", ajuste.horasDroneLogs);
            cmd.Parameters.AddWithValue("@lim_bat_hist", ajuste.limpiarBateriaHistorial);
            cmd.Parameters.AddWithValue("@horas_bat_hist", ajuste.horasBateriaHistorial);
            cmd.Parameters.AddWithValue("@vacuum_lim", ajuste.vacuumLimpiar);
            cmd.Parameters.AddWithValue("@reindex_lim", ajuste.reindexLimpiar);
            cmd.Parameters.AddWithValue("@ping_int", ajuste.pingInterval);
            cmd.Parameters.AddWithValue("@timeout", ajuste.timeout);
            cmd.Parameters.AddWithValue("@web_port", ajuste.puertoWeb);
            cmd.Parameters.AddWithValue("@oc_port", ajuste.puertoOC);
            cmd.Parameters.AddWithValue("@hash_rounds", ajuste.hashIteraciones);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            short id = rdr.GetInt16(0);
            rdr.Close();
            ajuste.id = id;

            if (Program.websocketPubSub != null)
                Program.websocketPubSub.onPush("config", serverHashes.SubscriptionChangeType.insert, id, JsonConvert.SerializeObject(getAjuste(id)));
            return id;
        }

        public bool updateAjuste(Ajuste ajuste)
        {
            string sql = "UPDATE public.config SET name=@name,lim_rob_gps=@lim_rob_gps,horas_rob_gps=@horas_rob_gps,lim_robot_log=@lim_robot_log,horas_rob_log=@horas_rob_log,lim_drn_gps=@lim_drn_gps,horas_drn_gps=@horas_drn_gps,lim_drn_log=@lim_drn_log,horas_drn_log=@horas_drn_log,lim_bat_hist=@lim_bat_hist,horas_bat_hist=@horas_bat_hist,vacuum_lim=@vacuum_lim,reindex_lim=@reindex_lim,ping_int=@ping_int,timeout=@timeout,web_port=@web_port,oc_port=@oc_port,hash_rounds=@hash_rounds WHERE id=@id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", ajuste.id);
            cmd.Parameters.AddWithValue("@name", ajuste.name);
            cmd.Parameters.AddWithValue("@lim_rob_gps", ajuste.limpiarRobotGps);
            cmd.Parameters.AddWithValue("@horas_rob_gps", ajuste.horasRobotGps);
            cmd.Parameters.AddWithValue("@lim_robot_log", ajuste.limpiarRobotLogs);
            cmd.Parameters.AddWithValue("@horas_rob_log", ajuste.horasRobotLogs);
            cmd.Parameters.AddWithValue("@lim_drn_gps", ajuste.limpiarDroneGps);
            cmd.Parameters.AddWithValue("@horas_drn_gps", ajuste.horasDroneGps);
            cmd.Parameters.AddWithValue("@lim_drn_log", ajuste.limpiarDroneLogs);
            cmd.Parameters.AddWithValue("@horas_drn_log", ajuste.horasDroneLogs);
            cmd.Parameters.AddWithValue("@lim_bat_hist", ajuste.limpiarBateriaHistorial);
            cmd.Parameters.AddWithValue("@horas_bat_hist", ajuste.horasBateriaHistorial);
            cmd.Parameters.AddWithValue("@vacuum_lim", ajuste.vacuumLimpiar);
            cmd.Parameters.AddWithValue("@reindex_lim", ajuste.reindexLimpiar);
            cmd.Parameters.AddWithValue("@ping_int", ajuste.pingInterval);
            cmd.Parameters.AddWithValue("@timeout", ajuste.timeout);
            cmd.Parameters.AddWithValue("@web_port", ajuste.puertoWeb);
            cmd.Parameters.AddWithValue("@oc_port", ajuste.puertoOC);
            cmd.Parameters.AddWithValue("@hash_rounds", ajuste.hashIteraciones);
            if (cmd.ExecuteNonQuery() == 0)
                return false;

            if (Program.ajuste.id == ajuste.id)
                Program.ajuste = Program.db.getAjuste(ajuste.id);

            Program.websocketPubSub.onPush("config", serverHashes.SubscriptionChangeType.update, ajuste.id, JsonConvert.SerializeObject(getAjuste(ajuste.id)));
            return true;
        }

        public bool deleteAjuste(short id)
        {
            string sql = "DELETE FROM config WHERE id = @id AND act = false";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            if (cmd.ExecuteNonQuery() == 0)
                return false;

            Program.websocketPubSub.onPush("config", serverHashes.SubscriptionChangeType.delete, id, string.Empty);
            return true;
        }

        public bool activarAjuste(short id)
        {
            string sql = "UPDATE public.config SET act = false; UPDATE public.config SET act = true WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            if (cmd.ExecuteNonQuery() == 0)
                return false;

            Ajuste ajuste = Program.db.getAjuste(id);
            Program.ajuste = ajuste;
            if (Program.websocketPubSub != null)
                Program.websocketPubSub.onPush("config", serverHashes.SubscriptionChangeType.update, id, JsonConvert.SerializeObject(ajuste));
            return true;
        }

        /* CLAVES DE API */

        public List<ClaveDeAPI> getApiKeys()
        {
            List<ClaveDeAPI> keys = new List<ClaveDeAPI>();
            string sql = "SELECT id,name,uuid,ultima_con FROM api_key";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                keys.Add(new ClaveDeAPI(rdr));
            }
            rdr.Close();
            return keys;
        }

        public ClaveDeAPI getApiKey(short id)
        {
            string sql = "SELECT id,name,uuid,ultima_con FROM api_key WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }

            rdr.Read();
            ClaveDeAPI key = new ClaveDeAPI(rdr);
            rdr.Close();
            return key;
        }

        public ClaveDeAPI getApiKey(Guid uuid)
        {
            string sql = "SELECT id,name,uuid,ultima_con FROM api_key WHERE uuid = @uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }

            rdr.Read();
            ClaveDeAPI key = new ClaveDeAPI(rdr);
            rdr.Close();
            return key;
        }

        public bool existsApiKey(Guid key)
        {
            string sql = "SELECT uuid FROM api_key WHERE uuid = @uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", key);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            bool exists = rdr.HasRows;
            rdr.Close();
            return exists;
        }

        public bool onlineApiKey(Guid key)
        {
            string sql = "UPDATE api_key SET ultima_con=CURRENT_TIMESTAMP(3) WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", key);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }

            ClaveDeAPI clave = getApiKey(key);
            Program.websocketPubSub.onPush("apiKeys", serverHashes.SubscriptionChangeType.update, clave.id, JsonConvert.SerializeObject(clave));
            return true;
        }

        public ClaveDeAPI addApiKey(string name)
        {
            ClaveDeAPI clave = new ClaveDeAPI(0, name, Guid.NewGuid());
            string sql = "INSERT INTO public.api_key(name,uuid) VALUES (@name,@uuid) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@uuid", clave.uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }
            rdr.Read();
            clave.id = rdr.GetInt16(0);
            rdr.Close();

            Program.websocketPubSub.onPush("apiKeys", serverHashes.SubscriptionChangeType.insert, clave.id, JsonConvert.SerializeObject(getApiKey(clave.id)));
            return clave;
        }

        public bool resetClaveDeApi(short id)
        {
            string sql = "UPDATE api_key SET uuid = @uuid WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@uuid", Guid.NewGuid());
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }

            ClaveDeAPI clave = getApiKey(id);
            Program.websocketPubSub.onPush("apiKeys", serverHashes.SubscriptionChangeType.update, clave.id, JsonConvert.SerializeObject(clave));
            return true;
        }

        public bool deleteClaveDeApi(short id)
        {
            string sql = "DELETE FROM api_key WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }

            Program.websocketPubSub.onPush("apiKeys", serverHashes.SubscriptionChangeType.delete, id, string.Empty);
            return true;
        }

        /* SERVERS */

        public List<Server> getServers()
        {
            List<Server> servers = new List<Server>();
            string sql = "SELECT uuid,name,dsc,online,ultima_con,autoreg,pwd,salt,iteraciones FROM servers";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                servers.Add(new Server(rdr));
            }

            rdr.Close();
            return servers;
        }

        public Server getServer(Guid uuid)
        {
            string sql = "SELECT uuid,name,dsc,online,ultima_con,autoreg,pwd,salt,iteraciones FROM servers WHERE uuid = @uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            Server server = new Server(rdr);
            rdr.Close();

            return server;
        }

        public bool addServer(Server server)
        {
            string sql = "INSERT INTO public.servers(uuid,name,dsc,autoreg) VALUES (@uuid,@name,@dsc,@autoreg)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", server.uuid);
            cmd.Parameters.AddWithValue("@name", server.name);
            cmd.Parameters.AddWithValue("@dsc", server.descripcion);
            cmd.Parameters.AddWithValue("@autoreg", server.permitirAutoregistro);
            try
            {
                if (cmd.ExecuteNonQuery() == 0)
                {
                    return false;
                }
            }
            catch (Exception) { return false; }

            Program.websocketPubSub.onPush("servers", serverHashes.SubscriptionChangeType.insert, 0, JsonConvert.SerializeObject(getServer(server.uuid)));
            return true;
        }

        public bool updateServer(Server server)
        {
            string sql = "UPDATE public.servers SET name=@name,dsc=@dsc,autoreg=@autoreg WHERE uuid=@uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", server.uuid);
            cmd.Parameters.AddWithValue("@name", server.name);
            cmd.Parameters.AddWithValue("@dsc", server.descripcion);
            cmd.Parameters.AddWithValue("@autoreg", server.permitirAutoregistro);
            try
            {
                if (cmd.ExecuteNonQuery() == 0)
                {
                    return false;
                }
            }
            catch (Exception) { return false; }

            Program.websocketPubSub.onPush("servers", serverHashes.SubscriptionChangeType.update, 0, JsonConvert.SerializeObject(getServer(server.uuid)));
            return true;
        }

        public bool updateServer(Guid uuid, string pwd, string salt, int iteraciones)
        {
            string sql = "UPDATE public.servers SET pwd=@pwd,salt=@salt,iteraciones=@iteraciones WHERE uuid=@uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.Parameters.AddWithValue("@pwd", pwd);
            cmd.Parameters.AddWithValue("@salt", salt);
            cmd.Parameters.AddWithValue("@iteraciones", iteraciones);
            try
            {
                if (cmd.ExecuteNonQuery() == 0)
                {
                    return false;
                }
            }
            catch (Exception) { return false; }

            Program.websocketPubSub.onPush("servers", serverHashes.SubscriptionChangeType.update, 0, JsonConvert.SerializeObject(getServer(uuid)));
            return true;
        }

        public bool updateServerOnline(Guid uuid, bool online)
        {
            bool ok;
            if (online)
            {
                string sql = "UPDATE public.servers SET online=@online,ultima_con=CURRENT_TIMESTAMP(3) WHERE uuid = @uuid";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.Parameters.AddWithValue("@online", true);
                ok = cmd.ExecuteNonQuery() > 0;
            }
            else
            {
                string sql = "UPDATE public.servers SET online=@online WHERE uuid = @uuid";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.Parameters.AddWithValue("@online", false);
                ok = cmd.ExecuteNonQuery() > 0;
            }

            if (!ok)
            {
                return false;
            }

            Program.websocketPubSub.onPush("servers", serverHashes.SubscriptionChangeType.update, 0, JsonConvert.SerializeObject(getServer(uuid)));
            return true;
        }

        public bool deleteServer(Guid uuid)
        {
            string sql = "DELETE FROM servers WHERE uuid = @uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }

            Program.websocketPubSub.onPush("servers", serverHashes.SubscriptionChangeType.delete, 0, uuid.ToString());
            return true;
        }

        public bool serverExists(Guid uuid)
        {
            string sql = "SELECT * FROM servers WHERE uuid = @uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            bool exists = rdr.HasRows;
            rdr.Close();
            return exists;
        }

        /* USUARIOS */

        public List<Usuario> getUsuarios()
        {
            List<Usuario> usuarios = new List<Usuario>();
            string sql = "SELECT id,name,pwd,salt,iteraciones,ultima_con,dsc,off FROM usuarios ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                usuarios.Add(new Usuario(rdr));
            }
            rdr.Close();
            return usuarios;
        }

        public List<Usuario> searchUsuarios(string text)
        {
            List<Usuario> usuarios = new List<Usuario>();
            string sql = "SELECT id,name,pwd,salt,iteraciones,ultima_con,dsc,off FROM usuarios WHERE name ILIKE @text ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@text", "%" + text + "%");
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                usuarios.Add(new Usuario(rdr));
            }
            rdr.Close();
            return usuarios;
        }

        public Usuario getUsuario(string username)
        {
            string sql = "SELECT id,name,pwd,salt,iteraciones,ultima_con,dsc,off FROM usuarios WHERE name = @username AND off = false";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@username", username);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }
            rdr.Read();
            Usuario usuario = new Usuario(rdr);
            rdr.Close();
            return usuario;
        }

        public Usuario getUsuario(short id)
        {
            string sql = "SELECT id,name,pwd,salt,iteraciones,ultima_con,dsc,off FROM usuarios WHERE id = @id AND off = false";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }
            rdr.Read();
            Usuario usuario = new Usuario(rdr);
            rdr.Close();
            return usuario;
        }

        public bool addUsuario(Usuario usuario)
        {
            string sql = "INSERT INTO usuarios (name,pwd,salt,iteraciones,ultima_con) VALUES(@username,@pwd,@salt,@iteraciones,@ultima_con)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@username", usuario.name);
            cmd.Parameters.AddWithValue("@pwd", usuario.pwd);
            cmd.Parameters.AddWithValue("@salt", usuario.salt);
            cmd.Parameters.AddWithValue("@iteraciones", usuario.iteraciones);
            cmd.Parameters.AddWithValue("@ultima_con", usuario.ultima_con);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return false;
            }

            rdr.Read();
            short id = rdr.GetInt16(0);
            rdr.Close();

            usuario.id = id;
            Program.websocketPubSub.onPush("usuarios", serverHashes.SubscriptionChangeType.insert, id, JsonConvert.SerializeObject(usuario));
            return true;
        }

        public bool updateUsuario(UsuarioEdit usuario)
        {
            string sql = "UPDATE usuarios SET name = @username, dsc = @dsc, off = @off WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", usuario.id);
            cmd.Parameters.AddWithValue("@username", usuario.name);
            cmd.Parameters.AddWithValue("@dsc", usuario.descripcion);
            cmd.Parameters.AddWithValue("@off", usuario.off);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }
            Program.websocketPubSub.onPush("usuarios", serverHashes.SubscriptionChangeType.update, usuario.id, JsonConvert.SerializeObject(getUsuario(usuario.id)));
            return true;
        }

        public bool updateUsuario(short id, string pwd, string salt, int interations)
        {
            string sql = "UPDATE usuarios SET pwd = @pwd, salt = @salt, iteraciones = @iteraciones WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@pwd", pwd);
            cmd.Parameters.AddWithValue("@salt", salt);
            cmd.Parameters.AddWithValue("@iteraciones", interations);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }
            Program.websocketPubSub.onPush("usuarios", serverHashes.SubscriptionChangeType.update, id, JsonConvert.SerializeObject(getUsuario(id)));
            return true;
        }

        public bool updateUsuarioOnline(short id)
        {
            string sql = "UPDATE usuarios SET ultima_con = @ultima_con WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@ultima_con", DateTime.Now);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }
            Program.websocketPubSub.onPush("usuarios", serverHashes.SubscriptionChangeType.update, id, JsonConvert.SerializeObject(getUsuario(id)));
            return true;
        }

        public bool deleteUsuario(short id)
        {
            string sql = "DELETE FROM usuarios WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }
            Program.websocketPubSub.onPush("usuarios", serverHashes.SubscriptionChangeType.delete, id, string.Empty);
            return true;
        }

        /* ARTÍCULOS */

        public List<Articulo> getArticulos()
        {
            List<Articulo> articulos = new List<Articulo>();
            string sql = "SELECT id, name, mine_id, cant, dsc FROM articulos ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                articulos.Add(new Articulo(rdr));
            }
            rdr.Close();
            return articulos;
        }

        public List<Articulo> searchArticulos(string texto)
        {
            try
            {
                List<Articulo> articulos = new List<Articulo>();
                string sql;
                if (texto.IndexOf(':') >= 0)
                {
                    sql = "SELECT id, name, mine_id, cant, dsc FROM articulos WHERE mine_id = @txt ORDER BY id ASC";
                }
                else
                {
                    sql = "SELECT id, name, mine_id, cant, dsc FROM articulos WHERE name @@ to_tsquery(@txt) ORDER BY id ASC";
                }
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@txt", texto);
                NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    articulos.Add(new Articulo(rdr));
                }
                rdr.Close();
                return articulos;
            }
            catch (Exception) { return getArticulos(); }
        }

        public List<ArticuloHead> localizarArticulos()
        {
            List<ArticuloHead> articulos = new List<ArticuloHead>();
            string sql = "SELECT id, name, mine_id FROM articulos ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                articulos.Add(new ArticuloHead(rdr));
            }
            rdr.Close();
            return articulos;
        }

        public Articulo getArticulo(short id)
        {
            string sql = "SELECT * FROM articulos WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }
            rdr.Read();
            Articulo articulo = new Articulo(rdr);
            rdr.Close();
            return articulo;
        }

        public string getArticuloName(short id)
        {
            string sql = "SELECT name FROM articulos WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            if (!rdr.HasRows)
            {
                rdr.Close();
                return "";
            }
            rdr.Read();
            string name = rdr.GetString(0);
            rdr.Close();
            return name;
        }

        public ArticuloSlot getArticuloSlot(short id)
        {
            string sql = "SELECT name FROM articulos WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }
            rdr.Read();
            ArticuloSlot articulo = new ArticuloSlot(id, rdr);
            rdr.Close();
            return articulo;
        }

        public short getArticulo(string minecraftID)
        {
            string sql = "SELECT id FROM articulos WHERE mine_id = @mine_id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@mine_id", minecraftID);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            if (!rdr.HasRows)
            {
                rdr.Close();
                return 0;
            }
            rdr.Read();
            short id = rdr.GetInt16(0);
            rdr.Close();
            return id;
        }

        public bool addArticulo(Articulo articulo)
        {
            string sql = "INSERT INTO articulos (name, mine_id, dsc) VALUES (@name,@mine_id,@dsc) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", articulo.name);
            cmd.Parameters.AddWithValue("@mine_id", articulo.minecraftID);
            cmd.Parameters.AddWithValue("@dsc", articulo.descripcion);
            NpgsqlDataReader rdr;
            try
            {
                rdr = cmd.ExecuteReader();
            }
            catch (Exception) { return false; }
            if (!rdr.HasRows)
            {
                rdr.Close();
                return false;
            }
            rdr.Read();
            short id = rdr.GetInt16(0);
            rdr.Close();
            articulo.id = id;
            Program.websocketPubSub.onPush("articulos", serverHashes.SubscriptionChangeType.insert, id, JsonConvert.SerializeObject(articulo));
            return true;
        }

        public bool updateArticulo(Articulo articulo)
        {
            string sql = "UPDATE articulos SET name = @name, mine_id = @mine_id, dsc = @dsc WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", articulo.id);
            cmd.Parameters.AddWithValue("@name", articulo.name);
            cmd.Parameters.AddWithValue("@mine_id", articulo.minecraftID);
            cmd.Parameters.AddWithValue("@dsc", articulo.descripcion);
            try
            {
                if (cmd.ExecuteNonQuery() == 0)
                {
                    return false;
                }
            }
            catch (Exception) { return false; }
            Program.websocketPubSub.onPush("articulos", serverHashes.SubscriptionChangeType.update, articulo.id, JsonConvert.SerializeObject(articulo));
            return true;
        }

        public bool updateArticuloCant(short id, short cantidad)
        {
            string sql = "UPDATE articulos SET cant = @cant WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@cant", cantidad);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }
            Program.websocketPubSub.onPush("articulos", serverHashes.SubscriptionChangeType.update, id, JsonConvert.SerializeObject(getArticulo(id)));
            return true;
        }

        public bool deleteArticulo(short id)
        {
            string sql = "DELETE FROM articulos WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }
            Program.websocketPubSub.onPush("articulos", serverHashes.SubscriptionChangeType.delete, id, string.Empty);
            return true;
        }

        // IMG

        public byte[] getArticuloImg(short id)
        {
            string sql = "SELECT img_size, img FROM articulos WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            if (!rdr.HasRows)
            {
                rdr.Close();
                return new byte[0];
            }
            rdr.Read();
            if (rdr.IsDBNull(1))
            {
                rdr.Close();
                return new byte[0];
            }
            byte[] img = new byte[(rdr.GetInt16(0))];
            rdr.GetBytes(1, 0, img, 0, rdr.GetInt16(0));
            rdr.Close();
            return img;
        }

        public bool setArticuloImg(short id, byte[] img)
        {
            string sql = "UPDATE articulos SET img = @img, img_size = @img_size WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@img", img);
            cmd.Parameters.AddWithValue("@img_size", img.Length);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }
            Program.websocketPubSub.onPush("articulosImg", serverHashes.SubscriptionChangeType.update, id, string.Empty);
            return true;
        }

        public bool deleteArticuloImg(short id)
        {
            string sql = "UPDATE articulos SET img = NULL, img_size = 0 WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }
            Program.websocketPubSub.onPush("articulosImg", serverHashes.SubscriptionChangeType.delete, id, string.Empty);
            return true;
        }

        /* ROBOTS */

        public List<Robot> getRobots(RobotQuery query)
        {
            List<Robot> robots = new List<Robot>();
            StringBuilder sql = new StringBuilder("SELECT id, name, uuid, tier, num_slots, num_stacks, num_items, estado, total_energia, energia_actual, upgrade_gen, items_gen, fecha_con, fecha_descon, dsc, upgrade_gps, pos_x, pos_y, pos_z, complejidad, date_add, date_upd, off, off_pos_x, off_pos_y, off_pos_z FROM robots");

            if (query.off && (!query.text.Equals(string.Empty)))
                sql.Append(" WHERE (uuid::text @@ to_tsquery(@text)) OR name ILIKE @textq");
            else if (!query.off)
            {
                sql.Append(" WHERE off = false");
                if (!query.text.Equals(string.Empty))
                    sql.Append(" AND (uuid::text @@ to_tsquery(@text) OR name ILIKE @textq)");
            }

            sql.Append(" ORDER BY id ASC");
            NpgsqlCommand cmd = new NpgsqlCommand(sql.ToString(), conn);
            if (!query.text.Equals(string.Empty))
                cmd.Parameters.AddWithValue("text", query.text);
            cmd.Parameters.AddWithValue("textq", query.text + "%");
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                robots.Add(new Robot(rdr));
            rdr.Close();
            return robots;
        }

        public Robot getRobot(short id)
        {
            string sql = "SELECT id, name, uuid, tier, num_slots, num_stacks, num_items, estado, total_energia, energia_actual, upgrade_gen, items_gen, fecha_con, fecha_descon, dsc, upgrade_gps, pos_x, pos_y, pos_z, complejidad, date_add, date_upd, off, off_pos_x, off_pos_y, off_pos_z FROM robots WHERE off = false AND id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }
            rdr.Read();
            Robot robot = new Robot(rdr);
            rdr.Close();
            return robot;
        }

        public Robot getRobot(Guid uuid)
        {
            string sql = "SELECT id, name, uuid, tier, num_slots, num_stacks, num_items, estado, total_energia, energia_actual, upgrade_gen, items_gen, fecha_con, fecha_descon, dsc, upgrade_gps, pos_x, pos_y, pos_z, complejidad, date_add, date_upd, off, off_pos_x, off_pos_y, off_pos_z FROM robots WHERE off = false AND uuid = @uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uuid", uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            Robot robot = new Robot(rdr);
            rdr.Close();
            return robot;
        }

        public List<RobotHead> getRobotsHead()
        {
            List<RobotHead> robots = new List<RobotHead>();
            string sql = "SELECT id, name, uuid FROM robots WHERE off = false ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                robots.Add(new RobotHead(rdr));
            }
            rdr.Close();
            return robots;
        }

        public string getRobotName(short id)
        {
            string sql = "SELECT name FROM robots WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }
            rdr.Read();
            string name = rdr.GetString(0);
            rdr.Close();
            return name;
        }

        public short addRobot(Robot r)
        {
            string sql = "INSERT INTO robots (name,uuid,tier,num_slots,total_energia,energia_actual,upgrade_gen,items_gen,dsc,upgrade_gps,pos_x,pos_y,pos_z,complejidad,off_pos_x,off_pos_y,off_pos_z) VALUES (@name,@uuid,@tier,@num_slots,@total_energia,@energia_actual,@upgrade_gen,@items_gen,@dsc,@upgrade_gps,@pos_x,@pos_y,@pos_z,@complejidad,@off_pos_x,@off_pos_y,@off_pos_z) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", r.name);
            cmd.Parameters.AddWithValue("@uuid", r.uuid);
            cmd.Parameters.AddWithValue("@tier", r.tier);
            cmd.Parameters.AddWithValue("@num_slots", r.numeroSlots);
            cmd.Parameters.AddWithValue("@total_energia", r.totalEnergia);
            cmd.Parameters.AddWithValue("@energia_actual", r.energiaActual);
            cmd.Parameters.AddWithValue("@upgrade_gen", r.upgradeGenerador);
            cmd.Parameters.AddWithValue("@items_gen", r.itemsGenerador);
            cmd.Parameters.AddWithValue("@dsc", r.descripcion);
            cmd.Parameters.AddWithValue("@upgrade_gps", r.upgradeGps);
            cmd.Parameters.AddWithValue("@pos_x", r.posX);
            cmd.Parameters.AddWithValue("@pos_y", r.posY);
            cmd.Parameters.AddWithValue("@pos_z", r.posZ);
            cmd.Parameters.AddWithValue("@complejidad", r.complejidad);
            cmd.Parameters.AddWithValue("@off_pos_x", r.offsetPosX);
            cmd.Parameters.AddWithValue("@off_pos_y", r.offsetPosY);
            cmd.Parameters.AddWithValue("@off_pos_z", r.offsetPosZ);
            NpgsqlDataReader rdr;
            try
            {
                rdr = cmd.ExecuteReader();
            }
            catch (Exception) { return 0; }

            if (!rdr.HasRows)
            {
                rdr.Close();
                return 0;
            }
            rdr.Read();
            short id = rdr.GetInt16(0);
            rdr.Close();

            r.id = id;
            Program.websocketPubSub.onPush("robots", serverHashes.SubscriptionChangeType.insert, id, JsonConvert.SerializeObject(r));
            return id;
        }

        public bool updateRobot(Robot r)
        {
            string sql = "UPDATE robots SET name=@name, uuid=@uuid, tier=@tier, num_slots=@num_slots, total_energia=@total_energia, energia_actual=@energia_actual, upgrade_gen=@upgrade_gen, items_gen=@items_gen, dsc=@dsc, upgrade_gps=@upgrade_gps, pos_x=@pos_x, pos_y=@pos_y, pos_z=@pos_z, complejidad=@complejidad, off = @off, off_pos_x = @off_pos_x, off_pos_y = @off_pos_y, off_pos_z = @off_pos_z WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", r.id);
            cmd.Parameters.AddWithValue("name", r.name);
            cmd.Parameters.AddWithValue("uuid", r.uuid);
            cmd.Parameters.AddWithValue("tier", r.tier);
            cmd.Parameters.AddWithValue("num_slots", r.numeroSlots);
            cmd.Parameters.AddWithValue("total_energia", r.totalEnergia);
            cmd.Parameters.AddWithValue("energia_actual", r.energiaActual);
            cmd.Parameters.AddWithValue("upgrade_gen", r.upgradeGenerador);
            cmd.Parameters.AddWithValue("items_gen", r.itemsGenerador);
            cmd.Parameters.AddWithValue("dsc", r.descripcion);
            cmd.Parameters.AddWithValue("upgrade_gps", r.upgradeGps);
            cmd.Parameters.AddWithValue("pos_x", r.posX);
            cmd.Parameters.AddWithValue("pos_y", r.posY);
            cmd.Parameters.AddWithValue("pos_z", r.posZ);
            cmd.Parameters.AddWithValue("complejidad", r.complejidad);
            cmd.Parameters.AddWithValue("off", r.off);
            cmd.Parameters.AddWithValue("off_pos_x", r.offsetPosX);
            cmd.Parameters.AddWithValue("off_pos_y", r.offsetPosY);
            cmd.Parameters.AddWithValue("off_pos_z", r.offsetPosZ);
            cmd.Prepare();
            try
            {
                if (cmd.ExecuteNonQuery() == 0)
                {
                    return false;
                }
            }
            catch (Exception) { return false; }

            Program.websocketPubSub.onPush("robots", serverHashes.SubscriptionChangeType.update, r.id, JsonConvert.SerializeObject(r));
            return true;
        }

        public bool deleteRobot(short id)
        {
            string sql = "DELETE FROM robots WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }

            Program.websocketPubSub.onPush("robots", serverHashes.SubscriptionChangeType.delete, id, string.Empty);
            Program.websocketPubSub.removeTopic("robotLog#" + id);
            Program.websocketPubSub.removeTopic("robotGPS#" + id);
            return true;
        }

        public void updateRobotOnline(string uuid, string name, short energiaActual, short totalEnergia, short posX, short posY, short posZ)
        {
            string sql = "UPDATE robots SET name=@name,total_energia=@total_energia,energia_actual=@energia_actual,pos_x=@pos_x,pos_y=@pos_y,pos_z=@pos_z,estado='O',fecha_con=@fecha_con WHERE uuid = @uuid RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uuid", Guid.Parse(uuid));
            cmd.Parameters.AddWithValue("name", name);
            cmd.Parameters.AddWithValue("total_energia", totalEnergia);
            cmd.Parameters.AddWithValue("energia_actual", energiaActual);
            cmd.Parameters.AddWithValue("pos_x", posX);
            cmd.Parameters.AddWithValue("pos_y", posY);
            cmd.Parameters.AddWithValue("pos_z", posZ);
            cmd.Parameters.AddWithValue("fecha_con", DateTime.Now);
            cmd.Prepare();
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return;
            }
            rdr.Read();
            short id = rdr.GetInt16(0);
            rdr.Close();

            Program.websocketPubSub.onPush("robots", serverHashes.SubscriptionChangeType.update, id, JsonConvert.SerializeObject(getRobot(id)));
        }

        public void autoRegisterRobot(Guid serveruuid, string serverpwd, string uuid, string name, short num_slots, short energiaActual, short totalEnergia, bool generatorUpgrade, short numItems, bool gpsUpgrade, short posX, short posY, short posZ)
        {
            // autentificar por el servidor
            string sql = "SELECT pwd,salt,iteraciones FROM servers WHERE uuid = @uuid AND autoreg = true";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", serveruuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return;
            }
            rdr.Read();
            string pwd = rdr.GetString(0);
            string salt = rdr.GetString(1);
            int iteraciones = rdr.GetInt32(2);
            rdr.Close();
            if (!Usuario.verifyHash(pwd, salt + serverpwd, iteraciones))
                return;

            // insertar robot con los atributos obtenidos
            sql = "INSERT INTO robots (name,uuid,num_slots,total_energia,energia_actual,pos_x,pos_y,pos_z,upgrade_gen,items_gen,upgrade_gps) VALUES (@name,@uuid,@num_slots,@total_energia,@energia_actual,@pos_x,@pos_y,@pos_z,@upgrade_gen,@items_gen,@upgrade_gps) RETURNING id";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@uuid", Guid.Parse(uuid));
            cmd.Parameters.AddWithValue("@num_slots", num_slots);
            cmd.Parameters.AddWithValue("@total_energia", totalEnergia);
            cmd.Parameters.AddWithValue("@energia_actual", energiaActual);
            cmd.Parameters.AddWithValue("@pos_x", posX);
            cmd.Parameters.AddWithValue("@pos_y", posY);
            cmd.Parameters.AddWithValue("@pos_z", posZ);
            cmd.Parameters.AddWithValue("upgrade_gen", generatorUpgrade);
            cmd.Parameters.AddWithValue("items_gen", numItems);
            cmd.Parameters.AddWithValue("upgrade_gps", gpsUpgrade);
            try
            {
                rdr = cmd.ExecuteReader();
            }
            catch (Exception) { return; }

            if (!rdr.HasRows)
            {
                rdr.Close();
                return;
            }
            rdr.Read();
            short id = rdr.GetInt16(0);
            rdr.Close();

            Program.websocketPubSub.onPush("robots", serverHashes.SubscriptionChangeType.insert, id, JsonConvert.SerializeObject(getRobot(id)));
        }

        public void updateRobotOffline(string uuid)
        {
            string sql = "UPDATE robots SET estado='F',fecha_descon=@fecha_descon WHERE uuid = @uuid RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("fecha_descon", DateTime.Now);
            cmd.Parameters.AddWithValue("uuid", Guid.Parse(uuid));
            cmd.Prepare();
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return;
            }
            rdr.Read();
            short id = rdr.GetInt16(0);
            rdr.Close();

            Program.websocketPubSub.onPush("robots", serverHashes.SubscriptionChangeType.update, id, JsonConvert.SerializeObject(getRobot(id)));
            Program.websocketPubSub.removeTopic("robotLog#" + id);
            Program.websocketPubSub.removeTopic("robotInv#" + id);
            Program.websocketPubSub.removeTopic("robotGPS#" + id);

        }

        // ROBOT - INVENTARIO

        public List<RobotInventario> getRobotInventario(short idRobot)
        {
            List<RobotInventario> inventario = new List<RobotInventario>();
            List<RobotInventarioGet> inventarioGet = new List<RobotInventarioGet>();
            string sql = "SELECT num_slot, cant, art FROM rob_inventario WHERE rob = @robot ORDER BY num_slot ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@robot", idRobot);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                RobotInventarioGet slot = new RobotInventarioGet(rdr);
                inventarioGet.Add(slot);
            }
            rdr.Close();

            foreach (RobotInventarioGet inv in inventarioGet)
            {
                inventario.Add(
                    new RobotInventario(inv.numeroSlot, inv.cant, inv.articulo == 0 ? null : getArticuloSlot(inv.articulo)));
            }

            return inventario;
        }

        public void setRobotInventario(Guid uuid, List<RobotInventarioSet> setInventario)
        {
            for (int i = 0; i < setInventario.Count; i++)
            {
                RobotInventarioSet inv = setInventario[i];

                string sql = "UPDATE rob_inventario SET art = @art, cant = @cant WHERE rob = (SELECT id FROM robots WHERE uuid = @uuid) AND num_slot = @num_slot";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.Parameters.AddWithValue("@num_slot", (i + 1));
                if (inv.idArticulo.Equals(string.Empty))
                {
                    cmd.Parameters.AddWithValue("@art", DBNull.Value);
                    cmd.Parameters.AddWithValue("@cant", 0);
                }
                else
                {
                    short art = getArticulo(inv.idArticulo);
                    if (art > 0)
                        cmd.Parameters.AddWithValue("@art", art);
                    else
                        cmd.Parameters.AddWithValue("@art", DBNull.Value);
                    cmd.Parameters.AddWithValue("@cant", inv.cant);
                }
                cmd.ExecuteNonQuery();
            }

            short id = getRobot(uuid).id;
            Program.websocketPubSub.addTopic("robotInv#" + id);
            Program.websocketPubSub.onPush("robotInv#" + id, serverHashes.SubscriptionChangeType.update, 0, JsonConvert.SerializeObject(getRobotInventario(id)));
        }

        public bool clearRobotInventario(Guid uuid)
        {
            string sql = "UPDATE rob_inventario SET art = NULL, cant = 0 WHERE rob = (SELECT id FROM robots WHERE uuid = @uuid)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            return cmd.ExecuteNonQuery() > 0;
        }

        // ROBOT - LOGS

        public List<RobotLog> getRobotLogs(RobotLogQuery query)
        {
            List<RobotLog> logs = new List<RobotLog>();
            string sql = "SELECT id, titulo, msg FROM rob_logs WHERE rob = @robot";

            if (query.start != DateTime.MinValue)
                sql += " AND id >= @start";
            if (query.end != DateTime.MaxValue)
                sql += " AND id <= @end";

            sql += " ORDER BY id DESC";
            if (query.limit > 0)
                sql += " LIMIT @limit OFFSET @offset";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@robot", query.idRobot);
            if (query.limit > 0)
                cmd.Parameters.AddWithValue("@limit", query.limit);

            if (query.start != DateTime.MinValue)
                cmd.Parameters.AddWithValue("@start", query.start);
            if (query.end != DateTime.MaxValue)
                cmd.Parameters.AddWithValue("@end", query.end);

            cmd.Parameters.AddWithValue("@offset", query.offset);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                logs.Add(new RobotLog(rdr));
            }
            rdr.Close();
            return logs;
        }

        public bool addRobotLog(Guid uuid, RobotLog log)
        {
            string sql = "INSERT INTO rob_logs (rob, id, titulo, msg) VALUES ((SELECT id FROM robots WHERE uuid = @uuid),@id,@titulo,@msg)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.Parameters.AddWithValue("@id", log.id);
            cmd.Parameters.AddWithValue("@titulo", log.name);
            cmd.Parameters.AddWithValue("@msg", log.mensaje);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }

            short id = getRobot(uuid).id;
            Program.websocketPubSub.addTopic("robotLog#" + id);
            Program.websocketPubSub.onPush("robotLog#" + id, serverHashes.SubscriptionChangeType.insert, 0, JsonConvert.SerializeObject(log));
            return true;
        }

        public bool clearRobotLogs(short idRobot)
        {
            string sql = "DELETE FROM rob_logs WHERE rob = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idRobot);
            if (cmd.ExecuteNonQuery() == 0)
                return false;

            Program.websocketPubSub.addTopic("robotLog#" + idRobot);
            Program.websocketPubSub.onPush("robotLog#" + idRobot, serverHashes.SubscriptionChangeType.delete, 0, string.Empty);
            return true;
        }

        public bool clearRobotLogs(short idRobot, DateTime start, DateTime end)
        {
            string sql = "DELETE FROM rob_logs WHERE rob = @id AND id >= @start AND id <= @end";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idRobot);
            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);
            if (cmd.ExecuteNonQuery() == 0)
                return false;

            Program.websocketPubSub.addTopic("robotLog#" + idRobot);
            Program.websocketPubSub.onPush("robotLog#" + idRobot, serverHashes.SubscriptionChangeType.delete, 0, string.Empty);
            return true;
        }

        // ROBOT - GPS

        public List<RobotGPS> getRobotGPS(short idRobot, int offset = 0, int limit = 10)
        {
            List<RobotGPS> gps = new List<RobotGPS>();
            string sql = "SELECT id, pos_x, pos_y, pos_z FROM rob_gps WHERE rob = @robot ORDER BY id DESC LIMIT @limit OFFSET @offset";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@robot", idRobot);
            cmd.Parameters.AddWithValue("@limit", limit);
            cmd.Parameters.AddWithValue("@offset", offset);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                gps.Add(new RobotGPS(rdr));
            }
            rdr.Close();
            return gps;
        }

        public bool addRobotGps(Guid uuid, short posX, short posY, short posZ)
        {
            string sql = "SELECT off_pos_x, off_pos_y, off_pos_z FROM robots WHERE uuid = @uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uuid", uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return false;
            }
            rdr.Read();
            short offsetPosX = rdr.GetInt16(0);
            short offsetPosY = rdr.GetInt16(1);
            short offsetPosZ = rdr.GetInt16(2);
            rdr.Close();

            sql = "INSERT INTO rob_gps (rob, pos_x, pos_y, pos_z) VALUES ((SELECT id FROM robots WHERE uuid = @uuid),@pos_x,@pos_y,@pos_z)";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.Parameters.AddWithValue("@pos_x", posX - offsetPosX);
            cmd.Parameters.AddWithValue("@pos_y", posY - offsetPosY);
            cmd.Parameters.AddWithValue("@pos_z", posZ - offsetPosZ);
            bool ok = cmd.ExecuteNonQuery() > 0;

            if (!ok)
                return false;

            sql = "UPDATE robots SET pos_x=@pos_x, pos_y=@pos_y, pos_z=@pos_z WHERE uuid = @uuid";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uuid", uuid);
            cmd.Parameters.AddWithValue("pos_x", posX - offsetPosX);
            cmd.Parameters.AddWithValue("pos_y", posY - offsetPosY);
            cmd.Parameters.AddWithValue("pos_z", posZ - offsetPosZ);
            cmd.Prepare();
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }

            short id = getRobot(uuid).id;
            Program.websocketPubSub.addTopic("robotGPS#" + id);
            Program.websocketPubSub.onPush("robotGPS#" + id, serverHashes.SubscriptionChangeType.insert, 0, JsonConvert.SerializeObject(
                new RobotGPS((short)(posX - offsetPosX), (short)(posY - offsetPosY), (short)(posZ - offsetPosZ))));
            return true;
        }

        public bool clearRobotGPS(short idRobot)
        {
            string sql = "DELETE FROM rob_gps WHERE rob = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idRobot);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool clearRobotGPS(short idRobot, DateTime start, DateTime end)
        {
            string sql = "DELETE FROM rob_gps WHERE rob = @id AND id >= @start AND id <= end";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idRobot);
            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);
            return cmd.ExecuteNonQuery() > 0;
        }

        /* REDES ELÉCTRICAS */

        public List<RedElectrica> getRedesElectricas()
        {
            List<RedElectrica> redesElectricas = new List<RedElectrica>();
            string sql = "SELECT id, name, cap_ele, carga_act, dsc FROM redes_electricas";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                redesElectricas.Add(new RedElectrica(rdr));
            }
            rdr.Close();

            foreach (RedElectrica red in redesElectricas)
            {
                red.baterias = this.getBaterias(red.id);
                red.generadores = this.getGeneradores(red.id);
            }

            return redesElectricas;
        }

        public RedElectrica getRedElectrica(short id)
        {
            string sql = "SELECT id, name, cap_ele, carga_act, dsc FROM redes_electricas WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            rdr.Read();
            RedElectrica redElectrica = new RedElectrica(rdr);
            rdr.Close();

            redElectrica.baterias = this.getBaterias(id);
            redElectrica.generadores = this.getGeneradores(id);

            return redElectrica;
        }

        public List<RedElectrica> searchRedesElectricas(string text)
        {
            List<RedElectrica> redesElectricas = new List<RedElectrica>();
            string sql = "SELECT id, name, cap_ele, carga_act, dsc FROM redes_electricas WHERE name ILIKE @txt";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@txt", "%" + text + "%");
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                redesElectricas.Add(new RedElectrica(rdr));
            }
            rdr.Close();

            foreach (RedElectrica red in redesElectricas)
            {
                red.baterias = this.getBaterias(red.id);
                red.generadores = this.getGeneradores(red.id);
            }

            return redesElectricas;
        }

        public bool addRedElectrica(RedElectrica red)
        {
            string sql = "INSERT INTO redes_electricas (name,dsc) VALUES (@name,@dsc) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", red.name);
            cmd.Parameters.AddWithValue("@dsc", red.descripcion);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return false;
            }
            rdr.Read();
            short id = rdr.GetInt16(0);
            rdr.Close();

            red.id = id;
            Program.websocketPubSub.onPush("electrico", serverHashes.SubscriptionChangeType.insert, id, JsonConvert.SerializeObject(red));
            return true;
        }

        public bool updateRedElectrica(RedElectrica red)
        {
            string sql = "UPDATE redes_electricas SET name = @name,dsc = @dsc WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", red.id);
            cmd.Parameters.AddWithValue("@name", red.name);
            cmd.Parameters.AddWithValue("@dsc", red.descripcion);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }
            Program.websocketPubSub.onPush("electrico", serverHashes.SubscriptionChangeType.update, red.id, JsonConvert.SerializeObject(getRedElectrica(red.id)));
            return true;
        }

        private bool updateCargaCapacidadRedElectrica(short idRed, long cargaActual, long capacidadElectrica)
        {
            string sql = "UPDATE redes_electricas SET cap_ele = cap_ele + @cap_ele,carga_act = carga_act + @carga_act WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idRed);
            cmd.Parameters.AddWithValue("@carga_act", cargaActual);
            cmd.Parameters.AddWithValue("@cap_ele", capacidadElectrica);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }
            Program.websocketPubSub.onPush("electrico", serverHashes.SubscriptionChangeType.update, idRed, JsonConvert.SerializeObject(getRedElectrica(idRed)));
            return true;
        }

        private bool updateCargaCapacidadRedElectrica(short idRed, long cargaActual)
        {
            string sql = "UPDATE redes_electricas SET carga_act = carga_act + @carga_act WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idRed);
            cmd.Parameters.AddWithValue("@carga_act", cargaActual);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }
            Program.websocketPubSub.onPush("electrico", serverHashes.SubscriptionChangeType.update, idRed, JsonConvert.SerializeObject(getRedElectrica(idRed)));
            return true;
        }

        public bool deleteRedElectrica(short id)
        {
            string sql = "DELETE FROM redes_electricas WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }
            Program.websocketPubSub.onPush("electrico", serverHashes.SubscriptionChangeType.delete, id, string.Empty);
            return true;
        }

        // BATERIA

        private List<Bateria> getBaterias(short idRed)
        {
            List<Bateria> baterias = new List<Bateria>();
            string sql = "SELECT id, red_ele, name, uuid, cap_ele, carga_act, dsc, tipo FROM bat WHERE red_ele = @red_ele ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@red_ele", idRed);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                baterias.Add(new Bateria(rdr));
            }
            rdr.Close();
            return baterias;
        }

        private Bateria getBateria(short idRed, short idBateria)
        {
            string sql = "SELECT id, red_ele, name, uuid, cap_ele, carga_act, dsc, tipo FROM bat WHERE red_ele = @red_ele AND id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@red_ele", idRed);
            cmd.Parameters.AddWithValue("@id", idBateria);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }

            rdr.Read();
            Bateria bateria = new Bateria(rdr);
            rdr.Close();
            return bateria;
        }

        private Bateria getBateria(Guid uuid)
        {
            string sql = "SELECT id, red_ele, name, uuid, cap_ele, carga_act, dsc, tipo FROM bat WHERE uuid = @uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }

            rdr.Read();
            Bateria bateria = new Bateria(rdr);
            rdr.Close();
            return bateria;
        }

        public short addBateriaRedElectrica(Bateria bat)
        {
            string sql = "INSERT INTO bat (red_ele,name,uuid,cap_ele,carga_act,dsc,tipo) VALUES (@red_ele,@name,@uuid,@cap_ele,@carga_act,@dsc,@tipo) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@red_ele", bat.redElectrica);
            cmd.Parameters.AddWithValue("@name", bat.name);
            cmd.Parameters.AddWithValue("@uuid", bat.uuid);
            cmd.Parameters.AddWithValue("@cap_ele", bat.capacidadElectrica);
            cmd.Parameters.AddWithValue("@carga_act", bat.cargaActual);
            cmd.Parameters.AddWithValue("@dsc", bat.descripcion);
            cmd.Parameters.AddWithValue("@tipo", bat.tipo);
            NpgsqlDataReader rdr;
            try
            {
                rdr = cmd.ExecuteReader();
            }
            catch (Exception) { return 0; }
            rdr.Read();
            short id = rdr.GetInt16(0);
            rdr.Close();

            // actualización de la red eléctrica
            if (id > 0)
            {
                this.updateCargaCapacidadRedElectrica(bat.redElectrica, bat.cargaActual, bat.capacidadElectrica);
                Program.websocketPubSub.onPush("bateria", serverHashes.SubscriptionChangeType.insert, bat.redElectrica, JsonConvert.SerializeObject(getBateria(bat.redElectrica, id)));
            }

            return id;
        }

        public bool updateBateriaRedElectrica(Bateria bat)
        {
            // actualización de la red eléctrica
            string sql = "SELECT carga_act,cap_ele FROM bat WHERE red_ele=@red_ele AND id=@id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", bat.id);
            cmd.Parameters.AddWithValue("@red_ele", bat.redElectrica);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            long cargaActual = 0;
            if (bat.cargaActual != rdr.GetInt64(0))
                cargaActual = bat.cargaActual - rdr.GetInt64(0);
            long capacidadElectrica = 0;
            if (bat.capacidadElectrica != rdr.GetInt64(1))
                capacidadElectrica = bat.capacidadElectrica - rdr.GetInt64(1);
            rdr.Close();
            this.updateCargaCapacidadRedElectrica(bat.redElectrica, cargaActual, capacidadElectrica);

            // modificar batería
            sql = "UPDATE bat SET name=@name,uuid=@uuid,cap_ele=@cap_ele,carga_act=@carga_act,dsc=@dsc,tipo=@tipo WHERE red_ele=@red_ele AND id=@id";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", bat.id);
            cmd.Parameters.AddWithValue("@red_ele", bat.redElectrica);
            cmd.Parameters.AddWithValue("@name", bat.name);
            cmd.Parameters.AddWithValue("@uuid", bat.uuid);
            cmd.Parameters.AddWithValue("@cap_ele", bat.capacidadElectrica);
            cmd.Parameters.AddWithValue("@carga_act", bat.cargaActual);
            cmd.Parameters.AddWithValue("@dsc", bat.descripcion);
            cmd.Parameters.AddWithValue("@tipo", bat.tipo);
            try
            {
                if (cmd.ExecuteNonQuery() == 0)
                    return false;
            }
            catch (Exception) { return false; }

            Program.websocketPubSub.onPush("bateria", serverHashes.SubscriptionChangeType.update, bat.redElectrica, JsonConvert.SerializeObject(getBateria(bat.redElectrica, bat.id)));
            return true;
        }

        public void updateBateriaRedElectrica(Guid uuid, long capacidadElectrica, long cargaActual)
        {
            // actualización de la red eléctrica
            string sql = "SELECT carga_act,cap_ele,red_ele,id FROM bat WHERE uuid=@uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            long redCargaActual = 0;
            if (cargaActual != rdr.GetInt64(0))
                redCargaActual = cargaActual - rdr.GetInt64(0);
            long redCapacidadElectrica = 0;
            if (capacidadElectrica != rdr.GetInt64(1))
                redCapacidadElectrica = capacidadElectrica - rdr.GetInt64(1);
            short redElectrica = rdr.GetInt16(2);
            short bateria = rdr.GetInt16(3);
            rdr.Close();
            this.updateCargaCapacidadRedElectrica(redElectrica, redCargaActual, redCapacidadElectrica);

            // modificar batería
            sql = "UPDATE bat SET cap_ele=@cap_ele,carga_act=@carga_act WHERE uuid=@uuid";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.Parameters.AddWithValue("@cap_ele", capacidadElectrica);
            cmd.Parameters.AddWithValue("@carga_act", cargaActual);
            cmd.ExecuteNonQuery();

            // añadir al historial de la batería
            sql = "INSERT INTO bat_historial (red_ele,bat,carga_act) VALUES (@red_ele,@bat,@carga_act)";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@red_ele", redElectrica);
            cmd.Parameters.AddWithValue("@bat", bateria);
            cmd.Parameters.AddWithValue("@carga_act", cargaActual);
            cmd.ExecuteNonQuery();

            Bateria bat = getBateria(uuid);
            Program.websocketPubSub.onPush("bateria", serverHashes.SubscriptionChangeType.update, bat.redElectrica, JsonConvert.SerializeObject(getBateria(bat.redElectrica, bat.id)));
        }

        public void updateBateriaRedElectrica(Guid uuid, long cargaActual)
        {
            // actualización de la red eléctrica
            string sql = "SELECT carga_act,red_ele,id FROM bat WHERE uuid=@uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            long redCargaActual = 0;
            if (cargaActual != rdr.GetInt64(0))
                redCargaActual = cargaActual - rdr.GetInt64(0);
            short redElectrica = rdr.GetInt16(1);
            short bateria = rdr.GetInt16(2);
            rdr.Close();
            this.updateCargaCapacidadRedElectrica(redElectrica, redCargaActual);

            // modificar batería
            sql = "UPDATE bat SET carga_act=@carga_act WHERE uuid=@uuid";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.Parameters.AddWithValue("@carga_act", cargaActual);
            cmd.ExecuteNonQuery();

            // añadir al historial de la batería
            sql = "INSERT INTO bat_historial (red_ele,bat,carga_act) VALUES (@red_ele,@bat,@carga_act)";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@red_ele", redElectrica);
            cmd.Parameters.AddWithValue("@bat", bateria);
            cmd.Parameters.AddWithValue("@carga_act", cargaActual);
            try
            {
                if (cmd.ExecuteNonQuery() == 0)
                    return;
            }
            catch (Exception) { return; }

            Bateria bat = getBateria(uuid);
            Program.websocketPubSub.onPush("bateria", serverHashes.SubscriptionChangeType.update, bat.redElectrica, JsonConvert.SerializeObject(getBateria(bat.redElectrica, bat.id)));
        }

        public bool deleteBateriaRedElectrica(short idRed, short idBateria)
        {
            Bateria bat = getBateria(idRed, idBateria);
            if (bat == null)
                return false;
            string sql = "DELETE FROM bat WHERE red_ele=@red_ele AND id=@id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idBateria);
            cmd.Parameters.AddWithValue("@red_ele", idRed);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }

            sql = "UPDATE redes_electricas SET cap_ele = cap_ele - @cap_ele, carga_act = carga_act - @carga_act WHERE id = @id";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idRed);
            cmd.Parameters.AddWithValue("@cap_ele", bat.capacidadElectrica);
            cmd.Parameters.AddWithValue("@carga_act", bat.cargaActual);
            cmd.ExecuteNonQuery();

            Program.websocketPubSub.onPush("bateria", serverHashes.SubscriptionChangeType.delete, idRed, JsonConvert.SerializeObject(bat));
            return true;
        }

        // BATERÍA - HISTORIAL

        public List<BateriaHistorial> getBateriaHistorial(short idRed, short idBateria, int offset, int limit)
        {
            List<BateriaHistorial> historial = new List<BateriaHistorial>();
            string sql = "SELECT red_ele, bat, id, tim, carga_act FROM bat_historial WHERE red_ele = @red_ele AND bat = @bat ORDER BY tim DESC OFFSET @offset LIMIT @limit";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@red_ele", idRed);
            cmd.Parameters.AddWithValue("@bat", idBateria);
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@limit", limit);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                historial.Add(new BateriaHistorial(rdr));
            }
            rdr.Close();
            return historial;
        }

        public bool clearBateriaHistorial(short idRed, short idBateria)
        {
            string sql = "DELETE FROM bat_historial WHERE red_ele = @red_ele AND bat = @bat";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@red_ele", idRed);
            cmd.Parameters.AddWithValue("@bat", idBateria);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool clearBateriaHistorial(short idRed, short idBateria, DateTime start, DateTime end)
        {
            string sql = "DELETE FROM bat_historial WHERE red_ele = @red_ele AND bat = @bat AND tim >= @start AND tim <= end";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@red_ele", idRed);
            cmd.Parameters.AddWithValue("@bat", idBateria);
            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);
            return cmd.ExecuteNonQuery() > 0;
        }

        // GENERADOR

        private List<Generador> getGeneradores(short idRed)
        {
            List<Generador> generadores = new List<Generador>();
            string sql = "SELECT red_ele, id, name, uuid, eu_t, act, tipo, dsc FROM gen WHERE red_ele = @red_ele";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@red_ele", idRed);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                generadores.Add(new Generador(rdr));
            }
            rdr.Close();
            return generadores;
        }

        private Generador getGenerador(short idRed, short idGenerador)
        {
            string sql = "SELECT red_ele, id, name, uuid, eu_t, act, tipo, dsc FROM gen WHERE red_ele = @red_ele AND id = @id ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@red_ele", idRed);
            cmd.Parameters.AddWithValue("@id", idGenerador);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }

            rdr.Read();
            Generador generador = new Generador(rdr);
            rdr.Close();
            return generador;
        }

        private Generador getGenerador(Guid uuid)
        {
            string sql = "SELECT red_ele, id, name, uuid, eu_t, act, tipo, dsc FROM gen WHERE uuid = @uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }

            rdr.Read();
            Generador generador = new Generador(rdr);
            rdr.Close();
            return generador;
        }

        public short addGeneradorRedElectrica(Generador gen)
        {
            string sql = "INSERT INTO gen (red_ele,name,uuid,eu_t,act,tipo,dsc) VALUES (@red_ele,@name,@uuid,@eu_t,@act,@tipo,@dsc) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@red_ele", gen.redElectrica);
            cmd.Parameters.AddWithValue("@name", gen.name);
            cmd.Parameters.AddWithValue("@uuid", gen.uuid);
            cmd.Parameters.AddWithValue("@eu_t", gen.euTick);
            cmd.Parameters.AddWithValue("@act", gen.activado);
            cmd.Parameters.AddWithValue("@tipo", gen.tipo);
            cmd.Parameters.AddWithValue("@dsc", gen.descripcion);
            NpgsqlDataReader rdr;
            try
            {
                rdr = cmd.ExecuteReader();
            }
            catch (Exception) { return 0; }
            rdr.Read();
            short id = rdr.GetInt16(0);
            rdr.Close();

            Program.websocketPubSub.onPush("generador", serverHashes.SubscriptionChangeType.insert, gen.redElectrica, JsonConvert.SerializeObject(getGenerador(gen.redElectrica, id)));
            return id;
        }

        public bool updateGeneradorRedElectrica(Generador gen)
        {
            string sql = "UPDATE gen SET name=@name,uuid=@uuid,eu_t=@eu_t,act=@act,dsc=@dsc,tipo=@tipo WHERE red_ele=@red_ele AND id=@id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", gen.id);
            cmd.Parameters.AddWithValue("@red_ele", gen.redElectrica);
            cmd.Parameters.AddWithValue("@name", gen.name);
            cmd.Parameters.AddWithValue("@uuid", gen.uuid);
            cmd.Parameters.AddWithValue("@eu_t", gen.euTick);
            cmd.Parameters.AddWithValue("@act", gen.activado);
            cmd.Parameters.AddWithValue("@dsc", gen.descripcion);
            cmd.Parameters.AddWithValue("@tipo", gen.tipo);
            try
            {
                if (cmd.ExecuteNonQuery() == 0)
                    return false;
            }
            catch (Exception) { return false; }

            Program.websocketPubSub.onPush("generador", serverHashes.SubscriptionChangeType.update, gen.redElectrica, JsonConvert.SerializeObject(getGenerador(gen.redElectrica, gen.id)));
            return true;
        }

        public bool updateGeneradorRedElectrica(Guid uuid, bool activado)
        {
            string sql = "UPDATE gen SET act=@act WHERE uuid = @uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.Parameters.AddWithValue("@act", activado);
            if (cmd.ExecuteNonQuery() == 0)
                return false;

            Generador gen = getGenerador(uuid);
            Program.websocketPubSub.onPush("generador", serverHashes.SubscriptionChangeType.update, gen.redElectrica, JsonConvert.SerializeObject(getGenerador(gen.redElectrica, gen.id)));
            return true;
        }

        public bool deleteGeneradorRedElectrica(short idRed, short id)
        {
            Generador gen = getGenerador(idRed, id);
            if (gen == null)
                return false;
            string sql = "DELETE FROM gen WHERE red_ele=@red_ele AND id=@id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@red_ele", idRed);
            if (cmd.ExecuteNonQuery() == 0)
                return false;

            Program.websocketPubSub.onPush("generador", serverHashes.SubscriptionChangeType.delete, idRed, JsonConvert.SerializeObject(gen));
            return true;
        }

        /* ORDENES DE MINADO */

        public List<OrdenMinado> getOrdenesDeMinado(char[] estado, short robot)
        {
            List<OrdenMinado> ordenes = new List<OrdenMinado>();
            StringBuilder sql = new StringBuilder("SELECT id,name,size,rob,pos_x,pos_y,pos_z,pos_f,gps_x,gps_y,gps_z,num_items,date_add,date_upd,date_inicio,date_fin,dsc,estado,recarga_unidad,energia_recarga,modo_minado,shutdown FROM public.ordenes_minado");
            if (estado.Length > 0 || robot > 0)
                sql.Append(" WHERE ");

            if (estado.Length > 0)
                sql.Append("(");
            for (int i = 0; i < estado.Length; i++)
            {
                if (i > 0)
                    sql.Append(" OR");
                sql.Append(" estado = @estado");
                sql.Append(i);
            }
            if (estado.Length > 0)
                sql.Append(")");

            if (robot > 0)
            {
                if (estado.Length > 0)
                    sql.Append(" AND");
                sql.Append(" rob = @rob");
            }
            NpgsqlCommand cmd = new NpgsqlCommand(sql.ToString(), conn);
            for (int i = 0; i < estado.Length; i++)
                cmd.Parameters.AddWithValue("@estado" + i, estado[i]);
            if (robot > 0)
                cmd.Parameters.AddWithValue("@rob", robot);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                ordenes.Add(new OrdenMinado(rdr));
            }
            rdr.Close();
            return ordenes;
        }

        public OrdenMinado getOrdenMinado(int idOrden)
        {
            string sql = "SELECT id,name,size,rob,pos_x,pos_y,pos_z,pos_f,gps_x,gps_y,gps_z,num_items,date_add,date_upd,date_inicio,date_fin,dsc,estado,recarga_unidad,energia_recarga,modo_minado,shutdown FROM public.ordenes_minado WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idOrden);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            rdr.Read();
            OrdenMinado orden = new OrdenMinado(rdr);
            rdr.Close();
            return orden;
        }

        /// <summary>
        /// Obtiene la orden de minado asociada al robot, o intenta asociar una orden nueva
        /// </summary>
        /// <param name="uuid">ID del Robot</param>
        /// <returns></returns>
        public OrdenMinado getRobotOrdenMinado(Guid uuid)
        {
            // ver si el robot tenia una orden de minado establecida
            string sql = "SELECT ord_min, id FROM robots WHERE off = false AND uuid = @uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uuid", uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            int ordenMinadoId = 0;
            if (!rdr.IsDBNull(0))
                ordenMinadoId = rdr.GetInt32(0);
            short robotId = rdr.GetInt16(1);
            rdr.Close();
            bool ordenAsociada = false;

            // buscar entre las siguientes órdenes de minado
            if (ordenMinadoId <= 0)
            {
                sql = "SELECT id FROM ordenes_minado WHERE estado = 'Q' AND rob = @rob LIMIT 1";
                cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("rob", robotId);
                rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    rdr.Read();
                    ordenMinadoId = rdr.GetInt32(0);
                }
                rdr.Close();
                ordenAsociada = true;
            }

            // buscar entre las órdenes de minado sin robot y asociar
            if (ordenMinadoId <= 0)
            {
                sql = "SELECT id FROM ordenes_minado WHERE estado = 'Q' AND rob = NULL LIMIT 1";
                cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("rob", robotId);
                rdr = cmd.ExecuteReader();
                bool ordenEncontrada = false;
                if (rdr.HasRows)
                {
                    rdr.Read();
                    ordenMinadoId = rdr.GetInt32(0);
                    ordenEncontrada = true;
                }
                rdr.Close();

                // as órdenes que se asocian al primer robot disponible, lo hacen solo la primera vez
                if (ordenEncontrada)
                {
                    sql = "UPDATE ordenes_minado SET rob=@rob WHERE id=@id";
                    cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", ordenMinadoId);
                    cmd.Parameters.AddWithValue("@rob", robotId);
                    cmd.ExecuteNonQuery();
                    ordenAsociada = true;
                }
            }

            // no hay trabajos a realizar
            if (ordenMinadoId <= 0)
                return null;

            // si se ha cambiado la orden de minado del robot, asociar como trabajo activo en la tabla de robots,
            // y guardar para auditoría el tempo en el que se ha asociado
            if (ordenAsociada)
            {
                sql = "UPDATE robots SET ord_min=@ord_min WHERE id=@id";
                cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", robotId);
                cmd.Parameters.AddWithValue("@ord_min", ordenMinadoId);
                cmd.ExecuteNonQuery();
            }

            // ajustar el estado a En curso en la orden de minado
            sql = "UPDATE ordenes_minado SET date_inicio=CURRENT_TIMESTAMP(3),estado='E' WHERE id=@id";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", ordenMinadoId);
            cmd.ExecuteNonQuery();

            // cargar orden para devolver
            sql = "SELECT id,size,pos_x,pos_y,pos_z,pos_f,gps_x,gps_y,gps_z,recarga_unidad,energia_recarga,modo_minado,shutdown FROM ordenes_minado WHERE id=@id";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", ordenMinadoId);
            rdr = cmd.ExecuteReader();

            rdr.Read();
            OrdenMinado ordenMinado = new OrdenMinado();
            ordenMinado.robotParse(rdr);
            rdr.Close();

            Program.websocketPubSub.onPush("ordenMinado", serverHashes.SubscriptionChangeType.update, ordenMinadoId, JsonConvert.SerializeObject(getOrdenMinado(ordenMinadoId)));

            return ordenMinado;
        }

        /// <summary>
        /// Usado cuando el robot se va offline con una orden de minado sin haber finalizado.
        /// Pasa de 'En curso' a 'Preparado' la orden de miando asociada.
        /// </summary>
        /// <param name="uuid">ID del Robot</param>
        public void stopRobotOrdenMinado(Guid uuid)
        {
            string sql = "UPDATE ordenes_minado SET estado='R' WHERE id = (SELECT ord_min FROM robots WHERE uuid = @uuid) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return;
            }
            rdr.Read();
            int ordenMinadoId = rdr.GetInt32(0);
            rdr.Close();

            Program.websocketPubSub.onPush("ordenMinado", serverHashes.SubscriptionChangeType.update, ordenMinadoId, JsonConvert.SerializeObject(getOrdenMinado(ordenMinadoId)));
        }

        /// <summary>
        /// Llamado por el robot al terminar la orden de miando.
        /// Marca la orden como finalizada completamente, y la desasocia del robot para que pueda soliciar nuevas ordenes de minado.
        /// </summary>
        /// <param name="uuid">ID del Robot</param>
        public void finRobotOrdenMinado(Guid uuid)
        {
            string sql = "UPDATE ordenes_minado SET estado='O',date_fin=CURRENT_TIMESTAMP(3) WHERE id = (SELECT ord_min FROM robots WHERE uuid = @uuid) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            int ordenMinadoId = rdr.GetInt32(0);
            rdr.Close();

            sql = "UPDATE robots SET ord_min = NULL WHERE uuid = @uuid";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.ExecuteNonQuery();

            Program.websocketPubSub.onPush("ordenMinado", serverHashes.SubscriptionChangeType.update, ordenMinadoId, JsonConvert.SerializeObject(getOrdenMinado(ordenMinadoId)));

        }

        public bool addOrdenesDeMinado(OrdenMinado orden)
        {
            string sql = "INSERT INTO public.ordenes_minado(name,size,rob,gps_x,gps_y,gps_z,dsc,recarga_unidad,energia_recarga,modo_minado,shutdown) VALUES (@name,@size,@rob,@gps_x,@gps_y,@gps_z,@dsc,@recarga_unidad,@energia_recarga,@modo_minado,@shutdown) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", orden.name);
            cmd.Parameters.AddWithValue("@size", orden.size);
            if (orden.robot == null || orden.robot == 0)
                cmd.Parameters.AddWithValue("@rob", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@rob", orden.robot);
            cmd.Parameters.AddWithValue("@gps_x", orden.posX);
            cmd.Parameters.AddWithValue("@gps_y", orden.gpsY);
            cmd.Parameters.AddWithValue("@gps_z", orden.gpsZ);
            cmd.Parameters.AddWithValue("@dsc", orden.descripcion);
            cmd.Parameters.AddWithValue("@recarga_unidad", orden.unidadRecarga);
            cmd.Parameters.AddWithValue("@energia_recarga", orden.energiaRecarga);
            cmd.Parameters.AddWithValue("@modo_minado", orden.modoMinado);
            cmd.Parameters.AddWithValue("@shutdown", orden.shutdown);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            int id = rdr.GetInt32(0);
            rdr.Close();

            Program.websocketPubSub.onPush("ordenMinado", serverHashes.SubscriptionChangeType.insert, 0, JsonConvert.SerializeObject(getOrdenMinado(id)));
            return true;
        }

        public bool updateOrdenesDeMinado(OrdenMinado orden)
        {
            string sql = "UPDATE public.ordenes_minado SET name=@name,size=@size,rob=@rob,gps_x=@gps_x,gps_y=@gps_y,gps_z=@gps_z,dsc=@dsc,recarga_unidad=@recarga_unidad,energia_recarga=@energia_recarga,modo_minado=@modo_minado,shutdown=@shutdown WHERE id=@id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", orden.id);
            cmd.Parameters.AddWithValue("@name", orden.name);
            cmd.Parameters.AddWithValue("@size", orden.size);
            if (orden.robot == null || orden.robot == 0)
                cmd.Parameters.AddWithValue("@rob", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@rob", orden.robot);
            cmd.Parameters.AddWithValue("@gps_x", orden.posX);
            cmd.Parameters.AddWithValue("@gps_y", orden.gpsY);
            cmd.Parameters.AddWithValue("@gps_z", orden.gpsZ);
            cmd.Parameters.AddWithValue("@dsc", orden.descripcion);
            cmd.Parameters.AddWithValue("@recarga_unidad", orden.unidadRecarga);
            cmd.Parameters.AddWithValue("@energia_recarga", orden.energiaRecarga);
            cmd.Parameters.AddWithValue("@modo_minado", orden.modoMinado);
            cmd.Parameters.AddWithValue("@shutdown", orden.shutdown);
            bool ok = cmd.ExecuteNonQuery() > 0;

            if (ok)
                Program.websocketPubSub.onPush("ordenMinado", serverHashes.SubscriptionChangeType.update, orden.id, JsonConvert.SerializeObject(getOrdenMinado(orden.id)));

            return ok;
        }

        public void updateProgresoOrdenDeMinado(Guid uuid, short posX, short posY, short posZ, short facing)
        {
            string sql = "UPDATE public.ordenes_minado SET pos_x=@pos_x,pos_y=@pos_y,pos_z=@pos_z,pos_f=@pos_f WHERE id=(SELECT ord_min FROM robots WHERE uuid=@uuid) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.Parameters.AddWithValue("@pos_x", posX);
            cmd.Parameters.AddWithValue("@pos_y", posY);
            cmd.Parameters.AddWithValue("@pos_z", posZ);
            cmd.Parameters.AddWithValue("@pos_f", facing);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            int id = rdr.GetInt32(0);
            rdr.Close();

            Program.websocketPubSub.onPush("ordenMinado", serverHashes.SubscriptionChangeType.update, id, JsonConvert.SerializeObject(getOrdenMinado(id)));

        }

        public bool deleteOrdenesDeMinado(int id)
        {
            string sql = "DELETE FROM ordenes_minado WHERE id=@id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            bool ok = cmd.ExecuteNonQuery() > 0;

            if (ok)
                Program.websocketPubSub.onPush("ordenMinado", serverHashes.SubscriptionChangeType.delete, id, "");

            return ok;
        }

        // ORDENES DE MINADO - INVENTARIO

        public List<OrdenMinadoInventario> getOrdenMinadoInventario(int idOrden)
        {
            List<OrdenMinadoInventario> inventario = new List<OrdenMinadoInventario>();
            List<OrdenMinadoInventarioGet> inventarioGet = new List<OrdenMinadoInventarioGet>();
            string sql = "SELECT id, art, cant FROM ord_min_inventario WHERE ord_min = @ord_min ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ord_min", idOrden);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                OrdenMinadoInventarioGet slot = new OrdenMinadoInventarioGet(rdr);
                inventarioGet.Add(slot);
            }
            rdr.Close();

            foreach (OrdenMinadoInventarioGet inv in inventarioGet)
            {
                inventario.Add(
                    new OrdenMinadoInventario(inv.numeroSlot, inv.cant, inv.articulo == 0 ? null : getArticuloSlot(inv.articulo)));
            }

            return inventario;
        }

        public void setOrdenMinadoInventario(int idOrden, List<OrdenMinadoInventarioSet> setInventario)
        {
            for (int i = 0; i < setInventario.Count; i++)
            {
                OrdenMinadoInventarioSet inv = setInventario[i];
                short articulo = getArticulo(inv.idArticulo);
                if (articulo == 0)
                    continue;

                string sql = "SELECT id, cant FROM public.ord_min_inventario WHERE ord_min = @ord_min AND art = @art AND cant < 64";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ord_min", idOrden);
                cmd.Parameters.AddWithValue("@art", articulo);
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows) // ya hay un slot que tiene de estos items
                {
                    rdr.Read();
                    short id = rdr.GetInt16(0);
                    short cantAnterior = rdr.GetInt16(1);
                    rdr.Close();
                    sql = "UPDATE ord_min_inventario SET cant = @cant WHERE ord_min = @ord_min AND id = @id";
                    cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@cant", (cantAnterior + inv.cant) > 64 ? 64 : (cantAnterior + inv.cant));
                    cmd.Parameters.AddWithValue("@ord_min", idOrden);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();

                    // en caso de que no haya cabido todo, hará que repartirlo en otro slot.
                    // también es posible que no haya otro slot a medias, por lo que se creará otro.
                    // esta es la lógica que se utiliza en esta misma iteración del bucle, así que se restará la cantidad
                    // repartida y se retrocederá la posición del bucle para volver a asociar este slot de manera natural
                    // y evitar duplicar este código
                    if (cantAnterior + inv.cant > 64)
                    {
                        inv.cant = (short)((inv.cant + cantAnterior) - 64);
                        i--;
                    }
                }
                else // no hay slots con cantidad disponible para este item, crear un slot
                {
                    rdr.Close();
                    sql = "INSERT INTO ord_min_inventario (ord_min,art,cant) VALUES (@ord_min,@art,@cant)";
                    cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ord_min", idOrden);
                    cmd.Parameters.AddWithValue("@art", articulo);
                    cmd.Parameters.AddWithValue("@cant", inv.cant);
                    cmd.ExecuteNonQuery();
                }
            }

            // acumular el total de ítems minados en la órden de minado
            short cantidad = 0;
            foreach (OrdenMinadoInventarioSet set in setInventario)
            {
                cantidad += set.cant;
            }
            string sqlOrden = "UPDATE ordenes_minado SET num_items = num_items + @cantidad WHERE id = @id";
            NpgsqlCommand cmdOrden = new NpgsqlCommand(sqlOrden, conn);
            cmdOrden.Parameters.AddWithValue("@cantidad", cantidad);
            cmdOrden.ExecuteNonQuery();

            // enviar actualizaciones a la web
            Program.websocketPubSub.onPush("ordenMinado", serverHashes.SubscriptionChangeType.update, idOrden, JsonConvert.SerializeObject(getOrdenMinado(idOrden)));
            Program.websocketPubSub.onPush("ordenMinadoInventario", serverHashes.SubscriptionChangeType.update, idOrden, JsonConvert.SerializeObject(getOrdenMinadoInventario(idOrden)));
        }

        /* ALMACENES */

        public List<Almacen> getAlmacenes()
        {
            List<Almacen> almacenes = new List<Almacen>();
            string sql = "SELECT id,name,dsc,slots,items,off,uuid,date_add,date_inv_upd FROM almacenes ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                almacenes.Add(new Almacen(rdr));
            }
            rdr.Close();
            return almacenes;
        }

        public List<AlmacenHead> localizarAlmacenes()
        {
            List<AlmacenHead> almacenes = new List<AlmacenHead>();
            string sql = "SELECT id,name,uuid FROM almacenes ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                almacenes.Add(new AlmacenHead(rdr));
            }
            rdr.Close();
            return almacenes;
        }

        /// <summary>
        /// Obtener el ID del almacén por el UUID de su controlador. Esto solo debe de funcionar si es almacén no está desactivado.
        /// </summary>
        /// <param name="uuid">UUID del controlador</param>
        /// <returns></returns>
        public short getAlmacenId(Guid uuid)
        {
            string sql = "SELECT id FROM almacenes WHERE uuid = @uuid AND off = false";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            if (!rdr.HasRows)
            {
                rdr.Close();
                return 0;
            }

            short id;
            rdr.Read();
            id = rdr.GetInt16(0);
            rdr.Close();

            sql = "UPDATE almacenes SET date_inv_upd = CURRENT_TIMESTAMP(3) WHERE id = @id";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            return id;
        }

        public string getAlmacenName(short id)
        {
            string sql = "SELECT name FROM almacenes WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            if (!rdr.HasRows)
            {
                rdr.Close();
                return "";
            }

            string name;
            rdr.Read();
            name = rdr.GetString(0);
            rdr.Close();
            return name;
        }

        public bool addAlmacen(Almacen almacen)
        {
            string sql = "INSERT INTO almacenes (name, dsc, off, uuid) VALUES (@name, @dsc, @off, @uuid) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", almacen.name);
            cmd.Parameters.AddWithValue("@dsc", almacen.descripcion);
            cmd.Parameters.AddWithValue("@off", almacen.off);
            cmd.Parameters.AddWithValue("@uuid", almacen.uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return false;
            }

            rdr.Read();
            short id = rdr.GetInt16(0);
            rdr.Close();

            almacen.id = id;
            Program.websocketPubSub.onPush("almacen", serverHashes.SubscriptionChangeType.insert, id, JsonConvert.SerializeObject(almacen));
            return true;
        }

        public bool updateAlmacen(Almacen almacen)
        {
            string sql = "UPDATE almacenes SET name = @name,dsc = @dsc,off=@off,uuid=@uuid WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", almacen.id);
            cmd.Parameters.AddWithValue("@name", almacen.name);
            cmd.Parameters.AddWithValue("@dsc", almacen.descripcion);
            cmd.Parameters.AddWithValue("@off", almacen.off);
            cmd.Parameters.AddWithValue("@uuid", almacen.uuid);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }
            Program.websocketPubSub.onPush("almacen", serverHashes.SubscriptionChangeType.update, almacen.id, JsonConvert.SerializeObject(almacen));
            return true;
        }

        public bool deleteAlmacen(short id)
        {
            string sql = "DELETE FROM almacenes WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }

            Program.websocketPubSub.onPush("almacen", serverHashes.SubscriptionChangeType.delete, id, string.Empty);
            Program.websocketPubSub.removeTopic("almacenInv#" + id);
            return true;
        }

        // ALMACÉN - INVENTARIO

        public List<AlmacenInventario> getInventarioAlmacen(short idAlmacen)
        {
            List<AlmacenInventario> inventario = new List<AlmacenInventario>();
            List<AlmacenInventarioGet> inventarioGet = new List<AlmacenInventarioGet>();
            string sql = "SELECT alm,id,art,cant FROM alm_inventario WHERE alm = @alm ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", idAlmacen);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                AlmacenInventarioGet slot = new AlmacenInventarioGet(rdr);
                inventarioGet.Add(slot);
            }
            rdr.Close();

            foreach (AlmacenInventarioGet invGet in inventarioGet)
            {
                inventario.Add(new AlmacenInventario(invGet.almacen, invGet.id, getArticuloSlot(invGet.articulo), invGet.cantidad));
            }

            return inventario;
        }

        public void setInventarioAlmacen(short idAlmacen, List<AlmacenInventarioSet> inventario)
        {
            NpgsqlTransaction trans = conn.BeginTransaction();
            // obtener los IDs de los artículos, si no se encuentra el ID en la base de datos, eliminar del array
            for (int i = (inventario.Count - 1); i >= 0; i--)
            {
                AlmacenInventarioSet slot = inventario[i];
                short articulo = getArticulo(slot.articulo);
                if (articulo == 0)
                {
                    inventario.RemoveAt(i);
                    continue;
                }
                else
                {
                    slot.articuloId = articulo;
                }
            }

            string sql = "SELECT art, cant FROM alm_inventario WHERE alm = @alm";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", idAlmacen);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            // guardar todo el inventario como copia, para ir eliminando los artículos encontrados
            // al final, solo quedarán los slots del almacén que no tienen lugar en el inventario recibido
            List<AlmacenInventarioSet> slots = new List<AlmacenInventarioSet>(inventario);
            List<AlmacenInventarioSet> slotsNoEncotrados = new List<AlmacenInventarioSet>();
            while (rdr.Read())
            {
                short articulo = rdr.GetInt16(0);
                bool found = false;

                // buscar el slot del inventario en el nuevo inventario recibido
                for (int i = (slots.Count - 1); i >= 0; i--)
                {
                    AlmacenInventarioSet slot = slots[i];
                    if (slot.articuloId == articulo)
                    {
                        slots.RemoveAt(i);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    slotsNoEncotrados.Add(new AlmacenInventarioSet(articulo, rdr.GetInt32(1)));
                }

            }
            rdr.Close();

            for (int i = 0; i < slotsNoEncotrados.Count; i++)
            {
                AlmacenInventarioSet slot = slotsNoEncotrados[i];
                // ya no queda de este artículo en este inventario. hacer movimiento de salida de todo el artículo y eliminar slot
                int cantidad = slot.cantidad;
                // crear un movimiento de almacén automático para guardar el cambio
                sql = "INSERT INTO mov_inventario(alm, art, cant) VALUES (@alm, @art, @cant)";
                cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@alm", idAlmacen);
                cmd.Parameters.AddWithValue("@art", slot.articuloId);
                cmd.Parameters.AddWithValue("@cant", -cantidad);
                cmd.ExecuteNonQuery();

                // actualizar el artículo
                sql = "UPDATE articulos SET cant = cant - @cant WHERE id = @id";
                cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", slot.articuloId);
                cmd.Parameters.AddWithValue("@cant", cantidad);
                cmd.ExecuteNonQuery();

                // eliminar el slot
                sql = "DELETE FROM alm_inventario WHERE alm = @alm AND art = @art";
                cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@alm", idAlmacen);
                cmd.Parameters.AddWithValue("@art", slot.articuloId);
                cmd.ExecuteNonQuery();
            }

            // modificar las cantidades de los slots que ya existen
            for (int i = 0; i < inventario.Count; i++)
            {
                AlmacenInventarioSet slot = inventario[i];
                short articulo = slot.articuloId;

                // ¿existe este artículo en este almacén?
                sql = "SELECT id,cant FROM alm_inventario WHERE alm = @alm AND art = @art";
                cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@alm", idAlmacen);
                cmd.Parameters.AddWithValue("@art", articulo);
                rdr = cmd.ExecuteReader();

                if (rdr.HasRows) // ya existe un slot para este artículo
                {
                    rdr.Read();
                    short almSlotId = rdr.GetInt16(0);
                    rdr.Close();

                    // cantidad anterior del artículo
                    sql = "SELECT cant FROM articulos WHERE id = @id";
                    cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", articulo);
                    rdr = cmd.ExecuteReader();
                    rdr.Read();
                    int cantidadAnterior = rdr.GetInt32(0);
                    rdr.Close();

                    // modificar el slot del inventario
                    sql = "UPDATE alm_inventario SET cant = @cant WHERE alm = @alm AND id = @id";
                    cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@cant", slot.cantidad);
                    cmd.Parameters.AddWithValue("@id", almSlotId);
                    cmd.Parameters.AddWithValue("@alm", idAlmacen);
                    cmd.ExecuteNonQuery();

                    // actualizar la cantidad del artículo
                    int cantidadDiferencia = slot.cantidad - cantidadAnterior;
                    if (cantidadDiferencia != 0)
                    {
                        sql = "UPDATE articulos SET cant = cant + @cant WHERE id = @id";
                        cmd = new NpgsqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@id", articulo);
                        cmd.Parameters.AddWithValue("@cant", cantidadDiferencia);
                        cmd.ExecuteNonQuery();

                        // crear un movimiento de almacén automático para guardar el cambio
                        sql = "INSERT INTO mov_inventario(alm, art, cant) VALUES (@alm, @art, @cant)";
                        cmd = new NpgsqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@alm", idAlmacen);
                        cmd.Parameters.AddWithValue("@art", articulo);
                        cmd.Parameters.AddWithValue("@cant", cantidadDiferencia);
                        cmd.ExecuteNonQuery();
                    }

                }
                else // no existe un slot para este artículo
                {
                    rdr.Close();

                    // crear el slot para el artículo
                    sql = "INSERT INTO alm_inventario (alm,art,cant) VALUES (@alm,@art,@cant)";
                    cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@alm", idAlmacen);
                    cmd.Parameters.AddWithValue("@art", articulo);
                    cmd.Parameters.AddWithValue("@cant", slot.cantidad);
                    cmd.ExecuteNonQuery();

                    // actualizar la cantidad del artículo
                    sql = "UPDATE articulos SET cant = cant + @cant WHERE id = @id";
                    cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", articulo);
                    cmd.Parameters.AddWithValue("@cant", slot.cantidad);
                    cmd.ExecuteNonQuery();

                    // crear un movimiento de almacén automático para guardar el cambio
                    sql = "INSERT INTO mov_inventario(alm, art, cant) VALUES (@alm, @art, @cant)";
                    cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@alm", idAlmacen);
                    cmd.Parameters.AddWithValue("@art", articulo);
                    cmd.Parameters.AddWithValue("@cant", slot.cantidad);
                    cmd.ExecuteNonQuery();
                }
            }

            trans.Commit();

            Program.websocketPubSub.addTopic("almacenInv#" + idAlmacen);
            Program.websocketPubSub.onPush("almacenInv#" + idAlmacen, serverHashes.SubscriptionChangeType.update, 0, JsonConvert.SerializeObject(getInventarioAlmacen(idAlmacen)));
        }

        /* MOVIMIENTOS DE ALMACÉN */

        public List<MovimientoAlmacen> getMovimientosAlmacen()
        {
            List<MovimientoAlmacen> movimientoAlmacen = new List<MovimientoAlmacen>();
            string sql = "SELECT alm, id, art, cant, origen, date_add, dsc FROM mov_inventario ORDER BY date_add DESC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                movimientoAlmacen.Add(new MovimientoAlmacen(rdr));
            rdr.Close();

            return movimientoAlmacen;
        }

        public List<MovimientoAlmacen> getMovimientosAlmacen(MovimientoAlmacenQuery query)
        {
            if (query.isDefault())
                return getMovimientosAlmacen();
            List<MovimientoAlmacen> movimientoAlmacen = new List<MovimientoAlmacen>();
            StringBuilder sql = new StringBuilder("SELECT alm, id, art, cant, origen, date_add, dsc FROM mov_inventario WHERE");

            bool primerParametro = false;
            if (query.almacen > 0)
            {
                if (!primerParametro)
                    primerParametro = true;
                sql.Append(" alm = @alm");
            }
            if (query.articulo > 0)
            {
                if (!primerParametro)
                    primerParametro = true;
                else
                    sql.Append(" AND");
                sql.Append(" art = @art");
            }
            if (query.dateInicio != DateTime.MinValue)
            {
                if (!primerParametro)
                    primerParametro = true;
                else
                    sql.Append(" AND");
                sql.Append(" date_add >= @dateInicio");
            }
            if (query.dateFin != DateTime.MinValue)
            {
                if (primerParametro)
                    sql.Append(" AND");
                sql.Append(" date_add <= @dateFin");
            }

            sql.Append(" ORDER BY date_add DESC");

            NpgsqlCommand cmd = new NpgsqlCommand(sql.ToString(), conn);

            if (query.almacen > 0)
                cmd.Parameters.AddWithValue("@alm", query.almacen);
            if (query.articulo > 0)
                cmd.Parameters.AddWithValue("@art", query.articulo);
            if (query.dateInicio != DateTime.MinValue)
                cmd.Parameters.AddWithValue("@dateInicio", query.dateInicio);
            if (query.dateFin != DateTime.MinValue)
                cmd.Parameters.AddWithValue("@dateFin", query.dateFin);

            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                movimientoAlmacen.Add(new MovimientoAlmacen(rdr));
            rdr.Close();

            return movimientoAlmacen;
        }

        public bool addMovimientoAlmacen(MovimientoAlmacen movimiento)
        {
            string sql = "INSERT INTO public.mov_inventario(alm, art, cant, origen, date_add, dsc) VALUES (@alm, @art, @cant, @origen, @date_add, @dsc)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", movimiento.almacen);
            cmd.Parameters.AddWithValue("@art", movimiento.articulo);
            cmd.Parameters.AddWithValue("@cant", movimiento.cantidad);
            cmd.Parameters.AddWithValue("@origen", movimiento.origen);
            cmd.Parameters.AddWithValue("@date_add", movimiento.dateAdd);
            cmd.Parameters.AddWithValue("@dsc", movimiento.descripcion);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool updateMovimientoAlmacen(MovimientoAlmacen movimiento)
        {
            string sql = "UPDATE mov_inventario SET dsc = @dsc WHERE alm = @alm AND id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", movimiento.id);
            cmd.Parameters.AddWithValue("@alm", movimiento.almacen);
            cmd.Parameters.AddWithValue("@dsc", movimiento.descripcion);
            return cmd.ExecuteNonQuery() > 0;
        }

        /* DRONES */

        public List<Drone> getDrones()
        {
            List<Drone> drones = new List<Drone>();
            string sql = "SELECT id, name, uuid, tier, num_slots, num_stacks, num_items, estado, total_energia, energia_actual, upgrade_gen, items_gen, fecha_con, fecha_descon, dsc, upgrade_gps, pos_x, pos_y, pos_z, complejidad, date_add, date_upd, off, off_pos_x, off_pos_y, off_pos_z FROM drones";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                drones.Add(new Drone(rdr));
            rdr.Close();
            return drones;
        }

        public List<Drone> searchDrones(string text)
        {
            List<Drone> drones = new List<Drone>();
            string sql = "SELECT id, name, uuid, tier, num_slots, num_stacks, num_items, estado, total_energia, energia_actual, upgrade_gen, items_gen, fecha_con, fecha_descon, dsc, upgrade_gps, pos_x, pos_y, pos_z, complejidad, date_add, date_upd, off, off_pos_x, off_pos_y, off_pos_z FROM drones WHERE (uuid::text @@ to_tsquery(@text) OR name ILIKE @textq) ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("text", text);
            cmd.Parameters.AddWithValue("textq", text + "%");
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                drones.Add(new Drone(rdr));
            rdr.Close();
            return drones;
        }

        public Drone getDrone(short id)
        {
            string sql = "SELECT id, name, uuid, tier, num_slots, num_stacks, num_items, estado, total_energia, energia_actual, upgrade_gen, items_gen, fecha_con, fecha_descon, dsc, upgrade_gps, pos_x, pos_y, pos_z, complejidad, date_add, date_upd, off, off_pos_x, off_pos_y, off_pos_z FROM drones WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }
            rdr.Read();

            Drone drone = new Drone(rdr);
            rdr.Close();
            return drone;
        }

        public short addDrone(Drone d)
        {
            string sql = "INSERT INTO drones (name,uuid,tier,num_slots,total_energia,energia_actual,upgrade_gen,items_gen,dsc,upgrade_gps,pos_x,pos_y,pos_z,complejidad,off_pos_x,off_pos_y,off_pos_z) VALUES (@name,@uuid,@tier,@num_slots,@total_energia,@energia_actual,@upgrade_gen,@items_gen,@dsc,@upgrade_gps,@pos_x,@pos_y,@pos_z,@complejidad,@off_pos_x,@off_pos_y,@off_pos_z) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", d.name);
            cmd.Parameters.AddWithValue("@uuid", d.uuid);
            cmd.Parameters.AddWithValue("@tier", d.tier);
            cmd.Parameters.AddWithValue("@num_slots", d.numeroSlots);
            cmd.Parameters.AddWithValue("@total_energia", d.totalEnergia);
            cmd.Parameters.AddWithValue("@energia_actual", d.energiaActual);
            cmd.Parameters.AddWithValue("@upgrade_gen", d.upgradeGenerador);
            cmd.Parameters.AddWithValue("@items_gen", d.itemsGenerador);
            cmd.Parameters.AddWithValue("@dsc", d.descripcion);
            cmd.Parameters.AddWithValue("@upgrade_gps", d.upgradeGps);
            cmd.Parameters.AddWithValue("@pos_x", d.posX);
            cmd.Parameters.AddWithValue("@pos_y", d.posY);
            cmd.Parameters.AddWithValue("@pos_z", d.posZ);
            cmd.Parameters.AddWithValue("@complejidad", d.complejidad);
            cmd.Parameters.AddWithValue("@off_pos_x", d.offsetPosX);
            cmd.Parameters.AddWithValue("@off_pos_y", d.offsetPosY);
            cmd.Parameters.AddWithValue("@off_pos_z", d.offsetPosZ);
            NpgsqlDataReader rdr;
            try
            {
                rdr = cmd.ExecuteReader();
            }
            catch (Exception) { return 0; }

            if (!rdr.HasRows)
            {
                rdr.Close();
                return 0;
            }
            rdr.Read();
            short id = rdr.GetInt16(0);
            rdr.Close();

            d.id = id;
            Program.websocketPubSub.onPush("drones", serverHashes.SubscriptionChangeType.insert, id, JsonConvert.SerializeObject(d));
            return id;
        }

        public bool updateDrone(Drone d)
        {
            string sql = "UPDATE drones SET name=@name, uuid=@uuid, tier=@tier, num_slots=@num_slots, total_energia=@total_energia, energia_actual=@energia_actual, upgrade_gen=@upgrade_gen, items_gen=@items_gen, dsc=@dsc, upgrade_gps=@upgrade_gps, pos_x=@pos_x, pos_y=@pos_y, pos_z=@pos_z, complejidad=@complejidad, off = @off, off_pos_x = @off_pos_x, off_pos_y = @off_pos_y, off_pos_z = @off_pos_z WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", d.id);
            cmd.Parameters.AddWithValue("name", d.name);
            cmd.Parameters.AddWithValue("uuid", d.uuid);
            cmd.Parameters.AddWithValue("tier", d.tier);
            cmd.Parameters.AddWithValue("num_slots", d.numeroSlots);
            cmd.Parameters.AddWithValue("total_energia", d.totalEnergia);
            cmd.Parameters.AddWithValue("energia_actual", d.energiaActual);
            cmd.Parameters.AddWithValue("upgrade_gen", d.upgradeGenerador);
            cmd.Parameters.AddWithValue("items_gen", d.itemsGenerador);
            cmd.Parameters.AddWithValue("dsc", d.descripcion);
            cmd.Parameters.AddWithValue("upgrade_gps", d.upgradeGps);
            cmd.Parameters.AddWithValue("pos_x", d.posX);
            cmd.Parameters.AddWithValue("pos_y", d.posY);
            cmd.Parameters.AddWithValue("pos_z", d.posZ);
            cmd.Parameters.AddWithValue("complejidad", d.complejidad);
            cmd.Parameters.AddWithValue("off", d.off);
            cmd.Parameters.AddWithValue("off_pos_x", d.offsetPosX);
            cmd.Parameters.AddWithValue("off_pos_y", d.offsetPosY);
            cmd.Parameters.AddWithValue("off_pos_z", d.offsetPosZ);
            cmd.Prepare();
            try
            {
                if (cmd.ExecuteNonQuery() == 0)
                {
                    return false;
                }
            }
            catch (Exception) { return false; }

            Program.websocketPubSub.onPush("drones", serverHashes.SubscriptionChangeType.update, d.id, JsonConvert.SerializeObject(getDrone(d.id)));
            return true;
        }

        public bool deleteDrone(short id)
        {
            string sql = "DELETE FROM drones WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }

            Program.websocketPubSub.onPush("drones", serverHashes.SubscriptionChangeType.delete, id, string.Empty);
            Program.websocketPubSub.removeTopic("droneLog#" + id);
            Program.websocketPubSub.removeTopic("droneGPS#" + id);
            return true;
        }

        public void updateDroneOnline(string uuid, string name, short energiaActual, short totalEnergia, short posX, short posY, short posZ)
        {
            string sql = "UPDATE drones SET name=@name,total_energia=@total_energia,energia_actual=@energia_actual,pos_x=@pos_x,pos_y=@pos_y,pos_z=@pos_z,estado='O',fecha_con=@fecha_con WHERE uuid = @uuid RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uuid", Guid.Parse(uuid));
            cmd.Parameters.AddWithValue("name", name);
            cmd.Parameters.AddWithValue("total_energia", totalEnergia);
            cmd.Parameters.AddWithValue("energia_actual", energiaActual);
            cmd.Parameters.AddWithValue("pos_x", posX);
            cmd.Parameters.AddWithValue("pos_y", posY);
            cmd.Parameters.AddWithValue("pos_z", posZ);
            cmd.Parameters.AddWithValue("fecha_con", DateTime.Now);
            cmd.Prepare();
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            short id = rdr.GetInt16(0);
            rdr.Close();

            Program.websocketPubSub.onPush("drones", serverHashes.SubscriptionChangeType.update, id, JsonConvert.SerializeObject(getDrone(id)));
        }

        // DRONE - INVENTARIO

        public List<DroneInventario> getDroneInventario(short idDrone)
        {
            List<DroneInventario> inventario = new List<DroneInventario>();
            List<DroneInventarioGet> inventarioGet = new List<DroneInventarioGet>();
            string sql = "SELECT num_slot, cant, art FROM drn_inventario WHERE drn = @drn ORDER BY num_slot ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@drn", idDrone);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                DroneInventarioGet slot = new DroneInventarioGet(rdr);
                inventarioGet.Add(slot);
            }
            rdr.Close();

            foreach (DroneInventarioGet inv in inventarioGet)
            {
                inventario.Add(
                    new DroneInventario(inv.numeroSlot, inv.cant, inv.articulo == 0 ? null : getArticuloSlot(inv.articulo)));
            }

            return inventario;
        }

        public void setDroneInventario(Guid uuid, List<DroneInventarioSet> setInventario)
        {
            for (int i = 0; i < setInventario.Count; i++)
            {
                DroneInventarioSet inv = setInventario[i];

                string sql = "UPDATE drn_inventario SET art = @art, cant = @cant WHERE drn = (SELECT id FROM drones WHERE uuid = @uuid) AND num_slot = @num_slot";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@uuid", uuid);
                cmd.Parameters.AddWithValue("@num_slot", (i + 1));
                if (inv.idArticulo.Equals(string.Empty))
                {
                    cmd.Parameters.AddWithValue("@art", DBNull.Value);
                    cmd.Parameters.AddWithValue("@cant", 0);
                }
                else
                {
                    short art = getArticulo(inv.idArticulo);
                    if (art > 0)
                        cmd.Parameters.AddWithValue("@art", art);
                    else
                        cmd.Parameters.AddWithValue("@art", DBNull.Value);
                    cmd.Parameters.AddWithValue("@cant", inv.cant);
                }
                cmd.ExecuteNonQuery();
            }

            short id = getRobot(uuid).id;
            Program.websocketPubSub.addTopic("droneInv#" + id);
            Program.websocketPubSub.onPush("droneInv#" + id, serverHashes.SubscriptionChangeType.update, 0, JsonConvert.SerializeObject(getDroneInventario(id)));
        }

        public bool clearDroneInventario(Guid uuid)
        {
            string sql = "UPDATE drn_inventario SET art = NULL, cant = 0 WHERE drn = (SELECT id FROM drones WHERE uuid = @uuid)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            return cmd.ExecuteNonQuery() > 0;
        }

        // DRONE - LOGS

        public List<DroneLog> getDroneLogs(DroneLogQuery query)
        {
            List<DroneLog> logs = new List<DroneLog>();
            string sql = "SELECT id, titulo, msg FROM drn_logs WHERE drn = @drn";

            if (query.start != DateTime.MinValue)
                sql += " AND id >= @start";
            if (query.end != DateTime.MaxValue)
                sql += " AND id <= @end";

            sql += " ORDER BY id DESC";
            if (query.limit > 0)
                sql += " LIMIT @limit OFFSET @offset";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@drn", query.idDrone);
            if (query.limit > 0)
                cmd.Parameters.AddWithValue("@limit", query.limit);

            if (query.start != DateTime.MinValue)
                cmd.Parameters.AddWithValue("@start", query.start);
            if (query.end != DateTime.MaxValue)
                cmd.Parameters.AddWithValue("@end", query.end);

            cmd.Parameters.AddWithValue("@offset", query.offset);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                logs.Add(new DroneLog(rdr));
            }
            rdr.Close();
            return logs;
        }

        public bool addDroneLog(Guid uuid, DroneLog log)
        {
            string sql = "INSERT INTO drn_logs (rob, id, titulo, msg) VALUES ((SELECT id FROM drones WHERE uuid = @uuid),@id,@titulo,@msg)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.Parameters.AddWithValue("@id", log.id);
            cmd.Parameters.AddWithValue("@titulo", log.name);
            cmd.Parameters.AddWithValue("@msg", log.mensaje);
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }

            short id = getRobot(uuid).id;
            Program.websocketPubSub.addTopic("droneLog#" + id);
            Program.websocketPubSub.onPush("droneLog#" + id, serverHashes.SubscriptionChangeType.insert, 0, JsonConvert.SerializeObject(log));
            return true;
        }

        public bool clearDroneLogs(short idDrone)
        {
            string sql = "DELETE FROM drn_logs WHERE drn = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idDrone);
            if (cmd.ExecuteNonQuery() == 0)
                return false;

            Program.websocketPubSub.addTopic("robotLog#" + idDrone);
            Program.websocketPubSub.onPush("robotLog#" + idDrone, serverHashes.SubscriptionChangeType.delete, 0, string.Empty);
            return true;
        }

        public bool clearDroneLogs(short idDrone, DateTime start, DateTime end)
        {
            string sql = "DELETE FROM drn_logs WHERE drn = @id AND id >= @start AND id <= @end";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idDrone);
            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);
            if (cmd.ExecuteNonQuery() == 0)
                return false;

            Program.websocketPubSub.addTopic("robotLog#" + idDrone);
            Program.websocketPubSub.onPush("robotLog#" + idDrone, serverHashes.SubscriptionChangeType.delete, 0, string.Empty);
            return true;
        }

        // DRONE - GPS

        public List<DroneGPS> getDroneGPS(short idDrone, int offset = 0, int limit = 10)
        {
            List<DroneGPS> gps = new List<DroneGPS>();
            string sql = "SELECT id, pos_x, pos_y, pos_z FROM drn_gps WHERE drn = @drone ORDER BY id DESC LIMIT @limit OFFSET @offset";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@drone", idDrone);
            cmd.Parameters.AddWithValue("@limit", limit);
            cmd.Parameters.AddWithValue("@offset", offset);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                gps.Add(new DroneGPS(rdr));
            }
            rdr.Close();
            return gps;
        }

        public bool addDroneGps(Guid uuid, short posX, short posY, short posZ)
        {
            string sql = "SELECT off_pos_x, off_pos_y, off_pos_z FROM drones WHERE uuid = @uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uuid", uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            short offsetPosX = rdr.GetInt16(0);
            short offsetPosY = rdr.GetInt16(1);
            short offsetPosZ = rdr.GetInt16(2);
            rdr.Close();

            sql = "INSERT INTO drn_gps (rob, pos_x, pos_y, pos_z) VALUES ((SELECT id FROM drones WHERE uuid = @uuid),@pos_x,@pos_y,@pos_z)";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.Parameters.AddWithValue("@pos_x", posX - offsetPosX);
            cmd.Parameters.AddWithValue("@pos_y", posY - offsetPosY);
            cmd.Parameters.AddWithValue("@pos_z", posZ - offsetPosZ);
            bool ok = cmd.ExecuteNonQuery() > 0;

            if (!ok)
                return false;

            sql = "UPDATE drones SET pos_x=@pos_x, pos_y=@pos_y, pos_z=@pos_z WHERE uuid = @uuid";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uuid", uuid);
            cmd.Parameters.AddWithValue("pos_x", posX - offsetPosX);
            cmd.Parameters.AddWithValue("pos_y", posY - offsetPosY);
            cmd.Parameters.AddWithValue("pos_z", posZ - offsetPosZ);
            cmd.Prepare();
            if (cmd.ExecuteNonQuery() == 0)
            {
                return false;
            }

            short id = getRobot(uuid).id;
            Program.websocketPubSub.addTopic("droneGPS#" + id);
            Program.websocketPubSub.onPush("droneGPS#" + id, serverHashes.SubscriptionChangeType.insert, 0, JsonConvert.SerializeObject(
                new DroneGPS((short)(posX - offsetPosX), (short)(posY - offsetPosY), (short)(posZ - offsetPosZ))));
            return true;
        }

    }
}
