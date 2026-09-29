# Keystone

后台管理系统，配套 Vue 3 前端，支持多语言（中 / 繁 / 英 / 日 / 韩）。

## 目录结构

```
Keystone/
├── Server/                      # 后端（.NET 10 / ASP.NET Core 解决方案 Keystone.slnx）
│   ├── Keystone.Server/         # Web API 宿主（Program.cs / Kestrel / Swagger·Scalar）
│   ├── Keystone.Common/         # 公共组件（缓存、动态 Api、邮件、微信等）
│   ├── Keystone.Infrastructure/ # 基础设施（鉴权、全局异常、缓存、扩展方法）
│   ├── Keystone.Model/          # 实体模型
│   ├── Keystone.Repository/     # 数据访问层
│   ├── Keystone.ServiceCore/    # 核心服务（SqlSugar ORM、SignalR）
│   ├── Keystone.Tasks/          # 定时任务（Quartz.NET）
│   ├── Keystone.CodeGenerator/  # 代码生成器
│   └── CommonRelyOn/            # 公共依赖
└── Web/                         # 前端（Vue 3 + Element Plus + Vite）
    ├── src/
    │   ├── i18n/                # 多语言翻译（zh-CN / zh-TW / en-US / ja-JP / ko-KR）
    │   ├── views/               # 页面组件
    │   ├── components/          # 公共组件
    │   ├── layout/              # 布局组件
    │   └── ...
    ├── package.json
    └── ...
```

> 说明：`Server/.gitignore`、`Web/.gitignore` 与根 `.gitignore` 已忽略 `bin/ obj/ .vs/ node_modules/ dist/` 等构建产物；运行时数据 `wwwroot/uploads`、`wwwroot/avatar`、`wwwroot/export` 及密钥（`appsettings.json`、`*.pfx`、`.env`）均不纳入版本库。

## 技术栈

- **后端**：.NET 10 / ASP.NET Core、SqlSugar ORM、SignalR、NLog、Swagger/Scalar、Quartz.NET、IP 限流、验证码
- **前端**：Vue 3、Element Plus、Vite、Pinia、vue-router、axios、ECharts、vxe-table、vue-i18n
- **数据库**：SQLite（可通过 `appsettings.json` 切换为 SQLServer / MySQL 等）
- **国际化**：vue-i18n，支持简体中文、繁体中文、English、日本語、한국어 五种语言

## 环境要求

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Node.js ≥ 18（建议 20+）与 npm / pnpm / yarn

## 后端运行

```bash
cd Server

# 1. 还原依赖
dotnet restore Keystone.slnx

# 2. 准备配置：appsettings.json 已被 gitignore（含数据库连接串等敏感信息），
#    请使用你本地的 appsettings.json，或参考后端配置绑定自行创建，
#    填入数据库连接串、Redis、JWT 等（切勿提交真实密钥）

# 3. 启动 API（端口见 appsettings.json，开发环境常见为 8011）
dotnet run --project Keystone.Server
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

## 多语言说明

前端使用 vue-i18n 实现国际化，翻译文件位于 `Web/src/i18n/`：

| 目录 | 说明 |
| --- | --- |
| `i18n/lang/` | 全局翻译（菜单、布局、通用按钮、业务页面等） |
| `i18n/pages/login/` | 登录页专用翻译 |
| `i18n/pages/menu/` | 菜单表单专用翻译 |

每个目录均包含 5 个语言文件（`zh-CN` / `zh-TW` / `en-US` / `ja-JP` / `ko-KR`），默认语言为简体中文。此外，后端 `sys_common_lang` 表支持动态维护翻译词条，运行时自动合并到前端。

## 分支说明

| 分支 | 说明 |
| --- | --- |
| `main` | 稳定分支。 |
| `dev`  | 开发分支，包含全部源码与配置模板。 |
| `organize` | 整理重构分支。 |

## 安全须知

以下文件 / 目录**不纳入版本库**（已被 `.gitignore` 忽略），请勿手动强制提交：

- `appsettings.json`、`appsettings.*.json`（含数据库连接串等敏感配置）
- `*.pfx`、`*.pem`、`*.key`、`*.crt`、`id_rsa*`（证书 / 私钥）
- `Web/.env`、`Web/.env.*`（前端环境变量）
- `Server/Keystone.Server/wwwroot/uploads`、`avatar`、`export`（运行时上传与生成数据）

请在上线前自行准备上述配置，并妥善保管密钥。

## 鸣谢

- 后端框架基于 [.NET 10]()（MIT）
- 前端基于其 Vue 3 + Element Plus 版本

© Keystone
