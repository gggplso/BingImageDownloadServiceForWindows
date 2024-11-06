using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
	public class MySQLiteHelper
	{
		private string _connectionString;

		public MySQLiteHelper(string connectionString)
		{
			_connectionString = connectionString;
		}

		/// <summary>
		/// 连接数据库
		/// </summary>
		/// <returns></returns>
		private SQLiteConnection GetConnection()
		{
			return new SQLiteConnection(_connectionString);
		}

		/// <summary>
		/// 执行传入的 SQL 脚本。
		/// </summary>
		/// <param name="script">要执行的 SQL 脚本。</param>
		/// <exception cref="ArgumentException"></exception>
		public void ExecuteScript(string script)
		{
			if (string.IsNullOrWhiteSpace(script))
			{
				throw new ArgumentException("脚本不能为空", nameof(script));
			}
			try
			{
				using (var connection = GetConnection())
				{
					connection.Open();
					using (var command = new SQLiteCommand(script, connection))
					{
						command.ExecuteNonQuery();
					}
				}
			}
			catch (Exception ex)
			{
				ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
			}
		}

		/// <summary>
		/// 建表
		/// </summary>
		/// <param name="tableName">表名</param>
		/// <param name="columns">列相关脚本，详细建表语句中列名括号内的那部分，如：CONSTRAINT uc_HashValue UNIQUE (HashValue)
		/// 示例：string createTableQuery = @"
		/// <para />CREATE TABLE IF NOT EXISTS FileHashes(
		///         Id INTEGER PRIMARY KEY AUTOINCREMENT,
		///         FilePath TEXT NOT NULL,FileName TEXT NOT NULL,
		///         HashValue TEXT NOT NULL,
		///         CONSTRAINT uc_HashValue UNIQUE (HashValue));
		/// <para />";</param>
		public void CreateTableIfNotExists(string tableName, string columnScript)
		{
			try
			{
				using (var connection = GetConnection())
				{
					connection.Open();
					string query = $"CREATE TABLE IF NOT EXISTS {tableName} ({columnScript});";
					using (var command = new SQLiteCommand(query, connection))
					{
						command.ExecuteNonQuery();
					}
				}
			}
			catch (Exception ex)
			{
				ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
			}
		}

		/// <summary>
		/// 检查表是否存在
		/// </summary>
		/// <param name="tableName">表名</param>
		/// <returns>表是否存在</returns>
		public bool TableExists(string tableName)
		{
			try
			{
				using (var connection = GetConnection())
				{
					connection.Open();
					string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
					using (var command = new SQLiteCommand(query, connection))
					{
						command.Parameters.AddWithValue("@tableName", tableName);
						using (var reader = command.ExecuteReader())
						{
							// 如果结果集有可以获取的行，则返回True
							return reader.HasRows;
						}
					}
				}
			}
			catch (Exception ex)
			{
				ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
				return false;
			}
		}

		/// <summary>
		/// 查询指定表中存在指定条件数据的行数
		/// </summary>
		/// <param name="tableName">表名</param>
		/// <param name="conditions">查询条件数组，每个元素是一个键值对字符串，如： ("HashValue", "abc123")； ("Age", 30)；</param>
		/// <returns>空：出错；count：表中数据行数</returns>
		public long? DataExists(string tableName, params (string column, object value)[] conditions)
		{
			try
			{
				using (var connection = GetConnection())
				{
					connection.Open();
					string query = $"SELECT COUNT(*) FROM {tableName} WHERE 1=1";
					if (conditions != null && conditions.Length > 0)
					{
						for (int i = 0; i < conditions.Length; i++)
						{
							query += $" AND {conditions[i].column} = @p{i}";
						}
					}
					query += ";";
					using (var command = new SQLiteCommand(query, connection))
					{
						if (conditions != null && conditions.Length > 0)
						{
							for (int i = 0; i < conditions.Length; i++)
							{
								command.Parameters.AddWithValue($"@p{i}", conditions[i].value);
							}
						}

						long count;
						object result = command.ExecuteScalar();
						if (result != null && Int64.TryParse(result.ToString(), out count))
						{
							return count;
						}
						else
						{
							return null;
						}
					}
				}
			}
			catch (Exception ex)
			{
				ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
				return null;
			}
		}

		/// <summary>
		/// 插入数据
		/// </summary>
		/// <param name="tableName">表名</param>
		/// <param name="necessary">必须的列和值数组，每个元素是一个键值对字符串，如： ("HashValue", "abc123")
		///  <para />用于拼凑：INSERT OR IGNORE INTO FileHashes(FilePath, FileName, HashValue) VALUES(@FilePath, @FileName, @HashValue);</param>
		public void InsertData(string tableName, (string column, object value)[] necessary)
		{
			if (necessary != null && necessary.Length > 0)
			{
				try
				{
					using (var connection = GetConnection())
					{
						connection.Open();
						StringBuilder sbColumns = new StringBuilder();
						StringBuilder sbValues = new StringBuilder();

						for (int i = 0; i < necessary.Length; i++)
						{
							sbColumns.Append(necessary[i].column + ",");
							sbValues.Append($"@p{i},");
						}
						sbColumns.Length--;
						sbValues.Length--;
						string query = $"INSERT OR IGNORE INTO {tableName}({sbColumns.ToString()}) VALUES ({sbValues.ToString()});";

						using (var command = new SQLiteCommand(query, connection))
						{
							for (int i = 0; i < necessary.Length; i++)
							{
								command.Parameters.AddWithValue($"@p{i}", necessary[i].value);
							}
							command.ExecuteNonQuery();
						}
					}
				}
				catch (Exception ex)
				{
					ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
				}
			}
		}

		/// <summary>
		/// 查询数据明细
		/// </summary>
		/// <param name="tableName">表名</param>
		/// <param name="columns">列名，如： FilePath,FileName,HashValue </param>
		/// <param name="conditions">查询条件数组，每个元素是一个键值对字符串，如： ("HashValue", "abc123") </param>
		/// <returns>SQLiteDataReader</returns>
		public DataTable QueryData(string tableName, string columns, params (string column, object value)[] conditions)
		{
			try
			{
				DataTable dtResult = new DataTable();
				using (var connection = GetConnection())
				{
					connection.Open();
					string query = $"SELECT {columns} FROM {tableName} WHERE 1=1";
					if (conditions != null && conditions.Length > 0)
					{
						for (int i = 0; i < conditions.Length; i++)
						{
							query += $" AND {conditions[i].column} = @p{i}";
						}
					}
					query += ";";
					using (var command = new SQLiteCommand(query, connection))
					{
						if (conditions != null && conditions.Length > 0)
						{
							for (int i = 0; i < conditions.Length; i++)
							{
								command.Parameters.AddWithValue($"@p{i}", conditions[i].value);
							}
						}
						dtResult.Load(command.ExecuteReader());
						return dtResult;
					}
				}
			}
			catch (Exception ex)
			{
				ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
				return null;
			}
		}

		public DataTable QueryData(string sqliteScript)
		{
			DataTable dtResult = new DataTable();
			if (string.IsNullOrWhiteSpace(sqliteScript))
			{
				throw new ArgumentException("脚本不能为空", nameof(sqliteScript));
			}
			try
			{
				using (var connection = GetConnection())
				{
					connection.Open();
					using (var command = new SQLiteCommand(sqliteScript, connection))
					{
						dtResult.Load(command.ExecuteReader());
					}
				}
			}
			catch (Exception ex)
			{
				ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
			}
			return dtResult;
		}
		#region 参数化 查询数据明细
		/*
		 
			使用示例
		无条件查询
				csharp
				class Program
				{
					static void Main(string[] args)
					{
						string connectionString = "Data Source=example.db;Version=3;";
						SQLiteHelper helper = new SQLiteHelper(connectionString);

						string tableName = "Users";
						string columns = "Id, Name, Age";

						using (var reader = helper.QueryData(tableName, columns))
						{
							while (reader.Read())
							{
								Console.WriteLine($"Id: {reader["Id"]}, Name: {reader["Name"]}, Age: {reader["Age"]}");
							}
						}
					}
				}
		带条件查询
				csharp
				class Program
				{
					static void Main(string[] args)
					{
						string connectionString = "Data Source=example.db;Version=3;";
						SQLiteHelper helper = new SQLiteHelper(connectionString);

						string tableName = "Users";
						string columns = "Id, Name, Age";
						var conditions = new (string parameter, object value)[]
						{
							("HashValue=@HashValue", "abc123"),
							("Age=@Age", 30)
						};

						using (var reader = helper.QueryData(tableName, columns, conditions))
						{
							while (reader.Read())
							{
								Console.WriteLine($"Id: {reader["Id"]}, Name: {reader["Name"]}, Age: {reader["Age"]}");
							}
						}
					}
				}
				解释
				参数声明:

				使用 params (string parameter, object value)[] conditions 作为参数，允许调用方法时传递任意数量的条件。
				如果不传递任何条件，conditions 将默认为 null。
				动态构建查询:

				在 QueryData 方法中，检查 conditions 是否为 null 或空数组。
				如果 conditions 不为空，则遍历 conditions 数组，将每个条件添加到查询字符串中，并将对应的值绑定到 SQLiteCommand 的参数中。
				返回 SQLiteDataReader:

				command.ExecuteReader() 返回一个 SQLiteDataReader 对象，可以在 using 语句中使用它来读取查询结果。
		 
		 */
		#endregion

		/// <summary>
		/// 执行常用操作ExecuteCommonOperations(判断表是否存在，查询数据是否存在，插入数据，返回结果)
		/// </summary>
		/// <param name="tableName">表名</param>
		/// <param name="columnScript">若表不存在则建表时用到的列脚本</param>
		/// <param name="necessary">必须的列和值数组，用于插入数据和查询列名</param>
		/// <param name="conditions">查询条件</param>
		/// <returns></returns>
		public DataTable ExecuteCommonOperations(string tableName, string columnScript,
						(string column, object value)[] necessary, params (string column, object value)[] conditions)
		{
			try
			{
				// 判断表是否存在
				if (!TableExists(tableName))
				{
					CreateTableIfNotExists(tableName, columnScript);
				}

				// 查询数据是否存在
				long? count = DataExists(tableName, conditions);
				bool ifInsert = false;
				if (count is null)
				{
					ifInsert = true;
				}
				else if (count == 0)
				{
					ifInsert = true;
				}
				else
				{
					ifInsert = false;
				}

				// 插入数据
				if (ifInsert)
				{
					InsertData(tableName, necessary);
				}

				// 查询结果(数据明细)
				StringBuilder sbColumns = new StringBuilder();
				// 从必须的列和值数组 necessary 中获取插入数据时的列名，作为查询列
				if (necessary != null && necessary.Length > 0)
				{
					for (int i = 0; i < necessary.Length; i++)
					{
						sbColumns.Append(necessary[i].column + ",");
					}
				}
				if (sbColumns.Length > 0)
				{
					sbColumns.Length--;
				}
				string columns = sbColumns.ToString();
				if (string.IsNullOrWhiteSpace(columns))
				{
					columns = "*";
				}

				return QueryData(tableName, columns, conditions);

			}
			catch (Exception ex)
			{
				ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
				return null;
			}
		}
		/// <summary>
		/// 将获取到的结果存入List列表返回
		/// </summary>
		/// <param name="connectionString">数据库连接字符串</param>
		/// <param name="tableName">表名</param>
		/// <param name="columnScript">建表脚本(括号内设定列的那部分)</param>
		/// <param name="necessary">必要条件组(包含列名及插入数据时列对应的值)</param>
		/// <param name="conditions">查询条件组(包含列名及查询值)</param>
		/// <returns></returns>
		public List<Dictionary<string, object>> GetExecuteResult(string tableName, string columnScript,
						(string column, object value)[] necessary, params (string column, object value)[] conditions)
		{
			List<Dictionary<string, object>> listResult = new List<Dictionary<string, object>>();
			if (necessary != null && necessary.Length > 0)
			{
				using (var connection = GetConnection())
				{
					connection.Open();

					using (var reader = ExecuteCommonOperations(tableName, columnScript, necessary, conditions))
					{
						// 输出 DataTable 的内容
						foreach (DataRow row in reader.Rows)
						{
							Dictionary<string, object> dicResult = new Dictionary<string, object>();
							foreach (DataColumn column in reader.Columns)
							{
								dicResult.Add(column.ColumnName, row[column]);
							}
							listResult.Add(dicResult);
						}
					}
				}
			}
			return listResult;
		}

		public List<Dictionary<string, object>> GetExecuteResult(string sqliteScript)
		{
			List<Dictionary<string, object>> listResult = new List<Dictionary<string, object>>();
			if (string.IsNullOrWhiteSpace(sqliteScript))
			{
				throw new ArgumentException("脚本不能为空", nameof(sqliteScript));
			}
			try
			{
				using (var reader = QueryData(sqliteScript))
				{
					// 输出 DataTable 的内容
					foreach (DataRow row in reader.Rows)
					{
						Dictionary<string, object> dicResult = new Dictionary<string, object>();
						foreach (DataColumn column in reader.Columns)
						{
							dicResult.Add(column.ColumnName, row[column]);
						}
						listResult.Add(dicResult);
					}
				}
			}
			catch (Exception ex)
			{
				ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
			}
			return listResult;
		}
	}
}
#region 报错记录
// 一
/*
 异常消息：无法加载 DLL“SQLite.Interop.dll”: 找不到指定的模块。 (异常来自 HRESULT:0x8007007E)。

因为该文件在ClassLibrary目录下，没有复制到应用程序目录
所以，需要在应用程序目录里添加

确保 SQLite.Interop.dll 文件被正确复制到输出目录。你可以使用 Post-Build 事件 来自动复制这些文件。
右键点击 MainProject -> 选择“属性”。
导航到“构建事件”选项卡。
在“Post-build event command line”中添加以下命令：
属性 - 生成事件 - 生成前/后事件命令行：
添加：

if $(PlatformTarget) == x64 (
  xcopy /y "$(SolutionDir)ClassLibrary\bin\$(ConfigurationName)\x64\SQLite.Interop.dll" "$(TargetDir)"
) else (
  xcopy /y "$(SolutionDir)ClassLibrary\bin\$(ConfigurationName)\x86\SQLite.Interop.dll" "$(TargetDir)"
)

或

if $(PlatformTarget) == x86 (
  xcopy /y "$(SolutionDir)ClassLibrary\bin\$(ConfigurationName)\x86\SQLite.Interop.dll" "$(TargetDir)"
) else (
  xcopy /y "$(SolutionDir)ClassLibrary\bin\$(ConfigurationName)\x64\SQLite.Interop.dll" "$(TargetDir)"
)

这里可以通过VS工具中的【开发者PowerShell】窗口中的命令来识别是32位还是64位程序
corflags D:\GitFiles\BingImageDownloadServiceForWindows\BingImageDownloadForConsoleApplication\bin\Debug\BingImageDownloadForConsoleApplication.exe

Microsoft (R) .NET Framework CorFlags Conversion Tool.  Version  4.8.3928.0
Copyright (c) Microsoft Corporation.  All rights reserved.

Version   : v4.0.30319
CLR Header: 2.5
PE        : PE32
CorFlags  : 0x20003
ILONLY    : 1
32BITREQ  : 0
32BITPREF : 1
Signed    : 0


参考资料：
1. 使用 corflags 工具
corflags 是一个命令行工具，可以用来查看和修改 .NET 可执行文件的标志。你可以使用它来检查生成的应用程序是 32 位还是 64 位。

打开命令提示符。

导航到生成的应用程序所在的目录。

运行以下命令：

sh
corflags YourApplication.exe
输出示例如下：

sh
Version   : v4.0.30319
CLR Header: 2.5
PE        : PE32
CorFlags  : 0x00000002
ILONLY    : 1
32BITREQ  : 0
32BITPREF : 0
Signed    : 0
PE32 表示 32 位应用程序。
PE32+ 表示 64 位应用程序。

2. 使用 dumpbin 工具
dumpbin 是一个 Microsoft 提供的命令行工具，可以用来查看可执行文件的详细信息。

打开命令提示符。

导航到生成的应用程序所在的目录。

运行以下命令：

sh
dumpbin /headers YourApplication.exe
输出示例如下：

sh
FILE HEADER VALUES
           14C machine (x86)
            6 number of sections
     5F0A0000 time date stamp Wed Oct 20 12:00:00 2021
            0 file pointer to symbol table
            0 number of symbols
           E0 size of optional header
           22 characteristics
                  32 bit word machine
machine (x86) 表示 32 位应用程序。
machine (x64) 表示 64 位应用程序。

 */

// 二
/*
报错“无法访问已释放的对象”

 原因：SQLiteDataReader 依赖于打开的连接对象，而 ExecuteCommonOperations 方法中的连接在方法返回时被关闭。
解决方案：将连接对象的管理移到 GetExecuteResult 方法中，确保连接在读取数据时仍然打开，或者使用 DataTable 等数据结构来存储查询结果。

DataTable dtResult = new DataTable()
dtResult.Load(reader);
return dtResult;

                    // 输出 DataTable 的内容
                    foreach (DataRow row in dataTable.Rows)
                    {
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            Console.WriteLine($"{column.ColumnName}: {row[column]}");
                        }
                    }

 */
#endregion