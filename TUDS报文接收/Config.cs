using System;
using System.Configuration;
using System.IO;

namespace TUDS报文接收
{
    public static class Config
    {
        public static log4net.ILog log = log4net.LogManager.GetLogger("Config");
        public static string InspectionAddress { get; }
        public static string ScrapeAddress { get; }
        public static string AeiAddress { get; }
        public static string DimensionAddress { get; }
        public static string DirectoryPath{ get;}
        public static string BackUpPath { get; }
        static Config()
        {
            try
            {
                InspectionAddress = ConfigurationManager.AppSettings["探伤网络地址"];
                ScrapeAddress = ConfigurationManager.AppSettings["擦伤网络地址"];
                AeiAddress = ConfigurationManager.AppSettings["AEI网络地址"];
                DimensionAddress = ConfigurationManager.AppSettings["几何尺寸网络地址"];
                DirectoryPath = ConfigurationManager.AppSettings["报文合成"];
                BackUpPath = ConfigurationManager.AppSettings["报文备份"];
                //create if directory doesn't exist
                DirectoryPath = Directory.CreateDirectory(DirectoryPath).FullName;
                BackUpPath = Directory.CreateDirectory(BackUpPath).FullName;
                if(DirectoryPath == BackUpPath)
                {
                    throw new ArgumentException("备份路径不能和合成路径相同");
                }
            }
            catch (Exception e)
            {
                throw new ConfigurationErrorsException("配置文件初始化错误", e);
            }
        }
    }
}
