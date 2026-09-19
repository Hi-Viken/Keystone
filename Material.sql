-- 材料管理表
CREATE TABLE IF NOT EXISTS Material (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Code TEXT NOT NULL UNIQUE,              -- 材料编号（自动生成，唯一，格式：MAT00001）
    Name TEXT NOT NULL,                     -- 材料名称
    MaterialTypeId INTEGER DEFAULT 0,       -- 材料类型ID（关联MaterialType表）
    CoverImageId INTEGER,                   -- 封面图片文件ID（关联FileManagement表）
    Content TEXT,                           -- 富文本内容（图文/视频描述）
    -- 继承自 SysBase 的字段
    Create_by TEXT,
    Create_time DATETIME DEFAULT CURRENT_TIMESTAMP,
    Update_by TEXT,
    Update_time DATETIME,
    Remark TEXT
);

-- 创建索引
CREATE INDEX IF NOT EXISTS idx_material_code ON Material(Code);
CREATE INDEX IF NOT EXISTS idx_material_name ON Material(Name);
CREATE INDEX IF NOT EXISTS idx_material_typeid ON Material(MaterialTypeId);

-- 示例数据（可选）
-- INSERT INTO Material (Code, Name, MaterialTypeId, Content, Create_by, Create_time)
-- VALUES ('MAT00001', '示例材料', 1, '<p>这是示例材料的详细描述</p>', 'admin', datetime('now'));
