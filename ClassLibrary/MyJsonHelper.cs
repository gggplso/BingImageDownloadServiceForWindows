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
		/// 反序列化：读取Json数据到(泛型)类对象
		/// </summary>
		/// <typeparam name="T">泛型类</typeparam>
		/// <param name="strJsonFile">Json数据字符串</param>
		/// <returns>返回泛型类 T 的对象</returns>
		public static T ReadJsonToClass<T>(string strJson)
		{
			try
			{
				T jsonClass = JsonConvert.DeserializeObject<T>(strJson);
				return jsonClass;
			}
			catch (Exception e1)
			{
				MyLogHelper.GetExceptionLocation(e1);
			}
			return default(T);
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



		#region 处理获取到的JSON数据
		// 额外的专属方法
		// 访问Windows聚焦API图片地址：
		// 得到的JSON数据中包含了另一个带转义符的Json数据，所以需要单独处理，以获取真正想要的Windows聚焦API图片下载地址

		/// <summary>
		/// 配置序列化选项，禁用 HTML 转义
		/// </summary>
		private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
		{
			Formatting = Newtonsoft.Json.Formatting.Indented,
			StringEscapeHandling = StringEscapeHandling.EscapeHtml,
		};
		/// <summary>
		/// 从 JSON 字符串中提取 Windows Spotlight 图片下载地址，提取指定的 JSON 节点并生成新的 JSON 字符串。。
		/// </summary>
		/// <param name="jsonString">输入的 JSON 字符串。</param>
		/// <returns>返回包含聚焦图片下载地址的新JSON字符串数据，如果未找到则返回 null。</returns>
		public static string ExtractWindowsSpotlightDownloadUrl(string jsonString)
		{
			string strTemp = string.Empty;
			try
			{
				// 解析 JSON 字符串
				var jsonObject = JObject.Parse(jsonString);

				// 检查是否包含 "batchrsp" 键
				if (jsonObject.ContainsKey("batchrsp"))
				{
					var batchrspElement = jsonObject["batchrsp"] as JObject;

					// 检查是否包含 "items" 键
					if (batchrspElement != null && batchrspElement.ContainsKey("items"))
					{
						var itemsElement = batchrspElement["items"] as JArray;

						if (itemsElement != null)
						{
							// 遍历 "items" 数组
							foreach (var itemElement in itemsElement)
							{
								// 检查是否包含 "item" 键
								if (itemElement is JObject itemObject && itemObject.ContainsKey("item"))
								{
									// 解析内部的 JSON 字符串
									var innerJsonString = itemElement["item"].ToString();
									var innerJsonObject = JObject.Parse(innerJsonString);

									// 创建一个新的 JObject 来存储提取的节点
									var newJsonObject = new JObject();
									// 定义需要提取的节点路径
									var nodesToExtract = new[]
									{
										"image_fullscreen_001_landscape",
										"image_fullscreen_001_portrait",
										"hs1_title_text",
										"title_text"
									};
									// 遍历需要提取的节点路径
									foreach (var nodePath in nodesToExtract)
									{
										// 尝试获取节点
										var token = innerJsonObject.SelectToken("ad").SelectToken(nodePath);

										if (token != null)
										{
											// 将节点添加到新的 JObject 中
											newJsonObject[nodePath] = token;
										}
									}

									// 序列化为格式化的字符串
									var formattedJsonNew = JsonConvert.SerializeObject(newJsonObject, _jsonSerializerSettings);

									// 如果只需要处理第一个 "item"，可以在这里返回
									return formattedJsonNew;
								}
							}
						}
						else
						{
							strTemp = $"JSON 数据中的 'items' 不是数组。";
                            Console.WriteLine(strTemp);
							ClassLibrary.MyLogHelper.LogSplit(strTemp);
						}
					}
					else
					{
						strTemp = $"JSON 数据中未找到 'items' 键。";
                        Console.WriteLine(strTemp);
                        ClassLibrary.MyLogHelper.LogSplit(strTemp);
					}
				}
				else
				{
					strTemp = $"JSON 数据中未找到 'batchrsp' 键。";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(strTemp);
				}
			}
			catch (Exception ex)
			{
				strTemp = $"解析 JSON 时发生错误: {ex.Message}";
                Console.WriteLine(strTemp);
                ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
				ClassLibrary.MyLogHelper.LogSplit(strTemp);
			}

			return null;
		}

		#endregion
	}
}
