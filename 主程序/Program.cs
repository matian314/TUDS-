using FileHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUDS报文接收;
using TUDS报文模型;
using TUDS入库;
using TUDS匹配策略;
using ExceptionHelper;
using System.Diagnostics;

namespace 主程序
{
    class Program
    {
        public static log4net.ILog log = log4net.LogManager.GetLogger("Main");
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            {
                while (true)
                {
                    try
                    {
                        Action act = () =>
                        {
                            Strategy.Run();
                        };
                        Task.Run(act);
                        new MessageReceiver().Run();
                        while (true) { }
                    }
                    catch (Exception e)
                    {
                        log.Error(e.ToRecord());
                    }
                }
            }
        }
        public static void ReceiveAeiMessage(string message)
        {
            string Time = message.Split(',')[2].Replace(" ", "").Replace(":", "");
            string path = Path.Combine(Config.DirectoryPath, Time + "Scrape.txt");
            path.WriteFile(message);
        }
    }

}
