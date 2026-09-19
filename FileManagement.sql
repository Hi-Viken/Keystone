-- =============================================
-- 文件管理表 - SQLite（支持多级目录和文件）
-- =============================================

-- 创建文件管理表
CREATE TABLE IF NOT EXISTS FileManagement (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ParentId INTEGER DEFAULT 0,          -- 父级ID（0表示顶级）
    FileType TEXT DEFAULT '0',           -- 文件类型（0-目录，1-文件）
    FileName TEXT NOT NULL,              -- 文件/目录名称（原始文件名）
    FilePath TEXT,                       -- 文件路径（仅文件类型使用）
    FileSize INTEGER,                    -- 文件大小（字节，仅文件类型使用）
    FileExtension TEXT,                  -- 文件扩展名（仅文件类型使用）
    FileHash TEXT,                       -- 文件哈希值（SHA256，仅文件类型使用）
    Description TEXT,                    -- 描述
    
    -- 继承自 SysBase 的字段
    Create_by TEXT,                      -- 创建人
    Create_time DATETIME DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    Update_by TEXT,                      -- 更新人
    Update_time DATETIME,                -- 更新时间
    Remark TEXT                          -- 备注
);

-- 创建索引（提高查询性能）
CREATE INDEX IF NOT EXISTS idx_filemanagement_parentid ON FileManagement(ParentId);
CREATE INDEX IF NOT EXISTS idx_filemanagement_filetype ON FileManagement(FileType);
CREATE INDEX IF NOT EXISTS idx_filemanagement_filename ON FileManagement(FileName);
CREATE INDEX IF NOT EXISTS idx_filemanagement_fileextension ON FileManagement(FileExtension);
CREATE INDEX IF NOT EXISTS idx_filemanagement_filehash ON FileManagement(FileHash);
CREATE INDEX IF NOT EXISTS idx_filemanagement_create_time ON FileManagement(Create_time);

-- 插入示例数据（可选）
-- INSERT INTO FileManagement (ParentId, FileType, FileName, FilePath, FileSize, FileExtension, Description, Create_by, Create_time) VALUES 
-- (0, '0', '文档', NULL, NULL, NULL, '文档目录', 'admin', datetime('now')),
-- (0, '0', '图片', NULL, NULL, NULL, '图片目录', 'admin', datetime('now')),
-- (1, '0', '合同文件', NULL, NULL, NULL, '合同文件目录', 'admin', datetime('now')),
-- (3, '1', '采购合同.pdf', '/docs/contracts/purchase_contract.pdf', 1024000, 'pdf', '2024年采购合同', 'admin', datetime('now')),
-- (3, '1', '销售合同.docx', '/docs/contracts/sales_contract.docx', 512000, 'docx', '2024年销售合同', 'admin', datetime('now')),
-- (2, '1', '产品图片.png', '/images/product.png', 2048000, 'png', '产品主图', 'admin', datetime('now'));
