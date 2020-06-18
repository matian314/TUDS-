using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace 工具类
{
    public static class MathExtensions
    {
        /// <summary>
        /// 将double类型数据转换为decimal类型，并取digits位小数
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="digits">取小数位数</param>
        /// <returns></returns>
        public static decimal Round(this double Value, int digits = 2)
        {
            return (decimal)Math.Round(Value, digits);
        }
        /// <summary>
        /// 将float类型数据转换为decimal类型，并取digits位小数
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="digits">取小数位数</param>
        /// <returns></returns>
        public static decimal Round(this float Value, int digits = 2)
        {
            return (decimal)Math.Round(Value, digits);
        }
    }
}