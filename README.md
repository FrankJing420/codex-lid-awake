# Codex Lid Awake

[简体中文](#简体中文) · [English](#english)

Keep a laptop awake, online, and able to continue a long-running Codex task after the lid is closed. macOS and Windows PowerShell implementations are included.

> [!WARNING]
> This intentionally prevents system sleep. Keep the Mac ventilated, preferably on power, and **never put it in a bag while this is active**. It increases power use and may cause overheating.

## English

### What it does

`Codex Lid Awake` has two native implementations:

- **macOS:** an AppleScript app that toggles the built-in `pmset disablesleep` setting for 1, 2, 4, or 8 hours.
- **Windows:** an elevated PowerShell script that temporarily changes the active plan's lid-close action to **Do nothing** for a chosen number of hours.

While active, closing the lid does not put the laptop to sleep, so a connected phone hotspot and ongoing Codex work can continue.

The app asks for administrator authentication only when enabling or disabling the setting. A root timer restores normal sleep automatically. The included LaunchDaemon resets the setting to `0` at boot as an additional safety guard.

`disablesleep` is an undocumented `pmset` setting, so test after macOS upgrades and use at your own risk.

### macOS requirements

- macOS with AppleScript and `pmset`
- An administrator account

### Build and install

```zsh
git clone https://github.com/FrankJing420/codex-lid-awake.git
cd codex-lid-awake
./scripts/build-app.sh
./scripts/install.sh
```

Open **Codex Lid Awake** from Applications, select a duration, then authenticate. Open it again to restore normal sleep early.

### Windows PowerShell

No Git installation, download, `cd`, or execution-policy change is needed.

1. Press the Windows key and type **Windows PowerShell**.
2. Right-click it, choose **Run as administrator**, then choose **Yes**.
3. Copy the entire block below, paste it into the blue PowerShell window, and press Enter. Change only `-Hours 2` if you want 1–24 hours.

```powershell
$scriptPath = Join-Path $env:TEMP 'CodexLidAwake.ps1'
Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/FrankJing420/codex-lid-awake/main/windows/CodexLidAwake.ps1' -OutFile $scriptPath
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Action Enable -Hours 2
```

When you see `Enabled...`, you can close the lid. The script records the current AC and battery lid-close actions, sets both to **Do nothing**, and registers a recovery task. It automatically restores the saved values when the duration expires or at the next system startup.

To restore normal lid behavior early, open **Windows PowerShell as Administrator** again and paste this entire block:

```powershell
$scriptPath = Join-Path $env:TEMP 'CodexLidAwake.ps1'
Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/FrankJing420/codex-lid-awake/main/windows/CodexLidAwake.ps1' -OutFile $scriptPath
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Action Disable
```

To check whether it is active, replace `Disable` in the last line with `Status`.

The PowerShell implementation uses the documented `powercfg /setacvalueindex` and `/setdcvalueindex` interfaces for power-scheme settings. [Microsoft documentation](https://learn.microsoft.com/windows-hardware/design/device-experiences/powercfg-command-line-options)

### Emergency recovery

If the app cannot be opened, restore default lid sleep in Terminal:

```zsh
sudo pmset -a disablesleep 0
```

### Uninstall

```zsh
sudo rm -rf "/Applications/Codex Lid Awake.app"
sudo launchctl bootout system/local.codex.lidawake-reset
sudo rm -f /Library/LaunchDaemons/local.codex.lidawake-reset.plist
```

## 简体中文

### 用途

项目包含两份原生实现：

- **macOS：**轻量的 AppleScript 工具，在选定的 1、2、4 或 8 小时内开启系统的 `pmset disablesleep`。
- **Windows：**需要管理员权限的 PowerShell 脚本，临时将当前电源计划的“合盖操作”改为“**不执行任何操作**”。

启用后，笔记本合盖不会进入睡眠，因此手机热点和正在运行的 Codex 任务可以继续工作。

启用和关闭时会要求管理员授权；根权限计时器会在到期时自动恢复正常睡眠。附带的 LaunchDaemon 会在每次开机时把该设置复位为 `0`，避免意外持续禁用睡眠。

`disablesleep` 是未公开文档化的 `pmset` 设置。请在 macOS 升级后自行验证，并自行承担使用风险。

### macOS 安装

```zsh
git clone https://github.com/FrankJing420/codex-lid-awake.git
cd codex-lid-awake
./scripts/build-app.sh
./scripts/install.sh
```

之后在“应用程序”中打开 **Codex Lid Awake**，选择时长并完成授权；需要提前结束时再次打开该工具即可。

### Windows PowerShell 使用方法

不需要安装 Git、不需要下载压缩包、不需要输入 `cd`，也不需要修改执行策略。

1. 按 Windows 键，输入 **Windows PowerShell**。
2. 右键点击它，选择“**以管理员身份运行**”，然后点击“是”。
3. 完整复制下面这一段，粘贴到蓝色 PowerShell 窗口并按回车。只需要按需把最后的 `-Hours 2` 改为 1–24 小时。

```powershell
$scriptPath = Join-Path $env:TEMP 'CodexLidAwake.ps1'
Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/FrankJing420/codex-lid-awake/main/windows/CodexLidAwake.ps1' -OutFile $scriptPath
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Action Enable -Hours 2
```

看到 `Enabled...` 后即可合盖。脚本会保存当前电源计划在接通电源和使用电池时的合盖策略，然后将两者临时设为“不执行任何操作”。它会创建恢复任务，在指定时长结束或下次系统启动时自动还原原始设置。

需要提前恢复正常合盖行为时，再次以**管理员身份**打开 Windows PowerShell，完整粘贴下面这一段：

```powershell
$scriptPath = Join-Path $env:TEMP 'CodexLidAwake.ps1'
Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/FrankJing420/codex-lid-awake/main/windows/CodexLidAwake.ps1' -OutFile $scriptPath
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Action Disable
```

若想查看是否仍开启，只需把最后一行的 `Disable` 改成 `Status`。

Windows 版本使用 Microsoft 文档化的 `powercfg /setacvalueindex` 和 `/setdcvalueindex` 接口管理电源计划。[Microsoft 文档](https://learn.microsoft.com/zh-cn/windows-hardware/design/device-experiences/powercfg-command-line-options)

### 紧急恢复

若无法打开工具，在终端执行：

```zsh
sudo pmset -a disablesleep 0
```

## License

[MIT](LICENSE)
