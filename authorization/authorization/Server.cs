using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace authorization
{
   public class Server
    {
        SQL sql = new SQL();
        public static Boolean answer= false;
       



        public async void CallPostRequestInsertTable(string date, string studID, string ip)
        {
            await PostRequestInsertTable(date, studID, ip);


        }
       


      public   async Task PostRequestInsertTable(string date, string studID, string ip)
        {
                try
                {
                    WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:80/0.php");
                    request.Method = "POST";

                    var js = new
                    {
                        time_attendace = date,
                        number_card = studID,
                        ip = ip
                    };
                    string json = JsonConvert.SerializeObject(js);

                    byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(json);


                    request.ContentType = "application/json";

                    request.ContentLength = byteArray.Length;

                    using (Stream dataStream = request.GetRequestStream())
                    {
                        dataStream.Write(byteArray, 0, byteArray.Length);
                    }

                    WebResponse response = await request.GetResponseAsync();
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            var Json = reader.ReadToEnd();
                            dynamic ap = JObject.Parse(Json);
                            Console.WriteLine(ap.state);
                            answer = ap.state;
                            
                        }
                    }
                if (answer == true)
                {
                    if (sql.ErrorLoad() == true) sql.ErrorList();

                    Console.WriteLine("Запрос выполнен...");
                }
                else
                {
                    sql.SQLInsert(date, studID, ip);
                }
                    response.Close();

                    
                }
                catch (Exception e)
                {
                    Console.WriteLine("SQLClose " + e.Message);
                }
        }




    }
    
}
