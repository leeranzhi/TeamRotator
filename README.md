# TeamRotator

TeamRotator 是一个任务轮值管理系统，用于自动化管理团队成员的轮值任务。

## 功能特点

- 支持多种轮值规则（每日、每周、每两周）
- 自动跳过节假日
- Slack 通知集成
- RESTful API 接口
- PostgreSQL 数据存储

## 项目结构

项目采用三层架构：

```
src/
├── TeamRotator.Core/         # 核心层：实体和接口定义
├── TeamRotator.Infrastructure/   # 基础设施层：数据访问和服务实现
└── TeamRotator.Api/         # API层：控制器和配置
```

## 开发环境要求

- .NET 8.0 SDK
- PostgreSQL 16
- Docker（可选，用于运行数据库）

## 快速开始

1. 克隆仓库：
```bash
git clone https://github.com/leeranzhi/TeamRotator.git
cd TeamRotator
```

2. 启动 PostgreSQL（使用 Docker）：
```bash
docker run --name teamrotator-postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_DB=teamrotator \
  -p 5433:5432 \
  -d postgres:16
```

3. 更新数据库：
```bash
cd src/TeamRotator.Api
dotnet ef database update
```

4. 运行应用：
```bash
dotnet run
```

应用将在 http://localhost:5000 启动。

## 配置

主要配置文件位于 `src/TeamRotator.Api/appsettings.json`：

- 数据库连接字符串
- Slack Webhook URL
- 节假日 API 设置
- 日志配置

## API 文档

API 文档可以通过以下方式访问：

- Swagger UI：运行应用后访问 http://localhost:5000/swagger
- OpenAPI 规范：查看 `src/TeamRotator.Api/swagger.yaml`

## 定时任务

系统包含两个主要的定时任务：

- AssignmentUpdateJob：每天凌晨执行，更新任务分配
- SendToSlackJob：每天早上 8:00 执行，发送通知

## 贡献

欢迎提交 Pull Request 或创建 Issue。

## 许可证

MIT 