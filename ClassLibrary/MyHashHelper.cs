using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Net.Http;

namespace ClassLibrary
{
	public class MyHashHelper
	{
		/// <summary>
		/// 获取文件的哈希值HASH(本地文件)
		/// </summary>
		/// <param name="filePath">传入文件路径</param>
		/// <param name="hashAlgorithm">Hash类型</param>
		/// <returns>返回文件的指定Hash值</returns>
		public static async Task<string> GetFileHashAsync_Local(string filePath, HashAlgorithm hashAlgorithm)
		{
			try
			{
				if (string.IsNullOrEmpty(filePath))
				{
					throw new ArgumentException("文件路径不能为空", nameof(filePath));
				}

				if (!File.Exists(filePath))
				{
					throw new FileNotFoundException("指定的文件不存在", filePath);
				}

				using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					byte[] buffer = new byte[4096];
					int bytesRead;
					while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
					{
						hashAlgorithm.TransformBlock(buffer, 0, bytesRead, null, 0);
					}

					hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
					byte[] hashBytes = hashAlgorithm.Hash;

					string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();

					return hash;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return null;
			}
		}
		/// <summary>
		/// 获取文件的哈希值HASH(网上可下载文件)
		/// </summary>
		/// <param name="url">文件的下载路径</param>
		/// <param name="hashAlgorithm">Hash类型</param>
		/// <returns>返回文件的指定Hash值</returns>
		public static async Task<string> GetFileHashAsync_Url(string url, HashAlgorithm hashAlgorithm)
		{
			try
			{
				using (var response = await ClassLibrary.MyHttpServiceHelper.GetClient().GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
				{
					if (response.IsSuccessStatusCode)
					{
						using (var stream = await response.Content.ReadAsStreamAsync())
						{
							byte[] buffer = new byte[4096];
							int bytesRead;

							while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
							{
								hashAlgorithm.TransformBlock(buffer, 0, bytesRead, null, 0);
							}

							hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
							byte[] hashBytes = hashAlgorithm.Hash;
							string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();

							return hash;
						}
					}
					else
					{
						throw new Exception("请求失败，状态码: " + (int)response.StatusCode);
					}
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return null;
			}
		}
	}
}
