# MiunaKKHelper

Miuna 的个人 **Koikatsu / Koikatsu Sunshine** 插件功能集合（BepInEx）。

以前仓库叫 `KK-API-Doc`，程序集叫 `KK_API_DOC.dll`，容易让人以为这是 API 文档项目。  
实际用途是：把日常用的 Studio/Maker 小工具收在一个插件里，`Alt+M` 打开面板。

工程结构和 [KKUnityExporter](https://github.com/MiunaKoyomi/KKUnityExporter) 一样：公共代码在 Shared，KK / KKS 各一个入口工程。

| | KK | KKS |
|--|--|--|
| 插件名 | `MiunaKKHelper` | `MiunaKKSHelper` |
| GUID | `org.miuna.plugins.KKHelper` | `org.miuna.plugins.KKSHelper` |
| 程序集 | `MiunaKKHelper.dll` | `MiunaKKSHelper.dll` |
| 目标框架 | net35 + KKAPI | net46 + KKSAPI |

仓库：https://github.com/MiunaKoyomi/MiunaKKHelper

## 依赖

- Koikatsu 或 Koikatsu Sunshine（CharaStudio）+ BepInEx 5
- [KKAPI / KKSAPI](https://github.com/IllusionMods/IllusionModdingAPI)（当前工程引用 1.46.1）

## 安装

1. 编译本仓库（Rider / VS）
2. 把对应 dll 复制到游戏插件目录：

   - KK：`Koikatu/BepInEx/plugins/MiunaPlugin/MiunaKKHelper.dll`
   - KKS：`KoikatsuSunshine/BepInEx/plugins/MiunaPlugin/MiunaKKSHelper.dll`

3. **删掉旧的** `KK_API_DOC.dll`，否则会和本插件抢同一个 GUID
4. 启动 CharaStudio 或角色制作，快捷键 **LeftAlt + M** 打开面板

BepInEx 配置节：KK 为 `Miuna_KKHelper`，KKS 为 `Miuna_KKSHelper`（热键可改）。

## 功能

面板只在 **Studio** / **Maker** 下可用。

### 通用

| 功能 | 说明 |
|------|------|
| Clear Log | 清空 BepInEx 控制台缓冲区 |
| Enable/Disable DynamicBone | 对当前选中对象子树开关 `DynamicBone` |
| Enable/Disable DynamicBoneVer02 | 对当前选中对象子树开关 `DynamicBone_Ver02` |
| ShowDynamicBone | 把选中对象上的 DB / DB02 打到日志 |

### Studio

| 功能 | 说明 |
|------|------|
| 提取选中角色 | 把 Workspace 选中的角色存成 png 卡，写入 `UserData/chara/male` 或 `female` |
| 提取场景全部角色 | 导出当前场景 `dicInfo` 里全部 `OCIChar` |
| 打开导出目录 | 打开上次写出卡的文件夹 |
| Export Animation Catalog | 导出 Studio 动作 displayName / AB 映射到 `UserData/MiunaExports/AnimationCatalog.json` |
| 覆盖环境光 (Ambient) | 原版只有全体陰影；开启后可调 Flat / Trilight / Skybox 与强度，并写入场景存档 |

角色卡文件名形如：`StudioExtract_{角色名}_{时间}_{序号}.png`。  
Studio 运行时卡面 `pngData` 经常为空，缩略图可能空白，角色数据本身完整。

环境光覆盖通过 KKAPI `StudioSaveLoadApi` 写进场景扩展数据（key 绑在对应游戏的 GUID 上）。

## 工程结构

```
Shared/                               公共代码（.shproj + .projitems）
  MiunaHelperHost.cs                  Logger / 热键 / 版本注入
  MiunaKKAPIUI.cs                     IMGUI 面板
  StudioTools/                        Studio 工具
KK/                                   net35 + KKAPI 入口 → MiunaKKHelper.dll
KKS/                                  net46 + KKSAPI 入口 → MiunaKKSHelper.dll
OtherPlugin/                          参考反编译（不编译）
```

## 从 KK-API-Doc 迁移

| 旧 | 新 |
|----|----|
| 仓库 `Nagi-no-Asukara/KK-API-Doc` | `MiunaKoyomi/MiunaKKHelper` |
| `KK_API_DOC.dll` | `MiunaKKHelper.dll` / `MiunaKKSHelper.dll` |
| 命名空间 `KK_API_DOC` | `MiunaKKHelper` |
| KK GUID `org.miuna.plugins.KKHelper` | 不变 |
