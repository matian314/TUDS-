using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace TUDS报文模型
{
    public static class DimensionHelpers
    {
        #region 辅助函数

        //几何 辅助功能函数
        public static int GetFlagIndex(int tStartIndex, char tSig, byte[] tM)
        {
            int sigIndex = 0;
            for (int i = tStartIndex; i < 1024 * 3; i++)
            {
                if (tM[i] == tSig)
                {
                    sigIndex = i;
                    break;
                }
            }
            return sigIndex;
        }

        public static int BSum(byte N1, byte N2)
        {
            int res;
            res = (N2 << 8) | N1;
            return res;
        }

        public static int BSum(byte N1, byte N2, byte N3)
        {
            int res;
            res = (N3 << 16) | (N2 << 8) | N1;
            return res;
        }

        public static double BSum100(byte N1, byte N2, byte N3)
        {
            double res = (double)((N3 << 16) | (N2 << 8) | N1) / 100;
            return res;
        }

        public static double BSum100(byte N1, byte N2)
        {
            double res = (double)((N2 << 8) | N1) / 100;
            return res;
        }

        public static double BSum10(byte N1, byte N2)
        {
            double res = (double)((N2 << 8) | N1) / 10;
            return res;
        }

        //下标转换成nL,nR名称
        public static string Index2LR(int t_index)
        {
            string res = "";
            if (t_index % 2 == 0)
                res = ((t_index / 2) + 1).ToString() + "L";
            else
                res = ((t_index / 2) + 1).ToString() + "R";
            return res;
        }
        #endregion
    }
}
