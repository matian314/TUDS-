using ExceptionHelper;
using IndexHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUDS报文模型;
using 工具类;

namespace TUDS入库
{
    public class Uploader
    {
        public TudsEntities db { get; set; } = new TudsEntities();
        public log4net.ILog log = log4net.LogManager.GetLogger("Uploader");
        private ScrapeInfo scrapeInfo;
        private DimensionInfo dimensionInfo;
        private InspectionInfo inspectionInfo;
        private AeiInfo aei;

        public Uploader(ScrapeInfo scrapeInfo, DimensionInfo dimensionInfo, InspectionInfo inspectionInfo, AeiInfo aei)
        {
            this.scrapeInfo = scrapeInfo ?? throw new ArgumentNullException(nameof(scrapeInfo));
            this.dimensionInfo = dimensionInfo ?? throw new ArgumentNullException(nameof(dimensionInfo));
            this.inspectionInfo = inspectionInfo ?? throw new ArgumentNullException(nameof(inspectionInfo));
            this.aei = aei ?? throw new ArgumentNullException(nameof(aei));
        }

        public void Insert()
        {
            try
            {
                TRAIN train = InitializeTrain();
                List<VEHICLE> vehicles = new List<VEHICLE>();
                List<WHEEL> wheels = new List<WHEEL>();
                List<FAULT> faults = new List<FAULT>();
                List<SCRAPE_DATA> scrape_data = new List<SCRAPE_DATA>();
                List<DIMENSION_DATA> dimension_data = new List<DIMENSION_DATA>();
                List<INSPECTION_DATA> inspection_data = new List<INSPECTION_DATA>();

                MakeTrain(train);
                if (dimensionInfo.BaseInfo.IsExceed == "有超标")
                {
                    train.ALARM2_COUNT++;
                    train.DIMENSION_ALARM2_COUNT++;
                    train.DIMENSION_ALARM_LEVEL = 2;
                }
                int wheelOrder = 0;
                for (int i = 0; i < train.VEHICLE_COUNT; i++)
                {
                    bool isLocomotive = dimensionInfo.Orders.Index(i).VehicleType == "JC";
                    VEHICLE vehicle = new VEHICLE
                    {
                        ID = Guid.NewGuid().ToString(),
                        TRAIN_ID = train.ID,
                        IS_LOCOMOTIVE = isLocomotive ? (short)0 : (short)1,
                        PASS_ORDER = (byte)aei.Vehicles.Index(i).Order,

                        ALARMLEVEL = 0,
                        ALARM1_COUNT = 0,
                        ALARM2_COUNT = 0,

                        SCRAPE_ALARMLEVEL = 0,
                        SCRAPE_ALARM1_COUNT = 0,
                        SCRAPE_ALARM2_COUNT = 0,

                        DIMENSION_ALARMLEVEL = 0,
                        DIMENSION_ALARM1_COUNT = 0,
                        DIMENSION_ALARM2_COUNT = 0,

                        DIAMETER_ALARMLEVEL = 0,
                        DIAMETER_ALARM1_COUNT = 0,
                        DIAMETER_ALARM2_COUNT = 0,

                        FLANGE_HEIGHT_ALARMLEVEL = 0,
                        FLANGE_HEIGHT_ALARM1_COUNT = 0,
                        FLANGE_HEIGHT_ALARM2_COUNT = 0,

                        FLANGE_THICKNESS_ALARMLEVEL = 0,
                        FLANGE_THICKNESS_ALARM1_COUNT = 0,
                        FLANGE_THICKNESS_ALARM2_COUNT = 0,

                        INSPECTION_ALARMLEVEL = 0,
                        INSPECTION_ALARM1_COUNT = 0,
                        INSPECTION_ALARM2_COUNT = 0,

                        QR_ALARMLEVEL = 0,
                        QR_ALARM1_COUNT = 0,
                        QR_ALARM2_COUNT = 0,

                        RIM_THICKNESS_ALARMLEVEL = 0,
                        RIM_THICKNESS_ALARM1_COUNT = 0,
                        RIM_THICKNESS_ALARM2_COUNT = 0,

                        THREADWEAR_ALARMLEVEL = 0,
                        THREADWEAR_ALRAM1_COUNT = 0,
                        THREADWEAR_ALRAM2_COUNT = 0,

                    };

                    if (isLocomotive)
                    {
                        int index = dimensionInfo.Orders.Index(i).Index;
                        var locomotive = dimensionInfo.Locomotives.Index(index);
                        vehicle.NAME = locomotive.VehicleNumber;
                        vehicle.AEI_END_DIRECTION = locomotive.AeiEndPosition;
                        vehicle.COACH_NUMBER = locomotive.VehicleNumber;
                        string carType = aei.Vehicles.Index(i).Model;
                        CAR_TYPE type = db.CAR_TYPE.Where(c => c.TYPE == carType).FirstOrDefault();
                        if (type == null)
                        {
                            //数据库中不存在的车型
                            CAR_TYPE newType = new CAR_TYPE()
                            {
                                ID = Guid.NewGuid().ToString(),
                                TYPE = aei.Vehicles.Index(i).Model
                            };
                            db.CAR_TYPE.Add(newType);
                            db.SaveChanges();
                            vehicle.CAR_TYPE_ID = newType.ID;
                        }
                        else
                        {
                            vehicle.CAR_TYPE_ID = type.ID;
                        }
                        vehicle.UNIT_NUMBER = locomotive.TrainTimes;
                        for (int j = 0; j < locomotive.AxleCount * 2; j++)
                        {
                            string scrapeStatus = scrapeInfo.Wheels.Index(i * 8 + j).Status?.ToString();
                            WHEEL wheel = new WHEEL
                            {
                                ID = Guid.NewGuid().ToString(),
                                WHEEL_ORDER = (byte)(j + 1),
                                VEHICLE = vehicle,
                                VEHICLE_ID = vehicle.ID,
                                SCRAPE_ALARMLEVEL = (byte)(scrapeStatus == "T" ? 0 : 2),
                                PASS_SPEED = scrapeInfo.Wheels.Index(wheelOrder).Speed.Round(),
                                ROUND = scrapeInfo.Wheels.Index(wheelOrder).Round.Round(),
                                MAX_BRUISE_DEPTH = scrapeInfo.Wheels.Index(wheelOrder).MaxBruiseDeep.Round(),
                                MAX_BRUISE_LENGTH = scrapeInfo.Wheels.Index(wheelOrder).MaxBruiseLen.Round(),
                                MAX_DIAMETER = scrapeInfo.Wheels.Index(wheelOrder).MaxDia.Round(),
                                MIN_DIAMETER = scrapeInfo.Wheels.Index(wheelOrder).MinDia.Round(),
                                AXLE_POSITION = (byte)(j / 2 + 1),
                                ALARMLEVEL = 0,
                                //同轴轮径差 
                                COAXIAL_DIAMETER_DIFFERENCE = Math.Abs(locomotive.Wheels[j / 2 * 2].Diameter - locomotive.Wheels.Index(j / 2 * 2 + 1).Diameter).Round(),
                                //BOGIE_DIAMETER_DIFFERENCE = (locomotive.TZXCs[j / 4].TongZhuanXiangLunJingMax - locomotive.TZXCs[j / 4].TongZhuanXiangLunJingMax).Round(),
                                VEHICLE_DIAMETER_DIFFERENCE = (locomotive.TongCheLunJingMax - locomotive.TongCheLunJingMin).Round(),
                                DIAMETER_ALARMLEVEL = 0,
                                DIAMETER = locomotive.Wheels.Index(j).Diameter.Round(),
                                DIMENSION_ALARMLEVEL = 0,
                                FLANGE_THICKNESS = locomotive.Wheels.Index(j).FlangeThickness.Round(),
                                FLANGE_HEIGHT = locomotive.Wheels.Index(j).FlangeHeight.Round(),
                                FLANGE_HEIGHT_ALARMLEVEL = 0,
                                FLANGE_THICKNESS_ALARMLEVEL = 0,
                                INSPECTION_ALARMLEVEL = 0,
                                POSITION = GetWheelPositionByIndex(j),
                                QR_ALARMLEVEL = 0,
                                QR = locomotive.Wheels.Index(j).QR.Round(),
                                RIM_THICKNESS_ALARMLEVEL = 0,
                                RIM_THICKNESS = locomotive.Wheels.Index(j).RimThickness.Round(),
                                TREAD_WEAR = locomotive.Wheels.Index(j).RimThickness.Round(),
                                WHEELSET_DISTANCE = locomotive.Wheels.Index(j).WheelsetDistance.Round()
                            };
                            wheel.NAME = vehicle.NAME + " " + wheel.POSITION;
                            if (scrapeStatus == "F")
                            {
                                vehicle.ALARM2_COUNT++;
                                vehicle.SCRAPE_ALARM2_COUNT++;
                                train.ALARM2_COUNT++;
                                train.SCRAPE_ALARM_LEVEL++;

                                vehicle.SCRAPE_ALARMLEVEL = 2;
                                train.SCRAPE_ALARM_LEVEL = 2;
                                FAULT fault = new FAULT()
                                {
                                    ID = Guid.NewGuid().ToString(),
                                    WHEEL_ID = wheel.ID,
                                    ITEM = "擦伤",
                                    VALUE = 2,
                                    ISRECHECKED = 0
                                };
                                faults.Add(fault);
                            }
                            wheels.Add(wheel);
                            wheelOrder++;
                        }
                    }
                    else
                    {
                        int index = dimensionInfo.Orders.Index(i).Index;
                        var coach = dimensionInfo.Coaches.Index(index);
                        vehicle.NAME = aei.Vehicles.Index(i)?.Model + "-" + aei.Vehicles.Index(i)?.CarNumber;
                        vehicle.AEI_END_DIRECTION = aei.Vehicles.Index(i)?.EndPosition;

                        string carType = aei.Vehicles.Index(i)?.Model;
                        CAR_TYPE type = db.CAR_TYPE.Where(c => c.TYPE == carType).FirstOrDefault();
                        if (type == null)
                        {
                            //数据库中不存在的车型
                            CAR_TYPE newType = new CAR_TYPE()
                            {
                                ID = Guid.NewGuid().ToString(),
                                TYPE = aei.Vehicles.Index(i)?.Model
                            };
                            db.CAR_TYPE.Add(newType);
                            db.SaveChanges();
                            vehicle.CAR_TYPE_ID = newType.ID;
                        }
                        else
                        {
                            vehicle.CAR_TYPE_ID = type.ID;
                        }
                        vehicle.COACH_NUMBER = aei.Vehicles.Index(i)?.Model + "-" + aei.Vehicles.Index(i)?.CarNumber;
                        vehicle.UNIT_NUMBER = aei.Vehicles.Index(i)?.Model + "-" + aei.Vehicles.Index(i).CarNumber.Substring(0, aei.Vehicles.Index(i).CarNumber.Length - 2);

                        for (int j = 0; j < aei.Vehicles.Index(i)?.AxleCount * 2; j++)
                        {
                            string scrapeStatus = scrapeInfo.Wheels.Index(i * 8 + j).Status?.ToString();
                            WHEEL wheel = new WHEEL
                            {
                                ID = Guid.NewGuid().ToString(),
                                WHEEL_ORDER = (byte)(j + 1),
                                VEHICLE = vehicle,
                                VEHICLE_ID = vehicle.ID,
                                SCRAPE_ALARMLEVEL = (byte)(scrapeStatus == "T" ? 0 : 2),
                                PASS_SPEED = scrapeInfo.Wheels.Index(wheelOrder).Speed.Round(),
                                ROUND = scrapeInfo.Wheels.Index(wheelOrder).Round.Round(),
                                MAX_BRUISE_DEPTH = scrapeInfo.Wheels.Index(wheelOrder).MaxBruiseDeep.Round(),
                                MAX_BRUISE_LENGTH = scrapeInfo.Wheels.Index(wheelOrder).MaxBruiseLen.Round(),
                                MAX_DIAMETER = scrapeInfo.Wheels.Index(wheelOrder).MaxDia.Round(),
                                MIN_DIAMETER = scrapeInfo.Wheels.Index(wheelOrder).MinDia.Round(),
                                AXLE_POSITION = (byte)(j / 2 + 1),
                                ALARMLEVEL = 0,
                                //同轴轮径差 
                                COAXIAL_DIAMETER_DIFFERENCE = Math.Abs(coach.Wheels.Index(j / 2 * 2).Diameter - coach.Wheels.Index(j / 2 * 2 + 1).Diameter).Round(),
                                BOGIE_DIAMETER_DIFFERENCE = (coach.TZXCs.Index(j / 4).TongZhuanXiangLunJingMax - coach.TZXCs.Index(j / 4).TongZhuanXiangLunJingMax).Round(),
                                VEHICLE_DIAMETER_DIFFERENCE = (coach.TongCheLunJingMax - coach.TongCheLunJingMin).Round(),
                                DIAMETER_ALARMLEVEL = 0,
                                DIAMETER = coach.Wheels.Index(j).Diameter.Round(),
                                DIMENSION_ALARMLEVEL = 0,
                                FLANGE_THICKNESS = coach.Wheels.Index(j).FlangeThickness.Round(),
                                FLANGE_HEIGHT = coach.Wheels.Index(j).FlangeHeight.Round(),
                                FLANGE_HEIGHT_ALARMLEVEL = 0,
                                FLANGE_THICKNESS_ALARMLEVEL = 0,
                                INSPECTION_ALARMLEVEL = 0,
                                POSITION = GetWheelPositionByIndex(j),
                                NAME = coach.Wheels.Index(j).Name,
                                QR_ALARMLEVEL = 0,
                                QR = coach.Wheels.Index(j).QR.Round(),
                                RIM_THICKNESS_ALARMLEVEL = 0,
                                RIM_THICKNESS = coach.Wheels.Index(j).RimThickness.Round(),
                                TREAD_WEAR = coach.Wheels.Index(j).TreadWear.Round(),
                                WHEELSET_DISTANCE = coach.Wheels.Index(j).WheelsetDistance.Round()
                            };
                            wheels.Add(wheel);
                        }
                    }
                    vehicles.Add(vehicle);
                }
                db.TRAIN.Add(train);
                foreach (var item in vehicles)
                {
                    db.VEHICLE.Add(item);
                }
                foreach (var item in wheels)
                {
                    db.WHEEL.Add(item);
                }
                foreach (var item in faults)
                {
                    db.FAULT.Add(item);
                }
                db.SaveChanges();
            }
            catch (Exception e)
            {
                log.Error(e.ToRecord());
            }
        }
        private void MakeTrain(TRAIN train)
        {
            train.AXLE_COUNT = (byte)aei.AxleCount;
            train.MAX_SPEED = (decimal)scrapeInfo.MaxSpeed;
            train.MIN_SPEED = (decimal)scrapeInfo.MinSpeed;
            train.LOCOMOTIVE_COUNT = (byte)dimensionInfo.Locomotives.Count();
            train.TRAIN_TYPE = "D";
            train.END_POSITION = dimensionInfo.BaseInfo.Direction.ToString();
            train.DETECTION_TIME = aei.Time;
            string stationCode = dimensionInfo.BaseInfo.StationCode.ToString();
            SITES site = db.SITES.Where(s => s.CODE == stationCode).FirstOrDefault();
            if (site is null)
            {
                SITES newSite = new SITES()
                {
                    SITE_ID = Guid.NewGuid().ToString(),
                    CODE = stationCode
                };
                db.SITES.Add(newSite);
                db.SaveChanges();
                train.SITE_ID = newSite.SITE_ID;
            }
            else
            {
                train.SITE_ID = site.SITE_ID;
            }

            train.NAME = aei.Vehicles[0].Model + aei.Vehicles[0].CarNumber.Substring(0, aei.Vehicles[0].CarNumber.Length - 2);
            train.UNIT_NUMBER1 = aei.Vehicles[0].Model + aei.Vehicles[0].CarNumber.Substring(0, aei.Vehicles[0].CarNumber.Length - 2);
            train.VEHICLE_COUNT = (byte)(aei.VehicleCount);
            train.BUREAUDICT_ID = "B";
            if (inspectionInfo.Defects.Count > 0)
            {
                train.INSPECTION_ALARM_LEVEL = (byte)inspectionInfo.Defects.Max(d => d.AlarmLevel);
                train.INSPECTION_ALARM1_COUNT = (byte)inspectionInfo.Defects.Where(d => d.AlarmLevel == 1).Count();
                train.INSPECTION_ALARM2_COUNT = (byte)inspectionInfo.Defects.Where(d => d.AlarmLevel == 2).Count();
            }

        }

        public TRAIN InitializeTrain()
        {
            TRAIN train = new TRAIN
            {
                ID = Guid.NewGuid().ToString(),

                ALARM_LEVEL = 0,
                INSPECTION_ALARM_LEVEL = 0,
                SCRAPE_ALARM_LEVEL = 0,
                DIAMETER_ALARM_LEVEL = 0,
                FLANGE_THICKNESS_ALARM_LEVEL = 0,
                FLANGE_HEIGHT_ALARM_LEVEL = 0,
                RIM_THICKNESS_ALARM_LEVEL = 0,
                QR_ALARMLEVEL = 0,
                THREADWEAR_ALARMLEVEL = 0,
                DIMENSION_ALARM_LEVEL = 0,

                ALARM1_COUNT = 0,
                ALARM2_COUNT = 0,
                INSPECTION_ALARM1_COUNT = 0,
                INSPECTION_ALARM2_COUNT = 0,
                SCRAPE_ALARM1_COUNT = 0,
                SCRAPE_ALARM2_COUNT = 0,
                THREADWEAR_ALRAM1_COUNT = 0,
                THREADWEAR_ALRAM2_COUNT = 0,
                DIAMETER_ALARM1_COUNT = 0,
                DIAMETER_ALARM2_COUNT = 0,
                DIMENSION_ALARM1_COUNT = 0,
                DIMENSION_ALARM2_COUNT = 0,
                FLANGE_HEIGHT_ALARM1_COUNT = 0,
                FLANGE_HEIGHT_ALARM2_COUNT = 0,
                FLANGE_THICKNESS_ALARM1_COUNT = 0,
                FLANGE_THICKNESS_ALARM2_COUNT = 0,
                RIM_THICKNESS_ALARM1_COUNT = 0,
                RIM_THICKNESS_ALARM2_COUNT = 0,
                QR_ALARM1_COUNT = 0,
                QR_ALARM2_COUNT = 0,

                CHECKED = 0,
                VEHICLE_COUNT = (byte)Math.Ceiling(aei.AxleCount / 4.0)
            };
            return train;
        }

        private string GetWheelPositionByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return "左1";
                case 1:
                    return "右1";
                case 2:
                    return "左2";
                case 3:
                    return "右2";
                case 4:
                    return "左3";
                case 5:
                    return "右3";
                case 6:
                    return "左4";
                case 7:
                    return "右4";
                case 8:
                    return "左5";
                case 9:
                    return "右5";
                case 10:
                    return "左6";
                case 11:
                    return "右6";
                default:
                    return "未知";
            }
        }
    }
}
