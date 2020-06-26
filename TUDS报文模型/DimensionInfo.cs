using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TUDS报文模型.DimensionHelpers;
namespace TUDS报文模型
{
    public class DimensionInfo
    {
        public HeadArea Head { get; set; }       // 解析头区
        public BaseInfoArea BaseInfo { get; set; }       // 基本信息区
        public AxisDistanceArea AxleDistance { get; set; }  // 轴距表区
        public List<LocomotiveInfo> Locomotives { get; set; }         // 机车 好几个
        public List<CoachInfo> Coaches { get; set; }  // 车厢 信息
        public List<PassOrder> Orders { get; set; }      // 整列车报文实际顺序

        public DimensionInfo()
        {
            Head = new HeadArea();   // 初始化解析头区
            BaseInfo = new BaseInfoArea();  // 初始化基本信息区
            Locomotives = new List<LocomotiveInfo>();    // 初始化机车们
            Coaches = new List<CoachInfo>(); // 初始化车厢们
            Orders = new List<PassOrder>();    //初始化实际顺序
        }

        public static DimensionInfo Create(FileInfo file)
        {
            if (file is null || !File.Exists(file.FullName))
            {
                throw new ArgumentNullException(nameof(file));
            }
            using (StreamReader sr = File.OpenText(file.FullName))
            {
                string json = sr.ReadToEnd();
                DimensionInfo result = JsonConvert.DeserializeObject<DimensionInfo>(json);
                return result;
            }
        }

        public static DimensionInfo CreateDefault(int axleCount, DateTime time)
        {
            int vehicleCount = (int)Math.Ceiling(axleCount / 4.0);
            DimensionInfo dimensionInfo = new DimensionInfo();
            dimensionInfo.BaseInfo.DetectionTime = time.ToString("yyyyMMddHHmmss");
            for (int i = 0; i < vehicleCount; i++)
            {
                dimensionInfo.Coaches.Add(new CoachInfo(4));
                dimensionInfo.Orders.Add(new PassOrder { Index = i, VehicleType = "CX" });
            }
            return dimensionInfo;
        }
        #region TWDS报文解析
        public static DimensionInfo Analyze(byte[] Message)
        {
            DimensionInfo dimensinInfo = new DimensionInfo();
            // 解析报头区
            dimensinInfo.Head.Flag = Message[0];//报头
            dimensinInfo.Head.Length = BSum(Message[1], Message[2]);//长度
            dimensinInfo.Head.MessageType = Message[3].ToString();//种类
            dimensinInfo.Head.LuYouCode = BSum(Message[4], Message[5]).ToString();//路由号
            dimensinInfo.Head.XinYuanCode = BSum(Message[6], Message[7]).ToString();//本站号
            dimensinInfo.Head.XinSuCode = BSum(Message[8], Message[9]).ToString();//目的站号
            dimensinInfo.Head.MessageLength = BSum(Message[10], Message[11]);//长度

            // 解析基本信息区
            dimensinInfo.BaseInfo.Flag = (char)Message[12];//A区标志
            dimensinInfo.BaseInfo.Length = BSum(Message[13], Message[14]);//长度
            dimensinInfo.BaseInfo.StationCode = BSum(Message[15], Message[16], Message[17]);//探测站号
            dimensinInfo.BaseInfo.DiscNo = BSum(Message[18], Message[19]);//探测站存盘号
            //应在此处格式化日期为yyyyMMddHHmmss
            dimensinInfo.BaseInfo.DetectionTime = string.Format("{0}{1}{2}{3}{4}{5}", BSum(Message[20], Message[21]).ToString("0000"), Message[22].ToString("00"), Message[23].ToString("00"), Message[24].ToString("00"), Message[25].ToString("00"), Message[26].ToString("00"));//日期
            dimensinInfo.BaseInfo.Type = ((char)Message[27]).ToString();//客货
            dimensinInfo.BaseInfo.TrainTimes = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}", ((char)Message[28]).ToString(), ((char)Message[29]).ToString(), ((char)Message[30]).ToString(), ((char)Message[31]).ToString(), ((char)Message[32]).ToString(),
                                                                                     ((char)Message[33]).ToString(), ((char)Message[34]).ToString(), ((char)Message[35]).ToString(), ((char)Message[36]).ToString(), ((char)Message[37]).ToString());
            dimensinInfo.BaseInfo.Direction = Message[38];        // 运行方向
            dimensinInfo.BaseInfo.AVGSpeed = BSum(Message[39], Message[40]) / (double)100;    // 平均速度
            dimensinInfo.BaseInfo.MinSpeed = BSum(Message[41], Message[42]) / (double)100;    // 最低速度
            dimensinInfo.BaseInfo.MaxSpeed = BSum(Message[43], Message[44]) / (double)100;    // 最高速度
            dimensinInfo.BaseInfo.VehicleCount = BSum(Message[45], Message[46]);     // 总辆数
            dimensinInfo.BaseInfo.LocomotiveCount = Message[47];                     // 机车数
            dimensinInfo.BaseInfo.AxleCount = BSum(Message[48], Message[49]);      // 总轴数
            dimensinInfo.BaseInfo.State = (Message[50] == 0) ? ("正常") : (Message[50] == 1 ? "异常" : "");        // 运行状态
            dimensinInfo.BaseInfo.OpenDoorCnt = BSum(Message[51], Message[52]);       // 开门次数 
            dimensinInfo.BaseInfo.CloseDoorCnt = BSum(Message[53], Message[54]);      // 关门次数 
            dimensinInfo.BaseInfo.StartCount = BSum(Message[55], Message[56]);        // 开机次数 
            dimensinInfo.BaseInfo.IsExceed = (Message[57] == 0) ? ("无超标") : (Message[57] == 1 ? "有超标" : "");    // 是否超标 
            dimensinInfo.BaseInfo.IsDeviceFail = (Message[58] == 0) ? ("无故障") : (Message[58] == 1 ? "有故障" : "");// 是否设备故障

            // 轴距表区
            int cursorIndex = 59;                             // 当前位置
            cursorIndex = GetFlagIndex(cursorIndex, 'B', Message);
            dimensinInfo.AxleDistance = new AxisDistanceArea(dimensinInfo.BaseInfo.AxleCount)
            {
                Flag = (char)Message[cursorIndex],                // B区标志;
                Length = BSum(Message[cursorIndex++], Message[cursorIndex++]), // 长度
                TableLength = BSum(Message[cursorIndex++], Message[cursorIndex++]) // 轴距表长度
            };   // 总轴数的 轴距初始化
            for (int i = 0; i < dimensinInfo.BaseInfo.AxleCount; i++)
            { dimensinInfo.AxleDistance.AxleDistances[i] = BSum(Message[++cursorIndex], Message[++cursorIndex]); } // 轴距数组  单位：cm  以2550结束            

            cursorIndex = GetFlagIndex(++cursorIndex, 'C', Message);
            dimensinInfo.BaseInfo.VehicleFlag = (char)Message[cursorIndex++];              // 车辆信息区标志C
            dimensinInfo.BaseInfo.VehicleLength = BSum(Message[cursorIndex++], Message[cursorIndex++]); // 车辆信息区长度

            /////////////////////////////////////////////////////////////////////////////////////
            for (int TAi = 0; TAi < dimensinInfo.BaseInfo.VehicleCount; TAi++)
            {
                //char SIGchar = (char)M[m_CurI + 25];
                if (((char)Message[cursorIndex] == 'J' || Message[cursorIndex + 24] < 128))// 机车
                {
                    LocomotiveInfo locomotive = new LocomotiveInfo();
                    locomotive.Index = TAi;  //保存机车位置
                    locomotive.Property = (char)Message[cursorIndex++];           // 车辆信息 属性码 J机车
                    locomotive.VehicleModel = string.Format("{0}{1}{2}", (char)Message[cursorIndex++], (char)Message[cursorIndex++], (char)Message[cursorIndex++]); // 车辆种类
                    locomotive.VehicleNumber = string.Format("{0}{1}{2}{3}", (char)Message[cursorIndex++], (char)Message[cursorIndex++], (char)Message[cursorIndex++], (char)Message[cursorIndex++]); // 车辆车号
                    locomotive.Bureau = string.Format("{0}{1}", (char)Message[cursorIndex++], (char)Message[cursorIndex++]);              // 配属局
                    locomotive.State = (char)Message[cursorIndex++];         // 机车状态
                    locomotive.KorH = (char)Message[cursorIndex++];             // 客货
                    locomotive.TrainTimes = string.Format("{0}{1}{2}{3}{4}{5}{6}", (char)Message[cursorIndex++], (char)Message[cursorIndex++], (char)Message[cursorIndex++], (char)Message[cursorIndex++], (char)Message[cursorIndex++], (char)Message[cursorIndex++], (char)Message[cursorIndex++]);//车次
                    locomotive.EndPosition = (char)Message[cursorIndex++];         // 端码
                    locomotive.AxleCount = Message[cursorIndex++];              // 轴数
                    cursorIndex++;
                    locomotive.AeiEndPosition = (Message[cursorIndex] == 'A') ? ("一端位") : (Message[cursorIndex] == 'B' ? "二端位" : "");  // AEI 端位
                    locomotive.Speed = BSum(Message[cursorIndex++], Message[cursorIndex++]);// 速度
                    locomotive.HasAeiLabel = (char)Message[cursorIndex++];          // 车辆标志属性码，判断是否有标签
                    dimensinInfo.Locomotives.Add(locomotive); // 机车信息

                    PassOrder xlun = new PassOrder();
                    xlun.Index = dimensinInfo.Locomotives.Count - 1;
                    xlun.VehicleType = "JC";//机车
                    dimensinInfo.Orders.Add(xlun);
                }
                else// if (M[m_CurI] == 128 || M[m_CurI] > 128) //车厢： 客车 || 货车
                {
                    CoachInfo cxin = new CoachInfo();
                    cxin.Flag = (char)Message[cursorIndex++];      // 标志T路用货车 或者Q企业自备车
                    cxin.Property = (char)Message[cursorIndex++];   // 车种
                    cxin.VehicleModel = string.Format("{0}{1}{2}{3}{4}", (char)Message[cursorIndex++], (char)Message[cursorIndex++], (char)Message[cursorIndex++], (char)Message[cursorIndex++], (char)Message[cursorIndex++]);// 车型
                    cxin.Name = string.Format("{0}{1}{2}{3}{4}{5}{6}", (char)Message[cursorIndex++], (char)Message[cursorIndex++], (char)Message[cursorIndex++], (char)Message[cursorIndex++], (char)Message[cursorIndex++], (char)Message[cursorIndex++], (char)Message[cursorIndex++]);//车号
                    cxin.HuanChang = string.Format("{0}.{1}", (char)Message[cursorIndex++], (char)Message[cursorIndex++]); // 换长？有待确定
                    cxin.Manufactor = (char)Message[cursorIndex++]; // 制造商
                    cxin.ProductionDate = string.Format("{0}{1}年{2}月", (char)Message[cursorIndex++], (char)Message[cursorIndex++], (char)Message[cursorIndex++]);   // 制造日期 YY年M月
                    cxin.AxleCount = Message[cursorIndex++];    // 轴数
                    cxin.AeiEndPosition = (Message[cursorIndex] == 'A') ? ("一端位") : (Message[cursorIndex] == 'B' ? "二端位" : ""); ;// AEI 端位
                    cursorIndex++;
                    cxin.Speed = BSum(Message[cursorIndex++], Message[cursorIndex++]).ToString();
                    cxin.HasAeiLabel = (char)Message[cursorIndex++];    // 车辆标志属性码，判断是否有标签
                    dimensinInfo.Coaches.Add(cxin); //车厢信息

                    PassOrder xlun = new PassOrder();
                    xlun.Index = dimensinInfo.Coaches.Count - 1;
                    xlun.VehicleType = "CX";//货车 或者 客车
                    dimensinInfo.Orders.Add(xlun);
                }
            }

            // 轮对
            dimensinInfo.BaseInfo.WheelSetFlag = (char)Message[cursorIndex++];                          // 大写字母D //m_CurI = SearchSigIndex(m_CurI, 'D', M);
            dimensinInfo.BaseInfo.WheelSetLength = BSum(Message[cursorIndex++], Message[cursorIndex++], Message[cursorIndex++]);// 长度

            foreach (PassOrder order in dimensinInfo.Orders)
            {
                if (order.VehicleType == "JC")    // 机车轮对解析
                {
                    for (int wheelOrder = 0; wheelOrder < dimensinInfo.Locomotives[order.Index].AxleCount * 2; wheelOrder++)
                    {
                        WheelInfo wheel = new WheelInfo();
                        wheel.Name = Index2LR(wheelOrder);  //名字
                        wheel.Diameter = BSum100(Message[cursorIndex++], Message[cursorIndex++], Message[cursorIndex++]); // 直径
                        wheel.FlangeThickness = BSum100(Message[cursorIndex++], Message[cursorIndex++]);   // 厚度
                        wheel.FlangeHeight = BSum100(Message[cursorIndex++], Message[cursorIndex++]);          // 高度
                        if ((wheelOrder % 2) == 0)
                            wheel.WheelsetDistance = BSum100(Message[cursorIndex++], Message[cursorIndex++], Message[cursorIndex++]); // 内侧距偶数有 ，奇数没有
                        wheel.QR = BSum100(Message[cursorIndex++], Message[cursorIndex++]);             // QR 值
                        wheel.ChuiZhiChaoXian = BSum100(Message[cursorIndex++], Message[cursorIndex++]);    // 垂直磨耗超限
                        wheel.RimThickness = BSum100(Message[cursorIndex++], Message[cursorIndex++]);       // 轮辋厚度
                        wheel.TreadWear = BSum100(Message[cursorIndex++], Message[cursorIndex++]);  // 踏面磨耗深度
                        wheel.TaMianPointCount = Message[cursorIndex++];                      // 踏面坐标点数
                        for (int PointIndex = 0; PointIndex < wheel.TaMianPointCount; PointIndex++)
                        {
                            wheel.PointCollection = wheel.PointCollection + string.Format("{0}{1},{2}{3};",
                                ((Message[cursorIndex++] == 0) ? ("﹣") : ""), BSum10(Message[cursorIndex++], Message[cursorIndex++]),
                                ((Message[cursorIndex++] == 0) ? ("﹣") : ""), BSum10(Message[cursorIndex++], Message[cursorIndex++])); //所有点的  字符串  x1，y1;x2，y2;x3，y3;……
                        }
                        dimensinInfo.Locomotives[order.Index].Wheels.Add(wheel);

                        //同转向架轮径差
                        if (wheelOrder % 4 == 3)
                        {
                            TongZhuanXiangLunJingCha tzxl = new TongZhuanXiangLunJingCha();
                            if (wheel.Diameter < tzxl.TongZhuanXiangLunJingMin)
                                tzxl.TongZhuanXiangLunJingMin = wheel.Diameter;
                            if (wheel.Diameter > tzxl.TongZhuanXiangLunJingMax)
                                tzxl.TongZhuanXiangLunJingMax = wheel.Diameter;
                            dimensinInfo.Locomotives[order.Index].TZXCs.Add(tzxl);
                        }

                        //同车轮径差
                        if (wheel.Diameter < dimensinInfo.Locomotives[order.Index].TongCheLunJingMin)
                            dimensinInfo.Locomotives[order.Index].TongCheLunJingMin = wheel.Diameter;
                        if (wheel.Diameter > dimensinInfo.Locomotives[order.Index].TongCheLunJingMax)
                            dimensinInfo.Locomotives[order.Index].TongCheLunJingMax = wheel.Diameter;

                    }
                }
                else if (order.VehicleType == "CX")// 车厢轮对解析
                {
                    for (int i = 0; i < dimensinInfo.Coaches[order.Index].AxleCount * 2; i++)
                    {
                        WheelInfo wheel = new WheelInfo();
                        wheel.Name = Index2LR(i);  //名字
                        wheel.Diameter = BSum100(Message[cursorIndex++], Message[cursorIndex++], Message[cursorIndex++]);// 直径
                        wheel.FlangeThickness = BSum100(Message[cursorIndex++], Message[cursorIndex++]);  // 轮缘厚度
                        wheel.FlangeHeight = BSum100(Message[cursorIndex++], Message[cursorIndex++]);         // 高度
                        if ((i % 2) == 0)
                            wheel.WheelsetDistance = BSum100(Message[cursorIndex++], Message[cursorIndex++], Message[cursorIndex++]); // 内侧距偶数有 ，奇数没有
                        wheel.QR = BSum100(Message[cursorIndex++], Message[cursorIndex++]);            // QR 值
                        wheel.ChuiZhiChaoXian = BSum100(Message[cursorIndex++], Message[cursorIndex++]);    // 垂直磨耗超限
                        wheel.RimThickness = BSum100(Message[cursorIndex++], Message[cursorIndex++]);       // 轮辋厚度
                        wheel.TreadWear = BSum100(Message[cursorIndex++], Message[cursorIndex++]);  // 磨耗深度
                        wheel.TaMianPointCount = Message[cursorIndex++];                         // 点数
                        for (int PointI = 0; PointI < wheel.TaMianPointCount; PointI++)
                        {
                            wheel.PointCollection = wheel.PointCollection + string.Format("{0}{1},{2}{3};",
                            ((Message[cursorIndex++] == 0) ? ("﹣") : ""), BSum10(Message[cursorIndex++], Message[cursorIndex++]),
                            ((Message[cursorIndex++] == 0) ? ("﹣") : ""), BSum10(Message[cursorIndex++], Message[cursorIndex++])); //所有点的  字符串  x1，y1；x2，y2；x3，y3；、、、、、、
                        }
                        dimensinInfo.Coaches[order.Index].Wheels.Add(wheel);

                        //同转向架轮径差
                        if (i % 4 == 3)
                        {
                            TongZhuanXiangLunJingCha tzxl = new TongZhuanXiangLunJingCha();
                            if (wheel.Diameter < tzxl.TongZhuanXiangLunJingMin)
                                tzxl.TongZhuanXiangLunJingMin = wheel.Diameter;
                            if (wheel.Diameter > tzxl.TongZhuanXiangLunJingMax)
                                tzxl.TongZhuanXiangLunJingMax = wheel.Diameter;
                            dimensinInfo.Coaches[order.Index].TZXCs.Add(tzxl);
                        }

                        //同车轮径差
                        if (wheel.Diameter < dimensinInfo.Coaches[order.Index].TongCheLunJingMin)
                            dimensinInfo.Coaches[order.Index].TongCheLunJingMin = wheel.Diameter;
                        if (wheel.Diameter > dimensinInfo.Coaches[order.Index].TongCheLunJingMax)
                            dimensinInfo.Coaches[order.Index].TongCheLunJingMax = wheel.Diameter;
                    }
                }
            }
            return dimensinInfo;
        }
        #endregion
    }

    // 机车信息
    public class LocomotiveInfo
    {
        public int Index { get; set; }      // 数组下标，用于轮训轮对信息时候使用
        /// <summary>
        /// 车辆种类，J为机车，其他为正常车厢
        /// </summary>
        public char Property { get; set; }   // 车辆信息 属性码 J机车
        /// <summary>
        /// 车型 字母与数字组合
        /// </summary>
        public string VehicleModel { get; set; } // 车辆种类
        public string VehicleNumber { get; set; }   // 车辆车号
        public string Bureau { get; set; }  // 配属局
        public char State { get; set; }  // 机车状态
        public char KorH { get; set; }      // 客货
        public string TrainTimes { get; set; }   // 车次
        public char EndPosition { get; set; }  // 端码
        public int AxleCount { get; set; }  // 轴数
        public string AeiEndPosition { get; set; } // AEI 端位
        public int Speed { get; set; }      // 速度
        public char HasAeiLabel { get; set; }  // 车辆标志属性码，判断是否有标签
        public List<TongZhuanXiangLunJingCha> TZXCs { get; set; }//同转向架轮径差

        public double TongCheLunJingMin { get; set; } = 2000; // 同车轮径最小值
        public double TongCheLunJingMax { get; set; } = 0;    // 同车轮径最大值

        public List<WheelInfo> Wheels;           // 机车轮对
        public LocomotiveInfo()
        {
            TZXCs = new List<TongZhuanXiangLunJingCha>();
            Wheels = new List<WheelInfo>();
        }
    }

    public class WheelInfo
    {
        /// <summary>
        /// 轮对名，形如1L, 1R, 2L, 2R....
        /// </summary>
        public string Name;
        public double Diameter { get; set; }//直径
        /// <summary>
        /// 轮缘厚度(mm)
        /// </summary>
        public double FlangeThickness { get; set; } // 厚度
        /// <summary>
        /// 轮缘高度(mm)
        /// </summary>
        public double FlangeHeight { get; set; } // 高度
        /// <summary>
        /// 轮对内侧距
        /// </summary>
        public double WheelsetDistance { get; set; } // 内侧距
        public double QR { get; set; }      // QR 值
        /// <summary>
        /// 垂直磨耗超限
        /// </summary>
        public double ChuiZhiChaoXian { get; set; } // 垂直磨耗超限
        /// <summary>
        /// 轮辋厚度(mm)
        /// </summary>
        public double RimThickness { get; set; }    // 轮辋厚度
        /// <summary>
        /// 踏面磨耗(mm)
        /// </summary>
        public double TreadWear { get; set; } // 踏面磨耗深度
        public int TaMianPointCount { get; set; }    // 踏面坐标点数
        public string PointCollection { get; set; }  // 所有点的  字符串  x1，y1；x2，y2；x3，y3；、、、、、、
    }

    //存储车辆序列
    public class PassOrder
    {
        /// <summary>
        /// 该Index下车辆类型，JC为机车，CX为普通车厢
        /// </summary>
        public string VehicleType { get; set; }
        public int Index { get; set; }//在机车表中 或者 客车表中的  索引
    }

    // 车体 信息
    public class CoachInfo
    {
        /// <summary>
        /// 标志T路用货车 或者Q企业自备车
        /// </summary>
        public char Flag { get; set; }
        /// <summary>
        /// 车辆种类，J为机车，其他为正常车厢
        /// </summary>
        public char Property { get; set; }
        /// <summary>
        /// 车型(字母与数字组合)
        /// </summary>
        public string VehicleModel { get; set; }
        /// <summary>
        ///  车号
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 换长(不知道啥意思)
        /// </summary>
        public string HuanChang { get; set; } // 换长
        public char Manufactor { get; set; } // 制造商
        public string ProductionDate { get; set; }   // 制造日期 YY年M月
        public int AxleCount { get; set; }    // 轴数
        public string AeiEndPosition { get; set; }// AEI 端位
        public string Speed { get; set; }     // 速度，通过速度
        public char HasAeiLabel { get; set; }   // 车辆标志属性码，判断是否有标签
        public List<TongZhuanXiangLunJingCha> TZXCs;//同转向架轮径差

        public double TongCheLunJingMin { get; set; } = 2000; // 同车轮径最小值
        public double TongCheLunJingMax { get; set; } = 0;    // 同车轮径最大值

        public List<WheelInfo> Wheels;
        public CoachInfo(int count = 0)
        {
            Wheels = new List<WheelInfo>();//初始化轮子表
            TZXCs = new List<TongZhuanXiangLunJingCha>(); //同转向架轮径差
            for(int i = 0; i < count; i++)
            {
                Wheels.Add(new WheelInfo());
                Wheels.Add(new WheelInfo());
            }
            TZXCs.Add(new TongZhuanXiangLunJingCha());
            TZXCs.Add(new TongZhuanXiangLunJingCha());
        }
    }

    public class TongZhuanXiangLunJingCha
    {
        public double TongZhuanXiangLunJingMin { get; set; } = 2000;// 同转向架轮径最小值
        public double TongZhuanXiangLunJingMax { get; set; } = 0;   // 同转向架轮径最大值
    }

    //基础信息区
    public class BaseInfoArea
    {
        public char Flag { get; set; }      // 区域标志
        public int Length { get; set; }       // 长度
        public int StationCode { get; set; }   // 探测站号
        public int DiscNo { get; set; }   // 存盘号
        public string DetectionTime { get; set; }      // 探测时间
        public string Type { get; set; }      // 客货标志
        public string TrainTimes { get; set; }    // 车次
        public int Direction { get; set; }  // 上下行,0上行，1下行
        public double AVGSpeed { get; set; }     // 平均速度
        public double MinSpeed { get; set; }     // 最低速度
        public double MaxSpeed { get; set; }    // 最高速度
        public int VehicleCount { get; set; }// 总辆数
        public int LocomotiveCount { get; set; }   // 机车数
        public int AxleCount { get; set; } // 总轴数
        public string State { get; set; }  // 运行状态
        public int OpenDoorCnt { get; set; }  // 开门次数
        public int CloseDoorCnt { get; set; } // 关门次数
        public int StartCount { get; set; }   // 开机次数
        public string IsExceed { get; set; }  // 是否超标
        public string IsDeviceFail { get; set; }// 是否设备故障
        public char VehicleFlag { get; set; }       // 标志C
        public int VehicleLength { get; set; }        // 信息区长度
        public char WheelSetFlag { get; set; } //轮对标志区
        public int WheelSetLength { get; set; }  //轮对区长度
    }

    public class HeadArea
    {
        public byte Flag { get; set; }    // 报文标志
        public int Length { get; set; }      // 报文长度
        public string MessageType { get; set; }     // 报文
        public string LuYouCode { get; set; }  // 路由站号
        public string XinYuanCode { get; set; }// 信源站号
        public string XinSuCode { get; set; }  // 信宿站号
        public int MessageLength { get; set; }   // A到Z的长度
    }

    public class AxisDistanceArea
    {
        public char Flag { get; set; }//标志B
        public int Length { get; set; }//长度
        public int TableLength { get; set; } //轴距表长度
        public int[] AxleDistances;//轴距数组  单位：cm  以2550结束
        public AxisDistanceArea(int ATLen)
        {
            AxleDistances = new int[ATLen];
        }
    }
}
