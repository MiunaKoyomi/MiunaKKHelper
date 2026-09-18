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

### Maker 角色卡评分与喜欢（1.2.5）

在 Maker 的「角色读取」列表外侧会出现收藏面板，无需打开 Alt+M。

- **未选中卡片**：同一位置变成筛选面板。`4 分` / `5 分` / `喜欢` 可多选；未勾选时显示全部，勾选后显示符合任一项的卡。原版分类筛选仍生效。
- **选中卡片**：面板回到打分/喜欢。

- **4 分**：静态蓝色金属圆角框；**5 分**：金属渐变、内金线、四角雕边，五层火线/辉光按真实周长匀速绕行，3 秒一圈。边框不显示星星、数字或品质徽标。
- 选中面板下方显示大图预览，与列表共用 CardFrameGraphic。透明中心、不拦截点击。只有可见金卡以 30Hz 更新绕行动画（卡之间错帧），路径点缓存，每条火线一层辉光+一层芯，避免网页那种多层 Screen 叠画。
- 辉光使用局部透明 UI 网格叠加，不依赖相机 Bloom；标准 UI alpha 混合近似 Web 的 Screen 效果。主框内缩以保留裁剪边界内的辉光，仍遵守原生列表 Mask。
- **喜欢**：独立的小爱心标记，可以只有爱心、不评分。
- **取消评分**保留喜欢；再次点击「已喜欢」只取消爱心。
- 卡面不放常驻操作按钮。未评分且未喜欢的卡不增加边框；列表关闭、切到服装卡或选中卡离开可见范围时，操作面板隐藏。
- 边框使用原列表的裁剪机制，不接收鼠标点击，只为可见卡片更新动画。

数据自动保存到 `BepInEx/config/org.miuna.plugins.KKHelper.card-preferences.xml`（KKS 为 `org.miuna.plugins.KKSHelper.card-preferences.xml`）。
PNG 不会被改写；每次替换保存保留 `.bak`。读取损坏文件时停止写入并提示日志，避免覆盖旧收藏。
标记使用 dataID → { Rating, Favorite } 字典，XML v2 以 dataId 保存。直接链接共享导出器的 CardPngIdentity 源码：完整原卡文件 MD5 的十六进制文本格式化为 GUID（不是 new Guid(md5Bytes) 的字节顺序）。
相同字节的副本、改名和移动共享标记；修改或重新保存导致文件字节改变会产生新身份。路径只用于读取文件和以文件长度/修改时间缓存 ID，可见卡片每 2 秒检查失效，不逐帧哈希。
自动迁移 v1 路径记录；相同文件合并取最高评分和喜欢的并集；找不到/无法读取的旧路径保留待迁移。保存使用原子替换及 .bak，不改写 PNG。

KK 与 KKS 均提供编译版本；本次在 KK Maker 实机验证。KK Party 的角色列表 API 不同，目前不保证兼容。

存储回归检查：用 MSBuild 构建 `Tests/CardPreferences/CardPreferences.Tests.csproj` 后运行 `Tests/CardPreferences/bin/MiunaCardPreferenceTests.exe`。
同一测试程序集也已通过游戏 MCP 在 KK 的实际 Mono 中执行，13 项检查通过。

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
