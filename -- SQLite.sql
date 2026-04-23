-- SQLite

-- 查看所有表
SELECT name FROM sqlite_master WHERE type='table';

-- 查看表结构
PRAGMA table_info(users);  -- 替换users为你的表名

PRAGMA table_info(FileHashes_old);  
PRAGMA table_info(sqlite_sequence);  
PRAGMA table_info(FileHashes);  

 -- 获取所有文件
SELECT * FROM FileHashes_old;
 -- 获取所有文件
SELECT * FROM sqlite_sequence;
 -- 获取所有文件
SELECT * FROM FileHashes;

-- 获取今日新增文件数
SELECT STRFTIME('%Y-%m-%d',datetime(CreatedTime)) AS CreatedDay,COUNT(1) AS Count FROM FileHashes WHERE 1=1 AND CreatedDay = date('now') GROUP BY CreatedDay;

-- SELECT TOP 10 * FROM FileHashes ORDER BY CreatedTime DESC;   是SQL Server/Access的语法，但SQLite并不支持TOP关键字，这就是报错的根本原因。
-- SQLite支持LIMIT关键字，用于分页查询。例如，SELECT * FROM FileHashes LIMIT 10 OFFSET 0; 将返回第10条数据。
-- 注意：SQLite的LIMIT关键字不支持负数偏移量，只能从第1条数据开始查询。
-- 查询最近10条数据（按CreatedTime倒序，最新的在前）
SELECT * FROM FileHashes 
ORDER BY CreatedTime DESC 
LIMIT 10;
-- 扩展技巧：分页查询
-- 如果需要查询"第11-20条"这样的分页数据，可以结合OFFSET：
-- 查询第11-20条数据（跳过前10条，取后面10条）
SELECT * FROM FileHashes 
ORDER BY CreatedTime DESC 
LIMIT 10 OFFSET 10;
-- 如果CreatedTime字段是字符串类型（而非DATETIME），需要确保格式是YYYY-MM-DD HH:MM:SS才能正确排序，否则可能需要先转换格式：
SELECT * FROM FileHashes 
ORDER BY strftime('%Y-%m-%d %H:%M:%S', CreatedTime) DESC 
LIMIT 10;



-- 2026-04-23 11:00:00 添加DownloadUrl字段用于记录下载地址
ALTER TABLE FileHashes ADD COLUMN DownloadUrl TEXT;
ALTER TABLE FileHashes ADD COLUMN DownloadUrl TEXT DEFAULT NULL;


-- 创建一个用户表的示例（包含常见数据类型）
CREATE TABLE IF NOT EXISTS users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,  -- 自增主键
    username TEXT NOT NULL UNIQUE,         -- 唯一且不能为空的用户名
    age INTEGER CHECK(age >= 0),           -- 年龄必须非负
    email TEXT,
    create_time DATETIME DEFAULT CURRENT_TIMESTAMP  -- 默认当前时间
);

-- 添加字段
ALTER TABLE users ADD COLUMN phone TEXT;
-- 注意事项：SQLite的ALTER TABLE功能有一定限制：只能添加字段、重命名表，不能修改或删除已有字段。如果后续需要修改字段，通常需要创建新表并迁移数据。

-- 修改字段名（SQLite仅支持重命名字段）
ALTER TABLE users RENAME COLUMN phone TO mobile;

-- 删除表
DROP TABLE IF EXISTS users;

-- 插入单条数据
INSERT INTO users (username, age, email)
VALUES ('zhangsan', 25, 'zhangsan@example.com');

-- 插入多条数据
INSERT INTO users (username, age)
VALUES ('lisi', 30), ('wangwu', 22);

-- 查询所有数据
SELECT * FROM users;

-- 查询指定字段
SELECT username, age FROM users;

-- 条件查询（年龄大于25）
SELECT * FROM users WHERE age > 25;

-- 排序查询（按年龄降序）
SELECT * FROM users ORDER BY age DESC;

-- 分页查询（前10条数据）
SELECT * FROM users LIMIT 10 OFFSET 0;

-- 更新指定数据
UPDATE users 
SET email = 'new_email@example.com', age = 26
WHERE username = 'zhangsan';  -- 必须加WHERE条件，否则会更新所有数据！

-- 删除指定数据
DELETE FROM users WHERE id = 1;  -- 同样必须加WHERE条件！

-- 删除所有数据（保留表结构）
DELETE FROM users;

-- 统计数量
SELECT COUNT(*) FROM users;

-- 计算平均值
SELECT AVG(age) FROM users;

-- 获取最大/最小值
SELECT MAX(age), MIN(age) FROM users;

-- 字符串操作
SELECT UPPER(username) FROM users;  -- 转大写



-- 开启事务
BEGIN TRANSACTION;

-- 执行多个操作
INSERT INTO users (username) VALUES ('zhaoliu');
UPDATE users SET age = 28 WHERE username = 'zhaoliu';

-- 提交事务（永久生效）
COMMIT;

-- 回滚事务（撤销操作）
ROLLBACK;
