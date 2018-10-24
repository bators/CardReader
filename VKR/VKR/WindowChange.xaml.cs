using MiFare;
using MiFare.Classic;
using MiFare.Devices;
using MiFare.PcSc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace VKR
{
    /// <summary>
    /// Логика взаимодействия для WindowChange.xaml
    /// </summary>
    public partial class WindowChange : Window
    {
        string id;
        string StudentID;
        string oldPass;

        
        Regex studIDReg = new Regex(@"^[0-9]{6}$");

        Boolean answer;
        Boolean changeFlag = false;
        Boolean flagSqlWrite = false;
        Boolean flagForReWrite = false;
        Boolean changeOrDelite ;
        Boolean emptyBase;
        

        private SmartCardReader reader;
        private MiFareCard card;
        public WindowChange()
        {
            GetDevices();
            InitializeComponent();
        }

#region devise
        private async void GetDevices()
        {
            

            try
            {
                reader = await CardReader.FindAsync("ACS ACR1252 1S CL Reader PICC 0");
                if (reader == null)
                {
                    return;
                }
                

                reader.CardAdded += CardAdded;
                reader.CardRemoved += CardRemoved;
            }
            catch (Exception e)
            {
                PopupMessage("Exception: " + e.Message);
            }
        }


        private void CardRemoved(object sender, EventArgs e)
        {
            card?.Dispose();
            card = null;
            changeFlag = false;
            id = String.Empty;
        }

        private async void CardAdded(object sender, CardEventArgs args)
        {
            try
            {
                
                    
                    await HandleCard(args);

                    
              
            }
            catch (Exception ex)
            {
                PopupMessage("CardAdded Exception: " + ex.Message);
            }
        }
#endregion

        #region ReWrite
        private async Task HandleCard(CardEventArgs args)
        {
            try
            {
                card?.Dispose();
                card = args.SmartCard.CreateMiFareCard();
                
                var localCard = card;
                var cardIdentification = await localCard.GetCardInfo();
                if (cardIdentification.PcscDeviceClass == MiFare.PcSc.DeviceClass.StorageClass
                     && (cardIdentification.PcscCardName == CardName.MifareStandard1K || cardIdentification.PcscCardName == CardName.MifareStandard4K))
                {
                    var uid = await localCard.GetUid();
                    string localID= String.Empty; 
                    for (int i = 0; i < uid.Length; i++)
                    {
                        localID += uid[i].ToString("X2") + " ";
                    }
                    id = localID;
                    //MessageBox.Show(string.Format(id));
                    localID= String.Empty;
                    changeFlag = true;
                   

                 }

                    
                
            }
            catch (Exception e)
            {
                PopupMessage("HandleCard Exception: " + e.Message);
            }
        }
#endregion

        #region Button

        private async void Change(object sender, RoutedEventArgs e)
        {
            string StudentID = studentID.Text;
            
            Match studIDRegMatch = studIDReg.Match(StudentID);
         
            if ( StudentID != "")
            {
                if (studIDRegMatch.Success)
                {
                    if (id != String.Empty)
                    {
                        if (changeFlag == true)
                        {
                            SQLVerify(StudentID);
                            if (flagForReWrite == true)
                            {
                                changeOrDelite = true;
                                
                                Boolean canChg = CanChancge();
                                
                                if (canChg == true)
                                {
                                    await PostRequestAsync();
                                    if (answer == true)
                                    {
                                        SQLReWWrite(StudentID);
                                    }
                                    else MessageBox.Show("Проблемы с сервером");
                                }
                                else MessageBox.Show("Старый и новый пароль совпадают");
                                 
                            }
                            else
                            {

                            }
                        }
                    }
                        
                   
                    
                }
                else
                {
                    MessageBox.Show("Поле заполнено не коректно");
                }
            }
            else
            {
                MessageBox.Show("Поле не заполнено");
            }
        }

        private async void Delite(object sender, RoutedEventArgs e)
        {
            string StudentID = studentID.Text;

            Match studIDRegMatch = studIDReg.Match(StudentID);

            if (StudentID != "")
            {
                if (studIDRegMatch.Success)
                {

                    if (id != String.Empty)
                    {
                        if (changeFlag == true)
                        {
                            SQLVerify(StudentID);
                            if (flagForReWrite == true)
                            {
                                changeOrDelite = false;
                                
                                    
                                if (emptyBase == false)
                                {
                                    await PostRequestAsync();
                                    if (answer == true)
                                    {
                                        SQLDelite(string.Format(StudentID));
                                    }
                                    else MessageBox.Show("Проблемы с сервером");
                                }
                                else MessageBox.Show("Нечего удолять");
                                    
                               

                            }
                            else
                            {

                            }
                        }
                    }
                    
                    
                }
                else
                {
                    MessageBox.Show("Поле заполнено не коректно");
                }
            }
            else
            {
                MessageBox.Show("Поле не заполнено");
            }
        }
        #endregion

        #region SQL
        private void SQLVerify(string Id)
        {
            

            SQLiteConnection conn = new SQLiteConnection("Data Source=Students.db;Version=3;");
            try
            {
                conn.Open();
            }
            catch (SQLiteException e)
            {
                PopupMessage("SQLOpen" + e.Message);
            }
            SQLiteCommand cmd = conn.CreateCommand();

            int studID =Convert.ToInt32(Id);
            

            cmd.CommandText = "SELECT * FROM students ";
            try
            {
                SQLiteDataReader r = cmd.ExecuteReader();

                string line = String.Empty;
                var caunt = r.StepCount;
                if (caunt != 0)
                {
                    emptyBase = false;
                    while (r.Read())
                    {
                        line = r["studID"].ToString();

                        if (line.Equals(studID.ToString()))
                        {




                            oldPass = r["studPas"].ToString();
                            flagForReWrite = true;
                            break;


                        }
                        else flagForReWrite = false;

                    }
                }
                else emptyBase = true;

                if (flagForReWrite == false) MessageBox.Show("Такого студенческого нет");
                r.Close();

                cmd.ExecuteNonQuery();

            }
            catch (SQLiteException e)
            {
                PopupMessage("SQLQuery " + e.Message);
            }

        }



        private Boolean CanChancge()
        {
            
            
            if (id.Equals(oldPass)) { flagSqlWrite = false; }
            else
            {
                flagSqlWrite = true;

            }

            return flagSqlWrite;
        }



        private  void  SQLReWWrite(string studID)
        {
            

            
            
            SQLiteConnection conn = new SQLiteConnection("Data Source=Students.db;Version=3;");
            try
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();


                if (flagForReWrite == true )
                {
                    int ID = Convert.ToInt32(studID);
                    string sql_command1 = "UPDATE students SET studPas='"+ id+ "', statuse="+ 1 +" WHERE studID= " + ID+ ";";

                    cmd.CommandText = sql_command1;

                    try
                    {
                        cmd.ExecuteNonQuery();
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
                        string line = "№ студенческого: " + ID + ", номер карты: " + id;
                        sql_command = "INSERT INTO Events (log_id, text)"
                           + "VALUES ( 1 , '" + line + "' );";


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
                    catch (SQLiteException e)
                    {
                        PopupMessage("SQLQuery " + e.Message);
                    }
                }
            }
            catch (SQLiteException e)
            {
                PopupMessage("SQLOpen" + e.Message);
            }
            
            try
            {
                conn.Close();
            }

            catch (SQLiteException e)
            {
                PopupMessage("SQLClose " + e.Message);
            }
        }





        private void SQLDelite(string studID)
        {
          
            SQLiteConnection conn = new SQLiteConnection("Data Source=Students.db;Version=3;");
            try
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();


                if (flagForReWrite == true )
                {
                    int ID = Convert.ToInt32(studID);

                    string sql_command1 = "DELETE FROM students WHERE studID=" + ID + ";";

                    cmd.CommandText = sql_command1;

                    try
                    {
                        cmd.ExecuteNonQuery();
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
                        string line = "№ студенческого: " + ID + ", номер карты: " + id;
                        sql_command = "INSERT INTO Events (log_id, text)"
                           + "VALUES ( 2 , '" + line + "' );";


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
                    catch (SQLiteException e)
                    {
                        PopupMessage("SQLQuery " + e.Message);
                    }
                }
            }
            catch (SQLiteException e)
            {
                PopupMessage("SQLOpen" + e.Message);
            }

            try
            {
                conn.Close();
            }

            catch (SQLiteException e)
            {
                PopupMessage("SQLClose " + e.Message);
            }
        }
        #endregion
#region Server
        private async Task PostRequestAsync()
        {

            try
            {
                WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:80/2.php");
                request.Method = "POST"; // для отправки используется метод Post
                                         // данные для отправки
                int statuse;
                if (changeOrDelite == true) statuse = 1;
                else statuse = 2;
                var js = new
                {
                    id_student = studentID.Text,
                    number_card = id,
                    status = statuse
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

                WebResponse response = await request.GetResponseAsync();
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var Json = reader.ReadToEnd();
                        dynamic ap = JObject.Parse(Json);
                        answer = ap.state;
                    }
                }
                response.Close();
                changeFlag = false;
                MessageBox.Show("Запрос выполнен...");
            }
            catch (SQLiteException e)
            {
                PopupMessage("SQLClose " + e.Message);
            }

        }
#endregion

        #region Invoke
        public async void PopupMessage(string message)
        {
            var ignored = this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                MessageBox.Show(message);
            }));
        }
        #endregion

        
    }
}
