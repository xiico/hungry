using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;
using System.Threading;

namespace WS
{
    class Program
    {
        static SqlCeConnection m_dbConnection;
        static volatile string name = string.Empty;
        static volatile int lastReceived = 0;
        static volatile string msg = string.Empty;
        static void Main(string[] args)
        {
            //[system message],[chat message],[board]
            bool close = false;

            m_dbConnection = new SqlCeConnection(@"Data Source=c:\users\francisco.cnmarao\documents\visual studio 2010\Projects\WS\WS\history.sdf");//Data Source=MyData.sdf;Persist Security Info=False;
            
            SqlCeCommand command;
            string sql = string.Empty;
            try
            {
                m_dbConnection.Open();
            }
            catch
            {
                //m_dbConnection = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;");
                m_dbConnection.Open();
                //sql = "create table chat (id int,name varchar(20), message varchar(20), status char)";
                command = new SqlCeCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
            }


            command = new SqlCeCommand(sql, m_dbConnection);
            command.CommandText = "SELECT id FROM history ORDER BY id DESC;";
            lastReceived = int.Parse(command.ExecuteScalar().ToString());
            Thread trd = new Thread(Listen);
            trd.Start();

            while (true)
            {
                sql = updateChat(command, sql);


                if (close)
                    m_dbConnection.Close();
            }

        }

        private static string updateChat(SqlCeCommand command, string sql)
        {
            Thread.Sleep(500);
            sql = "select count(*) from history where status = 'N'";
            command.CommandText = sql;
            command.CommandType = System.Data.CommandType.Text;
            var messages = command.ExecuteScalar().ToString();
            if (messages != "0")
            {
                sql = "select * from history where id > " + lastReceived;
                command.CommandText = sql;
                command.CommandType = System.Data.CommandType.Text;
                SqlCeDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("¬" + reader["message"].ToString() + "¬");
                    command.CommandText = sql;
                    command.CommandType = System.Data.CommandType.Text;
                    //command.CommandText = "update history set status = 'O' where id = " + reader["id"].ToString();
                    lastReceived = int.Parse(reader["id"].ToString());
                    command.ExecuteNonQuery();
                }
            }
            return sql;
        }


        static void Listen()
        {
            //[ClientID],[Number],[chat message],[move],[start]
            while (true)
            {
                string sql;
                string rcvd = Console.ReadLine();

                if (rcvd.Split('¬')[2] != "")
                {
                    sql = "insert into history(ClientID,name,message,status) values (" + System.Diagnostics.Process.GetCurrentProcess().Id + ",'" + name + "','" + msg + "','N')";
                    SqlCeCommand command = new SqlCeCommand(sql, m_dbConnection);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
