using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelper;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using TUDS报文模型;
using System.Net.Sockets;
using System.Net;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using ExceptionHelper;

namespace TUDS报文接收
{
    public class MessageReceiver
    {
        public log4net.ILog log = log4net.LogManager.GetLogger("MessageReceiver");
        public string NetMQReceiveString(string connectionString)
        {
            using (SubscriberSocket subSocket = new SubscriberSocket())
            {
                subSocket.Connect(connectionString);

                subSocket.Subscribe(String.Empty);
                string result = subSocket.ReceiveFrameString();

                return result;
            }
        }

        public byte[] NetMQReceiveBytes(string connectionString)
        {
            using (SubscriberSocket subSocket = new SubscriberSocket())
            {
                subSocket.Connect(connectionString);

                subSocket.Subscribe(String.Empty);
                byte[] result = subSocket.ReceiveFrameBytes();

                return result;
            }
        }

        //public byte[] TcpReceiveBytes()
        ////public byte[] TcpReceiveBytes(string Ip, int port)
        //{
        //    byte[] recvData = new byte[1024 * 1000];
        //    using (TcpClient client = new TcpClient(AddressFamily.InterNetwork))
        //    {
        //        IPAddress DimensionHostIpAddress = IPAddress.Parse(Config.DimensionIp);
        //        //IPAddress DimensionHostIpAddress = IPAddress.Parse(ip);
        //        client.Connect(DimensionHostIpAddress, Config.DimensionPort);
        //        //client.Connect(DimensionHostIpAddress, port);

        //        using (NetworkStream clientStream = client.GetStream())
        //        {
        //            clientStream.Read(recvData, 0, recvData.Length);
        //            return recvData;
        //        }
        //    }
        //}
        public void ReceiveScrapeMessage(string message)
        {
            JObject JMessage = (JObject)JsonConvert.DeserializeObject(message);
            if (JMessage["Type"].ToString().Trim() == "SysCheck")
            {
                //do nothing
            }
            else if (JMessage["Type"].ToString().Trim() == "TrainRecord")
            {
                string Time = JMessage["Time"].ToString();
                string path = Path.Combine(Config.DirectoryPath, Time + "Scrape.txt");
                path.WriteFile(message);
            }
        }

        public void ReceiveAeiMessage(string message)
        {
            string Time = message.Split(',')[2].Replace(" ", "").Replace(":", "");
            string path = Path.Combine(Config.DirectoryPath, Time + "AEI.txt");
            path.WriteFile(message);
        }

        public void ReceiveInspectionMessage(string message)
        {
            JObject JMessage = (JObject)JsonConvert.DeserializeObject(message);
            string Time = JMessage["Time"].ToString();
            string path = Path.Combine(Config.DirectoryPath, Time + "Inspection.txt");
            path.WriteFile(message);
        }

        public void ReceiveDimensionMessage(string value)
        {
            byte[] message = Encoding.Default.GetBytes(value);
            DimensionInfo dimension = DimensionInfo.Analyze(message);
            string Time = dimension.BaseInfo.DetectionTime;
            string path = Path.Combine(Config.DirectoryPath, Time + "Dimension.txt");
            path.WriteFile(JsonConvert.SerializeObject(dimension));
        }

        public void Run()
        {
            Action aeiAction = () =>
            {
                while (true)
                {
                    try
                    {
                        string aei = NetMQReceiveString(Config.AeiAddress);
                        ReceiveAeiMessage(aei);
                    }
                    catch (Exception e)
                    {
                        log.Error(e.ToRecord());
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                    }

                }
            };
            Action scrapeAction = () =>
            {
                while (true)
                {
                    try
                    {
                        string scrape = NetMQReceiveString(Config.ScrapeAddress);
                        ReceiveScrapeMessage(scrape);
                    }
                    catch (Exception e)
                    {
                        log.Error(e.ToRecord());
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                    }
                }
            };
            Action inspectionAction = () =>
            {
                while (true)
                {
                    try
                    {
                        string inspection = NetMQReceiveString(Config.InspectionAddress);
                        ReceiveInspectionMessage(inspection);
                    }
                    catch (Exception e)
                    {
                        log.Error(e.ToRecord());
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                    }
                }
            };
            Action dimensionAction = () =>
            {
                while (true)
                {
                    try
                    {
                        string dimension = NetMQReceiveString(Config.DimensionAddress);
                        ReceiveDimensionMessage(dimension);
                    }
                    catch (Exception e)
                    {
                        log.Error(e.ToRecord());
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                    }
                }
            };

            TaskFactory factory = new TaskFactory();
            factory.StartNew(aeiAction);
            factory.StartNew(scrapeAction);
            factory.StartNew(inspectionAction);
            factory.StartNew(dimensionAction);
        }
    }
}
