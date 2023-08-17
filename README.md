# FallGuysRecord
  读取本地log数据，展示各种信息
## 说明
  **SS3时间戳回来了。**\
  没有时间戳后最精确的糖豆人计时器，流畅不卡顿，详细的信息可用于自用、直播、比赛等形式。\
  浮窗的形式展现（胜场，PING值，回合名称、类型，计时器等）。\
  回合信息窗口（设置中快捷键开启，实时玩家剩余数，各平台玩家数量，具体排名，服务器IP等）。\
（糖豆人新赛季更新导致计时器出现了问题，FallGuysStats没有修复计时器的问题，我就着手汉化了shinq的FallGuysRecord，但是用起来很不方便，Java的界面实在是难言。后来我还用Java+JavaFX写了一个，但是打包出现了问题，和汉化的FallGuysRecord一样需要Java环境文件。在这之后我就新写了这个WPF项目。）
## 下载
  - 更新 [FallGuysRecord.exe](https://raw.githubusercontent.com/mzj21/FallGuysRecord/main/FallGuysRecord.exe)
  - 单独图片资源包 [糖豆人资源包 SS4 2023-08-16.7z](https://raw.githubusercontent.com/mzj21/FallGuysRecord/main/糖豆人资源包%20SS4%202023-08-16.7z)
## 使用
解压即用，右键浮窗设置。**默认F7为浮窗显示快捷键，F8为回合信息显示快捷键。**
## 截图和说明
1.1.1版本演示视频https://www.bilibili.com/video/BV14V4y137yS/
![简易模式](https://github.com/mzj21/FallGuysRecord/blob/main/images/1.png)
![正常模式](https://github.com/mzj21/FallGuysRecord/blob/main/images/2.png)
![回合信息](https://github.com/mzj21/FallGuysRecord/blob/main/images/3.png)
![设置](https://github.com/mzj21/FallGuysRecord/blob/main/images/4.png)
## 功能预告
  - 与FallGuysStats类似的数据库界面，图形暂未设计。（版本1.4.+）
## 更新日志
 - 1.3.15
 >- 添加新的回合信息
 - 1.3.14
 >- 添加新的回合信息
 >- 更新资源包
 - 1.3.13
 >- 修复回合显示问题
 - 1.3.12
 >- 添加新的回合信息
 >- 更新资源包
 - 1.3.11
 >- 添加新的回合信息
 - 1.3.10
 >- 回合信息框中同时添加了官方的翻译和分享代码
 - 1.3.9
 >- 添加官方自定义地图数据，部分关卡没有官方翻译
 >- 修正窗口置顶
 - 1.3.8
 >- 修复场次计数错误
 >- 更新第四赛季(SS4)资源包
 >- 修复设置没有正确保存的情况
 - 1.3.7
>- 修复关卡名问题
>- 修复自定义模式
>- 在信息框中将玩家ID改为Player+数字，并为自己做出标识
>- 在信息框中会将分享代码显示出来
 - 1.3.6
>- 修复第四赛季 奇想成真(自定义关卡暂时有BUG，MT没有添加关卡名称)
>- 更新语言文件
 - 1.3.5
>- 修复获胜判断
 - 1.3.4
>- 小队分数更快的显示
 - 1.3.3
>- 修复皇冠错误
 - 1.3.2
>- 所有数据都未存储，需要1.4.x版本添加数据库后支持
>- 第一名时间修改为皇冠和碎片
>- 第一名昵称修改为PB（暂时未做，1.4.x制作）
>- 回合信息中增加自己小队分数详情
>- 修复小队连胜时错误
 - 1.3.1
>- 修复奖励结算后，不再统计的问题
 - 1.3.0
>- 新增设置界面（浮窗右键进入，支持多语言实时切换，热键支持单一组合键）
>- 新增连胜，位于浮窗第一栏中
>- 新增log路径设置，一般情况不用设置，复原请清空
>- 修改回合信息中的专题为多语言
 - 1.2.0
>- 时间戳回来了，计时精确，适配时间戳形式。
>- 修复log读取指针问题
>- 重新修改了获胜判断和比赛判断，使其更加准确。（适配游戏忽然崩溃，网络断线等极端情况）
>- 优化置顶逻辑。
>- 修复切换语言显示不正确的问题。
>- 内部增加结算解析，为以后版本预留。
 - 1.1.17
>- 修复比赛轮次不正确的BUG
 - 1.1.16
>- 修复计时器无法正确运行的BUG
>- 更新资源包和语言包
 - 1.1.15
>- 完善对多屏幕的支持
 - 1.1.14
>- 修复部分情况不算场次的问题
>- 修复部分情况回合信息不输出所有平台信息的问题
>- 修正置顶
>- 修改程序关闭逻辑
 - 1.1.13
>- 添加对多屏幕的支持
>- 修正切换语言
 - 1.1.12
>- 修复单人获胜判定
 - 1.1.11
>- 优化计时器判断，提升计时精度
>- 修正获胜判断（个人是获取昵称出问题了，组队是没有适配）
>- 修复字体为白色是回合信息字体为黑色（以前版本背景不可修改，白色会与背景一致，导致看不见）
>- 改进置顶
>- 更新一波图片资源（使用无损放大2倍）
 - 1.1.10
>- 添加回合信息背景修改（资源包已更新）
>- 修改图片填充方式
 - 1.1.9
>- 添加浮窗和回合信息显示的快捷键（默认F7和F8，Setting.json中修改，不支持组合键）
>- 正确显示总计时
 - 1.1.8
>- 添加浮窗简易模式（只有计时器和自己的时间）
>- 更加精确的计时
 - 1.1.7
>- 添加浮窗自己的排名
>- 修改回合信息操作为（全选，复制，保存）
>- 修复某些置顶错误的情况
 - 1.1.6
>- 添加实时剩余人数到浮窗
>- 修复实时剩余人数错误的问题
 - 1.1.5
>- 修复获胜判断
>- 计时器显示后3位小数
 - 1.1.4
>- 修复获胜时浮窗显示问题
 - 1.1.3
>- 添加第一名ID显示开关（默认关闭，设置中的isShowFastestName值为true开启）
>- 添加实时剩余玩家数和电脑玩家数量（如果有），回合信息框添加输出平台玩家信息输出
>- 修复程序在游戏前开启无效的问题
>- 修复直接退出会加胜场的问题
 - 1.1.2
>- 修改文字错误，对齐回合输出，回合窗口颜色可以跟随设置
>- 使用异步更新UI，解决界面卡顿问题
 - 1.1.1
>- 修复进程依旧存在的BUG
 - 1.1.0
>- 添加新的回合信息窗口（右键浮窗开启关闭）
>- 添加了相关设置，优化了设置判断
 - 1.0.4
>- 修复窗口可能消失不见的问题
>- 提高读取精度
 - 1.0.3
>- 降低计时器误差
 - 1.0.2
>- 修复CPU占用率高的问题。
>- 优化代码，清理无用代码和资源。
 - 1.0.1
>- 修复设置字体同步问题