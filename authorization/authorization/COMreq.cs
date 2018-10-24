using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace authorization
{
    public class COMreq
    {
        int response;
        
        byte[] resp1 = new byte[6];

        public void Send(byte[] req, SerialPort sp)
        {
            string result = string.Join(" ", req.Select(d => d.ToString("X2")));
            Console.WriteLine(result);
            // отправка сообщения(байтовая команда, начало команды, конец команды)
            sp.Write(req, 0, req.Length);

        }

        public string CardID(SerialPort sp)
        {
            int BuffSize = 0;
            BuffSize = sp.BytesToRead;
            byte[] resp = new byte[BuffSize];
            string key = String.Empty;
            
            try
            {   
                for (int i = 0; i < BuffSize; i++)
                {
                    resp[i] = Convert.ToByte(sp.ReadByte());
                }

                string hexString = "";
                for (int i = 0; i < resp.Length; i++)
                {

                    hexString += resp[i].ToString("X2") + " ";


                }
                Console.WriteLine(hexString);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exeption:" + e);
            }                  
            try
            {
                if (resp[0] == 0x15)
                {
                    Console.WriteLine("Ошибка запроса");
                }

                if (resp[0] == 0x02 && resp[1] == 0x41 && resp[2] == 0x01 &&
                    resp[3] != 0x00 && resp[6] != 0x00 && resp[4] != 0x00
                    && resp[5] != 0x00 && resp[10] == 0x03 && resp[11] == 0x38)
                {
                    for (int i = 0; i < resp.Length; i++)
                    {
                        if (i > 2 && i <= 6)
                        {
                            key += resp[i].ToString("X2") + " ";
                        }
                    }
                    Console.WriteLine("UID  получен: " + key);
                }
                else
                {
                    key = "-1";
                    Console.WriteLine("Карта не обноружена ");
                }
                
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception: " + e.ToString());
            }
            return key;
        }

        public Boolean SomeCard(SerialPort sp)
        {
            
            Boolean card = false ;
            string kay = String.Empty;
            int BuffSize = 0;
            BuffSize = sp.BytesToRead;
            byte[] resp1 = new byte[BuffSize];
            try
            {
                
                for (int i = 0; i < BuffSize; i++)
                { 
                    resp1[i] = Convert.ToByte(sp.ReadByte());
                 }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exeption:" + e);
            }


            string hexString = "";
            for (int i = 0; i < resp1.Length; i++)
            {

                hexString += resp1[i].ToString("X2") + " ";


            }
            Console.WriteLine(hexString);

            if (resp1[0] == 0x02 && resp1[1] == 0x40 && resp1[3] == 0x00 && resp1[4] == 0x03 && resp1[5] == 0x41)
            {
                Console.WriteLine("Карты не обнаружено");
                card= false;
            }

            if (resp1[0] == 0x15)
            {
                Console.WriteLine("Ошибка запроса");
                card= false;
            }

            if (resp1[0] == 0x02 && resp1[1] == 0x40 && resp1[3] == 0x01 && resp1[4] == 0x03 && resp1[5] == 0x40)
            {
                Console.WriteLine("Карта поднесена");

                card= true;
            }
            return card;




        }
    }
}
