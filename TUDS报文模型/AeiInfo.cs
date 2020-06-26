using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;

namespace TUDS报文模型
{
    public class AeiInfo
    {
        public DateTime Time { get; set; }
        public int VehicleCount { get; set; }
        public int AxleCount { get; private set; }
        public List<AeiVehicle> Vehicles { get; set; }

        public AeiInfo()
        {
            Vehicles = new List<AeiVehicle>();
        }

        public static AeiInfo Create(FileInfo file)
        {
            AeiInfo aei = new AeiInfo();
            using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string[] line = sr.ReadLine().Split(',');
                    string strTime = line[2];
                    aei.Time = DateTime.ParseExact(strTime, "yyyyMMdd HH:mm:ss", null);
                    aei.VehicleCount = int.Parse(line[5]);
                    aei.AxleCount = int.Parse(line[6]);
                    for (int i = 0; i < aei.VehicleCount; i++)
                    {
                        if (!sr.EndOfStream)
                        {
                            aei.Vehicles.Add(new AeiVehicle(sr.ReadLine()));
                        }
                    }
                }
            }
            return aei;
        }

        public static AeiInfo CreateDefault(int AxleCount, DateTime time)
        {
            AeiInfo aei = new AeiInfo();
            aei.Time = time;
            aei.VehicleCount = (int)Math.Ceiling(AxleCount / 4.0);
            for(int i = 0;i <aei.VehicleCount;i++)
            {
                aei.Vehicles.Add(new AeiVehicle(i));
            }
            return aei;
        }

        public class AeiVehicle
        {
            public AeiVehicle(string value)
            {
                string[] v = value.Split(',');
                Order = Int32.Parse(v[1]);
                Model = v[3];
                CarNumber = Int32.Parse(v[4]).ToString();
                EndPosition = v[5];
                AxleCount = Int32.Parse(v[7]);
            }
            public AeiVehicle(int i)
            {
                Order = i + 1;
                CarNumber = "";
                Model = "";
                EndPosition = "";
                AxleCount = 4;
            }
            public int Order { get; set; }
            public string CarNumber { get; set; }
            public string Model { get; set; }
            public string EndPosition { get; set; }

            public int AxleCount;
        }
    }
}
