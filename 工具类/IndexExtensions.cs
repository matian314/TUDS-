using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IndexHelper
{
    public static class IndexExtension
    {
        /// <summary>
        /// 数组越界不会导致异常产生的扩展方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T Index<T>(this IList<T> value, int index)
        {
            if (index < 0)
                throw new IndexOutOfRangeException("数组索引不能小于0");
            if (index >= value.Count)
            {
                return default(T);
            }
            else
            {
                return value[index];
            }
        }
    }
}

