using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUDS报文模型
{
    public class ScrapeInfo
    {
        public string Type { get; set; }
        public string Time { get; set; }
        public double MaxSpeed { get; set; }
        public double MinSpeed { get; set; }
        public int AxleCnt { get; set; }
        public string Status { get; set; }

        public List<ScrapeWheel> Wheels { get; set; } 
        public ScrapeInfo() 
        {
            Wheels = new List<ScrapeWheel>();
        }

        public static ScrapeInfo Create(FileInfo file)
        {
            if(file is null || !File.Exists(file.FullName))
            {
                throw new ArgumentNullException(nameof(file));
            }
            ScrapeInfo si;
            using (StreamReader sr = File.OpenText(file.FullName))
            {
                string content = sr.ReadToEnd();
                if(string.IsNullOrEmpty(content))
                {
                    throw new ArgumentNullException(nameof(file));
                }
                si = JsonConvert.DeserializeObject<ScrapeInfo>(content);
            }
            return si;
        }

        public static ScrapeInfo CreateDefault(int axleCount, DateTime time)
        {
            int vehicleCount = (int)Math.Ceiling(axleCount / 4.0);
            ScrapeInfo scrapeInfo = new ScrapeInfo();
            scrapeInfo.AxleCnt = axleCount;
            scrapeInfo.Time = time.ToString("yyyyMMddHHmmss");
            for (int i = 0; i < axleCount * 2; i++)
            {
                scrapeInfo.Wheels.Add(new ScrapeWheel());
            }
            return scrapeInfo;
        }
    }

    public class ScrapeWheel
    {
        public ScrapeWheel() { }
        public string WheelNo { get; set; }
        public string Status { get; set; }
        public double Round { get; set; }
        public double Speed { get; set; }
        public double MaxBruiseDeep { get; set; }
        public double MaxBruiseLen { get; set; }
        public double MaxDia { get; set; }
        public double MinDia { get; set; }
        public int DataLen { get; set; }
        public List<double> DataArr { get; set; }

    }
}
