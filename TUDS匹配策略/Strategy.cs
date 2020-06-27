using ExceptionHelper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TUDS入库;
using TUDS报文模型;

namespace TUDS匹配策略
{
    public static class Strategy
    {
        public static log4net.ILog log = log4net.LogManager.GetLogger("Strategy");
        public static string DirectoryPath { get; }
        public static string BackUpPath { get; }
        public static int MaxCheckTime { get; }
        public static FileInfo ScrapeFile { get; set; }
        public static FileInfo AeiFile { get; set; }

        public static FileInfo DimensionFile { get; set; }

        public static FileInfo InspectionFile { get; set; }


        static Strategy()
        {
            DirectoryPath = ConfigurationManager.AppSettings["报文合成"];
            BackUpPath = ConfigurationManager.AppSettings["报文备份"];
            //create if directory doesn't exist
            DirectoryPath = Directory.CreateDirectory(DirectoryPath).FullName;
            BackUpPath = Directory.CreateDirectory(BackUpPath).FullName;
            if (DirectoryPath == BackUpPath)
            {
                throw new ArgumentException("备份路径不能和合成路径相同");
            }
            MaxCheckTime = Int32.Parse(ConfigurationManager.AppSettings["报文合成超时时间"]) / 10;
        }
        public static int UpdateFile()
        {
            ScrapeFile = GetLatestFile("*Scrape.txt");
            AeiFile = GetLatestFile("*AEI.txt");
            DimensionFile = GetLatestFile("*Dimension.txt");
            InspectionFile = GetLatestFile("*Inspection.txt");
            int count = 0;
            if (ScrapeFile != null)
            {
                count++;
            }
            if (InspectionFile != null)
            {
                count++;
            }
            if (DimensionFile != null)
            {
                count++;
            }
            if (AeiFile != null)
            {
                count++;
            }
            return count;
        }

        private static FileInfo GetLatestFile(string flag)
        {
            FileInfo file;
            DirectoryInfo dir = new DirectoryInfo(DirectoryPath);
            var scrapes = dir.GetFiles(flag).OrderByDescending(f => f.CreationTime).ToArray();
            if (scrapes.Length == 0)
            {
                file = null;
            }
            else if (scrapes.Length == 1)
            {
                file = scrapes[0];
            }
            else
            {
                file = scrapes[0];
                for (int i = 1; i < scrapes.Length; i++)
                {
                    File.Copy(scrapes[i].FullName, Path.Combine(BackUpPath, scrapes[i].Name));
                    File.Delete(scrapes[i].FullName);
                }
            }
            return file;
        }

        public static void Run()
        {
            while (true)
            {
                try
                {

                    int fileNumber = UpdateFile();
                    if (fileNumber == 0)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                        continue;
                    }
                    if (fileNumber == 4)
                    {
                        Load();
                    }
                    else
                    {
                        for (int i = 0; i < MaxCheckTime; i++)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(10));
                            int counter = UpdateFile();
                            if (counter == 4)
                            {//正常入库
                                Load();
                                break;
                            }
                        }
                        if (UpdateFile() != 0)
                        {
                            //数据没到齐
                            log.Warn("缺少报文,执行入库");
                            Load();
                        }

                    }
                }
                catch (Exception e)
                {
                    log.Error(e.ToRecord());
                }
            }
        }
        public static void Load()
        {
            int axleCount;
            DateTime time;
            //找出该列车的探测时间和轴数
            //采用数据的优先级为 AEI -> 探伤 -> 几何尺寸 -> 擦伤
            if (AeiFile != null)
            {
                var a = AeiInfo.Create(AeiFile);
                axleCount = a.AxleCount;
                time = a.Time;
            }
            else if (InspectionFile != null)
            {
                var ins = InspectionInfo.Create(InspectionFile);
                axleCount = ins.Formations.Count * 32;
                time = DateTime.ParseExact(ins.Time, "yyyyMMddHHmmss", null);
            }
            else if (DimensionFile != null)
            {
                var dim = DimensionInfo.Create(DimensionFile);
                axleCount = dim.BaseInfo.AxleCount;
                time = DateTime.ParseExact(dim.BaseInfo.DetectionTime, "yyyyMMddHHmmss", null);
            }
            else if (ScrapeFile != null)
            {
                var scr = ScrapeInfo.Create(ScrapeFile);
                axleCount = scr.AxleCnt;
                time = DateTime.ParseExact(scr.Time, "yyyyMMddHHmmss", null);
            }
            else
            {
                throw new Exception("AEI,探伤，几何尺寸，擦伤报文均不存在，无法入库");
            }
            if (axleCount >= 22 && axleCount <= 42)
            {
                axleCount = 32;
            }
            else if (axleCount > 42 && axleCount <= 74)
            {
                axleCount = 64;
            }
            ScrapeInfo scrape;
            DimensionInfo dimension;
            AeiInfo aei;
            InspectionInfo inspection;
            if (ScrapeFile is null)
            {
                scrape = ScrapeInfo.CreateDefault(axleCount, time);
            }
            else
            {
                scrape = ScrapeInfo.Create(ScrapeFile);
            }
            if (AeiFile is null)
            {
                aei = AeiInfo.CreateDefault(axleCount, time);
            }
            else
            {
                aei = AeiInfo.Create(AeiFile);
            }
            if (DimensionFile is null)
            {
                dimension = DimensionInfo.CreateDefault(axleCount, time);
            }
            else
            {
                dimension = DimensionInfo.Create(DimensionFile);
            }
            if (InspectionFile is null)
            {
                inspection = InspectionInfo.CreateDefault(axleCount, time);
            }
            else
            {
                inspection = InspectionInfo.Create(InspectionFile);
            }
            var loader = new Uploader(scrape, dimension, inspection, aei);
            loader.Insert();


            Clear();
        }
        /// <summary>
        /// 清理工作
        /// </summary>
        private static void Clear()
        {
            string newPath = Path.Combine(BackUpPath, DateTime.Now.ToString("yyyyMMddHHmmss"));
            Directory.CreateDirectory(newPath);
            if (ScrapeFile != null)
            {
                File.Move(ScrapeFile.FullName, Path.Combine(newPath, ScrapeFile.Name));
            }
            if (DimensionFile != null)
            {
                File.Move(DimensionFile.FullName, Path.Combine(newPath, DimensionFile.Name));
            }
            if (AeiFile != null)
            {
                File.Move(AeiFile.FullName, Path.Combine(newPath, AeiFile.Name));
            }
            if (InspectionFile != null)
            {
                File.Move(InspectionFile.FullName, Path.Combine(newPath, InspectionFile.Name));
            }

            ScrapeFile = null;
            DimensionFile = null;
            AeiFile = null;
            InspectionFile = null;
        }
    }
}
