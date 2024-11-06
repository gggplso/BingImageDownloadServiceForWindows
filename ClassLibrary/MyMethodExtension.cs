using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
	public class MyMethodExtension
	{
		/// <summary>
		/// 查询 FileHashes 表，判断字段 HashValue 中是否已存在传入参数的 Hash 值
		/// </summary>
		/// <param name="hash">传入参数的 Hash 值</param>
		/// <returns>存在返回：True，否则返回：False。</returns>
		public static bool FileExistsByHash(string hash)
		{
			bool result = false;

			ClassLibrary.MySQLiteHelper mySQLite = new MySQLiteHelper(ClassLibrary.ShareClass._sqliteConnectionString);

			// 查询数据是否存在
			var conditions = new (string column, object value)[] { ("HashValue", hash) };
			long? count = mySQLite.DataExists("FileHashes", conditions);
			if (count > 0)
			{
				result = true;
			}
			return result;
		}
		/// <summary>
		/// 将文件的Hash值等相关信息插入表中
		/// </summary>
		/// <param name="filePath">文件完整路径</param>
		/// <param name="hash">文件的Hash值</param>
		/// <param name="IsPortrait">图片的方向是否是竖向的（对应数据库中：横向：0 否，竖向：1 是）
		/// <para/>（Page Orientation即页面方向，指的是矩形平面以长边为底还是以短边为底，分为 Portrait（纵向：Portrait指的是人物画像）和 Landscape（横向：Landscape指的则是风景画）两类。）</param>
		public static void InsertHashData(string filePath, string hash, int? IsPortrait = null)
		{
			var necessary = new (string column, object value)[] {
									("FileName",Path.GetFileName(filePath)),
									("FilePath",filePath),
									("HashValue",hash),
									("IsPortrait",IsPortrait),
							};
			ClassLibrary.MySQLiteHelper mySQLite = new MySQLiteHelper(ClassLibrary.ShareClass._sqliteConnectionString);
			mySQLite.InsertData("FileHashes", necessary);
		}
	}
}
