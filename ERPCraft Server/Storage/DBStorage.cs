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
            string sql = "UPDATE robots SET estado = 'F' WHERE estado = 'O'";
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
        /// Checks that the tables exists and that the dabatase version is up to date
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
            {
                sql = "SELECT ver FROM config WHERE act = true";
                cmd = new NpgsqlCommand(sql, conn);
                try
                {
                    rdr = cmd.ExecuteReader();
                    if (!rdr.HasRows)
                    {
                        rdr.Close();
                        return;
                    }
                    rdr.Read();
                    short version = rdr.GetInt16(0);
                    rdr.Close();

                    if (version < 2)
                    {
                        for (int i = version; i < 2; i++)
                        {
                            Stream file = Assembly.GetExecutingAssembly().GetManifestResourceStream("ERPCraft_Server.Embedded.db_update_" + i + ".sql");
                            TextReader reader = new StreamReader(file);
                            string upgrade = reader.ReadToEnd();
                            rdr.Close();
                            file.Close();
                            cmd = new NpgsqlCommand(upgrade, conn);
                            if (cmd.ExecuteNonQuery() == 0)
                            {
                                Console.WriteLine("THERE WAS AN ERROR UPGRADING THE DATABASE.");
                                return;
                            }

                        }
                    }
                }
                catch (Exception) { }

                sql = "SET search_path TO public";
                cmd = new NpgsqlCommand(sql, conn);
                cmd.ExecuteNonQuery();

                sql = "UPDATE config SET ver = 2";
                cmd = new NpgsqlCommand(sql, conn);
                cmd.ExecuteNonQuery();

                return;
            }

            //  read the SQL file that creates the schema from an embedded file
            Console.WriteLine("*** THE DATABASE IS EMPTY. CREATING THE SCHEMA INTO THE DATABASE. ***");
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ERPCraft_Server.Embedded.db_create.sql");
            TextReader tr = new StreamReader(stream);
            string schema = tr.ReadToEnd();
            rdr.Close();
            stream.Close();

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

            Console.WriteLine("*** COPYING ITEMS INTO THE DATABASE ***");

            stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ERPCraft_Server.Embedded.articulos.sql");
            tr = new StreamReader(stream);
            schema = tr.ReadToEnd();
            rdr.Close();
            stream.Close();

            cmd = new NpgsqlCommand(schema, conn);
            if (cmd.ExecuteNonQuery() == 0)
            {
                Console.WriteLine("THERE WAS AN ERROR COPYING THE MINECRAFT ITEMS INTO THE DATABASE.");
                return;
            }

            Console.WriteLine("*** MINECRAFT ITEMS CREATED OK ***");
        }

        public void limpiar()
        {
            DateTime time = DateTime.Now;
            // Robot
            if (Program.ajuste.limpiarRobotGps)
            {
                string sql = "DELETE FROM public.rob_gps WHERE id < @tim";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tim", time.AddDays(-Program.ajuste.diasRobotGps));
                cmd.ExecuteNonQuery();

            }
            if (Program.ajuste.limpiarRobotLogs)
            {
                string sql = "DELETE FROM public.rob_logs WHERE id < @tim";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tim", time.AddDays(-Program.ajuste.diasRobotLogs));
                cmd.ExecuteNonQuery();
            }
            // Drone
            if (Program.ajuste.limpiarDroneGps)
            {
                string sql = "DELETE FROM public.drn_gps WHERE id < @tim";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tim", time.AddDays(-Program.ajuste.diasDroneGps));
                cmd.ExecuteNonQuery();

            }
            if (Program.ajuste.limpiarDroneLogs)
            {
                string sql = "DELETE FROM public.drn_logs WHERE id < @tim";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tim", time.AddDays(-Program.ajuste.diasDroneLogs));
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
            // Notificaciones
            if (Program.ajuste.limpiarNotificaciones)
            {
                string sql = "DELETE FROM notificaciones WHERE id < @tim AND leido = false";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tim", time.AddHours(-Program.ajuste.horasNotificaciones));
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
            string sql = "SELECT id,name,act,lim_rob_gps,dias_rob_gps,lim_robot_log,dias_rob_log,lim_drn_gps,dias_drn_gps,lim_drn_log,dias_drn_log,lim_bat_hist,horas_bat_hist,vacuum_lim,reindex_lim,ping_int,timeout,web_port,oc_port,hash_rounds,lim_notif,horas_notif,permitir_autoreg,pwd,salt,iteraciones FROM public.config ORDER BY id ASC";
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
            string sql = "SELECT id,name,act,lim_rob_gps,dias_rob_gps,lim_robot_log,dias_rob_log,lim_drn_gps,dias_drn_gps,lim_drn_log,dias_drn_log,lim_bat_hist,horas_bat_hist,vacuum_lim,reindex_lim,ping_int,timeout,web_port,oc_port,hash_rounds,lim_notif,horas_notif,permitir_autoreg,pwd,salt,iteraciones FROM public.config WHERE act = true";
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
            string sql = "SELECT id,name,act,lim_rob_gps,dias_rob_gps,lim_robot_log,dias_rob_log,lim_drn_gps,dias_drn_gps,lim_drn_log,dias_drn_log,lim_bat_hist,horas_bat_hist,vacuum_lim,reindex_lim,ping_int,timeout,web_port,oc_port,hash_rounds,lim_notif,horas_notif,permitir_autoreg,pwd,salt,iteraciones FROM public.config WHERE id = @id";
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
            string sql = "INSERT INTO public.config(name,act,lim_rob_gps,dias_rob_gps,lim_robot_log,dias_rob_log,lim_drn_gps,dias_drn_gps,lim_drn_log,dias_drn_log,lim_bat_hist,horas_bat_hist,vacuum_lim,reindex_lim,ping_int,timeout,web_port,oc_port,hash_rounds,lim_notif,horas_notif,permitir_autoreg) VALUES (@name,false,@lim_rob_gps,@dias_rob_gps,@lim_robot_log,@dias_rob_log,@lim_drn_gps,@dias_drn_gps,@lim_drn_log,@dias_drn_log,@lim_bat_hist,@horas_bat_hist,@vacuum_lim,@reindex_lim,@ping_int,@timeout,@web_port,@oc_port,@hash_rounds,@lim_notif,@horas_notif,@permitir_autoreg) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", ajuste.name);
            cmd.Parameters.AddWithValue("@lim_rob_gps", ajuste.limpiarRobotGps);
            cmd.Parameters.AddWithValue("@dias_rob_gps", ajuste.diasRobotGps);
            cmd.Parameters.AddWithValue("@lim_robot_log", ajuste.limpiarRobotLogs);
            cmd.Parameters.AddWithValue("@dias_rob_log", ajuste.diasRobotLogs);
            cmd.Parameters.AddWithValue("@lim_drn_gps", ajuste.limpiarDroneGps);
            cmd.Parameters.AddWithValue("@dias_drn_gps", ajuste.diasDroneGps);
            cmd.Parameters.AddWithValue("@lim_drn_log", ajuste.limpiarDroneLogs);
            cmd.Parameters.AddWithValue("@dias_drn_log", ajuste.diasDroneLogs);
            cmd.Parameters.AddWithValue("@lim_bat_hist", ajuste.limpiarBateriaHistorial);
            cmd.Parameters.AddWithValue("@horas_bat_hist", ajuste.horasBateriaHistorial);
            cmd.Parameters.AddWithValue("@vacuum_lim", ajuste.vacuumLimpiar);
            cmd.Parameters.AddWithValue("@reindex_lim", ajuste.reindexLimpiar);
            cmd.Parameters.AddWithValue("@ping_int", ajuste.pingInterval);
            cmd.Parameters.AddWithValue("@timeout", ajuste.timeout);
            cmd.Parameters.AddWithValue("@web_port", ajuste.puertoWeb);
            cmd.Parameters.AddWithValue("@oc_port", ajuste.puertoOC);
            cmd.Parameters.AddWithValue("@hash_rounds", ajuste.hashIteraciones);
            cmd.Parameters.AddWithValue("@lim_notif", ajuste.limpiarNotificaciones);
            cmd.Parameters.AddWithValue("@horas_notif", ajuste.horasNotificaciones);
            cmd.Parameters.AddWithValue("@permitir_autoreg", ajuste.permitirAutoregistrar);
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
            string sql = "UPDATE public.config SET name=@name,lim_rob_gps=@lim_rob_gps,dias_rob_gps=@dias_rob_gps,lim_robot_log=@lim_robot_log,dias_rob_log=@dias_rob_log,lim_drn_gps=@lim_drn_gps,dias_drn_gps=@dias_drn_gps,lim_drn_log=@lim_drn_log,dias_drn_log=@dias_drn_log,lim_bat_hist=@lim_bat_hist,horas_bat_hist=@horas_bat_hist,vacuum_lim=@vacuum_lim,reindex_lim=@reindex_lim,ping_int=@ping_int,timeout=@timeout,web_port=@web_port,oc_port=@oc_port,hash_rounds=@hash_rounds,lim_notif=@lim_notif,horas_notif=@horas_notif,permitir_autoreg=@permitir_autoreg WHERE id=@id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", ajuste.id);
            cmd.Parameters.AddWithValue("@name", ajuste.name);
            cmd.Parameters.AddWithValue("@lim_rob_gps", ajuste.limpiarRobotGps);
            cmd.Parameters.AddWithValue("@dias_rob_gps", ajuste.diasRobotGps);
            cmd.Parameters.AddWithValue("@lim_robot_log", ajuste.limpiarRobotLogs);
            cmd.Parameters.AddWithValue("@dias_rob_log", ajuste.diasRobotLogs);
            cmd.Parameters.AddWithValue("@lim_drn_gps", ajuste.limpiarDroneGps);
            cmd.Parameters.AddWithValue("@dias_drn_gps", ajuste.diasDroneGps);
            cmd.Parameters.AddWithValue("@lim_drn_log", ajuste.limpiarDroneLogs);
            cmd.Parameters.AddWithValue("@dias_drn_log", ajuste.diasDroneLogs);
            cmd.Parameters.AddWithValue("@lim_bat_hist", ajuste.limpiarBateriaHistorial);
            cmd.Parameters.AddWithValue("@horas_bat_hist", ajuste.horasBateriaHistorial);
            cmd.Parameters.AddWithValue("@vacuum_lim", ajuste.vacuumLimpiar);
            cmd.Parameters.AddWithValue("@reindex_lim", ajuste.reindexLimpiar);
            cmd.Parameters.AddWithValue("@ping_int", ajuste.pingInterval);
            cmd.Parameters.AddWithValue("@timeout", ajuste.timeout);
            cmd.Parameters.AddWithValue("@web_port", ajuste.puertoWeb);
            cmd.Parameters.AddWithValue("@oc_port", ajuste.puertoOC);
            cmd.Parameters.AddWithValue("@hash_rounds", ajuste.hashIteraciones);
            cmd.Parameters.AddWithValue("@lim_notif", ajuste.limpiarNotificaciones);
            cmd.Parameters.AddWithValue("@horas_notif", ajuste.horasNotificaciones);
            cmd.Parameters.AddWithValue("@permitir_autoreg", ajuste.permitirAutoregistrar);
            if (cmd.ExecuteNonQuery() == 0)
                return false;

            if (Program.ajuste.id == ajuste.id)
                Program.ajuste = Program.db.getAjuste(ajuste.id);

            Program.websocketPubSub.onPush("config", serverHashes.SubscriptionChangeType.update, ajuste.id, JsonConvert.SerializeObject(getAjuste(ajuste.id)));
            return true;
        }

        public bool updateAjuste(short id, string hash, string salt, int hashIteraciones)
        {
            string sql = "UPDATE public.config SET pwd=@pwd,salt=@salt,iteraciones=@iteraciones WHERE id=@id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@pwd", hash);
            cmd.Parameters.AddWithValue("@salt", salt);
            cmd.Parameters.AddWithValue("@iteraciones", hashIteraciones);
            return cmd.ExecuteNonQuery() > 0;
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
            string sql = "SELECT id,name,uuid,ultima_con,date_add FROM api_key";
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
            string sql = "SELECT id,name,uuid,ultima_con,date_add FROM api_key WHERE id = @id";
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
            string sql = "SELECT id,name,uuid,ultima_con,date_add FROM api_key WHERE uuid = @uuid";
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
            string sql = "SELECT uuid,name,dsc,online,ultima_con,autoreg,pwd,salt,iteraciones,notif_on,notif_off,date_add FROM servers ORDER BY date_add ASC";
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
            string sql = "SELECT uuid,name,dsc,online,ultima_con,autoreg,pwd,salt,iteraciones,notif_on,notif_off,date_add FROM servers WHERE uuid = @uuid";
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
            string sql = "INSERT INTO public.servers(uuid,name,dsc,autoreg,notif_on,notif_off) VALUES (@uuid,@name,@dsc,@autoreg,@notif_on,@notif_off)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", server.uuid);
            cmd.Parameters.AddWithValue("@name", server.name);
            cmd.Parameters.AddWithValue("@dsc", server.descripcion);
            cmd.Parameters.AddWithValue("@autoreg", server.permitirAutoregistro);
            cmd.Parameters.AddWithValue("@notif_on", server.notificacionOnline);
            cmd.Parameters.AddWithValue("@notif_off", server.notificacionOffline);
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

        /// <summary>
        /// Se usa para los servidores que se autoregistren en el sistema, se usan parámetros por defecto
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public bool addServer(Guid uuid)
        {
            string sql = "INSERT INTO public.servers(uuid,name,autoreg,notif_on,notif_off) VALUES (@uuid,@name,@autoreg,@notif_on,@notif_off)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.Parameters.AddWithValue("@name", "Servidor autoregistrado #" + uuid.ToString().Substring(0, 4));
            cmd.Parameters.AddWithValue("@autoreg", false);
            cmd.Parameters.AddWithValue("@notif_on", false);
            cmd.Parameters.AddWithValue("@notif_off", false);
            try
            {
                if (cmd.ExecuteNonQuery() == 0)
                {
                    return false;
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); return false; }

            Program.websocketPubSub.onPush("servers", serverHashes.SubscriptionChangeType.insert, 0, JsonConvert.SerializeObject(getServer(uuid)));
            return true;
        }

        public bool updateServer(Server server)
        {
            string sql = "UPDATE public.servers SET name=@name, dsc=@dsc, autoreg=@autoreg, notif_on=@notif_on, notif_off=@notif_off WHERE uuid=@uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", server.uuid);
            cmd.Parameters.AddWithValue("@name", server.name);
            cmd.Parameters.AddWithValue("@dsc", server.descripcion);
            cmd.Parameters.AddWithValue("@autoreg", server.permitirAutoregistro);
            cmd.Parameters.AddWithValue("@notif_on", server.notificacionOnline);
            cmd.Parameters.AddWithValue("@notif_off", server.notificacionOffline);
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
            else
            {
                Server server = getServer(uuid);
                if (server.notificacionOnline && online)
                    addNotificacion(new Notificacion("Server online", "El servidor " + server.name + " (" + server.uuid + ") se ha conectado", NotificacionOrigen.ServerOnline));
                else if (server.notificacionOffline)
                    addNotificacion(new Notificacion("Server offline", "El servidor " + server.name + " (" + server.uuid + ") se ha desconectado", NotificacionOrigen.ServerOffline));
                Program.websocketPubSub.onPush("servers", serverHashes.SubscriptionChangeType.update, 0, JsonConvert.SerializeObject(getServer(uuid)));
                return true;
            }
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
            string sql = "SELECT id,name,pwd,salt,iteraciones,ultima_con,dsc,off,date_add FROM usuarios WHERE off = false ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                usuarios.Add(new Usuario(rdr));
            }
            rdr.Close();
            return usuarios;
        }

        public List<Usuario> searchUsuarios(string text, bool off)
        {
            List<Usuario> usuarios = new List<Usuario>();
            string sql = "SELECT id,name,pwd,salt,iteraciones,ultima_con,dsc,off,date_add FROM usuarios WHERE name ILIKE @text";
            if (!off)
                sql += " AND off = false";
            sql += " ORDER BY id ASC";
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
            string sql = "SELECT id,name,pwd,salt,iteraciones,ultima_con,dsc,off,date_add FROM usuarios WHERE name = @username AND off = false";
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
            string sql = "SELECT id,name,pwd,salt,iteraciones,ultima_con,dsc,off,date_add FROM usuarios WHERE id = @id";
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
            string sql = "INSERT INTO usuarios (name,pwd,salt,iteraciones,ultima_con) VALUES(@username,@pwd,@salt,@iteraciones,@ultima_con) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@username", usuario.name);
            cmd.Parameters.AddWithValue("@pwd", usuario.pwd);
            cmd.Parameters.AddWithValue("@salt", usuario.salt);
            cmd.Parameters.AddWithValue("@iteraciones", usuario.iteraciones);
            cmd.Parameters.AddWithValue("@ultima_con", usuario.ultima_con);
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

            Program.websocketPubSub.onPush("usuarios", serverHashes.SubscriptionChangeType.insert, id, JsonConvert.SerializeObject(getUsuario(id)));
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
            bool ok;
            try
            {
                ok = cmd.ExecuteNonQuery() == 0;
            }
            catch (Exception) { return false; }
            if (ok)
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
                    sql = "SELECT id, name, mine_id, cant, dsc FROM articulos WHERE mine_id ILIKE @txt ORDER BY id ASC";
                }
                else
                {
                    sql = "SELECT id, name, mine_id, cant, dsc FROM articulos WHERE name ILIKE @txt ORDER BY id ASC";
                }
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@txt", "%" + texto + "%");
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
            string sql = "SELECT id, name, mine_id, cant, dsc FROM articulos WHERE id = @id";
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
            StringBuilder sql = new StringBuilder("SELECT id, name, uuid, tier, num_slots, num_stacks, num_items, estado, total_energia, energia_actual, upgrade_gen, items_gen, fecha_con, fecha_descon, dsc, upgrade_gps, pos_x, pos_y, pos_z, complejidad, date_add, date_upd, off, off_pos_x, off_pos_y, off_pos_z, notif_con, notif_descon, notif_bat_baj, upgrade_inv_con, upgrade_geo FROM robots");

            if (query.off && (!query.text.Equals(string.Empty)))
                sql.Append(" WHERE (uuid::text ILIKE @text OR name ILIKE @text)");
            else if (!query.off)
            {
                sql.Append(" WHERE off = false");
                if (!query.text.Equals(string.Empty))
                    sql.Append(" AND (uuid::text ILIKE @text OR name ILIKE @text)");
            }

            sql.Append(" ORDER BY id ASC");
            NpgsqlCommand cmd = new NpgsqlCommand(sql.ToString(), conn);
            if (!query.text.Equals(string.Empty))
                cmd.Parameters.AddWithValue("text", "%" + query.text + "%");
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                robots.Add(new Robot(rdr));
            rdr.Close();
            return robots;
        }

        public Robot getRobot(short id)
        {
            string sql = "SELECT id, name, uuid, tier, num_slots, num_stacks, num_items, estado, total_energia, energia_actual, upgrade_gen, items_gen, fecha_con, fecha_descon, dsc, upgrade_gps, pos_x, pos_y, pos_z, complejidad, date_add, date_upd, off, off_pos_x, off_pos_y, off_pos_z, notif_con, notif_descon, notif_bat_baj, upgrade_inv_con, upgrade_geo FROM robots WHERE off = false AND id = @id";
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
            string sql = "SELECT id, name, uuid, tier, num_slots, num_stacks, num_items, estado, total_energia, energia_actual, upgrade_gen, items_gen, fecha_con, fecha_descon, dsc, upgrade_gps, pos_x, pos_y, pos_z, complejidad, date_add, date_upd, off, off_pos_x, off_pos_y, off_pos_z, notif_con, notif_descon, notif_bat_baj, upgrade_inv_con, upgrade_geo FROM robots WHERE off = false AND uuid = @uuid";
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
            string sql = "INSERT INTO robots (name,uuid,tier,num_slots,total_energia,energia_actual,upgrade_gen,items_gen,dsc,upgrade_gps,pos_x,pos_y,pos_z,complejidad,off_pos_x,off_pos_y,off_pos_z,upgrade_inv_con,upgrade_geo) VALUES (@name,@uuid,@tier,@num_slots,@total_energia,@energia_actual,@upgrade_gen,@items_gen,@dsc,@upgrade_gps,@pos_x,@pos_y,@pos_z,@complejidad,@off_pos_x,@off_pos_y,@off_pos_z,@upgrade_inv_con,@upgrade_geo) RETURNING id";
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
            cmd.Parameters.AddWithValue("@upgrade_inv_con", r.upgradeInventoryController);
            cmd.Parameters.AddWithValue("@upgrade_geo", r.upgradeGeolyzer);
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

            Program.websocketPubSub.onPush("robots", serverHashes.SubscriptionChangeType.insert, id, JsonConvert.SerializeObject(getRobot(id)));
            return id;
        }

        public bool updateRobot(Robot r)
        {
            string sql = "UPDATE robots SET name=@name, uuid=@uuid, tier=@tier, num_slots=@num_slots, total_energia=@total_energia, energia_actual=@energia_actual, upgrade_gen=@upgrade_gen, items_gen=@items_gen, dsc=@dsc, upgrade_gps=@upgrade_gps, pos_x=@pos_x, pos_y=@pos_y, pos_z=@pos_z, complejidad=@complejidad, off = @off, off_pos_x = @off_pos_x, off_pos_y = @off_pos_y, off_pos_z = @off_pos_z, notif_con = @notif_con, notif_descon = @notif_descon, notif_bat_baj = @notif_bat_baj, upgrade_inv_con = @upgrade_inv_con, upgrade_geo = @upgrade_geo, date_upd = CURRENT_TIMESTAMP(3) WHERE id = @id";
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
            cmd.Parameters.AddWithValue("notif_con", r.notificacionConexion);
            cmd.Parameters.AddWithValue("notif_descon", r.notificacionDesconexion);
            cmd.Parameters.AddWithValue("notif_bat_baj", r.notificacionBateriaBaja);
            cmd.Parameters.AddWithValue("@upgrade_inv_con", r.upgradeInventoryController);
            cmd.Parameters.AddWithValue("@upgrade_geo", r.upgradeGeolyzer);
            cmd.Prepare();
            try
            {
                if (cmd.ExecuteNonQuery() == 0)
                {
                    return false;
                }
            }
            catch (Exception) { return false; }

            Program.websocketPubSub.onPush("robots", serverHashes.SubscriptionChangeType.update, r.id, JsonConvert.SerializeObject(getRobot(r.id)));
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

        public void updateRobotBateria(string uuid, int energiaActual)
        {
            string sql = "SELECT id,energia_actual FROM robots WHERE uuid = @uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uuid", Guid.Parse(uuid));
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return;
            }
            rdr.Read();
            short id = rdr.GetInt16(0);
            int energiaAnterior = rdr.GetInt32(1);
            rdr.Close();

            sql = "UPDATE robots SET energia_actual=@energia_actual WHERE id = @id";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("energia_actual", energiaActual);
            cmd.Prepare();
            if (cmd.ExecuteNonQuery() <= 0)
            {
                return;
            }

            Robot r = getRobot(id);
            Program.websocketPubSub.onPush("robots", serverHashes.SubscriptionChangeType.update, id, JsonConvert.SerializeObject(r));
            if (r.notificacionBateriaBaja && ((energiaAnterior / r.totalEnergia) * 100) >= 10 && ((r.energiaActual / r.totalEnergia) * 100) < 10)
            {
                addNotificacion(new Notificacion("Batería del robot baja", "El robot " + r.name + "#" + r.id + " tiene la batería por debajo del 10%.", NotificacionOrigen.RobotBateriaBaja));
            }
        }

        public void updateRobotOnline(string uuid, string name, int energiaActual, int totalEnergia, short posX, short posY, short posZ)
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

            Robot r = getRobot(id);
            Program.websocketPubSub.onPush("robots", serverHashes.SubscriptionChangeType.update, id, JsonConvert.SerializeObject(r));
            if (r.notificacionConexion)
            {
                addNotificacion(new Notificacion("Robot online", "El robot " + r.name + "#" + r.id + " se ha conectado.", NotificacionOrigen.RobotConectado));
            }
        }

        public void autoRegisterRobot(Guid serveruuid, string serverpwd, string uuid, string name, short num_slots, int energiaActual, int totalEnergia, bool generatorUpgrade, short numItems, bool gpsUpgrade, short posX, short posY, short posZ, bool inventory_controller, bool geolyzer)
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
            sql = "INSERT INTO robots (name,uuid,num_slots,total_energia,energia_actual,pos_x,pos_y,pos_z,upgrade_gen,items_gen,upgrade_gps,upgrade_inv_con,upgrade_geo) VALUES (@name,@uuid,@num_slots,@total_energia,@energia_actual,@pos_x,@pos_y,@pos_z,@upgrade_gen,@items_gen,@upgrade_gps,@upgrade_inv_con,@upgrade_geo) RETURNING id";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@uuid", Guid.Parse(uuid));
            cmd.Parameters.AddWithValue("@num_slots", num_slots);
            cmd.Parameters.AddWithValue("@total_energia", totalEnergia);
            cmd.Parameters.AddWithValue("@energia_actual", energiaActual);
            cmd.Parameters.AddWithValue("@pos_x", posX);
            cmd.Parameters.AddWithValue("@pos_y", posY);
            cmd.Parameters.AddWithValue("@pos_z", posZ);
            cmd.Parameters.AddWithValue("@upgrade_gen", generatorUpgrade);
            cmd.Parameters.AddWithValue("@items_gen", numItems);
            cmd.Parameters.AddWithValue("@upgrade_gps", gpsUpgrade);
            cmd.Parameters.AddWithValue("@upgrade_inv_con", inventory_controller);
            cmd.Parameters.AddWithValue("@upgrade_geo", geolyzer);
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

        public void updateRobotOffline(string uuid, bool outOfRange = false)
        {
            string sql = "SELECT id,energia_actual FROM robots WHERE uuid = @uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uuid", Guid.Parse(uuid));
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return;
            }
            rdr.Read();
            short id = rdr.GetInt16(0);
            int energia = rdr.GetInt32(1);
            rdr.Close();

            sql = "UPDATE robots SET estado=@estado,fecha_descon=@fecha_descon WHERE uuid = @uuid";
            cmd = new NpgsqlCommand(sql, conn);
            char estado = 'F';
            if (outOfRange)
                estado = 'L';
            else if (energia <= 250)
                estado = 'B';
            cmd.Parameters.AddWithValue("estado", estado);
            cmd.Parameters.AddWithValue("fecha_descon", DateTime.Now);
            cmd.Parameters.AddWithValue("uuid", Guid.Parse(uuid));
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Robot r = getRobot(id);
            Program.websocketPubSub.onPush("robots", serverHashes.SubscriptionChangeType.update, id, JsonConvert.SerializeObject(r));
            Program.websocketPubSub.removeTopic("robotLog#" + id);
            Program.websocketPubSub.removeTopic("robotInv#" + id);
            Program.websocketPubSub.removeTopic("robotGPS#" + id);

            if (r.notificacionDesconexion)
                addNotificacion(new Notificacion("Robot offline", "El robot " + r.name + "#" + r.id + " se ha desconectado.", NotificacionOrigen.RobotDesconectado));
        }

        public List<RobotInventario> loadRobotReferenceInventario(short[] ids)
        {
            List<RobotInventario> inventario = new List<RobotInventario>();
            List<RobotInventarioGet> inventarioGet = new List<RobotInventarioGet>();
            for (int i = 0; i < ids.Length; i++)
            {
                string sql = "SELECT num_slot, cant, art FROM rob_inventario WHERE rob = @robot ORDER BY num_slot ASC";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@robot", ids[i]);
                NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    RobotInventarioGet slot = new RobotInventarioGet(rdr);
                    inventarioGet.Add(slot);
                }
                rdr.Close();
            }

            foreach (RobotInventarioGet inv in inventarioGet)
            {
                inventario.Add(
                    new RobotInventario(inv.numeroSlot, inv.cant, inv.articulo == 0 ? null : getArticuloSlot(inv.articulo)));
            }

            return inventario;
        }

        public List<RobotGPS> loadRobotReferenceGPS(short[] ids)
        {
            List<RobotGPS> gps = new List<RobotGPS>();
            for (int i = 0; i < ids.Length; i++)
            {
                string sql = "SELECT id, pos_x, pos_y, pos_z FROM rob_gps WHERE rob = @robot ORDER BY id DESC";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@robot", ids[i]);
                NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    gps.Add(new RobotGPS(rdr));
                }
                rdr.Close();
            }
            return gps;
        }

        public List<RobotLog> loadRobotReferenceLog(short[] ids)
        {
            List<RobotLog> logs = new List<RobotLog>();
            for (int i = 0; i < ids.Length; i++)
            {
                string sql = "SELECT id, titulo, msg FROM rob_logs WHERE rob = @robot ORDER BY id DESC";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@robot", ids[i]);
                NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    logs.Add(new RobotLog(rdr));
                }
                rdr.Close();
            }
            return logs;
        }

        public string getRobotEnsamblado(short id)
        {
            string sql = "SELECT ensamblado FROM robots WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return "null";
            }
            rdr.Read();
            if (rdr.IsDBNull(0))
            {
                rdr.Close();
                return "null";
            }
            string json = rdr.GetString(0);
            rdr.Close();
            return json;
        }

        public bool setRobotEnsamblado(short id, RobotEnsamblado ensamblado)
        {
            string sql = "UPDATE robots SET ensamblado = CAST(@ensamblado AS json) WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@ensamblado", JsonConvert.SerializeObject(ensamblado));
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool getRobotInventoryController(short idRobot)
        {
            string sql = "SELECT upgrade_inv_con FROM robots WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idRobot);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return false;
            }
            rdr.Read();
            bool upgradeInventoryController = rdr.GetBoolean(0);
            rdr.Close();
            return upgradeInventoryController;
        }

        public bool getRobotGeolyzer(short idRobot)
        {
            string sql = "SELECT upgrade_geo FROM robots WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idRobot);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return false;
            }
            rdr.Read();
            bool upgradeGeolyzer = rdr.GetBoolean(0);
            rdr.Close();
            return upgradeGeolyzer;
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
            string sql = "SELECT id, red_ele, name, uuid, cap_ele, carga_act, dsc, tipo, notif, notif_carga FROM bat WHERE red_ele = @red_ele ORDER BY id ASC";
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
            string sql = "SELECT id, red_ele, name, uuid, cap_ele, carga_act, dsc, tipo, notif, notif_carga FROM bat WHERE red_ele = @red_ele AND id = @id";
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
            string sql = "SELECT id, red_ele, name, uuid, cap_ele, carga_act, dsc, tipo, notif, notif_carga FROM bat WHERE uuid = @uuid";
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
            string sql = "INSERT INTO bat (red_ele,name,uuid,cap_ele,carga_act,dsc,tipo,notif,notif_carga) VALUES (@red_ele,@name,@uuid,@cap_ele,@carga_act,@dsc,@tipo,@notif,@notif_carga) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@red_ele", bat.redElectrica);
            cmd.Parameters.AddWithValue("@name", bat.name);
            cmd.Parameters.AddWithValue("@uuid", bat.uuid);
            cmd.Parameters.AddWithValue("@cap_ele", bat.capacidadElectrica);
            cmd.Parameters.AddWithValue("@carga_act", bat.cargaActual);
            cmd.Parameters.AddWithValue("@dsc", bat.descripcion);
            cmd.Parameters.AddWithValue("@tipo", bat.tipo);
            cmd.Parameters.AddWithValue("@notif", bat.notificacion);
            cmd.Parameters.AddWithValue("@notif_carga", bat.cargaNotificacion);
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
            sql = "UPDATE bat SET name=@name,uuid=@uuid,cap_ele=@cap_ele,carga_act=@carga_act,dsc=@dsc,tipo=@tipo,notif=@notif,notif_carga=@notif_carga WHERE red_ele=@red_ele AND id=@id";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", bat.id);
            cmd.Parameters.AddWithValue("@red_ele", bat.redElectrica);
            cmd.Parameters.AddWithValue("@name", bat.name);
            cmd.Parameters.AddWithValue("@uuid", bat.uuid);
            cmd.Parameters.AddWithValue("@cap_ele", bat.capacidadElectrica);
            cmd.Parameters.AddWithValue("@carga_act", bat.cargaActual);
            cmd.Parameters.AddWithValue("@dsc", bat.descripcion);
            cmd.Parameters.AddWithValue("@tipo", bat.tipo);
            cmd.Parameters.AddWithValue("@notif", bat.notificacion);
            cmd.Parameters.AddWithValue("@notif_carga", bat.cargaNotificacion);
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
            long cargaAnterior = rdr.GetInt64(0);
            if (cargaActual != cargaAnterior)
                redCargaActual = cargaActual - cargaAnterior;
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

            if (bat.notificacion && cargaAnterior > bat.cargaNotificacion && cargaActual <= bat.cargaNotificacion)
            {
                addNotificacion(new Notificacion("Batería baja", "La carga de la batería " + bat.id + "#" + bat.name + " es baja", NotificacionOrigen.BateriaLevel));
            }
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
            string sql = "SELECT red_ele, id, name, uuid, eu_t, act, tipo, dsc, notif FROM gen WHERE red_ele = @red_ele";
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
            string sql = "SELECT red_ele, id, name, uuid, eu_t, act, tipo, dsc, notif FROM gen WHERE red_ele = @red_ele AND id = @id ORDER BY id ASC";
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
            string sql = "SELECT red_ele, id, name, uuid, eu_t, act, tipo, dsc, notif FROM gen WHERE uuid = @uuid";
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
            string sql = "INSERT INTO gen (red_ele,name,uuid,eu_t,act,tipo,dsc,notif) VALUES (@red_ele,@name,@uuid,@eu_t,@act,@tipo,@dsc,@notif) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@red_ele", gen.redElectrica);
            cmd.Parameters.AddWithValue("@name", gen.name);
            cmd.Parameters.AddWithValue("@uuid", gen.uuid);
            cmd.Parameters.AddWithValue("@eu_t", gen.euTick);
            cmd.Parameters.AddWithValue("@act", gen.activado);
            cmd.Parameters.AddWithValue("@tipo", gen.tipo);
            cmd.Parameters.AddWithValue("@dsc", gen.descripcion);
            cmd.Parameters.AddWithValue("@notif", gen.notificacion);
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
            string sql = "UPDATE gen SET name=@name,uuid=@uuid,eu_t=@eu_t,act=@act,dsc=@dsc,tipo=@tipo,notif=@notif WHERE red_ele=@red_ele AND id=@id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", gen.id);
            cmd.Parameters.AddWithValue("@red_ele", gen.redElectrica);
            cmd.Parameters.AddWithValue("@name", gen.name);
            cmd.Parameters.AddWithValue("@uuid", gen.uuid);
            cmd.Parameters.AddWithValue("@eu_t", gen.euTick);
            cmd.Parameters.AddWithValue("@act", gen.activado);
            cmd.Parameters.AddWithValue("@dsc", gen.descripcion);
            cmd.Parameters.AddWithValue("@tipo", gen.tipo);
            cmd.Parameters.AddWithValue("@notif", gen.notificacion);
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
            Program.websocketPubSub.onPush("generador", serverHashes.SubscriptionChangeType.update, gen.redElectrica, JsonConvert.SerializeObject(gen));

            if (gen.notificacion)
            {
                addNotificacion(new Notificacion("Cambio de estado generador", "El generador " + gen.id + "#" + gen.name + " se ha " + (activado ? "activado" : "desactivado"), NotificacionOrigen.GeneradorEstado));
            }

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
            StringBuilder sql = new StringBuilder("SELECT id,name,size,rob,pos_x,pos_y,pos_z,pos_f,gps_x,gps_y,gps_z,num_items,date_add,date_upd,date_inicio,date_fin,dsc,estado,recarga_unidad,energia_recarga,modo_minado,shutdown,notif FROM public.ordenes_minado");
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
            string sql = "SELECT id,name,size,rob,pos_x,pos_y,pos_z,pos_f,gps_x,gps_y,gps_z,num_items,date_add,date_upd,date_inicio,date_fin,dsc,estado,recarga_unidad,energia_recarga,modo_minado,shutdown,notif FROM public.ordenes_minado WHERE id = @id";
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
            bool ordenAsociada = false;
            // ver si el robot tenia una orden de minado establecida
            string sql = "SELECT ord_min, id FROM robots WHERE off = false AND uuid = @uuid";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uuid", uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            int ordenMinadoId = 0;
            if (!rdr.IsDBNull(0))
            {
                ordenAsociada = true;
                ordenMinadoId = rdr.GetInt32(0);
            }
            short robotId = rdr.GetInt16(1);
            rdr.Close();

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
                sql = "UPDATE robots SET ord_min=@ord_min,estado='M' WHERE id=@id";
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

            OrdenMinado ordenMinado = getOrdenMinado(ordenMinadoId);
            Program.websocketPubSub.onPush("ordenMinado", serverHashes.SubscriptionChangeType.update, ordenMinadoId, JsonConvert.SerializeObject(ordenMinado));
            if (ordenMinado.notificacion)
            {
                addNotificacion(new Notificacion("Orden de minado finalizada", "La orden de minado " + ordenMinado.id + "#" + ordenMinado.name + " ha finalizado.", NotificacionOrigen.OrdenMinadoDone));
            }

        }

        public bool addOrdenesDeMinado(OrdenMinado orden)
        {
            string sql = "INSERT INTO public.ordenes_minado(name,size,rob,gps_x,gps_y,gps_z,dsc,recarga_unidad,energia_recarga,modo_minado,shutdown,notif) VALUES (@name,@size,@rob,@gps_x,@gps_y,@gps_z,@dsc,@recarga_unidad,@energia_recarga,@modo_minado,@shutdown,@notif) RETURNING id";
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
            cmd.Parameters.AddWithValue("@notif", orden.notificacion);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            int id = rdr.GetInt32(0);
            rdr.Close();

            Program.websocketPubSub.onPush("ordenMinado", serverHashes.SubscriptionChangeType.insert, 0, JsonConvert.SerializeObject(getOrdenMinado(id)));
            return true;
        }

        public bool addOrdenesDeMinado(List<OrdenMinado> ordenes)
        {
            NpgsqlTransaction trans = conn.BeginTransaction();

            foreach (OrdenMinado ordenMinado in ordenes)
            {
                if (!addOrdenesDeMinado(ordenMinado))
                {
                    trans.Rollback();
                    return false;
                }
            }

            trans.Commit();
            return true;
        }

        public bool updateOrdenesDeMinado(OrdenMinado orden)
        {
            string sql = "UPDATE public.ordenes_minado SET name=@name,size=@size,rob=@rob,gps_x=@gps_x,gps_y=@gps_y,gps_z=@gps_z,dsc=@dsc,recarga_unidad=@recarga_unidad,energia_recarga=@energia_recarga,modo_minado=@modo_minado,shutdown=@shutdown,notif=@notif,date_upd = CURRENT_TIMESTAMP(3) WHERE id=@id";
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
            cmd.Parameters.AddWithValue("@notif", orden.notificacion);
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
            string sql = "SELECT id,name,dsc,tipos,items,off,uuid,date_add,date_inv_upd,stacks,almacenamiento,max_stacks,max_tipos,max_items FROM almacenes ORDER BY id ASC";
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
            string sql = "INSERT INTO almacenes (name, dsc, off, uuid, almacenamiento, max_stacks, max_tipos, max_items) VALUES (@name, @dsc, @off, @uuid, @almacenamiento, @max_stacks, @max_tipos, @max_items) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", almacen.name);
            cmd.Parameters.AddWithValue("@dsc", almacen.descripcion);
            cmd.Parameters.AddWithValue("@off", almacen.off);
            cmd.Parameters.AddWithValue("@uuid", almacen.uuid);
            cmd.Parameters.AddWithValue("@almacenamiento", almacen.almacenamiento);
            cmd.Parameters.AddWithValue("@max_stacks", almacen.maximoStacks);
            cmd.Parameters.AddWithValue("@max_tipos", almacen.maximoTipos);
            cmd.Parameters.AddWithValue("@max_items", almacen.maximoItems);
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

            almacen.id = id;
            Program.websocketPubSub.onPush("almacen", serverHashes.SubscriptionChangeType.insert, id, JsonConvert.SerializeObject(almacen));
            return true;
        }

        public bool updateAlmacen(Almacen almacen)
        {
            string sql = "UPDATE almacenes SET name = @name,dsc = @dsc,off=@off,uuid=@uuid,almacenamiento=@almacenamiento,max_stacks=@max_stacks,max_tipos=@max_tipos,max_items=@max_items WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", almacen.id);
            cmd.Parameters.AddWithValue("@name", almacen.name);
            cmd.Parameters.AddWithValue("@dsc", almacen.descripcion);
            cmd.Parameters.AddWithValue("@off", almacen.off);
            cmd.Parameters.AddWithValue("@uuid", almacen.uuid);
            cmd.Parameters.AddWithValue("@almacenamiento", almacen.almacenamiento);
            cmd.Parameters.AddWithValue("@max_stacks", almacen.maximoStacks);
            cmd.Parameters.AddWithValue("@max_tipos", almacen.maximoTipos);
            cmd.Parameters.AddWithValue("@max_items", almacen.maximoItems);
            bool ok;
            try
            {
                ok = cmd.ExecuteNonQuery() == 0;
            }
            catch (Exception) { return false; }
            if (!ok)
            {
                return false;
            }
            Program.websocketPubSub.onPush("almacen", serverHashes.SubscriptionChangeType.update, almacen.id, JsonConvert.SerializeObject(almacen));
            return true;
        }

        public bool deleteAlmacen(short id)
        {
            NpgsqlTransaction trans = conn.BeginTransaction();
            string sql = "SELECT art, cant FROM alm_inventario WHERE alm = @alm";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            List<Tuple<short, int>> inventario = new List<Tuple<short, int>>();

            while (rdr.Read())
            {
                inventario.Add(new Tuple<short, int>(rdr.GetInt16(0), rdr.GetInt32(1)));
            }
            rdr.Close();

            foreach (Tuple<short, int> slot in inventario)
            {
                sql = "UPDATE articulos SET cant = cant - @cant WHERE id = @id";
                cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", slot.Item1);
                cmd.Parameters.AddWithValue("@cant", slot.Item2);
                if (cmd.ExecuteNonQuery() == 0)
                {
                    trans.Rollback();
                    return false;
                }
            }

            sql = "DELETE FROM almacenes WHERE id = @id";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            if (cmd.ExecuteNonQuery() == 0)
            {
                trans.Rollback();
                return false;
            }
            trans.Commit();

            Program.websocketPubSub.onPush("almacen", serverHashes.SubscriptionChangeType.delete, id, string.Empty);
            Program.websocketPubSub.removeTopic("almacenInv#" + id);
            return true;
        }

        // ALMACÉN - INVENTARIO

        public List<AlmacenInventario> getInventarioAlmacen(short idAlmacen)
        {
            List<AlmacenInventario> inventario = new List<AlmacenInventario>();
            List<AlmacenInventarioGet> inventarioGet = new List<AlmacenInventarioGet>();
            string sql = "SELECT alm,id,art,cant,cant_disp FROM alm_inventario WHERE alm = @alm ORDER BY id ASC";
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
                inventario.Add(new AlmacenInventario(invGet.almacen, invGet.id, getArticuloSlot(invGet.articulo), invGet.cantidad, invGet.cantidadDisponible));
            }

            return inventario;
        }

        public void setInventarioAlmacen(short idAlmacen, List<AlmacenInventarioSet> inventario)
        {
            if (idAlmacen <= 0)
                return;

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

            procesarNotificacionesAlmacen(idAlmacen, inventario);

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
                    sql = "SELECT cant FROM alm_inventario WHERE alm = @alm AND art = @art";
                    cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@alm", idAlmacen);
                    cmd.Parameters.AddWithValue("@art", articulo);
                    rdr = cmd.ExecuteReader();
                    rdr.Read();
                    int cantidadAnterior = rdr.GetInt32(0);
                    rdr.Close();
                    int cantidadDiferencia = slot.cantidad - cantidadAnterior;

                    // modificar el slot del inventario
                    sql = "UPDATE alm_inventario SET cant = @cant, cant_disp = cant_disp + @cant_disp WHERE alm = @alm AND id = @id";
                    cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@cant", slot.cantidad);
                    cmd.Parameters.AddWithValue("@cant_disp", cantidadDiferencia);
                    cmd.Parameters.AddWithValue("@id", almSlotId);
                    cmd.Parameters.AddWithValue("@alm", idAlmacen);
                    cmd.ExecuteNonQuery();

                    // actualizar la cantidad del artículo
                    if (cantidadDiferencia != 0)
                    {
                        // acumular la cantidad total de existencias del artículo
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
                else if (slot.cantidad > 0) // no existe un slot para este artículo
                {
                    rdr.Close();

                    // crear el slot para el artículo
                    sql = "INSERT INTO alm_inventario (alm,art,cant,cant_disp) VALUES (@alm,@art,@cant,@cant)";
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
                else
                {
                    rdr.Close();
                }
            }

            // actualizar el almacén con la fecha de actualización, y los items y slots utilitzados
            int items = 0;
            int stacks = 0;
            for (int i = 0; i < inventario.Count; i++)
            {
                items += inventario[i].cantidad;
                if (inventario[i].cantidad > 0)
                {
                    if (inventario[i].cantidad % 64 != 0)
                        stacks++;
                    stacks += inventario[i].cantidad / 64;
                }
            }

            sql = "UPDATE almacenes SET date_inv_upd = CURRENT_TIMESTAMP(3), tipos = @tipos, items = @items, stacks = @stacks WHERE id = @id";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idAlmacen);
            cmd.Parameters.AddWithValue("@tipos", inventario.Count);
            cmd.Parameters.AddWithValue("@items", items);
            cmd.Parameters.AddWithValue("@stacks", stacks);
            cmd.ExecuteNonQuery();

            trans.Commit();

            Program.websocketPubSub.addTopic("almacenInv#" + idAlmacen);
            Program.websocketPubSub.onPush("almacenInv#" + idAlmacen, serverHashes.SubscriptionChangeType.update, 0, JsonConvert.SerializeObject(getInventarioAlmacen(idAlmacen)));
        }

        public AlmacenInventarioGet getInventarioAlmacen(short idAlmacen, short idArticulo)
        {
            string sql = "SELECT alm,id,art,cant,cant_disp FROM alm_inventario WHERE alm = @alm AND art = @art ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", idAlmacen);
            cmd.Parameters.AddWithValue("@art", idArticulo);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            if (!rdr.HasRows)
            {
                rdr.Close();
                return new AlmacenInventarioGet(idAlmacen, idAlmacen, 0);
            }

            rdr.Read();
            AlmacenInventarioGet slot = new AlmacenInventarioGet(rdr);
            rdr.Close();

            return slot;
        }

        // NOTIFICACIONES DE ALMACÉN

        public List<AlmacenInventarioNotificacion> getNotificacionesAlmacen(short idAlmacen)
        {
            List<AlmacenInventarioNotificacion> notificaciones = new List<AlmacenInventarioNotificacion>();
            string sql = "SELECT alm,id,name,art,modo,cantidad FROM alm_inv_notificacion WHERE alm = @alm ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idAlmacen);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                notificaciones.Add(new AlmacenInventarioNotificacion(rdr));
            rdr.Close();

            return notificaciones;
        }

        public bool addNotificacionAlmacen(AlmacenInventarioNotificacion notificacion)
        {
            string sql = "INSERT INTO alm_inv_notificacion(alm, name, art, modo, cantidad) VALUES (@alm, @name, @art, @modo, @cantidad)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", notificacion.idAlmacen);
            cmd.Parameters.AddWithValue("@name", notificacion.name);
            cmd.Parameters.AddWithValue("@art", notificacion.idArticulo);
            cmd.Parameters.AddWithValue("@modo", notificacion.modo);
            cmd.Parameters.AddWithValue("@cantidad", notificacion.cantidad);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool deleteNotificacionAlmacen(short idAlmacen, short idNotificacion)
        {
            string sql = "DELETE FROM alm_inv_notificacion WHERE alm = @alm AND id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", idAlmacen);
            cmd.Parameters.AddWithValue("@id", idNotificacion);
            return cmd.ExecuteNonQuery() > 0;
        }

        public void procesarNotificacionesAlmacen(short idAlmacen, List<AlmacenInventarioSet> inventario)
        {
            // cargar notificaciones
            List<AlmacenInventarioNotificacion> notificaciones = getNotificacionesAlmacen(idAlmacen);
            for (int i = 0; i < notificaciones.Count; i++)
            {
                AlmacenInventarioNotificacion notificacion = notificaciones[i];

                // obtener el stock anterior del artículo, 0 si no se encuentra registro de existencias
                string sql = "SELECT cant FROM alm_inventario WHERE alm = @alm AND art = @art";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@alm", idAlmacen);
                cmd.Parameters.AddWithValue("@art", notificacion.idArticulo);
                NpgsqlDataReader rdr = cmd.ExecuteReader();
                int cantidadAnterior = 0;
                if (rdr.HasRows)
                {
                    rdr.Read();
                    cantidadAnterior = rdr.GetInt32(0);
                }
                rdr.Close();

                // obtener el nuevo stock del artículo, 0 si no se encuentra registro de existencias
                int nuevaCantidad = 0;
                foreach (AlmacenInventarioSet slot in inventario)
                {
                    if (slot.articuloId == notificacion.idArticulo)
                    {
                        nuevaCantidad = slot.cantidad;
                        break;
                    }
                }

                // comprobar si no se cumplía la condición y ahora se cumple
                bool condicionAnterior = false;
                switch (notificacion.modo)
                {
                    case '<':
                        {
                            condicionAnterior = cantidadAnterior < notificacion.cantidad;
                            break;
                        }
                    case '>':
                        {
                            condicionAnterior = cantidadAnterior > notificacion.cantidad;
                            break;
                        }
                    case '=':
                        {
                            condicionAnterior = cantidadAnterior > notificacion.cantidad;
                            break;
                        }
                    case '-':
                        {
                            condicionAnterior = cantidadAnterior <= notificacion.cantidad;
                            break;
                        }
                    case '+':
                        {
                            condicionAnterior = cantidadAnterior >= notificacion.cantidad;
                            break;
                        }
                }
                bool condicionPosterior = false;
                switch (notificacion.modo)
                {
                    case '<':
                        {
                            condicionPosterior = nuevaCantidad < notificacion.cantidad;
                            break;
                        }
                    case '>':
                        {
                            condicionPosterior = nuevaCantidad > notificacion.cantidad;
                            break;
                        }
                    case '=':
                        {
                            condicionPosterior = nuevaCantidad > notificacion.cantidad;
                            break;
                        }
                    case '-':
                        {
                            condicionPosterior = nuevaCantidad <= notificacion.cantidad;
                            break;
                        }
                    case '+':
                        {
                            condicionPosterior = nuevaCantidad >= notificacion.cantidad;
                            break;
                        }
                }

                if ((!condicionAnterior) && condicionPosterior)
                {
                    addNotificacion(new Notificacion("Inventario de almacén", notificacion.name, NotificacionOrigen.AlmacenInventario));
                }
            }
        }

        // ALMACÉN - AE2 STORAGE CELLS

        public List<AE2StorageCell> getStrorageCells(short idAlmacen)
        {
            List<AE2StorageCell> storageCells = new List<AE2StorageCell>();
            string sql = "SELECT alm,id,tier,date_add FROM alm_ae2_storage_cells WHERE alm = @alm";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", idAlmacen);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                storageCells.Add(new AE2StorageCell(rdr));
            rdr.Close();

            return storageCells;
        }

        public bool addStorageCell(AE2StorageCell storageCell)
        {
            NpgsqlTransaction trans = conn.BeginTransaction();
            string sql = "INSERT INTO alm_ae2_storage_cells (alm,tier) VALUES (@alm,@tier)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", storageCell.idAlmacen);
            cmd.Parameters.AddWithValue("@tier", storageCell.tier);
            if (cmd.ExecuteNonQuery() == 0)
            {
                trans.Rollback();
                return false;
            }

            sql = "UPDATE almacenes SET max_tipos=max_tipos+63,max_items=max_items+@max_items WHERE id=@alm";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", storageCell.idAlmacen);
            int maxItems;
            switch (storageCell.tier)
            {
                case 1:
                    {
                        maxItems = 8192;
                        break;
                    }
                case 2:
                    {
                        maxItems = 32768;
                        break;
                    }
                case 3:
                    {
                        maxItems = 131072;
                        break;
                    }
                case 4:
                    {
                        maxItems = 524288;
                        break;
                    }
                default:
                    {
                        trans.Rollback();
                        return false;
                    }
            }
            cmd.Parameters.AddWithValue("@max_items", maxItems);
            if (cmd.ExecuteNonQuery() == 0)
            {
                trans.Rollback();
                return false;
            }

            trans.Commit();
            return true;
        }

        public bool deleteStorageCell(AE2StorageCellDelete storageCell)
        {
            string sql = "SELECT tier FROM alm_ae2_storage_cells WHERE alm=@alm AND id=@id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", storageCell.idAlmacen);
            cmd.Parameters.AddWithValue("@id", storageCell.id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return false;
            }
            rdr.Read();
            short tier = rdr.GetInt16(0);
            rdr.Close();

            NpgsqlTransaction trans = conn.BeginTransaction();
            sql = "DELETE FROM alm_ae2_storage_cells WHERE alm=@alm AND id=@id";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", storageCell.idAlmacen);
            cmd.Parameters.AddWithValue("@id", storageCell.id);
            if (cmd.ExecuteNonQuery() == 0)
            {
                trans.Rollback();
                return false;
            }

            sql = "SELECT max_tipos,max_items FROM almacenes WHERE id=@alm";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", storageCell.idAlmacen);
            rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                trans.Rollback();
                return false;
            }
            rdr.Read();
            short maxTipos = rdr.GetInt16(0);
            int maxItems = rdr.GetInt32(1);
            rdr.Close();

            maxTipos -= 63;
            switch (tier)
            {
                case 1:
                    {
                        maxItems -= 8192;
                        break;
                    }
                case 2:
                    {
                        maxItems -= 32768;
                        break;
                    }
                case 3:
                    {
                        maxItems -= 131072;
                        break;
                    }
                case 4:
                    {
                        maxItems -= 524288;
                        break;
                    }
                default:
                    {
                        trans.Rollback();
                        return false;
                    }
            }
            if (maxTipos < 0)
                maxTipos = 0;
            if (maxItems < 0)
                maxItems = 0;

            sql = "UPDATE almacenes SET max_tipos=@max_tipos,max_items=@max_items WHERE id=@id";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", storageCell.idAlmacen);
            cmd.Parameters.AddWithValue("@max_tipos", maxTipos);
            cmd.Parameters.AddWithValue("@max_items", maxItems);
            if (cmd.ExecuteNonQuery() == 0)
            {
                trans.Rollback();
                return false;
            }

            trans.Commit();
            return true;
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
            if (query.dateInicio > DateTime.MinValue)
            {
                if (!primerParametro)
                    primerParametro = true;
                else
                    sql.Append(" AND");
                sql.Append(" date_add >= @dateInicio");
            }
            if (query.dateFin > DateTime.MinValue)
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
            string sql = "SELECT id, name, uuid, tier, num_slots, num_stacks, num_items, estado, total_energia, energia_actual, upgrade_gen, items_gen, fecha_con, fecha_descon, dsc, upgrade_gps, pos_x, pos_y, pos_z, complejidad, date_add, date_upd, off, off_pos_x, off_pos_y, off_pos_z, notif_con, notif_descon, notif_bat_baj FROM drones";
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
            string sql = "SELECT id, name, uuid, tier, num_slots, num_stacks, num_items, estado, total_energia, energia_actual, upgrade_gen, items_gen, fecha_con, fecha_descon, dsc, upgrade_gps, pos_x, pos_y, pos_z, complejidad, date_add, date_upd, off, off_pos_x, off_pos_y, off_pos_z, notif_con, notif_descon, notif_bat_baj FROM drones WHERE (uuid::text ILIKE @text OR name ILIKE @text) ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("text", "%" + text + "%");
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                drones.Add(new Drone(rdr));
            rdr.Close();
            return drones;
        }

        public Drone getDrone(short id)
        {
            string sql = "SELECT id, name, uuid, tier, num_slots, num_stacks, num_items, estado, total_energia, energia_actual, upgrade_gen, items_gen, fecha_con, fecha_descon, dsc, upgrade_gps, pos_x, pos_y, pos_z, complejidad, date_add, date_upd, off, off_pos_x, off_pos_y, off_pos_z, notif_con, notif_descon, notif_bat_baj FROM drones WHERE id = @id";
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
            string sql = "UPDATE drones SET name=@name, uuid=@uuid, tier=@tier, num_slots=@num_slots, total_energia=@total_energia, energia_actual=@energia_actual, upgrade_gen=@upgrade_gen, items_gen=@items_gen, dsc=@dsc, upgrade_gps=@upgrade_gps, pos_x=@pos_x, pos_y=@pos_y, pos_z=@pos_z, complejidad=@complejidad, off = @off, off_pos_x = @off_pos_x, off_pos_y = @off_pos_y, off_pos_z = @off_pos_z, notif_con = @notif_con, notif_descon = @notif_descon, notif_bat_baj = @notif_bat_baj, date_upd = CURRENT_TIMESTAMP(3) WHERE id = @id";
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
            cmd.Parameters.AddWithValue("notif_con", d.notificacionConexion);
            cmd.Parameters.AddWithValue("notif_descon", d.notificacionDesconexion);
            cmd.Parameters.AddWithValue("notif_bat_baj", d.notificacionBateriaBaja);
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

        /* NOTIFICACIONES */

        public int countNotificaciones()
        {
            string sql = "SELECT COUNT(*) FROM notificaciones WHERE leido = false";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            int count = rdr.GetInt32(0);
            rdr.Close();
            return count;
        }

        public List<Notificacion> getNotificaciones()
        {
            List<Notificacion> notificaciones = new List<Notificacion>();
            string sql = "SELECT id, name, dsc, leido, origen FROM notificaciones ORDER BY id DESC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                notificaciones.Add(new Notificacion(rdr));
            rdr.Close();

            return notificaciones;
        }

        public bool addNotificacion(Notificacion n)
        {
            string sql = "INSERT INTO notificaciones (name,dsc,leido,origen) VALUES (@name,@dsc,@leido,@origen) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", n.name);
            cmd.Parameters.AddWithValue("@dsc", n.descripcion);
            cmd.Parameters.AddWithValue("@leido", n.leido);
            cmd.Parameters.AddWithValue("@origen", (short)n.origen);
            NpgsqlDataReader rdr;
            try
            {
                rdr = cmd.ExecuteReader();
            }
            catch (Exception) { return false; }
            rdr.Read();
            DateTime id = rdr.GetDateTime(0);
            rdr.Close();

            n.id = id;
            Program.websocketPubSub.onPush("notificaciones", serverHashes.SubscriptionChangeType.insert, 0, JsonConvert.SerializeObject(n));
            return true;
        }

        public bool notificacionesLeidas()
        {
            string sql = "UPDATE notificaciones SET leido=true WHERE leido=false";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            if (cmd.ExecuteNonQuery() > 0)
            {
                Program.websocketPubSub.onPush("notificaciones", serverHashes.SubscriptionChangeType.update, 0, string.Empty);
                return true;
            }
            return false;
        }

        /* CRAFTEOS */

        public List<Crafteo> getCrafteos()
        {
            List<Crafteo> crafteos = new List<Crafteo>();
            string sql = "SELECT id, name, art_resultado, cant_resultado, art_slot1, cant_slot1, art_slot2, cant_slot2, art_slot3, cant_slot3, art_slot4, cant_slot4, art_slot5, cant_slot5, art_slot6, cant_slot6, art_slot7, cant_slot7, art_slot8, cant_slot8, art_slot9, cant_slot9, date_add, date_upd, off FROM crafting";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                crafteos.Add(new Crafteo(rdr));
            rdr.Close();

            return crafteos;
        }

        public Crafteo getCrafteo(int id)
        {
            string sql = "SELECT id, name, art_resultado, cant_resultado, art_slot1, cant_slot1, art_slot2, cant_slot2, art_slot3, cant_slot3, art_slot4, cant_slot4, art_slot5, cant_slot5, art_slot6, cant_slot6, art_slot7, cant_slot7, art_slot8, cant_slot8, art_slot9, cant_slot9, date_add, date_upd, off FROM crafting WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }

            rdr.Read();
            Crafteo crafteo = new Crafteo(rdr);
            rdr.Close();

            return crafteo;
        }

        public bool addCrafteo(Crafteo crafteo)
        {
            string sql = "INSERT INTO crafting(name, art_resultado, cant_resultado, art_slot1, cant_slot1, art_slot2, cant_slot2, art_slot3, cant_slot3, art_slot4, cant_slot4, art_slot5, cant_slot5, art_slot6, cant_slot6, art_slot7, cant_slot7, art_slot8, cant_slot8, art_slot9, cant_slot9) VALUES (@name, @art_resultado, @cant_resultado, @art_slot1, @cant_slot1, @art_slot2, @cant_slot2, @art_slot3, @cant_slot3, @art_slot4, @cant_slot4, @art_slot5, @cant_slot5, @art_slot6, @cant_slot6, @art_slot7, @cant_slot7, @art_slot8, @cant_slot8, @art_slot9, @cant_slot9)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", crafteo.name);
            cmd.Parameters.AddWithValue("@art_resultado", crafteo.idArticuloResultado);
            cmd.Parameters.AddWithValue("@cant_resultado", crafteo.cantidadResultado);
            if (crafteo.idArticuloSlot1 != null)
                cmd.Parameters.AddWithValue("@art_slot1", crafteo.idArticuloSlot1);
            else
                cmd.Parameters.AddWithValue("@art_slot1", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot1", crafteo.cantidadArticuloSlot1);
            if (crafteo.idArticuloSlot2 != null)
                cmd.Parameters.AddWithValue("@art_slot2", crafteo.idArticuloSlot2);
            else
                cmd.Parameters.AddWithValue("@art_slot2", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot2", crafteo.cantidadArticuloSlot2);
            if (crafteo.idArticuloSlot3 != null)
                cmd.Parameters.AddWithValue("@art_slot3", crafteo.idArticuloSlot3);
            else
                cmd.Parameters.AddWithValue("@art_slot3", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot3", crafteo.cantidadArticuloSlot3);
            if (crafteo.idArticuloSlot4 != null)
                cmd.Parameters.AddWithValue("@art_slot4", crafteo.idArticuloSlot4);
            else
                cmd.Parameters.AddWithValue("@art_slot4", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot4", crafteo.cantidadArticuloSlot4);
            if (crafteo.idArticuloSlot5 != null)
                cmd.Parameters.AddWithValue("@art_slot5", crafteo.idArticuloSlot5);
            else
                cmd.Parameters.AddWithValue("@art_slot5", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot5", crafteo.cantidadArticuloSlot5);
            if (crafteo.idArticuloSlot6 != null)
                cmd.Parameters.AddWithValue("@art_slot6", crafteo.idArticuloSlot6);
            else
                cmd.Parameters.AddWithValue("@art_slot6", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot6", crafteo.cantidadArticuloSlot6);
            if (crafteo.idArticuloSlot7 != null)
                cmd.Parameters.AddWithValue("@art_slot7", crafteo.idArticuloSlot7);
            else
                cmd.Parameters.AddWithValue("@art_slot7", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot7", crafteo.cantidadArticuloSlot7);
            if (crafteo.idArticuloSlot8 != null)
                cmd.Parameters.AddWithValue("@art_slot8", crafteo.idArticuloSlot8);
            else
                cmd.Parameters.AddWithValue("@art_slot8", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot8", crafteo.cantidadArticuloSlot8);
            if (crafteo.idArticuloSlot9 != null)
                cmd.Parameters.AddWithValue("@art_slot9", crafteo.idArticuloSlot9);
            else
                cmd.Parameters.AddWithValue("@art_slot9", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot9", crafteo.cantidadArticuloSlot9);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool updateCrafteo(Crafteo crafteo)
        {
            string sql = "UPDATE crafting SET name=@name, art_resultado=@art_resultado, cant_resultado=@cant_resultado, art_slot1=@art_slot1, cant_slot1=@cant_slot1, art_slot2=@art_slot2, cant_slot2=@cant_slot2, art_slot3=@art_slot3, cant_slot3=@cant_slot3, art_slot4=@art_slot4, cant_slot4=@cant_slot4, art_slot5=@art_slot5, cant_slot5=@cant_slot5, art_slot6=@art_slot6, cant_slot6=@cant_slot6, art_slot7=@art_slot7, cant_slot7=@cant_slot7, art_slot8=@art_slot8, cant_slot8=@cant_slot8, art_slot9=@art_slot9, cant_slot9=@cant_slot9, off=@off, date_upd=CURRENT_TIMESTAMP(3) WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", crafteo.id);
            cmd.Parameters.AddWithValue("@name", crafteo.name);
            cmd.Parameters.AddWithValue("@art_resultado", crafteo.idArticuloResultado);
            cmd.Parameters.AddWithValue("@cant_resultado", crafteo.cantidadResultado);
            if (crafteo.idArticuloSlot1 != null)
                cmd.Parameters.AddWithValue("@art_slot1", crafteo.idArticuloSlot1);
            else
                cmd.Parameters.AddWithValue("@art_slot1", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot1", crafteo.cantidadArticuloSlot2);
            if (crafteo.idArticuloSlot2 != null)
                cmd.Parameters.AddWithValue("@art_slot2", crafteo.idArticuloSlot2);
            else
                cmd.Parameters.AddWithValue("@art_slot2", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot2", crafteo.cantidadArticuloSlot2);
            if (crafteo.idArticuloSlot3 != null)
                cmd.Parameters.AddWithValue("@art_slot3", crafteo.idArticuloSlot3);
            else
                cmd.Parameters.AddWithValue("@art_slot3", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot3", crafteo.cantidadArticuloSlot3);
            if (crafteo.idArticuloSlot4 != null)
                cmd.Parameters.AddWithValue("@art_slot4", crafteo.idArticuloSlot4);
            else
                cmd.Parameters.AddWithValue("@art_slot4", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot4", crafteo.cantidadArticuloSlot4);
            if (crafteo.idArticuloSlot5 != null)
                cmd.Parameters.AddWithValue("@art_slot5", crafteo.idArticuloSlot5);
            else
                cmd.Parameters.AddWithValue("@art_slot5", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot5", crafteo.cantidadArticuloSlot5);
            if (crafteo.idArticuloSlot6 != null)
                cmd.Parameters.AddWithValue("@art_slot6", crafteo.idArticuloSlot6);
            else
                cmd.Parameters.AddWithValue("@art_slot6", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot6", crafteo.cantidadArticuloSlot6);
            if (crafteo.idArticuloSlot7 != null)
                cmd.Parameters.AddWithValue("@art_slot7", crafteo.idArticuloSlot7);
            else
                cmd.Parameters.AddWithValue("@art_slot7", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot7", crafteo.cantidadArticuloSlot7);
            if (crafteo.idArticuloSlot8 != null)
                cmd.Parameters.AddWithValue("@art_slot8", crafteo.idArticuloSlot8);
            else
                cmd.Parameters.AddWithValue("@art_slot8", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot8", crafteo.cantidadArticuloSlot8);
            if (crafteo.idArticuloSlot9 != null)
                cmd.Parameters.AddWithValue("@art_slot9", crafteo.idArticuloSlot9);
            else
                cmd.Parameters.AddWithValue("@art_slot9", DBNull.Value);
            cmd.Parameters.AddWithValue("@cant_slot9", crafteo.cantidadArticuloSlot9);
            cmd.Parameters.AddWithValue("@off", crafteo.off);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool deleteCrafteo(int id)
        {
            string sql = "DELETE FROM crafting WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public List<CrafteoHead> getCrafteosHead()
        {
            List<CrafteoHead> crafteos = new List<CrafteoHead>();
            string sql = "SELECT id,name,(SELECT name FROM articulos WHERE id = crafting.art_resultado) FROM crafting WHERE off = false";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                crafteos.Add(new CrafteoHead(rdr));
            rdr.Close();

            return crafteos;
        }

        // HORNO

        public List<Smelting> getSmelting()
        {
            List<Smelting> smeltings = new List<Smelting>();
            string sql = "SELECT id, name, art_resultado, cant_resultado, art_entrada, cant_entrada, date_add, date_upd, off FROM smelting ORDER BY id ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                smeltings.Add(new Smelting(rdr));
            rdr.Close();

            return smeltings;
        }

        public Smelting getSmelting(int id)
        {
            string sql = "SELECT id, name, art_resultado, cant_resultado, art_entrada, cant_entrada, date_add, date_upd, off FROM smelting WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }

            rdr.Read();
            Smelting smelting = new Smelting(rdr);
            rdr.Close();

            return smelting;
        }

        public bool addSmelting(Smelting smelting)
        {
            string sql = "INSERT INTO smelting(name, art_resultado, cant_resultado, art_entrada, cant_entrada) VALUES (@name, @art_resultado, @cant_resultado, @art_entrada, @cant_entrada)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", smelting.name);
            cmd.Parameters.AddWithValue("@art_resultado", smelting.idArticuloResultado);
            cmd.Parameters.AddWithValue("@cant_resultado", smelting.cantidadResultado);
            cmd.Parameters.AddWithValue("@art_entrada", smelting.idArticuloEntrada);
            cmd.Parameters.AddWithValue("@cant_entrada", smelting.cantidadEntrada);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool updateSmelting(Smelting smelting)
        {
            string sql = "UPDATE public.smelting SET name=@name, art_resultado=@art_resultado, cant_resultado=@cant_resultado, art_entrada=@art_entrada, cant_entrada=@cant_entrada, date_upd=CURRENT_TIMESTAMP(3), off=@off WHERE id=@id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", smelting.id);
            cmd.Parameters.AddWithValue("@name", smelting.name);
            cmd.Parameters.AddWithValue("@art_resultado", smelting.idArticuloResultado);
            cmd.Parameters.AddWithValue("@cant_resultado", smelting.cantidadResultado);
            cmd.Parameters.AddWithValue("@art_entrada", smelting.idArticuloEntrada);
            cmd.Parameters.AddWithValue("@cant_entrada", smelting.cantidadEntrada);
            cmd.Parameters.AddWithValue("@off", smelting.off);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool deleteSmelting(int id)
        {
            string sql = "DELETE FROM smelting WHERE id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public List<SmeltingHead> getSmeltingHead()
        {
            List<SmeltingHead> smelting = new List<SmeltingHead>();
            string sql = "SELECT id,name,(SELECT name FROM articulos WHERE id = smelting.art_resultado) FROM smelting WHERE off = false";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                smelting.Add(new SmeltingHead(rdr));
            rdr.Close();

            return smelting;
        }

        /* FABRICACIÓN */

        public List<FabricacionHead> getFabricacionesHead(short idAlmacen)
        {
            List<FabricacionHead> fabricaciones = new List<FabricacionHead>();
            string sql = "SELECT id, name FROM alm_fabricacion WHERE alm = @alm AND off = false ORDER BY id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", idAlmacen);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                fabricaciones.Add(new FabricacionHead(rdr));
            rdr.Close();

            return fabricaciones;
        }

        public List<Fabricacion> getFabricaciones(short idAlmacen)
        {
            List<Fabricacion> fabricaciones = new List<Fabricacion>();
            string sql = "SELECT id, alm, name, tipo, date_add, uuid, off, chest_side, furnace_side FROM alm_fabricacion WHERE alm = @alm ORDER BY id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", idAlmacen);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                fabricaciones.Add(new Fabricacion(rdr));
            rdr.Close();

            return fabricaciones;
        }

        public Fabricacion getFabricacion(Guid uuid)
        {
            string sql = "SELECT id, alm, name, tipo, date_add, uuid, off, chest_side, furnace_side FROM alm_fabricacion WHERE uuid = @uuid AND off = false";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uuid", uuid);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }

            rdr.Read();
            Fabricacion fabricacion = new Fabricacion(rdr);
            rdr.Close();

            return fabricacion;
        }

        public short addFabricacion(Fabricacion fabricacion)
        {
            string sql = "INSERT INTO alm_fabricacion(alm, name, tipo, uuid, off, chest_side, furnace_side) VALUES (@alm, @name, @tipo, @uuid, @off, @chest_side, @furnace_side) RETURNING id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", fabricacion.idAlmacen);
            cmd.Parameters.AddWithValue("@name", fabricacion.name);
            cmd.Parameters.AddWithValue("@tipo", fabricacion.tipo);
            cmd.Parameters.AddWithValue("@uuid", fabricacion.uuid);
            cmd.Parameters.AddWithValue("@off", fabricacion.off);
            cmd.Parameters.AddWithValue("@chest_side", fabricacion.cofreSide);
            cmd.Parameters.AddWithValue("@furnace_side", fabricacion.hornoSide);
            NpgsqlDataReader rdr;
            try
            {
                rdr = cmd.ExecuteReader();
            }
            catch (Exception)
            {
                return 0;
            }
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

        public bool updateFabricacion(Fabricacion fabricacion)
        {
            string sql = "UPDATE alm_fabricacion SET name=@name,tipo=@tipo,uuid=@uuid,off=@off,chest_side=@chest_side,furnace_side=@furnace_side WHERE alm = @alm AND id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", fabricacion.idAlmacen);
            cmd.Parameters.AddWithValue("@id", fabricacion.id);
            cmd.Parameters.AddWithValue("@name", fabricacion.name);
            cmd.Parameters.AddWithValue("@tipo", fabricacion.tipo);
            cmd.Parameters.AddWithValue("@uuid", fabricacion.uuid);
            cmd.Parameters.AddWithValue("@off", fabricacion.off);
            cmd.Parameters.AddWithValue("@chest_side", fabricacion.cofreSide);
            cmd.Parameters.AddWithValue("@furnace_side", fabricacion.hornoSide);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool deleteFabricacion(FabricacionDelete fabricacion)
        {
            string sql = "DELETE FROM alm_fabricacion WHERE alm = @alm AND id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", fabricacion.idAlmacen);
            cmd.Parameters.AddWithValue("@id", fabricacion.id);
            return cmd.ExecuteNonQuery() > 0;
        }

        /* ÓRDENES DE FABRICACIÓN */

        public List<OrdenFabricacion> searchOrdenesFabricacion(OrdenFabricacionSearch query)
        {
            List<OrdenFabricacion> ordenesFabricacion = new List<OrdenFabricacion>();
            StringBuilder sql = new StringBuilder("SELECT id, alm, name, fab, craft, smelt, date_add, date_fin, estado, cant, resultado, error_code FROM alm_ordenes_fab WHERE alm = @alm AND fab = @fab");
            if (query.tipoReceta != 'T')
            {
                if (query.tipoReceta == 'C' && query.idReceta == 0)
                    sql.Append(" AND craft IS NOT null");
                else if (query.tipoReceta == 'C' && query.idReceta > 0)
                    sql.Append(" AND craft = @craft");
                else if (query.tipoReceta == 'S' && query.idReceta == 0)
                    sql.Append(" AND smelt IS NOT null");
                else if (query.tipoReceta == 'S' && query.idReceta > 0)
                    sql.Append("AND smelt = @smelt");
            }
            if (query.inicio != DateTime.MinValue)
                sql.Append(" AND date_add > @inicio");
            if (query.fin != DateTime.MinValue)
                sql.Append(" AND date_add < @fin");
            if (query.estado != 4)
            {
                switch (query.estado)
                {
                    case 0:
                        {
                            sql.Append(" AND (estado = 'Q' OR estado = 'R')");
                            break;
                        }
                    case 1:
                        {
                            sql.Append(" AND estado = 'Q'");
                            break;
                        }
                    case 2:
                        {
                            sql.Append(" AND estado = 'R'");
                            break;
                        }
                    case 3:
                        {
                            sql.Append(" AND estado = 'D'");
                            break;
                        }
                }
            }
            sql.Append(" ORDER BY date_add ASC");
            NpgsqlCommand cmd = new NpgsqlCommand(sql.ToString(), conn);
            cmd.Parameters.AddWithValue("@alm", query.idAlmacen);
            cmd.Parameters.AddWithValue("@fab", query.idFabricacion);
            if (query.tipoReceta == 'C' && query.idReceta > 0)
                cmd.Parameters.AddWithValue("@craft", query.idReceta);
            else if (query.tipoReceta == 'S' && query.idReceta > 0)
                cmd.Parameters.AddWithValue("@smelt", query.idReceta);
            if (query.inicio != DateTime.MinValue)
                cmd.Parameters.AddWithValue("@inicio", query.inicio);
            if (query.fin != DateTime.MinValue)
                cmd.Parameters.AddWithValue("@fin", query.fin);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                ordenesFabricacion.Add(new OrdenFabricacion(rdr));
            rdr.Close();

            return ordenesFabricacion;
        }

        public List<OrdenFabricacion> getOrdenesFabricacion(short idAlmacen, short idFabricacion)
        {
            List<OrdenFabricacion> ordenesFabricacion = new List<OrdenFabricacion>();
            string sql = "SELECT id, alm, name, fab, craft, smelt, date_add, date_fin, estado, cant, resultado, error_code FROM alm_ordenes_fab WHERE alm = @alm AND fab = @fab AND (estado = 'Q' OR estado = 'R') ORDER BY date_add ASC";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", idAlmacen);
            cmd.Parameters.AddWithValue("@fab", idFabricacion);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
                ordenesFabricacion.Add(new OrdenFabricacion(rdr));
            rdr.Close();

            return ordenesFabricacion;
        }

        public OrdenFabricacion getNextOrdenFabricacion(short idAlmacen, short idFabricacion)
        {
            string sql = "SELECT id, alm, name, fab, craft, smelt, date_add, date_fin, estado, cant, resultado, error_code FROM alm_ordenes_fab WHERE alm = @alm AND fab = @fab AND estado = 'R' ORDER BY date_add ASC LIMIT 1";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", idAlmacen);
            cmd.Parameters.AddWithValue("@fab", idFabricacion);
            NpgsqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
            }
            else
            {
                rdr.Read();
                OrdenFabricacion ordenFabricacion = new OrdenFabricacion(rdr);
                rdr.Close();
                return ordenFabricacion;
            }

            sql = "SELECT id, alm, name, fab, craft, smelt, date_add, date_fin, estado, cant, resultado, error_code FROM alm_ordenes_fab WHERE alm = @alm AND fab = @fab AND estado = 'Q' ORDER BY date_add ASC LIMIT 1";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", idAlmacen);
            cmd.Parameters.AddWithValue("@fab", idFabricacion);
            rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return null;
            }
            else
            {
                rdr.Read();
                OrdenFabricacion ordenFabricacion = new OrdenFabricacion(rdr);
                rdr.Close();
                return ordenFabricacion;
            }
        }

        public bool setOrdenFabricacionReady(int id, short idAlmacen, short idFabricacion)
        {
            string sql = "UPDATE alm_ordenes_fab SET estado = 'R' WHERE alm = @alm AND fab = @fab AND id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", idAlmacen);
            cmd.Parameters.AddWithValue("@fab", idFabricacion);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool addOrdenFabricacion(OrdenFabricacion ordenFabricacion)
        {
            OrdenFabricacionPreview preview = ordenFabricacion.idCrafteo != null ?
                previewCrafteo(ordenFabricacion.idAlmacen, (int)ordenFabricacion.idCrafteo) : previewSmelting(ordenFabricacion.idAlmacen, (int)ordenFabricacion.idSmelting);

            if (preview == null)
                return false;
            if (ordenFabricacion.cantidad < preview.cantidadCrafteo || ordenFabricacion.cantidad > preview.maxCantidadCrafteo || (ordenFabricacion.cantidad % preview.cantidadCrafteo) != 0)
                return false;

            NpgsqlTransaction trans = conn.BeginTransaction();

            string sql = "INSERT INTO alm_ordenes_fab(alm, name, fab, craft, smelt, cant) VALUES (@alm, @name, @fab, @craft, @smelt, @cant)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", ordenFabricacion.idAlmacen);
            cmd.Parameters.AddWithValue("@name", ordenFabricacion.name);
            cmd.Parameters.AddWithValue("@fab", ordenFabricacion.idFabricacion);
            if (ordenFabricacion.idCrafteo == null)
                cmd.Parameters.AddWithValue("@craft", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@craft", ordenFabricacion.idCrafteo);
            if (ordenFabricacion.idSmelting == null)
                cmd.Parameters.AddWithValue("@smelt", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@smelt", ordenFabricacion.idSmelting);
            cmd.Parameters.AddWithValue("@cant", ordenFabricacion.cantidad);

            if (cmd.ExecuteNonQuery() == 0)
                return false;

            bool ok;
            if (ordenFabricacion.idCrafteo != null)
                ok = this.manageStockAvailableCrafteo(ordenFabricacion.idAlmacen, (int)(ordenFabricacion.idCrafteo), ordenFabricacion.cantidad, true);
            else if (ordenFabricacion.idSmelting != null)
                ok = this.manageStockAvailableSmelting(ordenFabricacion.idAlmacen, (int)(ordenFabricacion.idSmelting), ordenFabricacion.cantidad, true);
            else
                ok = false;

            if (ok)
            {
                trans.Commit();
                return true;
            }
            else
            {
                trans.Rollback();
                return false;
            }
        }

        private bool manageStockAvailableCrafteo(short idAlmacen, int idCrafteo, int cantidad, bool restarStock)
        {
            Crafteo crafteo = getCrafteo(idCrafteo);
            string sql;
            if (restarStock)
                sql = "UPDATE alm_inventario SET cant_disp = cant_disp - @cant WHERE alm = @alm AND art = @id";
            else
                sql = "UPDATE alm_inventario SET cant_disp = cant_disp + @cant WHERE alm = @alm AND art = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", idAlmacen);

            if (crafteo.idArticuloSlot1 != null)
            {
                cmd.Parameters.AddWithValue("@id", crafteo.idArticuloSlot1);
                cmd.Parameters.AddWithValue("@cant", crafteo.cantidadArticuloSlot1 * cantidad);
                if (cmd.ExecuteNonQuery() == 0)
                    return false;
            }

            if (crafteo.idArticuloSlot2 != null)
            {
                cmd.Parameters.AddWithValue("@id", crafteo.idArticuloSlot2);
                cmd.Parameters.AddWithValue("@cant", crafteo.cantidadArticuloSlot2 * cantidad);
                if (cmd.ExecuteNonQuery() == 0)
                    return false;
            }

            if (crafteo.idArticuloSlot3 != null)
            {
                cmd.Parameters.AddWithValue("@id", crafteo.idArticuloSlot3);
                cmd.Parameters.AddWithValue("@cant", crafteo.cantidadArticuloSlot3 * cantidad);
                if (cmd.ExecuteNonQuery() == 0)
                    return false;
            }

            if (crafteo.idArticuloSlot4 != null)
            {
                cmd.Parameters.AddWithValue("@id", crafteo.idArticuloSlot4);
                cmd.Parameters.AddWithValue("@cant", crafteo.cantidadArticuloSlot4 * cantidad);
                if (cmd.ExecuteNonQuery() == 0)
                    return false;
            }

            if (crafteo.idArticuloSlot5 != null)
            {
                cmd.Parameters.AddWithValue("@id", crafteo.idArticuloSlot5);
                cmd.Parameters.AddWithValue("@cant", crafteo.cantidadArticuloSlot5 * cantidad);
                if (cmd.ExecuteNonQuery() == 0)
                    return false;
            }

            if (crafteo.idArticuloSlot6 != null)
            {
                cmd.Parameters.AddWithValue("@id", crafteo.idArticuloSlot6);
                cmd.Parameters.AddWithValue("@cant", crafteo.cantidadArticuloSlot6 * cantidad);
                if (cmd.ExecuteNonQuery() == 0)
                    return false;
            }

            if (crafteo.idArticuloSlot7 != null)
            {
                cmd.Parameters.AddWithValue("@id", crafteo.idArticuloSlot7);
                cmd.Parameters.AddWithValue("@cant", crafteo.cantidadArticuloSlot7 * cantidad);
                if (cmd.ExecuteNonQuery() == 0)
                    return false;
            }

            if (crafteo.idArticuloSlot8 != null)
            {
                cmd.Parameters.AddWithValue("@id", crafteo.idArticuloSlot8);
                cmd.Parameters.AddWithValue("@cant", crafteo.cantidadArticuloSlot8 * cantidad);
                if (cmd.ExecuteNonQuery() == 0)
                    return false;
            }

            if (crafteo.idArticuloSlot9 != null)
            {
                cmd.Parameters.AddWithValue("@id", crafteo.idArticuloSlot9);
                cmd.Parameters.AddWithValue("@cant", crafteo.cantidadArticuloSlot9 * cantidad);
                if (cmd.ExecuteNonQuery() == 0)
                    return false;
            }

            return true;
        }

        private bool manageStockAvailableSmelting(short idAlmacen, int idSmelting, int cantidad, bool restarStock)
        {
            Smelting smelting = getSmelting(idSmelting);
            string sql;
            if (restarStock)
                sql = "UPDATE alm_inventario SET cant_disp = cant_disp - @cant WHERE alm = @alm AND art = @id";
            else
                sql = "UPDATE alm_inventario SET cant_disp = cant_disp + @cant WHERE alm = @alm AND art = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", idAlmacen);
            cmd.Parameters.AddWithValue("@cant", smelting.cantidadEntrada * cantidad);
            cmd.Parameters.AddWithValue("@id", smelting.idArticuloEntrada);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool deleteOrdenFabricacion(OrdenFabricacionDelete ordenFabricacion)
        {
            string sql = "SELECT craft, smelt, cant, estado FROM alm_ordenes_fab WHERE alm = @alm AND fab = @fab AND id = @id";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", ordenFabricacion.idAlmacen);
            cmd.Parameters.AddWithValue("@fab", ordenFabricacion.idFabricacion);
            cmd.Parameters.AddWithValue("@id", ordenFabricacion.id);
            NpgsqlDataReader rdr = cmd.ExecuteReader();

            if (!rdr.HasRows)
            {
                rdr.Close();
                return false;
            }
            rdr.Read();
            int? idCrafteo;
            if (rdr.IsDBNull(0))
                idCrafteo = null;
            else
                idCrafteo = rdr.GetInt32(0);
            int? idSmelting;
            if (rdr.IsDBNull(1))
                idSmelting = null;
            else
                idSmelting = rdr.GetInt32(1);
            int cantidad = rdr.GetInt32(2);
            char estado = rdr.GetChar(3);
            rdr.Close();

            NpgsqlTransaction trans = conn.BeginTransaction();
            if (estado == 'Q' || estado == 'R')
            {
                bool ok;
                if (idCrafteo != null)
                    ok = this.manageStockAvailableCrafteo(ordenFabricacion.idAlmacen, (int)idCrafteo, cantidad, false);
                else if (idSmelting != null)
                    ok = this.manageStockAvailableSmelting(ordenFabricacion.idAlmacen, (int)idSmelting, cantidad, false);
                else
                    ok = false;

                if (!ok)
                {
                    trans.Rollback();
                    return false;
                }
            }

            sql = "DELETE FROM alm_ordenes_fab WHERE alm = @alm AND fab = @fab AND id = @id";
            cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@alm", ordenFabricacion.idAlmacen);
            cmd.Parameters.AddWithValue("@fab", ordenFabricacion.idFabricacion);
            cmd.Parameters.AddWithValue("@id", ordenFabricacion.id);
            if (cmd.ExecuteNonQuery() == 0)
            {
                trans.Rollback();
                return false;
            }

            trans.Commit();
            return true;
        }

        public OrdenFabricacionPreview previewCrafteo(short idAlmacen, int idCrafteo)
        {
            Crafteo crafteo = getCrafteo(idCrafteo);
            if (crafteo == null)
                return null;

            List<FabricacionCrafteo> articulosCrafteo = new List<FabricacionCrafteo>();
            if (crafteo.idArticuloSlot1 != null)
                FabricacionCrafteo.agregarArticulo(ref articulosCrafteo, (short)crafteo.idArticuloSlot1, crafteo.cantidadArticuloSlot1);
            if (crafteo.idArticuloSlot2 != null)
                FabricacionCrafteo.agregarArticulo(ref articulosCrafteo, (short)crafteo.idArticuloSlot2, crafteo.cantidadArticuloSlot2);
            if (crafteo.idArticuloSlot3 != null)
                FabricacionCrafteo.agregarArticulo(ref articulosCrafteo, (short)crafteo.idArticuloSlot3, crafteo.cantidadArticuloSlot3);
            if (crafteo.idArticuloSlot4 != null)
                FabricacionCrafteo.agregarArticulo(ref articulosCrafteo, (short)crafteo.idArticuloSlot4, crafteo.cantidadArticuloSlot4);
            if (crafteo.idArticuloSlot5 != null)
                FabricacionCrafteo.agregarArticulo(ref articulosCrafteo, (short)crafteo.idArticuloSlot5, crafteo.cantidadArticuloSlot5);
            if (crafteo.idArticuloSlot6 != null)
                FabricacionCrafteo.agregarArticulo(ref articulosCrafteo, (short)crafteo.idArticuloSlot6, crafteo.cantidadArticuloSlot6);
            if (crafteo.idArticuloSlot7 != null)
                FabricacionCrafteo.agregarArticulo(ref articulosCrafteo, (short)crafteo.idArticuloSlot7, crafteo.cantidadArticuloSlot7);
            if (crafteo.idArticuloSlot8 != null)
                FabricacionCrafteo.agregarArticulo(ref articulosCrafteo, (short)crafteo.idArticuloSlot8, crafteo.cantidadArticuloSlot8);
            if (crafteo.idArticuloSlot9 != null)
                FabricacionCrafteo.agregarArticulo(ref articulosCrafteo, (short)crafteo.idArticuloSlot9, crafteo.cantidadArticuloSlot9);

            if (articulosCrafteo.Count < 0)
                return null;
            short maxCantidadCrafteo = Int16.MaxValue;
            for (int i = 0; i < articulosCrafteo.Count; i++)
            {
                articulosCrafteo[i].cantidadDisponible = getInventarioAlmacen(idAlmacen, articulosCrafteo[i].idArticulo).cantidadDisponible;

                if ((articulosCrafteo[i].cantidadDisponible / articulosCrafteo[i].cantidadCrafteo) < maxCantidadCrafteo)
                {
                    maxCantidadCrafteo = ((short)(articulosCrafteo[i].cantidadDisponible / articulosCrafteo[i].cantidadCrafteo));
                }
            }

            return new OrdenFabricacionPreview(articulosCrafteo, crafteo.cantidadResultado, maxCantidadCrafteo);
        }

        public OrdenFabricacionPreview previewSmelting(short idAlmacen, int idSmelting)
        {
            Smelting smelting = getSmelting(idSmelting);
            if (smelting == null)
                return null;

            int cantidad = getInventarioAlmacen(idAlmacen, smelting.idArticuloEntrada).cantidadDisponible;

            List<FabricacionCrafteo> articulosCrafteo = new List<FabricacionCrafteo>();
            articulosCrafteo.Add(new FabricacionCrafteo(smelting.idArticuloEntrada, smelting.cantidadEntrada, cantidad));

            short maxCantidadCrafteo = ((short)(cantidad / smelting.cantidadEntrada));
            return new OrdenFabricacionPreview(articulosCrafteo, smelting.cantidadResultado, maxCantidadCrafteo);
        }

        public bool setResultadoOrdenFabricaicon(int id, short idAlmacen, short idFabricacion, bool ok, short errorCode = 0)
        {
            // si la órden de fabricación se ha completado correctamente, desacumular la resta del stock disponible
            if (ok)
            {
                // cargar la receta de la orden de fabricación
                string sql = "SELECT craft, smelt, cant FROM alm_ordenes_fab WHERE alm = @alm AND fab = @fab AND id = @id";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@alm", idAlmacen);
                cmd.Parameters.AddWithValue("@fab", idFabricacion);
                cmd.Parameters.AddWithValue("@id", id);
                NpgsqlDataReader rdr = cmd.ExecuteReader();

                if (!rdr.HasRows)
                {
                    rdr.Close();
                    return false;
                }
                rdr.Read();
                int? idCrafteo;
                if (rdr.IsDBNull(0))
                    idCrafteo = null;
                else
                    idCrafteo = rdr.GetInt32(0);
                int? idSmelting;
                if (rdr.IsDBNull(1))
                    idSmelting = null;
                else
                    idSmelting = rdr.GetInt32(1);
                int cantidad = rdr.GetInt32(2);
                rdr.Close();

                // deshacer cambios del stock disponible
                NpgsqlTransaction trans = conn.BeginTransaction();
                bool stockOk;
                if (idCrafteo != null)
                    stockOk = this.manageStockAvailableCrafteo(idAlmacen, (int)idCrafteo, cantidad, false);
                else if (idSmelting != null)
                    stockOk = this.manageStockAvailableSmelting(idAlmacen, (int)idSmelting, cantidad, false);
                else
                    stockOk = false;

                if (!stockOk)
                {
                    trans.Rollback();
                    return false;
                }

                // completar la orden de fabricación
                sql = "UPDATE alm_ordenes_fab SET estado = 'D', date_fin = CURRENT_TIMESTAMP(3), resultado = cant WHERE alm = @alm AND fab = @fab AND id = @id";
                cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@alm", idAlmacen);
                cmd.Parameters.AddWithValue("@fab", idFabricacion);
                cmd.Parameters.AddWithValue("@id", id);
                if (cmd.ExecuteNonQuery() == 0)
                {
                    trans.Rollback();
                    return false;
                }

                trans.Commit();
                return true;
            }
            else
            {
                // guardar el error de la órden de fabricación
                string sql = "UPDATE alm_ordenes_fab SET estado = 'D', date_fin = CURRENT_TIMESTAMP(3), resultado = 0, error_code = @error_code WHERE alm = @alm AND fab = @fab AND id = @id";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@alm", idAlmacen);
                cmd.Parameters.AddWithValue("@fab", idFabricacion);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@error_code", errorCode);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
