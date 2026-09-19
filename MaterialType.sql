-- =============================================
-- 材料类型表 - SQLite（支持多层级树形结构）
-- =============================================

-- 创建材料类型表
CREATE TABLE IF NOT EXISTS MaterialType (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ParentId INTEGER DEFAULT 0,          -- 父级ID（0表示顶级）
    Code TEXT NOT NULL UNIQUE,           -- 材料类型编码（6位，自动生成，唯一）
    Name TEXT NOT NULL,                  -- 材料类型名称
    Description TEXT,                    -- 描述
    
    -- 继承自 SysBase 的字段
    Create_by TEXT,                      -- 创建人
    Create_time DATETIME DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    Update_by TEXT,                      -- 更新人
    Update_time DATETIME,                -- 更新时间
    Remark TEXT                          -- 备注
);

-- 创建索引（提高查询性能）
CREATE INDEX IF NOT EXISTS idx_materialtype_parentid ON MaterialType(ParentId);
CREATE INDEX IF NOT EXISTS idx_materialtype_code ON MaterialType(Code);
CREATE INDEX IF NOT EXISTS idx_materialtype_name ON MaterialType(Name);
CREATE INDEX IF NOT EXISTS idx_materialtype_create_time ON MaterialType(Create_time);

-- 插入示例数据（可选）
-- INSERT INTO MaterialType (ParentId, Code, Name, Description, Create_by, Create_time) VALUES 
-- (0, 'MT0001', '金属材料', '各类金属材料及制品', 'admin', datetime('now')),
-- (0, 'MT0002', '非金属材料', '各类非金属材料及制品', 'admin', datetime('now')),
-- (1, 'MT0003', '钢铁材料', '钢铁类材料', 'admin', datetime('now')),
-- (1, 'MT0004', '有色金属', '有色金属材料', 'admin', datetime('now')),
-- (2, 'MT0005', '高分子材料', '高分子材料', 'admin', datetime('now'));
