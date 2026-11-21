# KK 通用头模筛选（2026-10-21）

独立 Miuna 辅助插件，头部选择窗口搜索栏下方增加「仅显示通用头模」开关。
不更换角色头模、不修改卡片/zipmod、不删除头模。默认关闭；本次当前运行会话已开启。

## 名单

按 Category 100、Sideloader GUID + 原始 Slot 精确匹配，不用启动后可能变化的 LocalSlot，也不依赖翻译或缩略图。
原版 0/200/201 + 代码 `Known` 中 29 个通用改版，目前本地共 32/321 项。
包括 miniH、i、通用鼻嘴脸型、轮廓和牙齿改版。命名为角色原型的头模不纳入。
这是经过整理的通用候选名单，不保证所有未来安装的通用头模自动被识别，也不保证每个候选都有相同的 UV、表情或 PBR 效果。

## 构建与安装

运行 `build.ps1`，直接引用本机 KK 的 Managed/BepInEx 程序集构建，目标为游戏实际 Mono API。
只安装 `bin/MiunaGenericHeadFilter.dll` 到 `BepInEx/plugins/MiunaPlugin/`。
依赖 KK_MakerSearch 1.5.1、Sideloader；本次验证含 MakerOptimizations 虚拟列表。
不替换这些第三方 DLL。版本 1.0.1，内部程序集名 V101 用来避开当前 Mono 对首次热载程序集的缓存。

## 实现与验证

保留完整原生列表，只叠加 disvisible；运行原有搜索/分类更新前恢复基线，嵌套调用结束后重新应用。
恢复筛选前保留其他工具的隐藏状态。虚拟列表通过 UpdateStateNew 标脏，避免逐帧扫描场景/重建缩略图。
搜索插件原本用调用栈判断临时隐藏，Harmony 包装会影响这一判断，因此显式保存/恢复搜索调用的 disvisibleMemory。

已通过实际 KK Maker 的相同编译 DLL 验证：
- 321 全部 / 32 通用；Modified 搜索交集 4 项；清空恢复 32。
- RememberDisvisibles 后关闭仍恢复 321；连续五轮开关通过。
- 实际 Button.onClick 回调通过；其他来源隐藏原版条目时关闭筛选仍保留该隐藏。
- 青雀 headId 始终为 100031149（本次会话解析 ID），没有调用换头或保存角色。
- 最终日志游标 1069–1103 无新增 Error；早前会话有其他插件错误，未清空。
- 通过 MCP 热载启用，持久 DLL 已安装且 SHA256 一致；未重启游戏验证冷启动，未声明 KKS 兼容。

安装文件 SHA256：`A99493261E06233EED3232F72C50F72D2AB38CF9097F6775EBC25A4CFEE3F1B1`。
