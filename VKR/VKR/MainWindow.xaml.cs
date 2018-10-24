using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MiFare;
using MiFare.Classic;
using MiFare.Devices;
using MiFare.PcSc;
using System.ComponentModel;
using Microsoft.Win32;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Data.SQLite;
using System.Globalization;
using System.Security.Cryptography;
using Newtonsoft.Json;
using VKR;
using Newtonsoft.Json.Linq;

namespace MiFareReader.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Boolean answer;
        public Boolean isValuble = false;
        public Boolean canChange = false;
        public Boolean flag = false;
        public Boolean flagForSQL = false;
        public string id;
        public Boolean emptyCard = false;
        public Boolean Readflag = false;
        public string pass;
        public string password;


        Regex nameReg = new Regex(@"^[А-ЯЁ][а-яё]+ [А-ЯЁ][а-яё]+ [А-ЯЁ][а-яё]+$");
        Regex phoneReg = new Regex(@"^[8][9][0-9]{9}$");
        Regex studIDReg = new Regex(@"^[0-9]{6}$");
        Regex groupReg = new Regex(@"^[А-ЯЁ]+-[0-9]{2}-[0-9]{1,2}$");



        private SmartCardReader reader;
        private MiFareCard card;
        public static string fileName;
        public MainWindow()
        {

            InitializeComponent();
            GetDevices();
            //DataContext = new Content();
        }


        #region Connect
        private async void GetDevices()
        {
            try
            {
                reader = await CardReader.FindAsync("ACS ACR1252 1S CL Reader PICC 0");
                if (reader == null)
                {

                    DisplayText(device, " не обноружен");
                    ChangeTextBlockFontColor(device, Colors.Red);

                    return;
                }
                DisplayText(device, " подключен");
                DisplayText(cards, " не обнаружена");
                ChangeTextBlockFontColor(device, Colors.Green);
                ChangeTextBlockFontColor(cards, Colors.Red);

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
            DisplayText(cards, " не обнаружена");
            ChangeTextBlockFontColor(cards, Colors.Red);
            Readflag = false;
            id = String.Empty;

        }

        private async void CardAdded(object sender, CardEventArgs args)
        {
            try
            {

                    DisplayText(cards, " найдена");
                    ChangeTextBlockFontColor(cards, Colors.Green);
                    await HandleCard(args);

            }
            catch (Exception ex)
            {
                PopupMessage("CardAdded Exception: " + ex.Message);
            }
        }

        #endregion

        #region Read card

        private async Task HandleCard(CardEventArgs args)
        {
            try
            {
                card?.Dispose();
                card = args.SmartCard.CreateMiFareCard();

                var localCard = card;
                var cardIdentification = await localCard.GetCardInfo();

                if (cardIdentification.PcscDeviceClass == MiFare.PcSc.DeviceClass.StorageClass
                     && (cardIdentification.PcscCardName == CardName.MifareStandard1K 
                     || cardIdentification.PcscCardName == CardName.MifareStandard4K))
                {
                    string locID = String.Empty;
                    var uid = await localCard.GetUid();
                    for (int i = 0; i < uid.Length; i++)
                    {
                        locID += uid[i].ToString("X2")+" ";
                    }
                    Readflag = true;
                    id = locID;
                    locID = String.Empty;
                    canChange = true;

                    MessageBox.Show(string.Format(id));


                }
            }

            catch (Exception e)
            {
                PopupMessage("HandleCard Exception: " + e.Message);
            }
        }
        #endregion

        #region Invoke

        private void ChangeTextBlockFontColor(TextBlock textBlock, Color color)
        {
            var ignored = this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                textBlock.Foreground = new SolidColorBrush(color);
            }));
        }



        private void DisplayText(TextBlock textBlock, string message)
        {

            var ignored = this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                textBlock.Text = message;
            }));
        }


        public async void PopupMessage(string message)
        {
            var ignored = this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                MessageBox.Show(message);
            }));
        }

        #endregion

        #region Server
        //private async Task PostRequestAsyncImage()
        //{
        //    string userfile = path.Content.ToString(); ;
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:81/2.php");
        //    request.ContentType = "multipart/form-data;";
        //    request.Method = "POST";
        //    request.KeepAlive = true;
        //    request.Credentials = System.Net.CredentialCache.DefaultCredentials;
        //    Stream requestStream = request.GetRequestStream();

        //    string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
        //    string header = string.Format(headerTemplate, "file", userfile, "image/png");
        //    byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
        //    requestStream.Write(headerbytes, 0, headerbytes.Length);

        //    FileStream fileStream = new FileStream(userfile, FileMode.Open, FileAccess.Read);
        //    byte[] buffer = new byte[4096];
        //    int bytesRead = 0;
        //    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
        //    {
        //        requestStream.Write(buffer, 0, bytesRead);
        //    }
        //    fileStream.Close();

        //    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        //    {
        //        MessageBox.Show(response.StatusCode.ToString());
        //    }
        //}

        private async Task PostRequestAsync()
        {
            try
            {
                WebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:80/1.php");
                request.Method = "POST"; 
                var js = new
                {
                    fio = name.Text,
                    phone_number = phone.Text,
                    group = group.Text,
                    id_student = studentID.Text,
                    number_card = id,
                    status = 0
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
                        answer = ap.state;
                        //MessageBox.Show(ap.state.ToString());
                        
                        
                    }
                }
                response.Close();
                MessageBox.Show("Запрос выполнен...");
            }
            catch (Exception e)
            {
                PopupMessage("Server Exception: " + e.Message);
                answer = false;
            }

        }
        #endregion Server

        #region SQL

        private void SQLVerify(string studID)
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
                PopupMessage("SQLQuery " + e.Message);
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
                    while (r.Read())
                    {
                        line = r["studID"].ToString();
                        linePass = r["studPas"].ToString();
                        if (line.Equals(studID) || linePass.Equals(id))
                        {

                            flagForSQL = false;
                            canChange = true;
                            break;
                        }
                        else
                        {
                            flagForSQL = true;
                            
                        }
                    }
                }
                else flagForSQL = true;
               
                r.Close();
                cmd.ExecuteNonQuery();
            }

            catch (SQLiteException e)
            {
                PopupMessage("SQLQuery " + e.Message);
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

        private void SQLWrite(string ID)
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
                PopupMessage("SQLQuery " + e.Message);
            }


            int studID = Convert.ToInt32(ID);

            string sql_command1 = "INSERT INTO students (studID, studPas, statuse)"
                + "VALUES ( " + studID + ", '" + id + "'," + 0 + ");";
            cmd.CommandText = sql_command1;

            try
            {
                cmd.ExecuteNonQuery();
                flagForSQL = false;

                
                 sql_command = "CREATE TABLE IF NOT EXISTS  Events("
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
                   + "VALUES ( 0 , '" + line + "' );";


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

        #region Button
        private void Button_OK(object sender, RoutedEventArgs e)
        {
            string Name = name.Text;
            string Phone = phone.Text;
            string Group = group.Text;
            string StudentID = studentID.Text;

            Match nameMatch = nameReg.Match(Name);
            Match phoneRegMatch = phoneReg.Match(Phone);
            Match studIDRegMatch = studIDReg.Match(StudentID);
            Match groupRegMatch = groupReg.Match(Group);
            if (Name != "" && Phone != ""  && Group != "" && StudentID != "")
            {
                if (nameMatch.Success && phoneRegMatch.Success  &&
                    studIDRegMatch.Success && groupRegMatch.Success)
                {                                     
                    if (Readflag == true)
                    {
                        SQLVerify(StudentID);
                        if (flagForSQL == true)
                        {
                            phone.IsReadOnly = true;
                            name.IsReadOnly = true;
                            studentID.IsReadOnly = true;
                            phone.IsReadOnly = true;

                            name.Background = Brushes.Gray;
                            studentID.Background = Brushes.Gray;
                            group.Background = Brushes.Gray;
                            phone.Background = Brushes.Gray;

                            isValuble = true;
                        }
                        else
                        {
                            MessageBox.Show("Такой студенческий билет уже занят");
                        }
                    }
                    
                }
                else
                {
                    MessageBox.Show("Плоя заполнены не коректно");
                }
            }
            else
            {
                MessageBox.Show("Есть пустые поля");
            }
        }

        private void Button_Cansle(object sender, RoutedEventArgs e)
        {
            if (isValuble == true)
            {
                phone.IsReadOnly = false;
                name.IsReadOnly = false;
                //thName.IsReadOnly = false;
                studentID.IsReadOnly = false;
                group.IsReadOnly = false;

                name.Background = Brushes.White;
                //thName.Background = Brushes.White;
                studentID.Background = Brushes.White;
                group.Background = Brushes.White;
                phone.Background = Brushes.White;


                id = null;

                
                isValuble = false;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (card != null)
            {
                if (isValuble == true)
                {
                    if (canChange == true)
                    {
                        await PostRequestAsync();
                        if (answer == true)
                        {
                            SQLWrite(studentID.Text);

                            name.Text = "";
                            phone.Text = "";
                            group.Text = "";
                            studentID.Text = "";
                            pass = "";
                            canChange = false;
                            phone.IsReadOnly = false;
                            name.IsReadOnly = false;
                            studentID.IsReadOnly = false;
                            group.IsReadOnly = false;
                            name.Background = Brushes.White;
                            studentID.Background = Brushes.White;
                            group.Background = Brushes.White;
                            phone.Background = Brushes.White;
                        }
                        else MessageBox.Show("Проблемы с сервером");

                    }
                    else MessageBox.Show("Поднесите карту снова");

                }
                else MessageBox.Show("Подтвердите ввод");

            }
            else
            {
                MessageBox.Show("Карта не найдена");
            }
        }

        private void Change(object sender, RoutedEventArgs e)
        {
            WindowChange windowChange = new WindowChange();
            windowChange.Show();

        }

        #endregion

        //        #region photo
        //    }
        //    public class Command : ICommand
        //    {
        //        public Command(Action action)
        //        {
        //            this.action = action;
        //        }

        //        Action action;

        //        EventHandler canExecuteChanged;
        //        event EventHandler ICommand.CanExecuteChanged
        //        {
        //            add { canExecuteChanged += value; }
        //            remove { canExecuteChanged -= value; }
        //        }

        //        public bool CanExecute(object parameter)
        //        {
        //            return true;
        //        }

        //        public void Execute(object parameter)
        //        {
        //            action();
        //        }
        //    }


        //    public class Content : INotifyPropertyChanged
        //    {

        //        public Content()
        //        {
        //            // Инициализация команды
        //            openFileDialogCommand = new Command(ExecuteOpenFileDialog);
        //            // Инициализация OpenFileDialog
        //            openFileDialog = new OpenFileDialog()
        //            {
        //                Multiselect = true,
        //                Filter = "Image files (*.BMP, *.JPG, *.GIF, *.TIF, *.PNG, *.ICO, *.EMF, *.WMF)|*.bmp;*.jpg;*.gif; *.tif; *.png; *.ico; *.emf; *.wmf"
        //            };
        //        }
        //        readonly OpenFileDialog openFileDialog;
        //        public ImageSource Image { get; private set; }
        //        public string path { get; private set; }
        //        readonly ICommand openFileDialogCommand;
        //        public ICommand OpenFileDialogCommand { get { return openFileDialogCommand; } }


        //        void ExecuteOpenFileDialog()
        //        {
        //            if (openFileDialog.ShowDialog() == true)
        //            {
        //                using (var stream = new FileStream(openFileDialog.FileName, FileMode.Open))
        //                {
        //                    path = openFileDialog.FileName;
        //                    Image = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        //                    RaisePropertyChanged("Image");
        //                    RaisePropertyChanged("path");
        //                }

        //            }


        //        }

        //        public event PropertyChangedEventHandler PropertyChanged;
        //        void RaisePropertyChanged(string propertyName)
        //        {
        //            if (PropertyChanged != null)
        //                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        //        }


        //#endregion
        //    
    }
}
