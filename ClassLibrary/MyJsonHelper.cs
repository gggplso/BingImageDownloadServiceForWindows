using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassLibrary
{
    public class MyJsonHelper
    {
        /// <summary>
        /// (简单json对象)读取Josn文件中的指定节点值，即通过Key获取Value
        /// </summary>
        /// <param name="strJsonFile">Json路径文件名</param>
        /// <param name="strConfigKey">指定需要获取的节点Key</param>
        /// <returns>返回指定节点的值Value</returns>
        public static string ReadJsonConfig(string strJsonFile, string strConfigKey)
        {
            string strResult = string.Empty;
            try
            {
                JObject keyValuePairs = JObject.Parse(File.ReadAllText(strJsonFile));
                strResult = keyValuePairs[strConfigKey].ToString();
            }
            catch (Exception ex)
            {
                MyLogHelper.GetExceptionLocation(ex);
            }
            return strResult;
        }
        /// <summary>
        /// (嵌套格式)读取Josn文件中的指定节点值，即通过Key获取Value
        /// </summary>
        /// <param name="strJsonFile">Json路径文件名</param>
        /// <param name="strSection">需要获取的KeyValue所在的上级节点</param>
        /// <param name="strConfigKey">指定需要获取的节点Key</param>
        /// <returns>返回指定节点的值Value</returns>
        public static string ReadJsonConfig(string strJsonFile, string strSection, string strConfigKey)
        {
            string strResult = string.Empty;
            try
            {
                //Newtonsoft.Json.Linq.JObject keyValuePairs = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(strJsonFile);
                //上面的代码是原文，但因为【使用UFT-8解析的结果带有BOM头】，所以需要使用下面自己写的方法来转换
                Newtonsoft.Json.Linq.JObject keyValuePairs = ReadJsonFileToClass<Newtonsoft.Json.Linq.JObject>(strJsonFile);
                strResult = keyValuePairs[strSection][strConfigKey].ToString();
            }
            catch (Exception ex)
            {
                MyLogHelper.GetExceptionLocation(ex);
            }
            return strResult;
        }
        /// <summary>
        /// 反序列化：读取Json文件到(泛型)类对象
        /// </summary>
        /// <typeparam name="T">泛型类</typeparam>
        /// <param name="strJsonFile">Json文件路径</param>
        /// <returns>返回泛型类 T 的对象</returns>
        public static T ReadJsonFileToClass<T>(string strJsonFile)
        {
            try
            {
                T jsonClass;
                if (File.Exists(strJsonFile))
                {
                    #region 抛异常的原因
                    /*原因：
                                        使用UTF-8解析的结果带有BOM头，
                                有的windows系统用记事本打开保存为UTF-8时模板带有BOM头(Byte Order Mark:字节序标记)，
                                文件读取转成byte[] 时，首部位置有固定的编码：EF BB BF。
                                为了防止这类情况发生，需要做一个判断，去除BOM头就可以了。
                                */
                    #endregion
                    //Unexpected character encountered while parsing value: o. Path '', line 0, position 0.
                    using (FileStream fsRead = new FileStream(strJsonFile, FileMode.Open, FileAccess.Read))
                    {
                        byte[] bufferSource = new byte[fsRead.Length];
                        fsRead.Read(bufferSource, 0, bufferSource.Length);
                        byte[] utf8Bom = new byte[] { 0xef, 0xbb, 0xbf };
                        string strJson = System.Text.Encoding.UTF8.GetString(bufferSource);
                        if (bufferSource[0] == utf8Bom[0] && bufferSource[0] == utf8Bom[0] && bufferSource[0] == utf8Bom[0])
                        {
                            int copyLength = bufferSource.Length - 3;
                            byte[] buffer = new byte[copyLength];
                            Buffer.BlockCopy(bufferSource, 3, buffer, 0, copyLength);
                            strJson = System.Text.Encoding.UTF8.GetString(buffer);
                        }
                        jsonClass = JsonConvert.DeserializeObject<T>(strJson);
                    }
                    return jsonClass;
                }
                else
                {
                    string strThrowTemp = $"指定的文件{strJsonFile}不存在。";

                    ClassLibrary.MyLogHelper.LogSplit(strThrowTemp);
                    throw new Exception(strThrowTemp);
                }
            }
            catch (Exception e1)
            {
                MyLogHelper.GetExceptionLocation(e1);
                return default(T);
            }
        }
        /// <summary>
        /// 序列化：将(泛型)类对象写入到Json文件
        /// </summary>
        /// <typeparam name="T">泛型类 T </typeparam>
        /// <param name="classSource">类的对象</param>
        /// <param name="strJsonFile">写入目标Json文件</param>
        /// <returns>写入成功返回True，否则返回false</returns>
        public static bool WriteClassToJsonFile<T>(T classSource, string strJsonFile)
        {
            try
            {
                if (classSource != null)
                {
                    string strJson = JsonConvert.SerializeObject(classSource, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(strJsonFile, strJson, Encoding.UTF8);
                }
                return true;
            }
            catch (Exception e1)
            {
                MyLogHelper.GetExceptionLocation(e1);
                return false;
            }
        }
        /// <summary>
        /// 将字符串数组序列化为 JSON 字符串
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string SerializeStringArrayToString(string[] array)
        {
            //return JsonConvert.SerializeObject(array);
            return JsonConvert.SerializeObject(array ?? Array.Empty<string>());
        }
        /// <summary>
        /// 将 JSON 字符串反序列化为字符串数组
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string[] DeserializeStringArrayFromString(string json)
        {
            //return JsonConvert.DeserializeObject<string[]>(json);
            try
            {
                return JsonConvert.DeserializeObject<string[]>(json ?? "") ?? Array.Empty<string>();
            }
            catch (JsonReaderException)
            {
                // 处理无效 JSON 输入，可以选择返回默认值（如空数组）或抛出特定异常
                return Array.Empty<string>();
            }
        }
    }
}
