# Codex Lid Awake for macOS

[简体中文](#简体中文) · [English](#english)

Keep a MacBook awake and online while its lid is closed, so long-running Codex tasks can continue.

> [!WARNING]
> This intentionally prevents system sleep. Keep the MacBook ventilated, preferably connected to power, and **never put it in a bag while active**.

## 简体中文

### 功能

`Codex Lid Awake` 是一个轻量的 macOS AppleScript 应用。开启后，即使合上 MacBook 盖子，系统也不会进入睡眠，因此手机热点、网络连接和正在运行的 Codex 任务可以继续工作。

- 可选择保持 1、2、4 或 8 小时；
- 到期后自动恢复正常睡眠；
- 再次打开应用可提前恢复；
- 附带开机复位保护，意外重启后恢复默认合盖睡眠；
- 不安装第三方后台服务。

### 构建与安装

打开“终端”，依次执行：

```zsh
git clone https://github.com/FrankJing420/codex-lid-awake.git
cd codex-lid-awake
./scripts/build-app.sh
./scripts/install.sh
```

安装完成后，在“应用程序”中打开 **Codex Lid Awake**，选择时长并完成管理员授权。需要提前结束时，再次打开应用并选择“恢复正常睡眠”。

### 工作原理

应用使用 macOS 的 `pmset disablesleep` 设置临时阻止系统睡眠，并使用根权限计时器按时恢复。附带的 LaunchDaemon 会在每次开机时将该设置复位为 `0`。

`disablesleep` 是未公开文档化的系统设置，请在 macOS 大版本升级后重新验证。

### 紧急恢复

若应用无法打开，可在终端执行：

```zsh
sudo pmset -a disablesleep 0
```

### 卸载

```zsh
sudo rm -rf "/Applications/Codex Lid Awake.app"
sudo launchctl bootout system/local.codex.lidawake-reset
sudo rm -f /Library/LaunchDaemons/local.codex.lidawake-reset.plist
```

## English

### Features

`Codex Lid Awake` is a lightweight macOS AppleScript app. While enabled, closing the MacBook lid does not put the system to sleep, allowing a phone hotspot, network connection, and long-running Codex tasks to continue.

- Timed sessions of 1, 2, 4, or 8 hours;
- Automatic restoration when the timer expires;
- Early restoration by opening the app again;
- A boot-time safety guard that restores normal lid sleep after an unexpected restart;
- No third-party background service.

### Build and install

```zsh
git clone https://github.com/FrankJing420/codex-lid-awake.git
cd codex-lid-awake
./scripts/build-app.sh
./scripts/install.sh
```

Open **Codex Lid Awake** from Applications, select a duration, and authenticate. Open it again to restore normal sleep early.

### How it works

The app temporarily enables the undocumented macOS `pmset disablesleep` setting. A privileged timer restores the original behavior when the session expires, and the included LaunchDaemon resets the setting to `0` at every boot.

### Emergency recovery

```zsh
sudo pmset -a disablesleep 0
```

## License

[MIT](LICENSE)
