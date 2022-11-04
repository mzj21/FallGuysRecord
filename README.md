# FallGuysRecord
  读取本地log数据，展示各种信息
## 说明
  没有时间戳后最精确的糖豆人计时器，流畅不卡顿，详细的信息可用于自用、直播、比赛等形式。\
  浮窗的形式展现（胜场，PING值，回合名称、类型，计时器等）。\
  回合信息窗口（鼠标右键浮窗开启，实时玩家剩余数，各平台玩家数量，具体排名，服务器IP等）。\
（糖豆人新赛季更新导致计时器出现了问题，FallGuysStats没有修复计时器的问题，我就着手汉化了shinq的FallGuysRecord，但是用起来很不方便，Java的界面实在是难言。后来我还用Java+JavaFX写了一个，但是打包出现了问题，和汉化的FallGuysRecord一样需要Java环境文件。在这之后我就新写了这个WPF项目。）
## 下载
  - 更新 [FallGuysRecord.exe](https://raw.githubusercontent.com/mzj21/FallGuysRecord/main/FallGuysRecord.exe)
  - 单独资源包 [FallGuysRecord资源包(不包含exe).7z](https://raw.githubusercontent.com/mzj21/FallGuysRecord/main/FallGuysRecord资源包(不包含exe).7z)<br>
## 使用
解压即用，右键浮窗有更多设置。
## 截图和说明
1.1.1版本演示视频https://www.bilibili.com/video/BV14V4y137yS/
![中文](https://github.com/mzj21/FallGuysRecord/blob/main/images/zh.png)
![English](https://github.com/mzj21/FallGuysRecord/blob/main/images/en.png)
![改变](https://github.com/mzj21/FallGuysRecord/blob/main/images/change.png)
## 更新日志
 - 1.1.4
    修复获胜时浮窗显示问题
 - 1.1.3 
    添加第一名ID显示开关（默认关闭，设置中的isShowFastestName值为true开启）
    添加实时剩余玩家数和电脑玩家数量（如果有），回合信息框添加输出平台玩家信息输出
    修复程序在游戏前开启无效的问题
    修复直接退出会加胜场的问题
 - 1.1.2
    修改文字错误，对齐回合输出，回合窗口颜色可以跟随设置
    使用异步更新UI，解决界面卡顿问题
 - 1.1.1
    修复进程依旧存在的BUG
 - 1.1.0
    添加新的回合信息窗口（右键浮窗开启关闭）
    添加了相关设置，优化了设置判断
 - 1.0.4
    修复窗口可能消失不见的问题
    提高读取精度
 - 1.0.3
    降低计时器误差
 - 1.0.2
    修复CPU占用率高的问题。
    优化代码，清理无用代码和资源。
 - 1.0.1
    修复设置字体同步问题