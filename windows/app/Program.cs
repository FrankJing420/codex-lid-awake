using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Xml.Linq;

namespace CodexLidAwake;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Any(arg => string.Equals(arg, "--restore", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                LidAwakeService.Restore();
            }
            catch (Exception exception)
            {
                LidAwakeService.LogFailure(exception);
                Environment.ExitCode = 1;
            }
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

internal sealed class MainForm : Form
{
    private readonly ComboBox durationBox = new();
    private readonly Label statusLabel = new();
    private readonly Button enableButton = new();
    private readonly Button restoreButton = new();

    public MainForm()
    {
        Text = "Codex 合盖联网";
        ClientSize = new Size(520, 390);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Microsoft YaHei UI", 10F);
        BackColor = Color.FromArgb(247, 249, 252);

        var title = new Label
        {
            Text = "Codex 合盖联网",
            Font = new Font("Microsoft YaHei UI", 22F, FontStyle.Bold),
            ForeColor = Color.FromArgb(24, 35, 54),
            AutoSize = true,
            Location = new Point(28, 24)
        };

        var subtitle = new Label
        {
            Text = "合上 Windows 笔记本盖子后，继续保持联网并运行任务。",
            ForeColor = Color.FromArgb(75, 87, 105),
            AutoSize = true,
            Location = new Point(31, 73)
        };

        var statusPanel = new Panel
        {
            BackColor = Color.White,
            Location = new Point(30, 111),
            Size = new Size(460, 67),
            BorderStyle = BorderStyle.FixedSingle
        };
        var statusCaption = new Label
        {
            Text = "当前状态",
            ForeColor = Color.FromArgb(110, 120, 136),
            AutoSize = true,
            Location = new Point(15, 10)
        };
        statusLabel.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
        statusLabel.AutoSize = true;
        statusLabel.Location = new Point(14, 34);
        statusPanel.Controls.Add(statusCaption);
        statusPanel.Controls.Add(statusLabel);

        var durationLabel = new Label
        {
            Text = "保持时长",
            AutoSize = true,
            Location = new Point(31, 205)
        };

        durationBox.DropDownStyle = ComboBoxStyle.DropDownList;
        durationBox.Location = new Point(119, 201);
        durationBox.Size = new Size(150, 30);
        durationBox.Items.AddRange(
        [
            new DurationOption("1 小时", 1),
            new DurationOption("2 小时", 2),
            new DurationOption("4 小时", 4),
            new DurationOption("8 小时", 8)
        ]);
        durationBox.SelectedIndex = 1;

        enableButton.Text = "开启合盖联网";
        enableButton.Size = new Size(218, 44);
        enableButton.Location = new Point(30, 259);
        enableButton.BackColor = Color.FromArgb(44, 105, 222);
        enableButton.ForeColor = Color.White;
        enableButton.FlatStyle = FlatStyle.Flat;
        enableButton.FlatAppearance.BorderSize = 0;
        enableButton.Click += EnableButton_Click;

        restoreButton.Text = "恢复正常睡眠";
        restoreButton.Size = new Size(218, 44);
        restoreButton.Location = new Point(272, 259);
        restoreButton.BackColor = Color.White;
        restoreButton.ForeColor = Color.FromArgb(40, 54, 76);
        restoreButton.FlatStyle = FlatStyle.Flat;
        restoreButton.FlatAppearance.BorderColor = Color.FromArgb(193, 201, 214);
        restoreButton.Click += RestoreButton_Click;

        var warning = new Label
        {
            Text = "⚠ 开启期间请保持通风，最好接通电源；绝对不要把电脑放进包里。",
            ForeColor = Color.FromArgb(164, 82, 20),
            AutoSize = true,
            Location = new Point(31, 329)
        };

        Controls.AddRange(
        [
            title, subtitle, statusPanel, durationLabel, durationBox,
            enableButton, restoreButton, warning
        ]);

        Shown += (_, _) => RefreshStatus();
    }

    private void EnableButton_Click(object? sender, EventArgs e)
    {
        if (durationBox.SelectedItem is not DurationOption option)
        {
            return;
        }

        var confirmation = MessageBox.Show(
            $"开启后，合盖不会让电脑睡眠，并将在 {option.Hours} 小时后自动恢复。\n\n" +
            "请保持通风，切勿放进包里。是否继续？",
            "确认开启",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        RunAction(() =>
        {
            LidAwakeService.Enable(option.Hours);
            MessageBox.Show(
                "已开启。现在可以合盖，网络和 Codex 任务会继续运行。",
                "Codex 合盖联网",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        });
    }

    private void RestoreButton_Click(object? sender, EventArgs e)
    {
        RunAction(() =>
        {
            var restored = LidAwakeService.Restore();
            MessageBox.Show(
                restored ? "已恢复正常睡眠。合盖后电脑会按原来的设置运行。" : "当前没有开启合盖联网。",
                "Codex 合盖联网",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        });
    }

    private void RunAction(Action action)
    {
        try
        {
            UseWaitCursor = true;
            enableButton.Enabled = false;
            restoreButton.Enabled = false;
            action();
        }
        catch (Exception exception)
        {
            LidAwakeService.LogFailure(exception);
            MessageBox.Show(
                exception.Message,
                "操作失败",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
            RefreshStatus();
        }
    }

    private void RefreshStatus()
    {
        var state = LidAwakeService.GetState();
        if (state is null)
        {
            statusLabel.Text = "未开启（合盖将正常睡眠）";
            statusLabel.ForeColor = Color.FromArgb(65, 78, 98);
            enableButton.Enabled = true;
            restoreButton.Enabled = false;
            durationBox.Enabled = true;
            return;
        }

        statusLabel.Text = $"已开启，将于 {state.RestoreAt.LocalDateTime:yyyy-MM-dd HH:mm} 自动恢复";
        statusLabel.ForeColor = Color.FromArgb(24, 139, 79);
        enableButton.Enabled = false;
        restoreButton.Enabled = true;
        durationBox.Enabled = false;
    }

    private sealed record DurationOption(string Label, int Hours)
    {
        public override string ToString() => Label;
    }
}

internal static class LidAwakeService
{
    private const string RestoreTaskName = "CodexLidAwake-Restore";
    private static readonly string StateDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "CodexLidAwake");
    private static readonly string StatePath = Path.Combine(StateDirectory, "state.json");
    private static readonly string TaskXmlPath = Path.Combine(StateDirectory, "restore-task.xml");
    private static readonly string LogPath = Path.Combine(StateDirectory, "error.log");

    public static LidAwakeState? GetState()
    {
        if (!File.Exists(StatePath))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<LidAwakeState>(File.ReadAllText(StatePath));
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("无法读取以前保存的电源设置。请重新安装软件或查看错误日志。", exception);
        }
    }

    public static void Enable(int hours)
    {
        if (hours is < 1 or > 24)
        {
            throw new ArgumentOutOfRangeException(nameof(hours), "时长必须在 1 到 24 小时之间。");
        }

        if (GetState() is not null)
        {
            throw new InvalidOperationException("合盖联网已经开启。请先恢复正常睡眠。");
        }

        Directory.CreateDirectory(StateDirectory);
        var scheme = NativePower.GetActiveScheme();
        var state = new LidAwakeState(
            scheme,
            NativePower.ReadAcValue(scheme),
            NativePower.ReadDcValue(scheme),
            DateTimeOffset.Now.AddHours(hours));

        File.WriteAllText(StatePath, JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }));

        try
        {
            RegisterRestoreTask(state.RestoreAt);
            NativePower.WriteValues(scheme, 0, 0);
            NativePower.SetActiveScheme(scheme);
        }
        catch
        {
            try
            {
                Restore();
            }
            catch
            {
                // Keep the original exception; the saved state remains available for recovery.
            }
            throw;
        }
    }

    public static bool Restore()
    {
        var state = GetState();
        if (state is null)
        {
            DeleteRestoreTask(ignoreFailure: true);
            return false;
        }

        NativePower.WriteValues(state.SchemeGuid, state.AcValue, state.DcValue);
        if (NativePower.GetActiveScheme() == state.SchemeGuid)
        {
            NativePower.SetActiveScheme(state.SchemeGuid);
        }

        File.Delete(StatePath);
        DeleteRestoreTask(ignoreFailure: true);
        return true;
    }

    public static void LogFailure(Exception exception)
    {
        try
        {
            Directory.CreateDirectory(StateDirectory);
            File.AppendAllText(LogPath, $"[{DateTimeOffset.Now:O}]\n{exception}\n\n");
        }
        catch
        {
            // Logging must never prevent recovery or UI shutdown.
        }
    }

    private static void RegisterRestoreTask(DateTimeOffset restoreAt)
    {
        var executablePath = Environment.ProcessPath
            ?? throw new InvalidOperationException("无法确定程序安装路径。");
        XNamespace task = "http://schemas.microsoft.com/windows/2004/02/mit/task";

        var document = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(task + "Task",
                new XAttribute("version", "1.4"),
                new XElement(task + "RegistrationInfo",
                    new XElement(task + "Description", "Restore the original laptop lid-close action.")),
                new XElement(task + "Triggers",
                    new XElement(task + "TimeTrigger",
                        new XElement(task + "StartBoundary", restoreAt.LocalDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss")),
                        new XElement(task + "Enabled", "true")),
                    new XElement(task + "BootTrigger",
                        new XElement(task + "Enabled", "true"))),
                new XElement(task + "Principals",
                    new XElement(task + "Principal",
                        new XAttribute("id", "Author"),
                        new XElement(task + "UserId", "S-1-5-18"),
                        new XElement(task + "LogonType", "ServiceAccount"),
                        new XElement(task + "RunLevel", "HighestAvailable"))),
                new XElement(task + "Settings",
                    new XElement(task + "MultipleInstancesPolicy", "IgnoreNew"),
                    new XElement(task + "DisallowStartIfOnBatteries", "false"),
                    new XElement(task + "StopIfGoingOnBatteries", "false"),
                    new XElement(task + "AllowHardTerminate", "true"),
                    new XElement(task + "StartWhenAvailable", "true"),
                    new XElement(task + "RunOnlyIfNetworkAvailable", "false"),
                    new XElement(task + "WakeToRun", "false"),
                    new XElement(task + "ExecutionTimeLimit", "PT5M"),
                    new XElement(task + "Priority", "7")),
                new XElement(task + "Actions",
                    new XAttribute("Context", "Author"),
                    new XElement(task + "Exec",
                        new XElement(task + "Command", executablePath),
                        new XElement(task + "Arguments", "--restore")))));

        document.Save(TaskXmlPath);
        RunScheduledTasks(["/Create", "/TN", RestoreTaskName, "/XML", TaskXmlPath, "/F"], ignoreFailure: false);
    }

    private static void DeleteRestoreTask(bool ignoreFailure) =>
        RunScheduledTasks(["/Delete", "/TN", RestoreTaskName, "/F"], ignoreFailure);

    private static void RunScheduledTasks(IEnumerable<string> arguments, bool ignoreFailure)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(Environment.SystemDirectory, "schtasks.exe"),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("无法启动 Windows 任务计划程序。");
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0 && !ignoreFailure)
        {
            var detail = string.IsNullOrWhiteSpace(error) ? output : error;
            throw new InvalidOperationException($"无法创建自动恢复任务：{detail.Trim()}");
        }
    }
}

internal sealed record LidAwakeState(
    Guid SchemeGuid,
    uint AcValue,
    uint DcValue,
    DateTimeOffset RestoreAt);

internal static class NativePower
{
    private static readonly Guid ButtonAndLidSubgroup =
        new("4f971e89-eebd-4455-a8de-9e59040e7347");
    private static readonly Guid LidCloseAction =
        new("5ca83367-6e45-459f-a27b-476b1d01c936");

    public static Guid GetActiveScheme()
    {
        Check(PowerGetActiveScheme(IntPtr.Zero, out var pointer), "读取当前电源计划");
        try
        {
            return Marshal.PtrToStructure<Guid>(pointer);
        }
        finally
        {
            LocalFree(pointer);
        }
    }

    public static uint ReadAcValue(Guid scheme)
    {
        var subgroup = ButtonAndLidSubgroup;
        var setting = LidCloseAction;
        Check(
            PowerReadACValueIndex(
                IntPtr.Zero, ref scheme, ref subgroup, ref setting, out var value),
            "读取接通电源时的合盖设置");
        return value;
    }

    public static uint ReadDcValue(Guid scheme)
    {
        var subgroup = ButtonAndLidSubgroup;
        var setting = LidCloseAction;
        Check(
            PowerReadDCValueIndex(
                IntPtr.Zero, ref scheme, ref subgroup, ref setting, out var value),
            "读取使用电池时的合盖设置");
        return value;
    }

    public static void WriteValues(Guid scheme, uint acValue, uint dcValue)
    {
        var subgroup = ButtonAndLidSubgroup;
        var setting = LidCloseAction;
        Check(
            PowerWriteACValueIndex(
                IntPtr.Zero, ref scheme, ref subgroup, ref setting, acValue),
            "修改接通电源时的合盖设置");
        Check(
            PowerWriteDCValueIndex(
                IntPtr.Zero, ref scheme, ref subgroup, ref setting, dcValue),
            "修改使用电池时的合盖设置");
    }

    public static void SetActiveScheme(Guid scheme) =>
        Check(PowerSetActiveScheme(IntPtr.Zero, ref scheme), "应用电源设置");

    private static void Check(uint result, string action)
    {
        if (result != 0)
        {
            throw new Win32Exception((int)result, $"{action}失败（Windows 错误 {result}）。");
        }
    }

    [DllImport("powrprof.dll")]
    private static extern uint PowerGetActiveScheme(IntPtr userRootPowerKey, out IntPtr activePolicyGuid);

    [DllImport("powrprof.dll")]
    private static extern uint PowerReadACValueIndex(
        IntPtr rootPowerKey,
        ref Guid schemeGuid,
        ref Guid subgroupGuid,
        ref Guid settingGuid,
        out uint valueIndex);

    [DllImport("powrprof.dll")]
    private static extern uint PowerReadDCValueIndex(
        IntPtr rootPowerKey,
        ref Guid schemeGuid,
        ref Guid subgroupGuid,
        ref Guid settingGuid,
        out uint valueIndex);

    [DllImport("powrprof.dll")]
    private static extern uint PowerWriteACValueIndex(
        IntPtr rootPowerKey,
        ref Guid schemeGuid,
        ref Guid subgroupGuid,
        ref Guid settingGuid,
        uint valueIndex);

    [DllImport("powrprof.dll")]
    private static extern uint PowerWriteDCValueIndex(
        IntPtr rootPowerKey,
        ref Guid schemeGuid,
        ref Guid subgroupGuid,
        ref Guid settingGuid,
        uint valueIndex);

    [DllImport("powrprof.dll")]
    private static extern uint PowerSetActiveScheme(IntPtr userRootPowerKey, ref Guid schemeGuid);

    [DllImport("kernel32.dll")]
    private static extern IntPtr LocalFree(IntPtr memory);
}
