# Keystone

> 视频监控 / 设备管理与后台系统（内部产品名 **SkyFrpPanel**）

基于 [ZrAdmin.NET](https://gitee.com/izory/ZrAdminNetCore) 二开的后台管理系统，配套 Vue 3 前端与工业设备通信模块（Kep），并提供文件 / 材料管理相关的 SQLite 数据库脚本。

## 目录结构

```
Keystone/
├── FileManagement.sql        # SQLite：文件管理表（支持多级目录）
├── Material.sql              # SQLite：材料表
├── MaterialType.sql          # SQLite：材料类型表（树形结构）
├── Server/                   # 后端（.NET 10 / ASP.NET Core 解决方案 SkyFrpPanel.slnx）
│   ├── SkyFrpPanel.Server/   # Web API 宿主（Program.cs / Kestrel / Swagger·Scalar）
│   ├── SkyFrpPanel.Common/   # 公共组件（缓存、动态 Api、邮件、微信、Excel 等）
│   ├── SkyFrpPanel.Infrastructure/ # 基础设施（鉴权、全局异常、缓存、扩展方法）
│   ├── SkyFrpPanel.Model/    # 实体模型
│   ├── SkyFrpPanel.Repository/ # 数据访问层
│   ├── SkyFrpPanel.Service/  # 业务服务
│   ├── SkyFrpPanel.ServiceCore/ # 核心服务（SqlSugar ORM、SignalR）
│   ├── SkyFrpPanel.Tasks/     # 定时任务（Quartz.NET）
│   ├── SkyFrpPanel.Mall/      # 商城模块
│   ├── SkyFrpPanel.CodeGenerator/ # 代码生成器
│   ├── CommonRelyOn/         # 公共依赖
│   └── Kep/                  # 工业设备通信模块
│       ├── KepServerCore/    # 服务端核心
│       ├── KepClientCore/    # 客户端核心
│       ├── KepServerHub/     # SignalR 服务 Hub
│       ├── KepDevSmartMeter/ # 智能电表设备
│       ├── KepDevSwitch/     # 交换机设备（WinForms Demo）
│       └── ZepCommon/        # 协议公共库（CRC、数据帧、环形缓冲、文件分片追踪）
└── Web/                      # 前端（Vue 3 + Element Plus + Vite，SkyFrpPanel）
    ├── src/                  # 前端源码
    ├── package.json
    └── ...
```

> 说明：`Server/.gitignore`、`Web/.gitignore` 与根 `.gitignore` 已忽略 `bin/ obj/ .vs/ node_modules/ dist/` 等构建产物；运行时数据 `wwwroot/uploads`、`wwwroot/avatar`、`wwwroot/export` 及密钥（`appsettings.json`、`*.pfx`、`.env`）均不纳入版本库。

## 技术栈

- **后端**：.NET 10 / ASP.NET Core、SqlSugar ORM、SignalR、NLog、Swagger/Scalar、Quartz.NET、IP 限流、验证码
- **前端**：Vue 3、Element Plus、Vite、Pinia、vue-router、axios、ECharts、vxe-table
- **数据库**：SQLite（详见根目录 SQL 脚本；可通过 `appsettings.json` 切换为 SQLServer / MySQL 等）
- **设备通信**：自研二进制协议（ZepCommon：CRC 校验、数据帧、环形缓冲、文件分片追踪）

## 环境要求

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Node.js ≥ 18（建议 20+）与 npm / pnpm / yarn
- SQLite 客户端（可选，用于初始化库表）

## 后端运行

```bash
cd Server

# 1. 还原依赖
dotnet restore SkyFrpPanel.slnx

# 2. 准备配置：appsettings.json 已被 gitignore（含数据库连接串等敏感信息），
#    请使用你本地的 appsettings.json，或参考后端配置绑定自行创建，
#    填入数据库连接串、Redis、JWT 等（切勿提交真实密钥）

# 3. 启动 API（端口见 appsettings.json，开发环境常见为 8011）
dotnet run --project SkyFrpPanel.Server
```

## 前端运行

```bash
cd Web

# 安装依赖
npm install            # 或 pnpm install / yarn

# 开发模式（默认 http://localhost:8887，接口代理到后端，见 .env.development）
npm run dev

# 生产 / 测试构建（输出到 dist/）
npm run build:prod     # 生产
npm run build:stage    # 测试
```

前端接口地址在 `Web/.env*` 中配置（已 gitignore，仓库仅保留 `Web/.env.example`）。

## 数据库初始化

根目录提供 SQLite 建表脚本，执行即可初始化表结构：

```bash
sqlite3 keystone.db < FileManagement.sql
sqlite3 keystone.db < Material.sql
sqlite3 keystone.db < MaterialType.sql
```

> 如使用其他数据库，请按对应方言调整脚本，并在 `appsettings.json` 中配置连接串。

## 分支说明

| 分支 | 说明 |
| --- | --- |
| `main` | 仅含本说明文档，保持空仓库。 |
| `dev`  | 开发分支，包含全部源码、配置模板与本文档。 |

## 安全须知

以下文件 / 目录**不纳入版本库**（已被 `.gitignore` 忽略），请勿手动强制提交：

- `appsettings.json`、`appsettings.*.json`（含数据库连接串等敏感配置）
- `*.pfx`、`*.pem`、`*.key`、`*.crt`、`id_rsa*`（证书 / 私钥）
- `Web/.env`、`Web/.env.*`（前端环境变量）
- `Server/SkyFrpPanel.Server/wwwroot/uploads`、`avatar`、`export`（运行时上传与生成数据）

请在上线前自行准备上述配置，并妥善保管密钥。

## 鸣谢

- 后端框架基于 [ZrAdmin.NET](https://gitee.com/izory/ZrAdminNetCore)（MIT）
- 前端基于其 Vue 3 + Element Plus 版本

© Keystone
