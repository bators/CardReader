using MiFare;
using MiFare.Classic;
using MiFare.Devices;
using MiFare.PcSc;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace authorization
{
    class Program
    {
        
        public string statuse = String.Empty;
        static void Main(string[] args)
        {

           string ip = "168.192.0.48";
            #region
            COMreq Request = new COMreq();
            Device device = new Device();
            SQL sql = new SQL();
            Server server = new Server();


           

            Boolean anyCard = false;
            Boolean hasTable;
            string id = String.Empty;
            string UID = String.Empty;

            string sqlReq = String.Empty;



            //инициализация
            SerialPort port;
            port = new SerialPort();
            
            //комаанды
            byte[] readCrad = { 0x01, 0x41, 0x00, 0x00, 0x04, 0x44 };
            byte[] chekCard = { 0x01, 0x40, 0x00, 0x00, 0x04, 0x45 };

            //выбор порта
            string[] ports = SerialPort.GetPortNames();
            #region
            for (int i = 0; i < ports.Length; i++)
            {

                Console.WriteLine("[" + i.ToString() + "] " + ports[i].ToString());
            }

            int num = 1;
           
          //настройки и открытия порта
            try
            {

                port.PortName = ports[num];
                port.BaudRate = 14400;
                port.DataBits = 8;
                port.Parity = System.IO.Ports.Parity.None;
                port.StopBits = System.IO.Ports.StopBits.One;
                port.ReadTimeout = 10000;
                port.WriteTimeout = 10000;
                port.Open();

            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: невозможно открыть порт:" + e.ToString());

                return;
            }

            Request.Send(readCrad, port);            
            Thread.Sleep(1500);
         
            while (true)
            {                
                hasTable = sql.FiersLoad();
                if (hasTable == true)
                {
                    Request.Send(readCrad, port);
                    UID = Request.CardID(port);
                    if (UID.Equals("-1")) { }
                    else
                    {
                        if (UID.Equals(id)) { }
                        else
                        {
                            id = UID;
                            sqlReq = sql.SQLVerify(UID);
                            if (sqlReq.Equals("-1")) { }
                            else
                            {
                                string time = Convert.ToString((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
                                server.CallPostRequestInsertTable(time, sqlReq, ip);
                                Thread.Sleep(2000);
                            }
                        }
                    }
                    Thread.Sleep(2000);
                    Console.WriteLine();
                }
                else
                {
                    server.CallPostRequestInsertTable("1", "1", "1");
                    Thread.Sleep(5000);
                }
            }

            #endregion



            



           
        }

        
        #endregion



    }

}