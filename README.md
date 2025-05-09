# NoteMaster - 桌面便签应用

## 项目概述
**NoteMaster** 是一款基于 C# 开发的桌面便签应用，旨在为用户提供便捷、高效的便签创建、管理和组织功能。应用基于 Windows 平台，使用 **WPF（Windows Presentation Foundation）** 作为前端框架，结合本地文件存储实现数据持久化，目标是提供简洁、响应迅速的用户体验。主要功能包括便签创建、编辑、自动存档、分类查找，并为未来云同步功能预留扩展可能性。

### 项目目标

- 提供直观的便签初始页面，支持快速创建和查看便签。
- 实现便签自动存档，确保数据安全。
- 支持便签分类查找，提升信息管理效率。
- 提供简洁流畅的用户界面，满足日常使用需求。
- 预留云同步功能接口，为未来扩展做准备。
- 在三周内完成核心功能的开发、测试和交付。

## 系统功能

### 核心功能模块
1. **便签初始页面**
   - 以列表或卡片形式展示所有便签概览。
   - 提供“新建便签”按钮，点击后进入编辑界面。
   - 显示便签标题和创建时间。
   - 包含搜索栏和分类筛选功能，支持快速定位便签。

2. **便签创建与编辑**
   - 支持创建新便签，输入标题和内容。
   - 支持富文本编辑（加粗、斜体、字体颜色等）。
   - 编辑界面包含保存、取消、删除按钮。
   - 编辑完成后自动返回初始页面。

3. **自动存档**
   - 编辑过程中每30秒自动保存便签内容至本地文件。
   - 数据以 JSON 格式存储于用户主目录下的 `NoteMaster` 文件夹。
   - 每次保存生成备份文件，防止数据丢失。

4. **分类查找**
   - 支持为便签添加自定义标签（如“工作”、“个人”、“紧急”）。
   - 初始页面提供标签筛选功能，点击标签显示对应便签列表。
   - 支持通过关键词搜索便签标题或内容，搜索结果实时更新。

5. **备选功能：云同步（未来扩展）**
   - 提供云同步接口，支持将便签数据上传至云端（如 OneDrive 或自定义服务器）。
   - 用户可选择开启或关闭云同步，同步时需登录账户。
   - 同步数据加密传输，确保安全性。

### 非功能性需求

- **性能**：软件启动时间不超过3秒，搜索响应时间小于1秒。
- **兼容性**：支持 Windows 10 及以上版本。
- **易用性**：界面简洁，操作逻辑符合用户习惯，提供提示信息。
- **安全性**：本地数据存储加密，防止未经授权访问。
- **可扩展性**：代码模块化设计，便于添加新功能（如云同步）。

## 主要业务流程

### 1. 创建便签
- 用户打开软件，进入便签初始页面。
- 点击“新建便签”按钮，跳转至编辑界面。
- 输入标题和内容，选择标签（可选）。
- 点击“保存”按钮，数据存储至本地，自动返回初始页面。

### 2. 编辑与存档

- 在初始页面点击某便签，进入编辑界面。
- 修改标题或内容，编辑过程中每30秒自动保存。
- 点击“保存”或“取消”返回初始页面，保存时生成备份。

### 3. 分类查找

- 在初始页面选择某标签，界面显示该标签下的便签。
- 或在搜索栏输入关键词，界面实时显示匹配的便签列表。
- 点击便签进入编辑界面，或继续筛选。

### 4. 删除便签

- 在初始页面或编辑界面点击“删除”按钮。
- 弹出确认提示，确认后删除便签并更新初始页面。

## 人员分工
项目由三人小组开发，分工如下：

| 姓名 | 角色 | 职责 |
|------|------|------|
| - | 后端开发与测试 | 负责数据存储、自动存档、分类查找逻辑实现；编写单元测试；参与系统集成测试。 |
| - | 前端开发 | 负责 WPF 界面设计与开发，包括初始页面、编辑页面、搜索与筛选功能；确保界面美观与响应性。 |
| - | 项目管理与文档 | 负责需求分析、文档编写、进度跟踪；协助测试与调试；负责云同步接口预留设计。 |

### 协作方式
- 使用 GitHub 进行代码版本管理，每日提交代码并进行 Code Review。
- 使用 Trello 管理任务，跟踪开发进度。
- 每日进行15分钟站会，讨论进展与问题。
- 遇到技术难点时，三人共同讨论，必要时查阅资料或请教导师。

## 时间进度安排
项目开发周期为三周（2025年4月28日 - 5月18日），具体安排如下：

### 第1周（4月23日 - 4月29日）：项目准备与需求设计
- **4月23日 - 4月24日**：项目立项、头脑风暴、功能讨论、初步需求确定。
- **4月25日 - 4月26日**：撰写需求文档、确定用户使用流程。
- **4月27日**：搭建 GitHub 仓库，配置项目框架、README、分支管理策略。
- **4月28日 - 4月29日**：确定系统架构、技术选型（WPF + C# + JSON）。
- **里程碑**：完成需求文档与系统设计，项目框架正式启动。

### 第2周（4月30日 - 5月6日）：基础功能开发
- **4月30日 - 5月1日**：设计主窗口 UI，开发便签初始界面（WPF 前端）。
- **5月2日 - 5月3日**：开发数据存储模块（JSON 格式的读写与管理）。
- **5月4日 - 5月5日**：实现便签的创建、编辑与删除功能。
- **5月6日**：初步集成测试，基本功能联调。
- **里程碑**：基本界面可用，便签可创建编辑，系统可运行。

### 第3周（5月7日 - 5月13日）：核心功能开发
- **5月7日 - 5月8日**：实现自动存档、历史记录与备份逻辑。
- **5月9日 - 5月10日**：开发便签分类功能，支持标签管理与搜索。
- **5月11日**：富文本编辑功能开发（支持字体、加粗、颜色等）。
- **5月12日 - 5月13日**：全面测试与 Bug 修复，优化响应速度与交互体验。
- **里程碑**：核心功能全部完成，系统进入可用版本阶段。

### 第4周（5月14日 - 5月18日）：测试打磨与交付准备
- **5月14日 - 5月15日**：用户体验优化、完善 UI 细节、修复测试 Bug。
- **5月16日**：预留云同步接口（Stub 函数或接口设计），编写开发文档。
- **5月17日**：最终集成测试，整理 PPT、准备展示与答辩演示流程。
- **5月18日**：项目最终提交，演示项目并答辩。
- **最终里程碑**：完成项目交付，提交 PPT、源代码、演示视频等材料。

## 风险与应对措施
1. **风险：开发进度延误**
   - **应对**：每日站会跟踪进度，及时调整任务分配；预留 Buffer 时间。
2. **风险：技术难点（如 WPF 布局或搜索性能）**
   - **应对**：提前学习相关技术，准备 Plan B（如简化功能）；必要时请教导师。
3. **风险：团队协作问题**
   - **应对**：明确分工，保持沟通，使用 Trello 和 GitHub 规范化协作。

## 交付成果
- **便签软件“NoteMaster”可执行文件**。
- **完整源代码**（GitHub 仓库）。
- **开发文档**（包含架构设计、接口说明）。
- **测试报告**。
- **用户使用手册**。
- **项目演示 PPT**。
