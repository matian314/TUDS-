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
    
    public partial class SCRAPE_DATA
    {
        public string ID { get; set; }
        public string WHEEL_ID { get; set; }
        public Nullable<short> LENGTH { get; set; }
        public string DATA { get; set; }
    
        public virtual WHEEL WHEEL { get; set; }
    }
}
