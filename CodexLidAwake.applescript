property pidFile : "/var/run/local.codex.lidawake.pid"
property logFile : "/var/log/local.codex.lidawake.log"

on run
	set sleepDisabled to do shell script "/usr/bin/pmset -g | /usr/bin/awk '$1 == \"SleepDisabled\" {print $2}'"

	if sleepDisabled is "1" then
		set resultDialog to display dialog "合盖联网当前已开启。恢复后，MacBook 合盖会重新进入正常睡眠。" with title "Codex 合盖联网" buttons {"保持开启", "恢复正常睡眠"} default button "恢复正常睡眠" cancel button "保持开启" with icon caution
		if button returned of resultDialog is "恢复正常睡眠" then
			my disableLidAwake()
		end if
		return
	end if

	set durationChoice to choose from list {"1 小时", "2 小时", "4 小时", "8 小时"} with title "Codex 合盖联网" with prompt "选择合盖后继续联网和运行 Codex 的时长：" default items {"2 小时"} OK button name "下一步" cancel button name "取消"
	if durationChoice is false then return

	set chosenDuration to item 1 of durationChoice
	if chosenDuration is "1 小时" then
		set durationSeconds to 3600
	else if chosenDuration is "2 小时" then
		set durationSeconds to 7200
	else if chosenDuration is "4 小时" then
		set durationSeconds to 14400
	else
		set durationSeconds to 28800
	end if

	set warningDialog to display dialog "开启后，即使合上盖子，Mac 也不会睡眠，手机热点和 Codex 任务可以继续运行。\n\n请保持通风，最好接通电源；绝对不要把仍在运行的 Mac 放进包里。" with title "确认开启 " & chosenDuration buttons {"取消", "开启"} default button "开启" cancel button "取消" with icon caution
	if button returned of warningDialog is not "开启" then return

	set timerScript to "/bin/sleep " & durationSeconds & "; /usr/bin/pmset -a disablesleep 0; /bin/rm -f " & quoted form of pidFile
	set rootScript to "if [ -r " & quoted form of pidFile & " ]; then oldpid=$(/bin/cat " & quoted form of pidFile & "); case \"$oldpid\" in ''|*[!0-9]*) ;; *) /bin/kill \"$oldpid\" 2>/dev/null || true ;; esac; fi; /usr/bin/pmset -a disablesleep 1; /usr/bin/nohup /bin/sh -c " & quoted form of timerScript & " >>" & quoted form of logFile & " 2>&1 & timerpid=$!; /bin/echo \"$timerpid\" >" & quoted form of pidFile

	try
		do shell script rootScript with administrator privileges
		display dialog "已开启 " & chosenDuration & "。\n\n现在可以合盖，Mac 会继续联网并运行 Codex；到时会自动恢复正常睡眠。若提前结束，再打开本工具即可关闭。" with title "Codex 合盖联网" buttons {"好"} default button "好" with icon note
	on error errorMessage
		display dialog "未能开启：\n" & errorMessage with title "Codex 合盖联网" buttons {"好"} default button "好" with icon stop
	end try
end run

on disableLidAwake()
	set rootScript to "/usr/bin/pmset -a disablesleep 0; if [ -r " & quoted form of pidFile & " ]; then timerpid=$(/bin/cat " & quoted form of pidFile & "); case \"$timerpid\" in ''|*[!0-9]*) ;; *) /bin/kill \"$timerpid\" 2>/dev/null || true ;; esac; fi; /bin/rm -f " & quoted form of pidFile
	try
		do shell script rootScript with administrator privileges
		display dialog "已恢复正常睡眠：MacBook 合盖后会睡眠并断开热点。" with title "Codex 合盖联网" buttons {"好"} default button "好" with icon note
	on error errorMessage
		display dialog "未能恢复正常睡眠：\n" & errorMessage with title "Codex 合盖联网" buttons {"好"} default button "好" with icon stop
	end try
end disableLidAwake
