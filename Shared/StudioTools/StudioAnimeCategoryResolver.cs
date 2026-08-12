using Studio;

namespace MiunaKKHelper.StudioTools
{
    /// <summary>
    /// 将 Studio 动作 group/category 索引解析为 CharaStudio UI 上的分组名（如 Character/Basic）。
    /// </summary>
    internal static class StudioAnimeCategoryResolver
    {
        public static void ResolveStudio(
            Info info,
            int group,
            int category,
            string displayName,
            out string groupName,
            out string categoryName,
            out string studioPath)
        {
            groupName = ResolveGroupName(info, group);
            categoryName = ResolveCategoryName(info, group, category);
            studioPath = BuildPath(groupName, categoryName, displayName);
        }

        public static void ResolveHList(
            string mode,
            string displayName,
            out string groupName,
            out string categoryName,
            out string studioPath)
        {
            groupName = "H";
            categoryName = string.IsNullOrEmpty(mode) ? "Unknown" : mode;
            studioPath = BuildPath(groupName, categoryName, displayName);
        }

        static string ResolveGroupName(Info info, int group)
        {
            if (info?.dicAGroupCategory != null
                && info.dicAGroupCategory.TryGetValue(group, out Info.GroupInfo groupInfo)
                && !string.IsNullOrEmpty(groupInfo.name))
            {
                return groupInfo.name;
            }

            return "Group_" + group;
        }

        static string ResolveCategoryName(Info info, int group, int category)
        {
            if (info?.dicAGroupCategory != null
                && info.dicAGroupCategory.TryGetValue(group, out Info.GroupInfo groupInfo)
                && groupInfo.dicCategory != null
                && groupInfo.dicCategory.TryGetValue(category, out string categoryName)
                && !string.IsNullOrEmpty(categoryName))
            {
                return categoryName;
            }

            return "Category_" + category;
        }

        static string BuildPath(string groupName, string categoryName, string displayName)
        {
            if (string.IsNullOrEmpty(groupName))
                groupName = "Unknown";

            if (string.IsNullOrEmpty(categoryName))
                return string.IsNullOrEmpty(displayName) ? groupName : groupName + "/" + displayName;

            if (string.IsNullOrEmpty(displayName))
                return groupName + "/" + categoryName;

            return groupName + "/" + categoryName + "/" + displayName;
        }
    }
}
