# ProjectEyeX 迁移下一步（可直接开工）

> 目标：把“规划”变成“执行”。本文件只关注接下来 7~10 天要做什么。

## 1) 本周目标（Sprint-0）

在不影响旧版 WPF 稳定运行的前提下，产出一个 **可编译、可运行、可演示** 的 Avalonia 雏形：

- [ ] 新建 `src/New/ProjectEye.Core`（.NET 8）
- [ ] 新建 `src/New/ProjectEye.App`（Avalonia + CommunityToolkit.Mvvm）
- [ ] 跑通 1 个主窗口 + 1 个设置页 + 1 个倒计时组件
- [ ] 完成 `config.xml -> config.json` 的最小导入器（只导通关键字段）
- [ ] 保留旧工程不动（并行开发）

## 2) 具体任务拆分（按顺序）

### Task A：初始化新工程骨架（Day 1）

- 建议目录：
  - `src/New/ProjectEye.Core`
  - `src/New/ProjectEye.App`
  - `src/New/ProjectEye.Platform.Abstractions`
  - `src/New/ProjectEye.Platform.Windows`
- 初始化依赖：
  - `CommunityToolkit.Mvvm`
  - `Microsoft.Extensions.DependencyInjection`
  - `Serilog`
  - `System.Text.Json`

**Definition of Done**
- `dotnet build` 全绿
- `ProjectEye.App` 能启动并显示空主窗体

### Task B：先迁核心状态机，不迁 UI（Day 2~3）

优先迁以下逻辑到 `ProjectEye.Core`：

- 工作计时/休息计时状态流转
- 番茄模式开始/暂停/结束
- 配置默认值与保存/加载（新 JSON 结构）

**建议接口（先定接口后写实现）**

- `IClock`
- `IAppConfigStore`
- `IStatisticsStore`
- `INotificationPort`
- `ITrayPort`（先空实现）

**Definition of Done**
- 核心层具备单元测试（至少 6 条：开始/暂停/重启/超时/番茄切换/配置变更）
- 核心层不引用任何 Avalonia/WPF 命名空间

### Task C：MVVM 最小闭环（Day 4~5）

在 `ProjectEye.App` 中建立：

- `MainViewModel`：显示当前状态（Working/Resting/Tomato）和剩余时间
- `SettingsViewModel`：可修改提醒时长、番茄时长
- 命令全部使用 `[RelayCommand]` / `[AsyncRelayCommand]`

**Definition of Done**
- UI 上可点击“开始/暂停/跳过”并驱动状态变化
- 配置修改后能持久化到 `config.json`

### Task D：旧配置导入器（Day 6）

实现一次性迁移：

- 输入：旧版 `Data/config.xml`
- 输出：新版 `Data/config.json`
- 仅迁关键字段：
  - 提醒间隔
  - 声音开关
  - 主题名
  - 语言
  - 番茄相关时长

**Definition of Done**
- 首次启动可自动检测旧配置并导入
- 导入失败不阻塞启动（记录日志，使用默认配置）

## 3) 先不做（避免过早复杂化）

以下内容暂缓到 Sprint-1：

- 托盘完整功能（右键菜单/动态图标）
- 全局快捷键
- 前台窗口全屏检测
- 音频峰值占用检测
- 自定义控件库 `Project1.UI` 的视觉 1:1 复刻

## 4) 风险兜底策略（务必执行）

- 所有 Windows 专属能力都包在 `Platform.Windows` 中，主业务仅依赖抽象接口。
- `Platform.Linux/Mac` 先给 no-op 实现，保证可编译可运行。
- 每做完一个任务都保留可回滚提交，不要在一个大提交里混 10 个改动。

## 5) 建议你现在就创建的 3 个 issue

1. `bootstrap-new-avalonia-solution`
2. `extract-core-state-machine-from-wpf-services`
3. `implement-config-importer-xml-to-json`

## 6) 验收清单（你可以直接拿来验收）

- [ ] 新老项目并行存在，互不影响
- [ ] 新项目可在 Windows 启动
- [ ] 状态机逻辑可测（单元测试通过）
- [ ] 配置可读写，且支持旧配置导入
- [ ] UI 可完成最小交互闭环（开始/暂停/跳过）

---

如果你同意，我下一步可以直接提交 **Task A 的真实代码骨架**（新建 .NET 8 + Avalonia + CommunityToolkit.Mvvm 目录与项目文件），并保证可 `dotnet build`。
