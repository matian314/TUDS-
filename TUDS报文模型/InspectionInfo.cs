using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUDS报文模型
{
    /// <summary>
    /// 擦伤报文
    /// </summary>
    public class InspectionInfo
    {
        /// <summary>
        /// 消息类型,过车数据为TrainRecord,查询数据为LastTrain
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 过车时间，14位时间字符串，格式为yyyyMMddHHmmss
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// 厂家编号,示例:00001
        /// </summary>
        public string Facotry { get; set; }
        /// <summary>
        /// 设备名称，如
        /// </summary>
        public string Device { get; set; }
        public List<Formation> Formations { get; set; }
        public List<InspectionVehicle> Vehicles { get; set; }

        public List<InspectionWheel> Wheels { get; set; }
        public List<Channel> Channels { get; set; }
        public List<InspectionData> DataList { get; set; }
        public List<Defect> Defects { get; set; }

        public InspectionInfo()
        {
            Formations = new List<Formation>();
            Vehicles = new List<InspectionVehicle>();
            Wheels = new List<InspectionWheel>();
            Channels = new List<Channel>();
            DataList = new List<InspectionData>();
            Defects = new List<Defect>();
        }

        public static InspectionInfo Create(FileInfo file)
        {
            if(file is null || !File.Exists(file.FullName))
            {
                throw new ArgumentNullException(nameof(file));
            } 
            using(StreamReader sr = File.OpenText(file.FullName))
            {
                string json = sr.ReadToEnd();
                InspectionInfo result = JsonConvert.DeserializeObject<InspectionInfo>(json);
                return result;
            }
        }

        public static InspectionInfo CreateDefault(int axleCount, DateTime time)
        {
            int vehicleCount = (int)Math.Ceiling(axleCount / 4.0);
            InspectionInfo info = new InspectionInfo();
            info.Time = time.ToString("yyyyMMddHHmmss");
            info.Type = "TrainRecord";
            for (int i = 0; i < vehicleCount; i++)
            {
                info.Vehicles.Add(new InspectionVehicle());
            }
            for (int i = 0; i < axleCount * 2; i++)
            {
                info.Wheels.Add(new InspectionWheel());
            }
            for (int i = 0; i < 0; i++)
            {
                //set info.Defects null
            }
            for (int i = 0; i < (int)Math.Ceiling(vehicleCount / 4.0); i++)
            {
                info.Formations.Add(new Formation());
            }
            return info;
        }
    }
    /// <summary>
    /// 传感器通道参数，包括编号，声程，增益等
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// 探头编号
        /// </summary>
        public string Order { get; set; }
        /// <summary>
        /// 声程开始,单位mm
        /// </summary>
        public double SoundPathStart { get; set; }
        /// <summary>
        /// 声程结束，单位mm
        /// </summary>
        public double SoundPathEnd { get; set; }
        /// <summary>
        /// 增益,单位dB
        /// </summary>
        public double Gain { get; set; }
    }
    /// <summary>
    /// 包括报警级别，深度，幅度，圆周角度，当量，部位，探头类型，探头名称，说明和有效性
    /// </summary>
    public class Defect
    {
        /// <summary>
        /// 轴位，车轴在整列车中的排序
        /// </summary>
        public int AxlePosition { get; set; }
        /// <summary>
        /// 报警级别，0为状态良好，1为跟踪控制，2为复查判断，3为缺陷超限，9为数据异常
        /// </summary>
        public int AlarmLevel { get; set; }
        /// <summary>
        /// 深度，单位mm
        /// </summary>
        public double Depth { get; set; }
        /// <summary>
        /// 幅值
        /// </summary>
        public double Amplitude { get; set; }
        /// <summary>
        /// 角度
        /// </summary>
        public double Angel { get; set; }
        /// <summary>
        /// 当量
        /// </summary>
        public double Equivalent { get; set; }
        /// <summary>
        /// 缺陷部位,如轮辋
        /// </summary>
        public string Position { get; set; }
        /// <summary>
        /// 探头类型，直探头/斜探头
        /// </summary>
        public string ProbeType { get; set; }
        /// <summary>
        /// 探头名称(探头编号)
        /// </summary>
        public string ProbeName { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 数据采集是否有效
        /// </summary>
        public int Valid { get; set; }
    }
    /// <summary>
    /// 车轮信息组成的列表，包括轮对序号、轴号、轮位、通过速度，是否正常等
    /// </summary>
    public class InspectionWheel
    {
        /// <summary>
        /// 轮位,如左1，右2等
        /// </summary>
        public string Position { get; set; }
        /// <summary>
        /// 车轮编号
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 轴位，车轮在车辆中的轴号
        /// </summary>
        public int AxlePosition { get; set; }
        /// <summary>
        /// 速度，单位Km/h
        /// </summary>
        public string Speed { get; set; }
    }
    public class InspectionVehicle
    {
        /// <summary>
        /// 车辆在编组中的序号
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// 车辆类型,例如CRH380A
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// 车辆编号，例如ZYS286701
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 踏面型号，例如LMA
        /// </summary>
        public string WheelScarType { get; set; }
        /// <summary>
        /// 转向架数
        /// </summary>
        public int BogieCount { get; set; }
        /// <summary>
        /// 车辆端位 A/B
        /// </summary>
        public string EndPosition { get; set; }
    }
    /// <summary>
    /// 编组名称（车型-车号）、在本次检测中的序号、入库端位、编组报警级别及说明
    /// </summary>
    public class Formation
    {
        /// <summary>
        /// 车型,如CRH380A
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// 车号,如2867，编组号则为CRH380A-2657
        /// </summary>
        public string TrainNumber { get; set; }
        /// <summary>
        /// 本次检测中的序号   编组1填1，编组2填2
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// 行驶方向，S向上N向下
        /// </summary>
        public string Direction { get; set; }
        /// <summary>
        /// 报警级别 取最高报警级别，0为状态良好，1为跟踪控制，2为复查判断，3为缺陷超限，9为数据异常
        /// </summary>
        public int AlarmLevel { get; set; }
        /// <summary>
        /// 正常/报警情况的说明
        /// </summary>
        public string Description { get; set; }
    }
    public class InspectionData
    {
        /// <summary>
        /// 通道编号，如直探头004等
        /// </summary>
        public string Order { get; set; }
        /// <summary>
        /// 通道状态，正常true，故障状态false
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 二进制，通道原始B扫图，含刻度，横坐标代表扫查点，纵坐标代表深度
        /// </summary>
        public List<Point> Bscan { get; set; }
        /// <summary>
        /// 各探头参数，包括开始位置、宽度、幅度
        /// </summary>
        public ProbeInfo ProbeParam { get; set; }
        public InspectionData()
        {
            Bscan = new List<Point>();
        }
    }
    /// <summary>
    /// B扫图上的点
    /// </summary>
    public class Point
    {
        /// <summary>
        /// 横坐标,代表扫描点数
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// 纵坐标,代表深度
        /// </summary>
        public double Y { get; set; }
    }
    /// <summary>
    /// 各探头参数，包括开始位置、宽度、幅度
    /// </summary>
    public class ProbeInfo
    {
        /// <summary>
        /// 开始位置，单位mm
        /// </summary>
        public double StartPosition { get; set; }
        /// <summary>
        /// 宽度，单位mm
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// 幅度，单位mm
        /// </summary>
        public double Amplitude { get; set; }
    }
}
