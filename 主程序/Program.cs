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
using Newtonsoft.Json;

namespace 主程序
{
    class Program
    {
        public static log4net.ILog log = log4net.LogManager.GetLogger("Main");
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            //主程序
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
            //生成几何尺寸报文的临时代码
            {
                //DimensionInfo di;
                //string directoryPath = @"C:\Users\matian314\Desktop\报文\32";
                //string[] files = Directory.GetFiles(directoryPath);
                //foreach (var path in files)
                //{
                //    FileInfo file = new FileInfo(path);
                //    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                //    using (BinaryReader br = new BinaryReader(fs))
                //    {
                //        di = DimensionInfo.Analyze(br.ReadBytes((int)fs.Length));

                //    }
                //    string destPath = Path.Combine(@"G:\几何尺寸报文\32轴", file.Name + "Dimension.txt");
                //    string content = JsonConvert.SerializeObject(di);
                //    destPath.WriteFile(content);
                //    Console.WriteLine("完成");
                //    Console.ReadKey();
                //}

            }
        }
    }

}
