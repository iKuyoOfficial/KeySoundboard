[README.txt](https://github.com/user-attachments/files/34208192/README.txt)
音效键盘（KeySoundboard）
========================

一个简洁的 Windows 音效板：按下键盘上的按键，立即播放对应的音效。
窗口化、无边框或独占全屏的游戏在前台时也可以使用。


启动
----
双击 D:\KeySoundboard\KeySoundboard.exe 即可，无需安装。

系统要求：Windows 10 / 11，.NET Framework 4.8（系统自带）。
程序同一时间只运行一个实例；重复双击会唤出已经打开的窗口。
首次启动是空列表，点「＋ 添加音效」开始。


快速开始
--------
1. 点右上角「＋ 添加音效」，选择一个或多个音频文件。
   支持：wav、mp3、wma、m4a、aac、flac（实际解码能力取决于系统）。
2. 音频会复制到 D:\KeySoundboard\sounds，原文件之后可以移动或删除。
3. 点卡片右上角的「···」→「设置按键」，然后按下想用的键。
   只支持单个按键，不支持 Ctrl / Alt / Shift 组合键。
4. 按这个键就会播放；直接点卡片也可以播放。
5. 底部滑块调节总音量；「全局按键」开关控制是否响应键盘按键。

按钮数量没有限制，添加多个音效即可；卡片会自动换行，超出时可滚动。


卡片菜单
--------
播放
设置按键
更换音频…
重命名…
在文件夹中显示
删除

「删除」会把这个按钮从列表移除，并删除 sounds 里的音频副本；
如果还有别的按钮使用同一个文件，文件会保留。


托盘与退出
----------
点窗口右上角的 X，程序默认最小化到托盘，按键仍然有效。
要真正退出：右键任务栏通知区域（托盘）的小图标 →「退出」。
托盘菜单还可以「显示主界面」或「打开音效文件夹」。


在游戏中使用
------------
- 全局按键通过 Windows 低级键盘钩子实现，按键仍会继续传给游戏，
  不会“吃掉”游戏原本的按键。
- 本程序窗口处于前台时，热键会暂时不触发（避免设置键位时误响）；
  切换到游戏或其他窗口后立即恢复。
- 如果游戏是以管理员身份运行的，请右键 KeySoundboard.exe →
  「以管理员身份运行」；否则 Windows 会阻止普通权限程序接收它的按键。
- 部分带反作弊系统的游戏可能拦截全局按键，这是游戏侧的限制。
- 个别安全软件会对“全局键盘钩子”报警，这是实现游戏内热键所必需的；
  如果被拦截，请将本程序加入信任列表。
- 绑定游戏正在使用的键时，游戏也会同时收到该键；建议选不冲突的键。
- 关闭「全局按键」后，只有在软件窗口内点击卡片才会播放。


数据与备份
----------
config.xml          按钮、键位、音量等配置（UTF-8）
config.xml.bak      上一次保存的配置备份
sounds\             音效文件副本
debug.log           仅在使用 --debug 参数启动时生成

整个 D:\KeySoundboard 文件夹可以复制/移动到其他位置，路径会按新位置计算。
如果配置文件损坏，程序会自动备份为 config.xml.bad-日期时间.xml，
并使用默认配置启动。

需要排查问题时，可以这样启动并在 debug.log 查看日志：
KeySoundboard.exe --debug


常见问题
--------
按键没有反应：
  1. 确认底部「全局按键」已开启，状态显示“全局按键已启用”。
  2. 确认卡片左上角显示的是键名，而不是“未绑定”。
  3. 如果游戏是管理员权限，请以管理员身份运行本程序。
  4. 换一个按键试试；个别键盘/游戏会占用特殊按键。

没有声音：
  1. 检查 Windows 默认播放设备和系统音量。
  2. 检查底部总音量是否被调到 0。
  3. 卡片状态显示“无法播放”时，通常是格式/编码不被系统支持，
     换成 wav 或 mp3 再试。

想开机自动启动（可选）：
  按 Win+R，输入 shell:startup，把 KeySoundboard.exe 的快捷方式放进去。

卸载：
  退出程序，删除 D:\KeySoundboard 文件夹即可。


开发者
------
源码在 src\，构建脚本为 build.ps1。
使用 C# WinForms + WPF MediaPlayer，无第三方 DLL；
通过 .NET Framework 4.8 的 Roslyn csc 直接编译为单文件 exe。

KeySoundboard
==============

A simple Windows soundboard: press a key on your keyboard and the corresponding sound plays immediately.
It also works when a windowed, borderless, or exclusive fullscreen game is in the foreground.


Launch
------
Double-click D:\KeySoundboard\KeySoundboard.exe. No installation required.

System requirements: Windows 10 / 11, .NET Framework 4.8 (included with the system).
Only one instance of the program can run at a time; double-clicking again brings up the already-open window.
The list is empty on first launch. Click "+ Add Sound" to get started.


Quick Start
-----------
1. Click "+ Add Sound" in the top-right corner and select one or more audio files.
   Supported: wav, mp3, wma, m4a, aac, flac (actual decoding capability depends on the system).
2. The audio is copied to D:\KeySoundboard\sounds; the original files can be moved or deleted afterward.
3. Click "···" in the top-right corner of a card → "Set Key", then press the key you want to use.
   Only single keys are supported; Ctrl / Alt / Shift combinations are not supported.
4. Press that key to play; you can also click the card directly to play.
5. Use the slider at the bottom to adjust the master volume; the "Global Keys" toggle controls whether keyboard presses are responded to.

There is no limit on the number of buttons; just add more sounds. Cards wrap automatically and scroll when they overflow.


Card Menu
---------
Play
Set Key
Change Audio…
Rename…
Show in Folder
Delete

"Delete" removes this button from the list and deletes the audio copy in sounds;
if another button uses the same file, the file is kept.


Tray and Exit
-------------
Clicking the X in the top-right corner of the window minimizes the program to the tray by default, and key presses still work.
To actually exit: right-click the small icon in the taskbar notification area (tray) → "Exit".
The tray menu also lets you "Show Main Window" or "Open Sounds Folder".


Using in Games
--------------
- Global keys are implemented via a Windows low-level keyboard hook, and key presses are still passed on to the game;
  they do not "swallow" the game's original key presses.
- When this program's window is in the foreground, hotkeys are temporarily disabled (to avoid accidental sounds while setting key bindings);
  they resume immediately after switching to a game or another window.
- If the game is running as administrator, right-click KeySoundboard.exe →
  "Run as administrator"; otherwise Windows will prevent a normal-privilege program from receiving its key presses.
- Some games with anti-cheat systems may block global keys; this is a limitation on the game side.
- Some security software may flag "global keyboard hooks" as a warning; this is necessary to implement in-game hotkeys.
  If blocked, add this program to the trust list.
- When you bind a key that the game is using, the game will also receive that key; it is recommended to choose a key that does not conflict.
- After turning off "Global Keys", sounds will only play when you click cards inside the software window.


Data and Backup
---------------
config.xml          Configuration such as buttons, key bindings, and volume (UTF-8)
config.xml.bak      Backup of the last saved configuration
sounds\             Copies of sound files
debug.log           Generated only when launched with the --debug parameter

The entire D:\KeySoundboard folder can be copied/moved elsewhere; paths will be calculated based on the new location.
If the configuration file is corrupted, the program automatically backs it up as config.xml.bad-<date-time>.xml
and starts with the default configuration.

To troubleshoot problems, launch like this and check the log in debug.log:
KeySoundboard.exe --debug


FAQ
---
Key presses have no effect:
  1. Make sure "Global Keys" at the bottom is enabled and the status shows "Global keys enabled".
  2. Make sure the top-left corner of the card shows a key name rather than "Unbound".
  3. If the game runs with administrator privileges, run this program as administrator.
  4. Try a different key; some keyboards/games occupy special keys.

No sound:
  1. Check the Windows default playback device and system volume.
  2. Check whether the master volume at the bottom has been turned down to 0.
  3. When the card status shows "Unable to play", it is usually because the format/encoding is not supported by the system.
     Try a wav or mp3 instead.

To start automatically at boot (optional):
  Press Win+R, enter shell:startup, and place a shortcut to KeySoundboard.exe in the folder.

Uninstall:
  Exit the program and delete the D:\KeySoundboard folder.


Developer
---------
The source code is in src\; the build script is build.ps1.
Uses C# WinForms + WPF MediaPlayer, with no third-party DLLs;
compiled directly into a single-file exe via the Roslyn csc from .NET Framework 4.8.
