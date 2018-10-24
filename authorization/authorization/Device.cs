using MiFare;
using MiFare.Classic;
using MiFare.Devices;
using MiFare.PcSc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace authorization
{
  public  class Device
    {
        SQL sql = new SQL();
        Server server = new Server();
        private SmartCardReader reader;
        private MiFareCard card;
        public string id;
        private string sqlReq = String.Empty;
        private string ip = "172.16.56.16";
        public async void GetDevices()
        {


            try
            {
                reader = await CardReader.FindAsync("ACS ACR1252 1S CL Reader PICC 0");
                if (reader == null)
                {


                    Console.WriteLine("не найден");
                    return;
                }


                reader.CardAdded += CardAdded;
                reader.CardRemoved += CardRemoved;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }
        public void CardRemoved(object sender, EventArgs e)
        {


            card?.Dispose();
            card = null;


        }

        public async void CardAdded(object sender, CardEventArgs args)
        {
            try
            {

                //if (flag == true)
                //{

                await HandleCard(args);
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("CardAdded Exception: " + ex.Message);
            }
        }

      public string IDCard()
        {
          

            return id;
        }

        public async Task HandleCard(CardEventArgs args)
        {
            //server.CallPostRequestChange("1","1","1");
            //server.CallPostRequestChange("2", "2", "2");
            //server.CallPostRequestChange("0", "0", "0");
            try
            {
                card?.Dispose();
                card = args.SmartCard.CreateMiFareCard();

                var localCard = card;
                var cardIdentification = await localCard.GetCardInfo();

                if (cardIdentification.PcscDeviceClass == MiFare.PcSc.DeviceClass.StorageClass
                     && (cardIdentification.PcscCardName == CardName.MifareStandard1K || cardIdentification.PcscCardName == CardName.MifareStandard4K))
                {
                    string locID = String.Empty;
                    var uid = await localCard.GetUid();
                    for (int i = 0; i < uid.Length; i++)
                    {
                        locID += uid[i].ToString();
                    }

                    id = locID;
                    locID = String.Empty;
                    sqlReq = sql.SQLVerify(id);
                    if (sqlReq.Equals("-1")) { Console.WriteLine("Карта не найдена в базе"); }
                    else
                    {
                        string time = DateTime.Now.ToString();
                        server.CallPostRequestRecord(time, sqlReq, ip);
                    }

                    Console.WriteLine(string.Format(id));


                }
            }

            catch (Exception e)
            {
                Console.WriteLine("HandleCard Exception: " + e.Message);
            }
        }

    }
}
