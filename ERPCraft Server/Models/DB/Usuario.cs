using Npgsql;
using System;
using System.Security.Cryptography;
using System.Text;

namespace ERPCraft_Server.Models.DB
{
    public class Usuario
    {
        public short id;
        public string name;
        public string pwd;
        public string salt;
        public int iteraciones;
        public DateTime ultima_con;
        public string descripcion;
        public bool off;

        public Usuario(NpgsqlDataReader rdr)
        {
            this.id = rdr.GetInt16(0);
            this.name = rdr.GetString(1);
            this.pwd = rdr.GetString(2);
            this.salt = rdr.GetString(3);
            this.iteraciones = rdr.GetInt32(4);
            this.ultima_con = rdr.GetDateTime(5);
            this.descripcion = rdr.GetString(6);
            this.off = rdr.GetBoolean(7);
        }

        public Usuario(string name, string pwd, string salt, int iteraciones)
        {
            this.name = name;
            this.pwd = pwd;
            this.salt = salt;
            this.iteraciones = iteraciones;
        }

        public static bool verifyHash(string hash, string pwd, int iteraciones)
        {
            byte[] currentHash = Encoding.UTF8.GetBytes(pwd);
            SHA512 sha512 = SHA512.Create();
            for (int i = 0; i < iteraciones; i++)
            {
                currentHash = sha512.ComputeHash(currentHash);
            }
            sha512.Dispose();
            return hashToHex(currentHash).Equals(hash);
        }

        public static string hash(string data, int rounds = 5000)
        {
            byte[] hash = Encoding.UTF8.GetBytes(data);
            SHA512 sha = SHA512.Create();
            for (int i = 0; i < rounds; i++)
            {
                hash = sha.ComputeHash(hash);
            }
            sha.Dispose();
            return hashToHex(hash);
        }

        private static string hashToHex(byte[] hash)
        {
            StringBuilder str = new StringBuilder();
            foreach (byte bSel in hash)
            {
                str.Append(bSel.ToString("x2"));
            }
            return str.ToString();
        }

        public static string generateSalt(int size = 30)
        {
            const string charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789,.;:!\"$%&/()=@#><-_";
            StringBuilder str = new StringBuilder();
            Random r = new Random();
            for (int i = 0; i < size; i++)
            {
                str.Append(charset[r.Next(0, 80)]);
            }
            return str.ToString();
        }
    }

    public class UsuarioLogin
    {
        public string name;
        public string pwd;

        public UsuarioLogin()
        {
        }

        public bool isValid()
        {
            if (name == null || name.Length == 0 || pwd == null || pwd.Length == 0)
                return false;
            return true;
        }
    }

    public class UsuarioEdit
    {
        public short id;
        public string name;
        public string descripcion;
        public bool off;

        public bool isValid()
        {
            if (name == null || name.Length == 0 || descripcion == null || id <= 0)
                return false;
            return true;
        }
    }

    public class UsuarioPwd
    {
        public short id;
        public string pwdActual;
        public string pwdNuevo;

        public UsuarioPwd()
        {
        }

        public bool isValid()
        {
            if (id <= 0 || pwdActual == null || pwdActual.Length == 0 || pwdNuevo == null || pwdNuevo.Length == 0)
                return false;
            return true;
        }
    }
}
