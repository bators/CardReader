using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace authorization
{
   public class SQL
    {
        SQLiteConnection conn = new SQLiteConnection("Data Source=Students.db;Version=3;");

        public async void CallPostRequestRecord(string date, string studID, string ip)
        {
            await PostRequestRecord(date, studID, ip);
        }
        async Task PostRequestRecord(string date, string studID, string ip)
        {

            try
            {
                WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:81/1.php");
                request.Method = "POST"; // для отправки используется метод Post
                                         // данные для отправки

                var js = new
                {
                    time_attendace = date,
                    number_card = studID,
                    ip = ip
                };
                string json = JsonConvert.SerializeObject(js);
                //MessageBox.Show(json);

                // преобразуем данные в массив байтов          
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(json);

                // устанавливаем тип содержимого - параметр ContentType
                request.ContentType = "application/json";
                // Устанавливаем заголовок Content-Length запроса - свойство ContentLength
                request.ContentLength = byteArray.Length;

                //записываем данные в поток запроса
                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
                string Json = String.Empty;
                WebResponse response = await request.GetResponseAsync();
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        Json = reader.ReadToEnd();
                    }
                }
                response.Close();
                ;
                Console.WriteLine("Данные о студенте отпрвлены ");
            }
            catch (Exception e)
            {
                Console.WriteLine("SQLClose " + e.Message);
            }

        }


        public Boolean  FiersLoad()
        {
            Boolean flagForRequestDB= false;

            try
            {
                conn.Open();
            }

            catch (SQLiteException e)
            {
                Console.WriteLine("SQLOpen" + e.Message);
            }

            SQLiteCommand cmd = conn.CreateCommand();
            string sql_command = "CREATE TABLE IF NOT EXISTS  students("
              + "id INTEGER PRIMARY KEY AUTOINCREMENT, "
              + "studID INTEGER,"
              + "studPas TEXT,"
              + "statuse INTEGER );";
            cmd.CommandText = sql_command;

            try
            {
                cmd.ExecuteNonQuery();


            }

            catch (SQLiteException e)
            {
                Console.WriteLine("SQLQuery " + e.Message);
            }

            cmd.CommandText = "SELECT * FROM students ";
            try
            {
                SQLiteDataReader r = cmd.ExecuteReader();

                string line = String.Empty;
                string linePass = String.Empty;
                var caunt = r.StepCount;
                if (caunt != 0)
                {
                    flagForRequestDB = true;

                }
                else { flagForRequestDB = false; Console.WriteLine("база пуста"); }


                r.Close();
                cmd.ExecuteNonQuery();
            }

            catch (SQLiteException e)
            {
                Console.WriteLine("SQLQuery " + e.Message);
            }

            try
            {
                conn.Close();
            }

            catch (SQLiteException e)
            {
                Console.WriteLine("SQLClose " + e.Message);
            }
            return flagForRequestDB;


        }
        public Boolean ErrorLoad()
        {
            Boolean flagForRequestDB=false;

            try
            {
                conn.Open();
            }

            catch (SQLiteException e)
            {
                Console.WriteLine("SQLOpen" + e.Message);
            }

            SQLiteCommand cmd = conn.CreateCommand();
            string sql_command = "CREATE TABLE IF NOT EXISTS  serverError("
               + "id INTEGER PRIMARY KEY AUTOINCREMENT, "
               + "data TEXT,"
               + "studPas TEXT,"
               + "ip TEXT );";
            cmd.CommandText = sql_command;

            try
            {
                cmd.ExecuteNonQuery();


            }

            catch (SQLiteException e)
            {
                Console.WriteLine("SQLQuery " + e.Message);
            }

            cmd.CommandText = "SELECT * FROM students ";
            try
            {
                SQLiteDataReader r = cmd.ExecuteReader();

                
                var caunt = r.StepCount;
                if (caunt != 0)
                {
                    flagForRequestDB = true;

                }
                else { flagForRequestDB = false; Console.WriteLine("база пуста"); }


                r.Close();
                cmd.ExecuteNonQuery();
            }

            catch (SQLiteException e)
            {
                Console.WriteLine("SQLQuery " + e.Message);
            }

            try
            {
                conn.Close();
            }

            catch (SQLiteException e)
            {
                Console.WriteLine("SQLClose " + e.Message);
            }
            return flagForRequestDB;


        }



        public string SQLVerify(string UID)
        {
            SQLiteCommand cmd = conn.CreateCommand();
            string sql_command = "CREATE TABLE IF NOT EXISTS  Events("
              + "id INTEGER PRIMARY KEY AUTOINCREMENT, "
              + "log_id INTEGER,"
              + "text TEXT;";
            cmd.CommandText = sql_command;

            try
            {
                cmd.ExecuteNonQuery();


            }

            catch (SQLiteException e)
            {
                Console.WriteLine("SQLQuery " + e.Message);
            }
            string personInfo = String.Empty;
            try
            {
                conn.Open();
            }
            catch (SQLiteException e)
            {
                Console.WriteLine("SQLOpen" + e.Message);
            }
          
            cmd.CommandText = "SELECT * FROM students ";
            try
            {
                SQLiteDataReader r = cmd.ExecuteReader();              
                string linePass = String.Empty;
                var caunt = r.StepCount;
                    while (r.Read())
                    {                        
                        linePass = r["studPas"].ToString();
                        if (linePass.Equals(UID))
                        {
                            personInfo =  linePass;
                            break;
                        }
                        else
                        {
                            personInfo = "-1";
                        }
                    }              
                r.Close();
                cmd.ExecuteNonQuery();
                Console.WriteLine("Студент найден");
            }

            catch (SQLiteException e)
            {
                Console.WriteLine("SQLQuery " + e.Message);
            }
            if (personInfo.Equals("-1"))
            {

            }
            else
            {
                sql_command = String.Empty;



                sql_command = "INSERT INTO Events (log_id, text)"
                    + "VALUES ( 3 , '" + UID + "');";


                cmd.CommandText = sql_command;

                try
                {
                    cmd.ExecuteNonQuery();

                }
                catch (SQLiteException ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
                try
            {
                conn.Close();
            }
            catch (SQLiteException e)
            {
                Console.WriteLine("SQLClose " + e.Message);
            }
            return personInfo;
        }

        public void ErrorList()
        {
            
            try
            {
                conn.Open();
            }
            catch (SQLiteException e)
            {
                Console.WriteLine("SQLOpen" + e.Message);
            }
            SQLiteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM serverError ";
            try
            {
                SQLiteDataReader r = cmd.ExecuteReader();
                string linePass = String.Empty;
                string lineData = String.Empty;
                string lineIP = String.Empty;
                var caunt = r.StepCount;
                while (r.Read())
                {
                    lineData= r["data"].ToString();
                    lineIP = r["ip"].ToString();
                    linePass = r["studPas"].ToString();
                    CallPostRequestRecord(lineData, lineIP, linePass);
                    string sql_command = String.Empty;



                    sql_command = "INSERT INTO Events (log_id, text)"
                        + "VALUES ( 5 , '" + linePass + "');";


                    cmd.CommandText = sql_command;

                    try
                    {
                        cmd.ExecuteNonQuery();

                    }
                    catch (SQLiteException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
                r.Close();
                cmd.ExecuteNonQuery();
                
            }

            catch (SQLiteException e)
            {
                Console.WriteLine("SQLQuery " + e.Message);
            }
            string sql_command1 = "DELETE * FROM serverError ;";

            cmd.CommandText = sql_command1;

            try
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine("Данные успешно удалены "); 


            }
            catch (SQLiteException e)
            {
                Console.WriteLine("SQLQuery " + e.Message);
            }
            try
            {
                conn.Close();
            }
            catch (SQLiteException e)
            {
                Console.WriteLine("SQLClose " + e.Message);
            }
            
        }

        public void SQLInsert(string  data, string studID, string ip)
        {
            
            try
            {
                conn.Open();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            SQLiteCommand cmd = conn.CreateCommand();
            string sql_command = "CREATE TABLE IF NOT EXISTS  serverError("
              + "id INTEGER PRIMARY KEY AUTOINCREMENT, "
              + "data TEXT,"
              + "studPas TEXT,"
              + "ip TEXT );";
            cmd.CommandText = sql_command;

            try
            {
                cmd.ExecuteNonQuery();


            }

            catch (SQLiteException e)
            {
                Console.WriteLine("SQLQuery " + e.Message);
            }
             sql_command = String.Empty;
            
           

                sql_command = "INSERT INTO serverEror (data, studPas, ip)"
                    + "VALUES ('" + data + "', '" + studID + "',   ' " + ip + " ');";
            

            cmd.CommandText = sql_command;

            try
            {
                cmd.ExecuteNonQuery();
                 sql_command = String.Empty;



                sql_command = "INSERT INTO Events (log_id, text)"
                    + "VALUES ( 5 , '" + studID + "');";


                cmd.CommandText = sql_command;

                try
                {
                    cmd.ExecuteNonQuery();

                }
                catch (SQLiteException ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

    }



}
