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
		/// 建表
		/// </summary>
		/// <param name="tableName">表名</param>
		/// <param name="columns">详细建表语句中列名括号内的那部分，如：CONSTRAINT uc_HashValue UNIQUE (HashValue)</param>
		/// <param>示例：string createTableQuery = @"</param>
		/// <param>CREATE TABLE IF NOT EXISTS FileHashes(Id INTEGER PRIMARY KEY AUTOINCREMENT,FilePath TEXT NOT NULL,FileName TEXT NOT NULL,HashValue TEXT NOT NULL,CONSTRAINT uc_HashValue UNIQUE (HashValue));</param>
		/// <param>";</param>
		public void CreateTableIfNotExists(string tableName, string columns)
		{
			try
			{
				using (var connection = GetConnection())
				{
					connection.Open();
					string query = $"CREATE TABLE IF NOT EXISTS {tableName} ({columns});";
					using (var command = new SQLiteCommand(query, connection))
					{
						command.ExecuteNonQuery();
					}
				}
			}
			catch (Exception ex)
			{
				MyLogHelper.GetExceptionLocation(ex);
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
				MyLogHelper.GetExceptionLocation(ex);
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
					string query = $"SELECT COUNT(*) FROM {tableName} WHERE 1=1;";
					if (conditions != null && conditions.Length > 0)
					{
						for (int i = 0; i < conditions.Length; i++)
						{
							query += $" AND {conditions[i].column} = @p{i}";
						}
					}
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
				MyLogHelper.GetExceptionLocation(ex);
				return null;
			}
		}

		/// <summary>
		/// 插入数据
		/// </summary>
		/// <param name="tableName">表名及列名，如：FileHashes(FilePath,FileName,HashValue)</param>
		/// <param name="values">列名对应的值，如：@FilePath,@FileName,@HashValue</param>
		public void InsertData(string tableName, string values)
		{
			try
			{
				using (var connection = GetConnection())
				{
					connection.Open();
					string query = $"INSERT OR IGNORE INTO {tableName} VALUES ({values});";
					using (var command = new SQLiteCommand(query, connection))
					{
						command.ExecuteNonQuery();
					}
				}
			}
			catch (Exception ex)
			{
				MyLogHelper.GetExceptionLocation(ex);
			}
		}

		/// <summary>
		/// 查询数据明细
		/// </summary>
		/// <param name="tableName">表名</param>
		/// <param name="columns">列名，如： FilePath,FileName,HashValue </param>
		/// <param name="conditions">查询条件数组，每个元素是一个键值对字符串，如： ("HashValue", "abc123") </param>
		/// <returns>SQLiteDataReader</returns>
		public SQLiteDataReader QueryData(string tableName, string columns, params (string column, object value)[] conditions)
		{
			try
			{
				using (var connection = GetConnection())
				{
					connection.Open();
					string query = $"SELECT {columns} FROM {tableName} WHERE 1=1;";
					if (conditions != null && conditions.Length > 0)
					{
						for (int i = 0; i < conditions.Length; i++)
						{
							query += $" AND {conditions[i].column} = @p{i}";
						}
					}
					using (var command = new SQLiteCommand(query, connection))
					{
						if (conditions != null && conditions.Length > 0)
						{
							for (int i = 0; i < conditions.Length; i++)
							{
								command.Parameters.AddWithValue($"@p{i}", conditions[i].value);
							}
						}
						return command.ExecuteReader();
					}
				}
			}
			catch (Exception ex)
			{
				MyLogHelper.GetExceptionLocation(ex);
				return null;
			}
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

		// 执行常用操作(判断表是否存在，查询数据是否存在，插入数据)
		public void ExecuteCommonOperations(string tableName,string CreateTableColumn)
		{
			using (var connection = GetConnection())
			{
				connection.Open();

				// 判断表是否存在
				if (!TableExists(tableName))
				{
					CreateTableIfNotExists("a","sd")
				}

				// 查询数据是否存在
				// 插入数据
			}
		}

		// AI 给的方法示例，可能用不到，先留着：
		// 组合方法：连接数据库、判断表是否存在、建表、查询数据
		public SQLiteDataReader ExecuteCommonOperations(string tableName, string columns, string values)
		{
			using (var connection = GetConnection())
			{
				connection.Open();

				// 判断表是否存在，不存在则创建
				if (!TableExists(tableName))
				{
					CreateTableIfNotExists(tableName, columns);
				}

				// 插入数据
				InsertData(tableName, values);

				// 查询数据明细
				string query = $"SELECT {columns} FROM {tableName};";
				using (var command = new SQLiteCommand(query, connection))
				{
					return command.ExecuteReader();
				}
			}
		}
	}
}
