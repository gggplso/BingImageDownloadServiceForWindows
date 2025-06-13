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
using System.Collections;

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
		///// <summary>
		///// 反序列化：读取Json数据到(泛型)类对象
		///// </summary>
		///// <typeparam name="T">泛型类</typeparam>
		///// <param name="strJsonFile">Json数据字符串</param>
		///// <returns>返回泛型类 T 的对象</returns>
		//public static T ReadJsonToClass<T>(string strJson)
		//{
		//	try
		//	{
		//		// 先尝试作为数组解析
		//		if (strJson.TrimStart().StartsWith("["))
		//		{
		//			// 如果是数组，创建List<T>类型并反序列化
		//			Type listType = typeof(List<>).MakeGenericType(typeof(T));
		//			object list = JsonConvert.DeserializeObject(strJson, listType);

		//			// 取第一个元素返回（或根据业务需求调整）
		//			return ((IList)list).Count > 0 ? (T)((IList)list)[0] : default(T);
		//		}

		//		// 否则按普通对象解析
		//		return JsonConvert.DeserializeObject<T>(strJson);
		//	}
		//	catch (Exception e1)
		//	{
		//		MyLogHelper.GetExceptionLocation(e1);
		//	}
		//	return default(T);
		//}
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
			// - 使生成的JSON字符串有缩进格式，便于阅读
			Formatting = Newtonsoft.Json.Formatting.Indented,
			// - 对HTML字符进行转义处理，防止XSS攻击
			StringEscapeHandling = StringEscapeHandling.EscapeHtml,
		};
		/// <summary>
		/// 从 JSON 字符串中提取 Windows Spotlight 图片下载地址，提取指定的 JSON 节点并生成新的 JSON 字符串。。
		/// </summary>
		/// <param name="jsonString">输入的 JSON 字符串。</param>
		/// <returns>返回包含聚焦图片下载地址的新JSON字符串数据，如果未找到则返回 null。</returns>
		public static string ExtractWindowsSpotlightDownloadUrl(string jsonString)
		{
			// 用于存储临时日志信息的字符串
			string strTemp = string.Empty;
			try
			{
				// 1. 解析输入的JSON字符串为JObject对象
				var jsonObject = JObject.Parse(jsonString);

				// 2. 检查JSON对象中是否包含"batchrsp"键
				if (jsonObject.ContainsKey("batchrsp"))
				{
					// 3. 获取batchrsp对象并转换为JObject
					var batchrspElement = jsonObject["batchrsp"] as JObject;

					// 4. 检查batchrsp对象中是否包含"items"键
					if (batchrspElement != null && batchrspElement.ContainsKey("items"))
					{
						// 5. 获取items数组并转换为JArray
						var itemsElement = batchrspElement["items"] as JArray;

						if (itemsElement != null)
						{
							// 6. 遍历items数组中的每个元素
							foreach (var itemElement in itemsElement)
							{
								// 7. 检查当前元素是否为JObject且包含"item"键
								if (itemElement is JObject itemObject && itemObject.ContainsKey("item"))
								{
									// 8. 获取item键的值并转换为字符串
									var innerJsonString = itemElement["item"].ToString();
									// 9. 解析内部的JSON字符串为JObject
									var innerJsonObject = JObject.Parse(innerJsonString);

									// 10. 创建新的JObject来存储提取的节点
									var newJsonObject = new JObject();
									// 11. 定义需要从ad节点中提取的字段路径
									var nodesToExtract = new[]
									{
										"image_fullscreen_001_landscape", // 横向图片URL
										"image_fullscreen_001_portrait",  // 纵向图片URL
										"hs1_title_text",                 // 热点1标题文本
										"title_text"                      // 主标题文本
									};

									// 12. 遍历需要提取的节点路径（也就是从原始Json文件中找"ad"节点，然后从"ad"节点中循环找nodesToExtract相的路径，提取其下的json数据）
									foreach (var nodePath in nodesToExtract)
									{
										// 13. 从ad节点中获取指定路径的token
										var token = innerJsonObject.SelectToken("ad").SelectToken(nodePath);

										if (token != null)
										{
											// 14. 将获取到的token添加到新JObject中
											newJsonObject[nodePath] = token;
										}
									}
									#region 老的，过期的Json处理

									/*
									 * 
									 原始Json
										{
										  "batchrsp": {
											"ver": "1.0",
											"items": [
											  {
												"item": "{\"f\":\"raf\",\"v\":\"1.0\",\"rdr\":[{\"c\":\"CDM\",\"u\":\"LockScreenHotSpots\"}],
												
									\"ad\":{
									         \"image_fullscreen_001_landscape\":{
									                 \"t\":\"img\",
									                 \"w\":\"1920\",
									                 \"h\":\"1080\",
									                 \"u\":\"https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RW1hdbA?ver=a873\",
									                 \"sha256\":\"FWpgKU8VIhsCRldNG9n55I5OIr5MzlO5kbVk9htPNVQ=\",
									                 \"fileSize\":\"1576426\"},
									         \"image_fullscreen_001_portrait\":{\"t\":\"img\",\"w\":\"1080\",\"h\":\"1920\",\"u\":\"https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RW1hdbC?ver=d916\",\"sha256\":\"+5dRvU8unVFr/Djma4A488h8Y2KxGRhIVtNZ9nK7aRE=\",\"fileSize\":\"1594603\"},
									         \"hs1_title_text\":{\"t\":\"txt\",\"tx\":\"这些鸟儿正在一个便于观赏的位置过冬，如果你想在自然环境下看到它们...\"},\"hs1_icon\":{\"t\":\"txt\",\"tx\":\"\"},\"hs1_cta_text\":{\"t\":\"txt\",\"tx\":\"这里就是绝佳的观赏地\"},\"hs1_destination_url\":{\"t\":\"url\",\"u\":\"microsoft-edge:https://cn.bing.com/images/search?q=lake+kussharo+whooper+swan&filters=IsConversation:%22True%22+BTWLKey:%22WhooperSwansJapan%22+BTWLType:%22Trivia%22&trivia=1&FORM=EMSDS0&qft=+filterui:photo-photo&ensearch=0\"},\"hs1_x_coordinate_001_landscape\":{\"t\":\"txt\",\"tx\":\"72\"},\"hs1_y_coordinate_001_landscape\":{\"t\":\"txt\",\"tx\":\"72\"},\"hs1_x_coordinate_001_portrait\":{\"t\":\"txt\",\"tx\":\"72\"},\"hs1_y_coordinate_001_portrait\":{\"t\":\"txt\",\"tx\":\"72\"},\"hs1_x_coordinate_002_landscape\":{\"t\":\"txt\",\"tx\":\"\"},\"hs1_y_coordinate_002_landscape\":{\"t\":\"txt\",\"tx\":\"\"},\"hs1_x_coordinate_002_portrait\":{\"t\":\"txt\",\"tx\":\"\"},\"hs1_y_coordinate_002_portrait\":{\"t\":\"txt\",\"tx\":\"\"},\"hs1_x_coordinate_003_landscape\":{\"t\":\"txt\",\"tx\":\"\"},\"hs1_y_coordinate_003_landscape\":{\"t\":\"txt\",\"tx\":\"\"},\"hs1_x_coordinate_003_portrait\":{\"t\":\"txt\",\"tx\":\"\"},\"hs1_y_coordinate_003_portrait\":{\"t\":\"txt\",\"tx\":\"\"},\"hs1_x_coordinate_004_landscape\":{\"t\":\"txt\",\"tx\":\"\"},\"hs1_y_coordinate_004_landscape\":{\"t\":\"txt\",\"tx\":\"\"},\"hs1_x_coordinate_004_portrait\":{\"t\":\"txt\",\"tx\":\"\"},\"hs1_y_coordinate_004_portrait\":{\"t\":\"txt\",\"tx\":\"\"},\"hs2_title_text\":{\"t\":\"txt\",\"tx\":\"它们是世界上最重的鸟之一，对于这种鸟来说，它们更愿意浮在湖面上或在空中飞翔，而不是...\"},\"hs2_icon\":{\"t\":\"txt\",\"tx\":\"\"},\"hs2_cta_text\":{\"t\":\"txt\",\"tx\":\"站着不动\"},\"hs2_destination_url\":{\"t\":\"url\",\"u\":\"microsoft-edge:https://cn.bing.com/spotlight?q=大天鹅&spotlightId=WhooperSwansJapan&FORM=EMSDS0\"},\"hs2_x_coordinate_001_landscape\":{\"t\":\"txt\",\"tx\":\"800\"},\"hs2_y_coordinate_001_landscape\":{\"t\":\"txt\",\"tx\":\"550\"},\"hs2_x_coordinate_001_portrait\":{\"t\":\"txt\",\"tx\":\"400\"},\"hs2_y_coordinate_001_portrait\":{\"t\":\"txt\",\"tx\":\"800\"},\"hs2_x_coordinate_002_landscape\":{\"t\":\"txt\",\"tx\":\"\"},\"hs2_y_coordinate_002_landscape\":{\"t\":\"txt\",\"tx\":\"\"},\"hs2_x_coordinate_002_portrait\":{\"t\":\"txt\",\"tx\":\"\"},\"hs2_y_coordinate_002_portrait\":{\"t\":\"txt\",\"tx\":\"\"},\"hs2_x_coordinate_003_landscape\":{\"t\":\"txt\",\"tx\":\"\"},\"hs2_y_coordinate_003_landscape\":{\"t\":\"txt\",\"tx\":\"\"},\"hs2_x_coordinate_003_portrait\":{\"t\":\"txt\",\"tx\":\"\"},\"hs2_y_coordinate_003_portrait\":{\"t\":\"txt\",\"tx\":\"\"},\"hs2_x_coordinate_004_landscape\":{\"t\":\"txt\",\"tx\":\"\"},\"hs2_y_coordinate_004_landscape\":{\"t\":\"txt\",\"tx\":\"\"},\"hs2_x_coordinate_004_portrait\":{\"t\":\"txt\",\"tx\":\"\"},\"hs2_y_coordinate_004_portrait\":{\"t\":\"txt\",\"tx\":\"\"},
									         \"title_text\":{\"t\":\"txt\",\"tx\":\"日本，屈斜路湖\"},\"copyright_text\":{\"t\":\"txt\",\"tx\":\"© Danita Delimont on Offset / Shutterstock\"},\"title_destination_url\":{\"t\":\"url\",\"u\":\"microsoft-edge:https://cn.bing.com/spotlight?q=大天鹅&spotlightId=WhooperSwansJapan&FORM=EMSDS0\"},\"image_fullscreen_002_landscape\":{\"t\":\"img\",\"u\":\"https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RW1harc?ver=6eed\",\"w\":\"1920\",\"h\":\"1200\",\"sha256\":\"eRBc9vSEj8mDM7kWR2y3TxwLvKl8LvwcubPLinFfx4A=\",\"fileSize\":\"736\"},\"image_fullscreen_002_portrait\":{\"t\":\"img\",\"u\":\"https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RW1h5tX?ver=b58a\",\"w\":\"1200\",\"h\":\"1920\",\"sha256\":\"eRBc9vSEj8mDM7kWR2y3TxwLvKl8LvwcubPLinFfx4A=\",\"fileSize\":\"736\"},\"image_fullscreen_003_landscape\":{\"t\":\"img\",\"u\":\"https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RW1h5tY?ver=ed48\",\"w\":\"1920\",\"h\":\"1280\",\"sha256\":\"eRBc9vSEj8mDM7kWR2y3TxwLvKl8LvwcubPLinFfx4A=\",\"fileSize\":\"736\"},\"image_fullscreen_003_portrait\":{\"t\":\"img\",\"u\":\"https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RW1h7VR?ver=b988\",\"w\":\"1280\",\"h\":\"1920\",\"sha256\":\"eRBc9vSEj8mDM7kWR2y3TxwLvKl8LvwcubPLinFfx4A=\",\"fileSize\":\"736\"},\"image_fullscreen_004_landscape\":{\"t\":\"img\",\"u\":\"https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RW1hard?ver=be2e\",\"w\":\"1920\",\"h\":\"1440\",\"sha256\":\"eRBc9vSEj8mDM7kWR2y3TxwLvKl8LvwcubPLinFfx4A=\",\"fileSize\":\"736\"},\"image_fullscreen_004_portrait\":{\"t\":\"img\",\"u\":\"https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RW1hdbE?ver=09fd\",\"w\":\"1440\",\"h\":\"1920\",\"sha256\":\"eRBc9vSEj8mDM7kWR2y3TxwLvKl8LvwcubPLinFfx4A=\",\"fileSize\":\"736\"},\"options\":{\"t\":\"txt\",\"tx\":\"4\"},\"tr_hint_hs1_1\":{\"t\":\"url\",\"u\":\"https://ris.api.iris.microsoft.com/v1/a/hover?CID=128000000004742929&region=CN&lang=ZH-CN&oem=&devFam=&ossku=&cmdVer=&mo=&cap=&EID={EID}&&PID=425967078&UIT=M&TargetID=700490783&AN=1212218756&PG=PC000P0FR5.0000000IRS&REQASID=3D64AD7FCA934CBAAE04EB4C4071E9F9&UNID=209567&ID=00000000000000000000000000000001&ASID={ASID}&DEVOSVER=&REQT=20241111T081119&TIME={DATETIME}&itemindex=1&&DS_EVTID=e4f1551e9818487ab48b1e0a04bc14cf&WFIDS=&ARCRAS=&CLR=CDM&bSrc=i.m\"},\"tr_click_hs1_1\":{\"t\":\"url\",\"u\":\"https://ris.api.iris.microsoft.com/v1/a/click?CID=128000000004742929&region=CN&lang=ZH-CN&oem=&devFam=&ossku=&cmdVer=&mo=&cap=&EID={EID}&&PID=425967078&UIT=M&TargetID=700490783&AN=1212218756&PG=PC000P0FR5.0000000IRS&REQASID=3D64AD7FCA934CBAAE04EB4C4071E9F9&UNID=209567&ID=00000000000000000000000000000001&ASID={ASID}&DEVOSVER=&REQT=20241111T081119&TIME={DATETIME}&itemindex=2&&DS_EVTID=e4f1551e9818487ab48b1e0a04bc14cf&WFIDS=&ER_AC=&ARCRAS=&CLR=CDM&bSrc=i.m\"},\"tr_hint_hs2_1\":{\"t\":\"url\",\"u\":\"https://ris.api.iris.microsoft.com/v1/a/hover?CID=128000000004742929&region=CN&lang=ZH-CN&oem=&devFam=&ossku=&cmdVer=&mo=&cap=&EID={EID}&&PID=425967078&UIT=M&TargetID=700490783&AN=1212218756&PG=PC000P0FR5.0000000IRS&REQASID=3D64AD7FCA934CBAAE04EB4C4071E9F9&UNID=209567&ID=00000000000000000000000000000001&ASID={ASID}&DEVOSVER=&REQT=20241111T081119&TIME={DATETIME}&itemindex=3&&DS_EVTID=e4f1551e9818487ab48b1e0a04bc14cf&WFIDS=&ARCRAS=&CLR=CDM&bSrc=i.m\"},\"tr_click_hs2_1\":{\"t\":\"url\",\"u\":\"https://ris.api.iris.microsoft.com/v1/a/click?CID=128000000004742929&region=CN&lang=ZH-CN&oem=&devFam=&ossku=&cmdVer=&mo=&cap=&EID={EID}&&PID=425967078&UIT=M&TargetID=700490783&AN=1212218756&PG=PC000P0FR5.0000000IRS&REQASID=3D64AD7FCA934CBAAE04EB4C4071E9F9&UNID=209567&ID=00000000000000000000000000000001&ASID={ASID}&DEVOSVER=&REQT=20241111T081119&TIME={DATETIME}&itemindex=4&&DS_EVTID=e4f1551e9818487ab48b1e0a04bc14cf&WFIDS=&ER_AC=&ARCRAS=&CLR=CDM&bSrc=i.m\"},\"tr_hint_hs3_1\":{\"t\":\"url\",\"u\":\"https://ris.api.iris.microsoft.com/v1/a/hover?CID=128000000004742929&region=CN&lang=ZH-CN&oem=&devFam=&ossku=&cmdVer=&mo=&cap=&EID={EID}&&PID=425967078&UIT=M&TargetID=700490783&AN=1212218756&PG=PC000P0FR5.0000000IRS&REQASID=3D64AD7FCA934CBAAE04EB4C4071E9F9&UNID=209567&ID=00000000000000000000000000000001&ASID={ASID}&DEVOSVER=&REQT=20241111T081119&TIME={DATETIME}&itemindex=5&&DS_EVTID=e4f1551e9818487ab48b1e0a04bc14cf&WFIDS=&ARCRAS=&CLR=CDM&bSrc=i.m\"},\"tr_click_hs3_1\":{\"t\":\"url\",\"u\":\"https://ris.api.iris.microsoft.com/v1/a/click?CID=128000000004742929&region=CN&lang=ZH-CN&oem=&devFam=&ossku=&cmdVer=&mo=&cap=&EID={EID}&&PID=425967078&UIT=M&TargetID=700490783&AN=1212218756&PG=PC000P0FR5.0000000IRS&REQASID=3D64AD7FCA934CBAAE04EB4C4071E9F9&UNID=209567&ID=00000000000000000000000000000001&ASID={ASID}&DEVOSVER=&REQT=20241111T081119&TIME={DATETIME}&itemindex=6&&DS_EVTID=e4f1551e9818487ab48b1e0a04bc14cf&WFIDS=&ER_AC=&ARCRAS=&CLR=CDM&bSrc=i.m\"},\"tr_click_title_1\":{\"t\":\"url\",\"u\":\"https://ris.api.iris.microsoft.com/v1/a/click?CID=128000000004742929&region=CN&lang=ZH-CN&oem=&devFam=&ossku=&cmdVer=&mo=&cap=&EID={EID}&&PID=425967078&UIT=M&TargetID=700490783&AN=1212218756&PG=PC000P0FR5.0000000IRS&REQASID=3D64AD7FCA934CBAAE04EB4C4071E9F9&UNID=209567&ID=00000000000000000000000000000001&ASID={ASID}&DEVOSVER=&REQT=20241111T081119&TIME={DATETIME}&itemindex=7&&DS_EVTID=e4f1551e9818487ab48b1e0a04bc14cf&WFIDS=&ER_AC=&ARCRAS=&CLR=CDM&bSrc=i.m\"}},\"prm\":{\"_id\":\"WW_128000000004742929_ZH-CN\",\"eid\":{\"t\":\"txt\",\"tx\":\"128000000004742929\"},\"expand_hotspots\":0,\"expireTime\":\"2035-12-31T08:00:00\",\"feedback_enabled\":1,\"hide_titles\":0,\"iaf_dislike\":{\"t\":\"url\",\"u\":\"https://ris.api.iris.microsoft.com/v1/a/dislike?CID=128000000004742929&region=CN&lang=ZH-CN&oem=&devFam=&ossku=&cmdVer=&mo=&cap=&EID={EID}&&PID=425967078&UIT=M&TargetID=700490783&AN=1212218756&PG=PC000P0FR5.0000000IRS&REQASID=3D64AD7FCA934CBAAE04EB4C4071E9F9&UNID=209567&ID=00000000000000000000000000000001&ASID={ASID}&DEVOSVER=&REQT=20241111T081119&TIME={DATETIME}&&DS_EVTID=e4f1551e9818487ab48b1e0a04bc14cf&WFIDS=&ARCRAS=&CLR=CDM&bSrc=i.f\"},\"iaf_impr\":{\"t\":\"url\",\"u\":\"https://ris.api.iris.microsoft.com/v1/a/iaf_impression?CID=128000000004742929&region=CN&lang=ZH-CN&oem=&devFam=&ossku=&cmdVer=&mo=&cap=&EID={EID}&&PID=425967078&UIT=M&TargetID=700490783&AN=1212218756&PG=PC000P0FR5.0000000IRS&REQASID=3D64AD7FCA934CBAAE04EB4C4071E9F9&UNID=209567&ID=00000000000000000000000000000001&ASID={ASID}&DEVOSVER=&REQT=20241111T081119&TIME={DATETIME}&&DS_EVTID=e4f1551e9818487ab48b1e0a04bc14cf&WFIDS=&ARCRAS=&CLR=CDM\"},\"iaf_like\":{\"t\":\"url\",\"u\":\"https://ris.api.iris.microsoft.com/v1/a/like?CID=128000000004742929&region=CN&lang=ZH-CN&oem=&devFam=&ossku=&cmdVer=&mo=&cap=&EID={EID}&&PID=425967078&UIT=M&TargetID=700490783&AN=1212218756&PG=PC000P0FR5.0000000IRS&REQASID=3D64AD7FCA934CBAAE04EB4C4071E9F9&UNID=209567&ID=00000000000000000000000000000001&ASID={ASID}&DEVOSVER=&REQT=20241111T081119&TIME={DATETIME}&&DS_EVTID=e4f1551e9818487ab48b1e0a04bc14cf&WFIDS=&ER_AC=&ARCRAS=&CLR=CDM&bSrc=i.f\"},\"reuseCount\":-1,\"rotationPeriod\":82800,\"requiresNetwork\":0,\"startTime\":\"2024-01-30T18:37:22\",\"_imp\":\"post:https://arc.msn.com/v3/Delivery/Events/Impression=&PID=425967078&TID=700490783&CID=128000000004742929&BID=1212218756&PG=PC000P0FR5.0000000IRS&TPID=425967078&REQASID=3D64AD7FCA934CBAAE04EB4C4071E9F9&ASID={ASID}&TIME={DATETIME}&SLOT=1&REQT=20241111T081119&MA_Score=2&&DS_EVTID=e4f1551e9818487ab48b1e0a04bc14cf&BCNT=1&PG=PC000P0FR5.0000000G77&UNID=209567&MAP_TID=EBD7C223-214B-45C4-9D5B-F8C38C25D38B&ASID=3D64AD7FCA934CBAAE04EB4C4071E9F9&REQASID=3D64AD7FCA934CBAAE04EB4C4071E9F9&ARC=1&EMS=1&LOCALE=ZH-CN&COUNTRY=CN&HTD=-1&LANG=2052&DEVLANG=ZH&CIP=122.227.186.30&OPTOUTSTATE=0&HTTPS=1&MARKETBASEDCOUNTRY=CN&CLR=CDM&CFMT=&SFT=GIF%2CAGIF%2CJPEG%2CJPG%2CBMP%2CPNG&H=320&W=240&FESVER=1.3&PL=ZH-CN&IEPE=1&CHNL=CFD\"}}"
									
											  }
											],
											"refreshtime": "2024-11-18T08:11:19"
										  }
										}

									 提取后的Json
										{
										  "image_fullscreen_001_landscape": {
											"t": "img",
											"w": "1920",
											"h": "1080",
											"u": "https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RW1hdbA?ver=a873",
											"sha256": "FWpgKU8VIhsCRldNG9n55I5OIr5MzlO5kbVk9htPNVQ=",
											"fileSize": "1576426"
										  },
										  "image_fullscreen_001_portrait": {
											"t": "img",
											"w": "1080",
											"h": "1920",
											"u": "https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RW1hdbC?ver=d916",
											"sha256": "+5dRvU8unVFr/Djma4A488h8Y2KxGRhIVtNZ9nK7aRE=",
											"fileSize": "1594603"
										  },
										  "hs1_title_text": {
											"t": "txt",
											"tx": "这些鸟儿正在一个便于观赏的位置过冬，如果你想在自然环境下看到它们..."
										  },
										  "title_text": {
											"t": "txt",
											"tx": "日本，屈斜路湖"
										  }
										}
									 *
									 */

									#endregion

									// 15. 将新JObject序列化为格式化的JSON字符串
									var formattedJsonNew = JsonConvert.SerializeObject(newJsonObject, _jsonSerializerSettings);

									// 16. 返回处理后的JSON字符串(只处理第一个item)
									return formattedJsonNew;
								}
							}
						}
						else
						{
							// 17. 记录错误日志：items不是数组
							strTemp = $"JSON 数据中的 'items' 不是数组。";
							Console.WriteLine(strTemp);
							ClassLibrary.MyLogHelper.LogSplit(strTemp);
						}
					}
					else
					{
						// 18. 记录错误日志：未找到items键
						strTemp = $"JSON 数据中未找到 'items' 键。";
						Console.WriteLine(strTemp);
						ClassLibrary.MyLogHelper.LogSplit(strTemp);
					}
				}
				else
				{
					// 19. 记录错误日志：未找到batchrsp键
					strTemp = $"JSON 数据中未找到 'batchrsp' 键。";
					Console.WriteLine(strTemp);
					ClassLibrary.MyLogHelper.LogSplit(strTemp);
				}
			}
			catch (Exception ex)
			{
				// 20. 捕获并记录JSON解析过程中的异常
				strTemp = $"解析 JSON 时发生错误: {ex.Message}";
				Console.WriteLine(strTemp);
				ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
				ClassLibrary.MyLogHelper.LogSplit(strTemp);
			}

			// 21. 如果出现任何错误或未找到所需数据，返回null
			return null;
		}

		#endregion
	}
}
