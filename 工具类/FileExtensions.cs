using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHelper
{
    public static class FileExtensions
    {
        /// <summary>
        /// 异步，读取文件内容
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<string> ReadFileAsync(this string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("未找到文件", path);
            }
            using (StreamReader sr = File.OpenText(path))
            {
                return await sr.ReadToEndAsync();
            }
        }
        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadFile(this string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("未找到文件", path);
            }
            using (StreamReader sr = File.OpenText(path))
            {
                return sr.ReadToEnd();
            }
        }
        /// <summary>
        /// 将content异步写入指定文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static async Task WriteFileAsync(this string path, string content)
        {
            using (StreamWriter sr = File.AppendText(path))
            {
                await sr.WriteLineAsync(content);
                await sr.FlushAsync();
            }
        }
        /// <summary>
        /// 将content写入指定文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static void WriteFile(this string path, string content)
        {
            using (StreamWriter sr = File.AppendText(path))
            {
                sr.WriteLine(content);
                sr.Flush();
            }
        }
        /// <summary>
        /// 根据文件路径，获取文件夹名(Windows系统)
        /// </summary>
        /// <param name="fullname">文件夹名，最后不带"\"</param>
        /// <returns></returns>
        public static string GetDirecory(this string fullname)
        {
            int index = fullname.LastIndexOf('\\');
            if (index == -1)
            {
                return string.Empty;
            }
            else
            {
                return fullname.Substring(0, index);
            }
        }
        /// <summary>
        /// 获取文件路径的文件名(Windows系统)
        /// </summary>
        /// <param name="fullname"></param>
        /// <returns></returns>
        public static string GetFileName(this string fullname)
        {
            int index = fullname.LastIndexOf('\\');
            if (index == -1)
            {
                return fullname;
            }
            else
            {
                return fullname.Substring(index + 1);
            }
        }
    }
}

