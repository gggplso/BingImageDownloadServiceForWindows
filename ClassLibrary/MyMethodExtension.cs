using System;
using System.Collections.Generic;
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
    }
}
