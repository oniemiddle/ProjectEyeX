# ProjectEyeX WPF -> AvaloniaUI 迁移路线图（CommunityToolkit.Mvvm）

## 1. 当前现状速览（扫描结论）

- 当前主程序 `ProjectEye` 与 UI 组件库 `Project1.UI` 都是 **.NET Framework 4.5 + WPF**。
- 解决方案包含 4 个项目：`ProjectEye`（主程序）、`Project1.UI`（自定义控件）、`ProjectEyeUp`（升级器）、`ProjectEyeBug`（崩溃上报/异常窗口）。
- 代码中存在大量 Windows 绑定能力：
  - Win32 P/Invoke（窗口信息、DPI、系统版本）
  - `System.Windows.Forms.NotifyIcon`（托盘）和多屏 `Screen`
  - Windows COM（音频峰值检测）
  - WScript.Shell 快捷方式（开机启动/桌面快捷方式）
  - WPF 专有 `ResourceDictionary`/样式体系、`DispatcherTimer`、`pack://` 资源路径

## 2. 迁移目标架构（建议）

建议拆成 **四层**，逐步把 WPF 依赖隔离出去：

1. `ProjectEye.Core`（新建，.NET 8，跨平台）
   - 业务状态机：工作/休息/番茄/统计
   - 配置、统计存储抽象接口
   - 不依赖任何 UI 框架
2. `ProjectEye.App`（新建，Avalonia）
   - View/ViewModel（CommunityToolkit.Mvvm）
   - 主题、国际化、窗口交互
3. `ProjectEye.Platform`（按需多实现）
   - `WindowsPlatformServices`（托盘、全局快捷键、前台窗口检测）
   - `Mac/LinuxPlatformServices`（受限能力的替代实现）
4. `ProjectEye.LegacyBridge`（临时）
   - 迁移过渡阶段复用旧数据模型/导入逻辑

## 3. 分阶段迁移路线

### Phase 0：基线与冻结（1~2 天）
- 冻结旧版功能，建立回归清单（提醒触发、番茄、托盘、语言、主题、统计）。
- 给旧工程打标签（如 `wpf-last-stable`）。

### Phase 1：先搬“可跨平台核心”（3~5 天）
- 提取纯业务逻辑：
  - `MainService`/`RestService`/`TomatoService` 的状态流转
  - 配置模型与默认值
  - 统计写入接口
- 定义接口：
  - `ITrayService`、`IHotkeyService`、`IForegroundWindowService`、`IAudioMeterService`、`IStartupService`
- 将 `DispatcherTimer` 替换为 `PeriodicTimer` / `System.Threading.Timer` + `CancellationToken`。

### Phase 2：MVVM 现代化（3~7 天）
- ViewModel 全量切换到 CommunityToolkit.Mvvm：
  - `ObservableObject`
  - `[ObservableProperty]`
  - `[RelayCommand]` / `[AsyncRelayCommand]`
- 逐步替换旧 `Command` 与手写 `INotifyPropertyChanged`。

### Phase 3：Avalonia UI 骨架（5~10 天）
- 创建 Avalonia 主程序（.NET 8）。
- 先落地窗口：设置、统计、关于、更新提示。
- 主题/语言迁移：
  - WPF ResourceDictionary => Avalonia Styles + `FluentTheme`
  - 语言文件 => i18n 资源（建议 JSON/RESX 统一）

### Phase 4：高风险能力分平台落地（7~14 天）
- 托盘：优先 Avalonia 生态插件（如 `Avalonia.Controls.Notifications` + 平台插件）。
- 全局快捷键：Windows 先实现（P/Invoke），Linux/macOS 走可降级方案。
- 前台窗口 + 全屏检测：保留 Windows 增强能力；非 Windows 改为“仅基于输入活动检测”。
- 音频占用检测：Windows 保留 COM 实现，其他平台可选脉冲/禁用。

### Phase 5：数据与升级链路（3~5 天）
- 数据层从 EF6 + System.Data.SQLite 迁移到：
  - 方案 A：EF Core + SQLite
  - 方案 B：Dapper + SQLite（更轻）
- 新增一次性迁移器：读取旧 `Data\config.xml` 和旧 `data.db`。
- `ProjectEyeUp`（升级器）建议改为单文件跨平台更新方案（或保留 Windows 专用更新器）。

### Phase 6：发布验证与灰度（3~7 天）
- 平台矩阵：Windows 10/11、Ubuntu LTS、macOS。
- 功能开关控制高风险模块，先发布 Windows Avalonia 版，再逐步开放 Linux/macOS。

## 4. 跨平台可行性验证（结论）

### 4.1 可直接跨平台迁移（低风险）
- 配置模型、番茄/休息状态机、统计计算、HTTP 下载/版本检查、JSON 读写。

### 4.2 可迁移但需改造（中风险）
- UI 样式体系（WPF XAML -> Avalonia XAML，需要重写模板）
- 自定义控件库 `Project1.UI`（基本需要重建）
- 定时器与线程模型（WPF Dispatcher 语义替换）

### 4.3 平台绑定能力（高风险）
- 托盘图标与托盘菜单
- 前台窗口检测 / 全屏识别
- 全局热键
- 音频峰值检测
- 开机启动快捷方式（WScript.Shell）

**建议策略**：定义能力分级
- L1（全平台必须）：提醒、番茄、统计、设置
- L2（Windows 增强）：托盘、全局快捷键、前台窗口全屏检测、音频占用感知

## 5. 建议的包与技术栈

- UI：`Avalonia` + `Avalonia.Desktop`
- MVVM：`CommunityToolkit.Mvvm`
- DI：`Microsoft.Extensions.DependencyInjection`
- 日志：`Serilog`
- 数据：`Microsoft.Data.Sqlite` + `EF Core`（或 Dapper）
- 配置：`System.Text.Json`

## 6. 第一批落地任务（可直接开工）

1. 新建 `ProjectEye.Core`（.NET 8 classlib）并迁出配置模型、计时状态机。
2. 新建 `ProjectEye.App`（Avalonia）并接入 CommunityToolkit.Mvvm。
3. 完成“设置页 + 提醒倒计时 + 番茄倒计时”的最小可运行版本（不含托盘）。
4. 封装平台服务接口，先做 Windows 实现，Linux/macOS 提供 no-op。
5. 实现旧配置导入（`config.xml` -> `config.json`）。

## 7. 粗略工期（1~2 人）

- MVP（Windows 可替代旧版，含核心功能不含全部增强）：**3~5 周**
- 跨平台稳定版（Windows + Linux + macOS 基础可用）：**6~10 周**
- 完整对齐旧版增强能力（含分平台特性细化）：**10~14 周**

## 8. 风险清单与缓解

- 风险：旧 `Project1.UI` 控件重写成本高
  - 缓解：先用 Avalonia 原生控件实现 MVP，再逐步重建视觉层
- 风险：平台能力差异导致“体验不一致”
  - 缓解：明确 L1/L2 能力分级，UI 提示“该功能仅 Windows 支持”
- 风险：数据迁移失败导致历史丢失
  - 缓解：只读导入 + 回滚包 + 首次启动备份

