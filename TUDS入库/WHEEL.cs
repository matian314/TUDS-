//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace TUDS入库
{
    using System;
    using System.Collections.Generic;
    
    public partial class WHEEL
    {
        public WHEEL()
        {
            this.FAULT = new HashSet<FAULT>();
        }
    
        public string ID { get; set; }
        public string NAME { get; set; }
        public string VEHICLE_ID { get; set; }
        public string POSITION { get; set; }
        public short AXLE_POSITION { get; set; }
        public Nullable<short> ALARMLEVEL { get; set; }
        public Nullable<short> INSPECTION_ALARMLEVEL { get; set; }
        public Nullable<short> SCRAPE_ALARMLEVEL { get; set; }
        public Nullable<short> DIMENSION_ALARMLEVEL { get; set; }
        public Nullable<short> DIAMETER_ALARMLEVEL { get; set; }
        public Nullable<short> FLANGE_THICKNESS_ALARMLEVEL { get; set; }
        public Nullable<short> FLANGE_HEIGHT_ALARMLEVEL { get; set; }
        public Nullable<short> RIM_THICKNESS_ALARMLEVEL { get; set; }
        public Nullable<short> QR_ALARMLEVEL { get; set; }
        public Nullable<decimal> DIAMETER { get; set; }
        public Nullable<decimal> FLANGE_THICKNESS { get; set; }
        public Nullable<decimal> FLANGE_HEIGHT { get; set; }
        public Nullable<decimal> RIM_THICKNESS { get; set; }
        public Nullable<decimal> QR { get; set; }
        public Nullable<decimal> COAXIAL_DIAMETER_DIFFERENCE { get; set; }
        public Nullable<decimal> TREAD_WEAR { get; set; }
        public Nullable<decimal> WHEELSET_DISTANCE { get; set; }
        public Nullable<decimal> BOGIE_DIAMETER_DIFFERENCE { get; set; }
        public Nullable<decimal> VEHICLE_DIAMETER_DIFFERENCE { get; set; }
        public Nullable<decimal> PASS_SPEED { get; set; }
        public Nullable<decimal> ROUND { get; set; }
        public Nullable<decimal> MAX_BRUISE_LENGTH { get; set; }
        public Nullable<decimal> MAX_BRUISE_DEPTH { get; set; }
        public Nullable<decimal> MAX_DIAMETER { get; set; }
        public Nullable<decimal> MIN_DIAMETER { get; set; }
        public Nullable<short> WHEEL_ORDER { get; set; }
        public Nullable<decimal> ET { get; set; }
        public Nullable<decimal> OR_VALUE { get; set; }
    
        public virtual ICollection<FAULT> FAULT { get; set; }
        public virtual VEHICLE VEHICLE { get; set; }
    }
}
