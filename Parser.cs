using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Ziedden.Mysql
{
    public class Parser
    {
        private string ConnectionString;

        public Parser(string ConnectionString)
        {
            MySqlConnection connection;            
            connection = new MySqlConnection(ConnectionString);
            connection.Open();
            connection.Close();
            this.ConnectionString = ConnectionString;
        }

        public Parser(string Server,string UserID,string Password,string Database)
        {
            MySqlConnection connection;
            connection = new MySqlConnection($"server={Server};userid={UserID};password={Password};database={Database}");
            connection.Open();
            connection.Close();
            this.ConnectionString = $"server={Server};userid={UserID};password={Password};database={Database}";
        }

        #region "Insert"
        public int Insert(object Data)
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(ConnectionString);
                connection.Open();
                string DBName = Data.GetType().Name;
                var cmd = new MySqlCommand();
                cmd.Connection = connection;

                cmd.CommandText = $"SHOW TABLES LIKE \"{DBName}\";";
                var reader = cmd.ExecuteReader();
                if (reader.HasRows == false)
                {
                    Console.WriteLine("MySQL: Table was not given!");
                    CreateTable(Data);
                }
                else
                {
                    CheckTableState(Data);
                }
                string InsertDatas = "";
                string FieldDatas = "";
                foreach (MemberInfo pi in Data.GetType().GetMembers())
                {
                    if (!(pi.GetCustomAttribute<MysqlDataAttribute>() == null))
                    {
                        if (pi.GetCustomAttribute<MysqlDataAttribute>().FieldName == null)
                        {
                            if (!(pi.Name.ToLower().Equals("id")))
                            {
                                string name = pi.Name.ToLower();
                                string data = Newtonsoft.Json.JsonConvert.SerializeObject(GetValue(pi, Data));
                                FieldDatas = $"{FieldDatas}, {name}";
                                InsertDatas = $"{InsertDatas}, '{data}'";
                            }
                        }
                        else
                        {
                            if (!(pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower().Equals("id")))
                            {
                                string name = pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower();
                                string data = Newtonsoft.Json.JsonConvert.SerializeObject(GetValue(pi, Data));
                                FieldDatas = $"{FieldDatas}, {name}";
                                InsertDatas = $"{InsertDatas}, '{data}'";
                            }
                        }
                    }
                }

                connection.Close();

                connection = new MySqlConnection(ConnectionString);
                connection.Open();
                cmd = new MySqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"INSERT INTO {Data.GetType().Name}(id{FieldDatas}) VALUES(0{InsertDatas})";
                cmd.ExecuteNonQuery();
                int id =  (int)cmd.LastInsertedId;
                connection.Close();
                return id;
            }
            catch(Exception ex) {
                Console.WriteLine(ex.ToString());
                return -1; }
        }

        private void CheckTableState(object Data)
        {
            List<string> DBFields = new List<string>();
            List<string> RefFields = new List<string>();

            Console.WriteLine("MySQL: Check Table ...");
            var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            var cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='{Data.GetType().Name}';";
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                string data = rdr.GetString(0);
                if (!(data.ToLower().Equals("id")))
                {
                    DBFields.Add(data.ToLower());
                }
            }
            connection.Close();

            foreach (MemberInfo pi in Data.GetType().GetMembers())
            {
                if (!(pi.GetCustomAttribute<MysqlDataAttribute>() == null))
                {
                    if (pi.GetCustomAttribute<MysqlDataAttribute>().FieldName == null)
                    {
                        if (!(pi.Name.ToLower().Equals("id")))
                        {
                            RefFields.Add(pi.Name.ToLower());
                        }
                    }
                    else
                    {
                        if (!(pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower().Equals("id")))
                        {
                            RefFields.Add(pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower());

                        }
                    }
                }

            }

            var NotPresent = RefFields.Except(DBFields).ToList();

            Console.WriteLine("MySQL: Tabelstate not Present");
            foreach(string s in NotPresent)
            {
                connection = new MySqlConnection(ConnectionString);
                connection.Open();
                cmd = new MySqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"ALTER TABLE {Data.GetType().Name} ADD COLUMN {s} TEXT;";
                cmd.ExecuteNonQuery();
                connection.Close();
            }

        }
        private void CheckTableState(Type Data)
        {
            List<string> DBFields = new List<string>();
            List<string> RefFields = new List<string>();

            Console.WriteLine($"MySQL: Check Table {Data.Name} ...");
            var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            var cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='{Data.Name}';";
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                string data = rdr.GetString(0);
                if (!(data.ToLower().Equals("id")))
                {
                    DBFields.Add(data.ToLower());
                }
            }
            connection.Close();

            foreach (MemberInfo pi in Data.GetMembers())
            {
                if (!(pi.GetCustomAttribute<MysqlDataAttribute>() == null))
                {
                    if (pi.GetCustomAttribute<MysqlDataAttribute>().FieldName == null)
                    {
                        if (!(pi.Name.ToLower().Equals("id")))
                        {
                            RefFields.Add(pi.Name.ToLower());
                        }
                    }
                    else
                    {
                        if (!(pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower().Equals("id")))
                        {
                            RefFields.Add(pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower());

                        }
                    }
                }

            }

            var NotPresent = RefFields.Except(DBFields).ToList();

            Console.WriteLine("MySQL: TabelState not Present");
            foreach (string s in NotPresent)
            {
                connection = new MySqlConnection(ConnectionString);
                connection.Open();
                cmd = new MySqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = $"ALTER TABLE {Data.Name} ADD COLUMN {s} TEXT;";
                cmd.ExecuteNonQuery();
                connection.Close();
            }

        }

        private object GetValue(MemberInfo memberInfo, object forObject)
        {
            try
            {
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Field:
                        return ((FieldInfo)memberInfo).GetValue(forObject);
                    case MemberTypes.Property:
                        return ((PropertyInfo)memberInfo).GetValue(forObject);
                    default:
                        throw new NotImplementedException();
                }
            }
            catch { return null; }
        }

        private Type GetUnderlyingType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                     "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }

        private string ParseMYSQLType(MemberInfo mi)
        {
            return "TEXT";
        }

        private void CreateTable(object Data)
        {
            Dictionary<string,string> ColoumnName = new Dictionary<string, string>();
            foreach (MemberInfo pi in Data.GetType().GetMembers())
            {
                if (!(pi.GetCustomAttribute<MysqlDataAttribute>() == null))
                {
                    if (pi.GetCustomAttribute<MysqlDataAttribute>().FieldName == null)
                    {
                        if (!(pi.Name.ToLower().Equals("id")))
                        {
                            ColoumnName.Add(pi.Name, ParseMYSQLType(pi));
                        }
                    }
                    else
                    {                        
                        if (!(pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower().Equals("id")))
                        {
                            ColoumnName.Add(pi.GetCustomAttribute<MysqlDataAttribute>().FieldName, ParseMYSQLType(pi));
                        }
                    }
                }
            }

            MySqlConnection connection = new MySqlConnection(ConnectionString);
            connection.Open();

            var cmd = new MySqlCommand();
            cmd.Connection = connection;

            string fields = "";
            foreach(string s in ColoumnName.Keys)
            {
                fields = $"{fields}, {s.ToLower()} TEXT";
            }

            cmd.CommandText = $"CREATE TABLE {Data.GetType().Name} (id INTEGER PRIMARY KEY AUTO_INCREMENT{fields})";
            cmd.ExecuteNonQuery();

            connection.Close();
        }
        
        #endregion

        #region "Update"

        public bool Update(object Data)
        {

            MySqlConnection connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string DBName = Data.GetType().Name;
            var cmd = new MySqlCommand();
            cmd.Connection = connection;

            cmd.CommandText = $"SHOW TABLES LIKE \"{DBName}\";";
            var reader = cmd.ExecuteReader();
            if (reader.HasRows == false)
            {
                Console.WriteLine("MYSQL: Table was not given!");
                return false;
            }
            CheckTableState(Data);

            int id = -1;
            foreach (MemberInfo pi in Data.GetType().GetMembers())
            {
                if (!(pi.GetCustomAttribute<MysqlDataAttribute>() == null))
                {
                    if (pi.GetCustomAttribute<MysqlDataAttribute>().FieldName == null)
                    {
                        if ((pi.Name.ToLower().Equals("id")))
                        {
                            
                            id = (int)GetValue(pi, Data);
                        }
                    }
                    else
                    {
                        if ((pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower().Equals("id")))
                        {
                            id = (int)GetValue(pi, Data);
                        }
                    }
                }
            }

            if(id == -1)
            {
                return false;
            }

            List<string> vars = new List<string>();
            foreach (MemberInfo pi in Data.GetType().GetMembers())
            {
                if (!(pi.GetCustomAttribute<MysqlDataAttribute>() == null))
                {
                    if (pi.GetCustomAttribute<MysqlDataAttribute>().FieldName == null)
                    {
                        if (!(pi.Name.ToLower().Equals("id")))
                        {
                            string name = pi.Name.ToLower();
                            string data = Newtonsoft.Json.JsonConvert.SerializeObject(GetValue(pi, Data));
                            vars.Add($"{name} = '{data}'");
                        }
                    }
                    else
                    {
                        if (!(pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower().Equals("id")))
                        {
                            string name = pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower();
                            string data = Newtonsoft.Json.JsonConvert.SerializeObject(GetValue(pi, Data));
                            vars.Add($"{name} = '{data}'");
                        }
                    }
                }
            }

            string UpdateString = String.Join(", ", vars.ToArray());

            connection.Close();

            connection = new MySqlConnection(ConnectionString);
            connection.Open();
            cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = $"UPDATE {DBName} SET {UpdateString} WHERE id = {id}";
            cmd.ExecuteNonQuery();
            connection.Close();

            return true;
        }

        public bool Update(object Data,string Column,object Value)
        {

            MySqlConnection connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string DBName = Data.GetType().Name;
            var cmd = new MySqlCommand();
            cmd.Connection = connection;

            cmd.CommandText = $"SHOW TABLES LIKE \"{DBName}\";";
            var reader = cmd.ExecuteReader();
            if (reader.HasRows == false)
            {
                Console.WriteLine("MYSQL: Table was not given!");
                return false;
            }
            CheckTableState(Data);


            List<string> vars = new List<string>();
            foreach (MemberInfo pi in Data.GetType().GetMembers())
            {
                if (!(pi.GetCustomAttribute<MysqlDataAttribute>() == null))
                {
                    if (pi.GetCustomAttribute<MysqlDataAttribute>().FieldName == null)
                    {
                        if (!(pi.Name.ToLower().Equals("id")))
                        {
                            string name = pi.Name.ToLower();
                            string data = Newtonsoft.Json.JsonConvert.SerializeObject(GetValue(pi, Data));
                            vars.Add($"{name} = '{data}'");
                        }
                    }
                    else
                    {
                        if (!(pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower().Equals("id")))
                        {
                            string name = pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower();
                            string data = Newtonsoft.Json.JsonConvert.SerializeObject(GetValue(pi, Data));
                            vars.Add($"{name} = '{data}'");
                        }
                    }
                }
            }

            string UpdateString = String.Join(", ", vars.ToArray());

            connection.Close();

            connection = new MySqlConnection(ConnectionString);
            connection.Open();
            cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = $"UPDATE {DBName} SET {UpdateString} WHERE {Column} = {Newtonsoft.Json.JsonConvert.SerializeObject(Value)}";
            cmd.ExecuteNonQuery();
            connection.Close();

            return true;
        }

        #endregion

        #region "Delete"
        public bool Delete(object Data)
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string DBName = Data.GetType().Name;
            var cmd = new MySqlCommand();
            cmd.Connection = connection;

            cmd.CommandText = $"SHOW TABLES LIKE \"{DBName}\";";
            var reader = cmd.ExecuteReader();
            if (reader.HasRows == false)
            {
                Console.WriteLine("MYSQL: Tabel was not given!");
                return false;
            }

            int id = -1;
            foreach (MemberInfo pi in Data.GetType().GetMembers())
            {
                if (!(pi.GetCustomAttribute<MysqlDataAttribute>() == null))
                {
                    if (pi.GetCustomAttribute<MysqlDataAttribute>().FieldName == null)
                    {
                        if ((pi.Name.ToLower().Equals("id")))
                        {

                            id = (int)GetValue(pi, Data);
                        }
                    }
                    else
                    {
                        if ((pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower().Equals("id")))
                        {
                            id = (int)GetValue(pi, Data);
                        }
                    }
                }
            }

            if (id == -1)
            {
                return false;
            }

            connection.Close();

            connection = new MySqlConnection(ConnectionString);
            connection.Open();
            cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = $"DELETE FROM {DBName} WHERE id = {id}";
            cmd.ExecuteNonQuery();
            connection.Close();

            return true;
        }
        public bool Delete(Type Data, Where[] Arguments)
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string DBName = Data.GetType().Name;
            var cmd = new MySqlCommand();
            cmd.Connection = connection;

            cmd.CommandText = $"SHOW TABLES LIKE \"{DBName}\";";
            var reader = cmd.ExecuteReader();
            if (reader.HasRows == false)
            {
                Console.WriteLine("MYSQL: Table was not given!");
                return false;
            }

            List<string> WhereDatas = new List<string>();
            foreach (Where w in Arguments)
            {
                WhereDatas.Add($"{w.FieldName.ToLower()} = '{Newtonsoft.Json.JsonConvert.SerializeObject(w.Value).Replace("'","")}'");
            }
            string WhereString = String.Join(" AND ", WhereDatas.ToArray());
            connection.Close();

            connection = new MySqlConnection(ConnectionString);
            connection.Open();
            cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = $"DELETE FROM {DBName} WHERE {WhereString}";
            cmd.ExecuteNonQuery();
            connection.Close();

            return true;
        }

        #endregion

        private void SetValue(MemberInfo memberInfo, object forObject,object value)
        {
            try
            {
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Field:
                        ((FieldInfo)memberInfo).SetValue(forObject, value); ;
                        break;
                    case MemberTypes.Property:
                        ((PropertyInfo)memberInfo).SetValue(forObject, value); 
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch {  }
        }

        public T[] FindAll<T>()
        {
            List<T> all = new List<T>();
            string DBName = typeof(T).Name;
            List<string> Datas = new List<string>();
            Dictionary<string, MemberInfo> Buffer = new Dictionary<string, MemberInfo>();
            Datas.Add("id");
            CheckTableState(typeof(T));
            foreach (MemberInfo pi in typeof(T).GetMembers())
            {
                if (!(pi.GetCustomAttribute<MysqlDataAttribute>() == null))
                {
                    if (pi.GetCustomAttribute<MysqlDataAttribute>().FieldName == null)
                    {
                        if (!(pi.Name.ToLower().Equals("id")))
                        {
                            string name = pi.Name.ToLower();
                            Datas.Add(name);
                            Buffer.Add(name, pi);
                        }
                        else
                        {
                            Buffer.Add("id", pi);
                        }
                    }
                    else
                    {
                        if (!(pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower().Equals("id")))
                        {
                            string name = pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower();
                            Datas.Add(name);
                            Buffer.Add(name, pi);
                        }
                        else
                        {
                            Buffer.Add("id", pi);
                        }
                    }
                }
            }

            string datasString = String.Join(", ", Datas.ToArray());

            var con = new MySqlConnection(ConnectionString);
            con.Open();

            string sql = $"SELECT {datasString} FROM {DBName}";
            var cmd = new MySqlCommand(sql, con);

            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {                
                T newItem = (T)Activator.CreateInstance(typeof(T));
                for (int i = 0; i < Datas.Count; i++)
                {
                    try
                    {
                        if (Datas[i].ToLower().Equals("id"))
                        {
                            SetValue(Buffer[Datas[i]], newItem, rdr.GetInt32(i));
                        }
                        else
                        {
                            SetValue(Buffer[Datas[i]], newItem, Newtonsoft.Json.JsonConvert.DeserializeObject(rdr.GetString(i), GetUnderlyingType(Buffer[Datas[i]])));
                        }

                    }
                    catch { }
                }
                all.Add(newItem);
            }
            con.Close();
            return all.ToArray();
        }

        public class Where
        {
            public string FieldName;
            public object Value;
            public Where(string FieldName, object Value)
            {
                this.FieldName = FieldName;
                this.Value = Value;
            }
        }

        public T[] FindWhere<T>(Where[] Arguments)
        {
            List<T> all = new List<T>();
            string DBName = typeof(T).Name;
            List<string> Datas = new List<string>();
            Dictionary<string, MemberInfo> Buffer = new Dictionary<string, MemberInfo>();
            Datas.Add("id");
            CheckTableState(typeof(T));
            foreach (MemberInfo pi in typeof(T).GetMembers())
            {
                if (!(pi.GetCustomAttribute<MysqlDataAttribute>() == null))
                {
                    if (pi.GetCustomAttribute<MysqlDataAttribute>().FieldName == null)
                    {
                        if (!(pi.Name.ToLower().Equals("id")))
                        {
                            string name = pi.Name.ToLower();
                            Datas.Add(name);
                            Buffer.Add(name, pi);
                        }
                        else
                        {
                            Buffer.Add("id", pi);
                        }
                    }
                    else
                    {
                        if (!(pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower().Equals("id")))
                        {
                            string name = pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower();
                            Datas.Add(name);
                            Buffer.Add(name, pi);
                        }
                        else
                        {
                            Buffer.Add("id", pi);
                        }
                    }
                }
            }

            string datasString = String.Join(", ", Datas.ToArray());

            var con = new MySqlConnection(ConnectionString);
            con.Open();

            List<string> WhereDatas = new List<string>();
            foreach(Where w in Arguments)
            {
                WhereDatas.Add($"{w.FieldName.ToLower()} = '{Newtonsoft.Json.JsonConvert.SerializeObject(w.Value).Replace("'","")}'");
            }
            string WhereString = String.Join(" AND ", WhereDatas.ToArray());

            string sql = $"SELECT {datasString} FROM {DBName} WHERE {WhereString};";
            var cmd = new MySqlCommand(sql, con);

            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                T newItem = (T)Activator.CreateInstance(typeof(T));
                for (int i = 0; i < Datas.Count; i++)
                {
                    try
                    {
                        if (Datas[i].ToLower().Equals("id"))
                        {
                            SetValue(Buffer[Datas[i]], newItem, rdr.GetInt32(i));
                        }
                        else
                        {
                            SetValue(Buffer[Datas[i]], newItem, Newtonsoft.Json.JsonConvert.DeserializeObject(rdr.GetString(i), GetUnderlyingType(Buffer[Datas[i]])));
                        }

                    }
                    catch { }
                }
                all.Add(newItem);
            }
            con.Close();
            return all.ToArray();
        }
        public T FindByID<T>(int ID)
        {
            string DBName = typeof(T).Name;
            List<string> Datas = new List<string>();
            Dictionary<string, MemberInfo> Buffer = new Dictionary<string, MemberInfo>();
            Datas.Add("id");
            CheckTableState(typeof(T));
            foreach (MemberInfo pi in typeof(T).GetMembers())
            {
                if (!(pi.GetCustomAttribute<MysqlDataAttribute>() == null))
                {
                    if (pi.GetCustomAttribute<MysqlDataAttribute>().FieldName == null)
                    {
                        if (!(pi.Name.ToLower().Equals("id")))
                        {
                            string name = pi.Name.ToLower();
                            Datas.Add(name);
                            Buffer.Add(name, pi);
                        }
                        else
                        {
                            Buffer.Add("id", pi);
                        }
                    }
                    else
                    {
                        if (!(pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower().Equals("id")))
                        {
                            string name = pi.GetCustomAttribute<MysqlDataAttribute>().FieldName.ToLower();
                            Datas.Add(name);
                            Buffer.Add(name, pi);
                        }
                        else
                        {
                            Buffer.Add("id", pi);
                        }
                    }
                }
            }

            string datasString = String.Join(", ", Datas.ToArray());

            var con = new MySqlConnection(ConnectionString);
            con.Open();            

            string sql = $"SELECT {datasString} FROM {DBName} WHERE id = {ID.ToString()};";
            var cmd = new MySqlCommand(sql, con);

            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                T newItem = (T)Activator.CreateInstance(typeof(T));
                for (int i = 0; i < Datas.Count; i++)
                {
                    try
                    {
                        if (Datas[i].ToLower().Equals("id"))
                        {
                            SetValue(Buffer[Datas[i]], newItem, rdr.GetInt32(i));
                        }
                        else
                        {
                            SetValue(Buffer[Datas[i]], newItem, Newtonsoft.Json.JsonConvert.DeserializeObject(rdr.GetString(i), GetUnderlyingType(Buffer[Datas[i]])));
                        }

                    }
                    catch { }
                }
                return newItem;
            }
            con.Close();
            return default(T);
        }
    }
}
