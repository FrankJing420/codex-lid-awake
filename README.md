# Codex Lid Awake

[简体中文](#简体中文) · [English](#english)

Keep a MacBook awake, online, and able to continue a long-running Codex task after the lid is closed.

> [!WARNING]
> This intentionally prevents system sleep. Keep the Mac ventilated, preferably on power, and **never put it in a bag while this is active**. It increases power use and may cause overheating.

## English

### What it does

`Codex Lid Awake` is a small macOS AppleScript app that toggles the built-in `pmset disablesleep` setting for a chosen duration (1, 2, 4, or 8 hours). While active, closing the lid does not put the Mac to sleep, so a connected iPhone Personal Hotspot and ongoing Codex work can continue.

The app asks for administrator authentication only when enabling or disabling the setting. A root timer restores normal sleep automatically. The included LaunchDaemon resets the setting to `0` at boot as an additional safety guard.

`disablesleep` is an undocumented `pmset` setting, so test after macOS upgrades and use at your own risk.

### Requirements

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

这是一个轻量的 macOS AppleScript 工具。它会在选定的 1、2、4 或 8 小时内开启系统的 `pmset disablesleep`，使 MacBook 合盖后不进入睡眠。这样手机热点和正在运行的 Codex 任务可以继续工作。

启用和关闭时会要求管理员授权；根权限计时器会在到期时自动恢复正常睡眠。附带的 LaunchDaemon 会在每次开机时把该设置复位为 `0`，避免意外持续禁用睡眠。

`disablesleep` 是未公开文档化的 `pmset` 设置。请在 macOS 升级后自行验证，并自行承担使用风险。

### 安装

```zsh
git clone https://github.com/FrankJing420/codex-lid-awake.git
cd codex-lid-awake
./scripts/build-app.sh
./scripts/install.sh
```

之后在“应用程序”中打开 **Codex Lid Awake**，选择时长并完成授权；需要提前结束时再次打开该工具即可。

### 紧急恢复

若无法打开工具，在终端执行：

```zsh
sudo pmset -a disablesleep 0
```

## License

[MIT](LICENSE)
