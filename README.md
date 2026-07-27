# Codex Lid Awake

[简体中文](#简体中文) · [English](#english)

Keep a Windows or macOS laptop awake and online while its lid is closed, so long-running Codex tasks can continue.

> [!WARNING]
> This intentionally prevents system sleep. Keep the laptop ventilated, preferably connected to power, and **never put it in a bag while active**.

## 简体中文

### Windows：下载安装包即可使用

[**下载最新版 Windows 安装包（CodexLidAwakeSetup.exe）**](https://github.com/FrankJing420/codex-lid-awake/releases/latest/download/CodexLidAwakeSetup.exe)

支持 64 位 Windows 10 和 Windows 11，不需要安装 PowerShell 脚本、Git 或 .NET。

1. 点击上面的下载链接。
2. 双击下载的 `CodexLidAwakeSetup.exe`。
3. 如果 Windows 显示“Windows 已保护你的电脑”，点击“更多信息”，再点击“仍要运行”。这是因为开源安装包目前没有商业代码签名证书。
4. 按安装向导完成安装。安装结束后软件会自动打开。
5. 在窗口中选择 1、2、4 或 8 小时，点击“开启合盖联网”。
6. 看到“已开启”后即可合盖；网络和 Codex 任务会继续运行。

需要提前结束时，再次打开桌面的 **Codex Lid Awake**，点击“恢复正常睡眠”。

Windows 版本会：

- 分别保存接通电源和使用电池时原有的合盖设置；
- 使用 Windows 原生电源 API 将合盖操作临时设为“不执行任何操作”；
- 在选定时间到期时自动恢复；
- 若电脑在开启期间重启，在下次开机时自动恢复；
- 卸载软件前恢复原有设置。

### macOS：构建并安装

```zsh
git clone https://github.com/FrankJing420/codex-lid-awake.git
cd codex-lid-awake
./scripts/build-app.sh
./scripts/install.sh
```

之后在“应用程序”中打开 **Codex Lid Awake**，选择时长并完成管理员授权。需要提前结束时再次打开即可恢复。

macOS 版本使用 `pmset disablesleep`，并附带定时恢复和开机复位保护。`disablesleep` 是未公开文档化的系统设置，请在 macOS 大版本升级后重新验证。

### 从源码构建 Windows 版本

需要 .NET 8 SDK：

```powershell
dotnet publish windows/app/CodexLidAwake.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  --output windows/publish `
  -p:PublishSingleFile=true
```

旧版命令行 PowerShell 实现仍保留在 [`windows/CodexLidAwake.ps1`](windows/CodexLidAwake.ps1)，仅供高级用户和源码参考。普通用户应使用安装包。

### 技术说明

Windows 桌面软件直接调用 Microsoft 的 `PowerGetActiveScheme`、`PowerReadACValueIndex`、`PowerReadDCValueIndex`、`PowerWriteACValueIndex`、`PowerWriteDCValueIndex` 和 `PowerSetActiveScheme` API，不依赖本地化的命令输出或可能缺失的注册表路径。

- [Microsoft：合盖操作及其取值](https://learn.microsoft.com/zh-cn/windows-hardware/customize/power-settings/power-button-and-lid-settings-lid-switch-close-action)
- [Microsoft：PowerGetActiveScheme](https://learn.microsoft.com/windows/win32/api/powersetting/nf-powersetting-powergetactivescheme)
- [Microsoft：PowerReadDCValueIndex](https://learn.microsoft.com/windows/win32/api/powrprof/nf-powrprof-powerreaddcvalueindex)

### macOS 紧急恢复

```zsh
sudo pmset -a disablesleep 0
```

## English

### Windows: install and use

[**Download the latest Windows installer (CodexLidAwakeSetup.exe)**](https://github.com/FrankJing420/codex-lid-awake/releases/latest/download/CodexLidAwakeSetup.exe)

The installer supports 64-bit Windows 10 and Windows 11. Git, .NET, and PowerShell setup are not required.

1. Download and double-click `CodexLidAwakeSetup.exe`.
2. If Microsoft Defender SmartScreen appears, select **More info**, then **Run anyway**. The open-source installer is not currently signed with a commercial code-signing certificate.
3. Complete the installer and open **Codex Lid Awake**.
4. Select 1, 2, 4, or 8 hours, then choose **Enable**.
5. Open the app again and choose **Restore** if you want normal lid sleep back early.

The Windows app saves both AC and battery lid actions, uses native Windows power APIs to set **Do nothing**, and restores the saved values at the selected time, on the next boot, or during uninstall.

### macOS: build and install

```zsh
git clone https://github.com/FrankJing420/codex-lid-awake.git
cd codex-lid-awake
./scripts/build-app.sh
./scripts/install.sh
```

Open **Codex Lid Awake** from Applications, select a duration, and authenticate. Open it again to restore normal sleep early.

The macOS implementation uses the undocumented `pmset disablesleep` setting, with timed and boot-time recovery guards.

### Build the Windows app from source

.NET 8 SDK is required:

```powershell
dotnet publish windows/app/CodexLidAwake.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  --output windows/publish `
  -p:PublishSingleFile=true
```

The earlier command-line implementation remains at [`windows/CodexLidAwake.ps1`](windows/CodexLidAwake.ps1) for advanced users and source reference.

## License

[MIT](LICENSE)
