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
    
    public partial class FAULT
    {
        public FAULT()
        {
            this.RECHECKS = new HashSet<RECHECKS>();
        }
    
        public string ID { get; set; }
        public string WHEEL_ID { get; set; }
        public string ITEM { get; set; }
        public decimal VALUE { get; set; }
        public short ISRECHECKED { get; set; }
        public Nullable<short> ALARM_LEVEL { get; set; }
    
        public virtual WHEEL WHEEL { get; set; }
        public virtual ICollection<RECHECKS> RECHECKS { get; set; }
    }
}
