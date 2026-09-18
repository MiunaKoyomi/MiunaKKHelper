namespace MiunaKKHelper.CardFavorites;

/// <summary>Maker 卡表筛选：未勾选时不过滤；勾选后显示符合任一项的卡。</summary>
public static class CardPreferenceFilter
{
    public static bool Active(bool rating4, bool rating5, bool favorite) =>
        rating4 || rating5 || favorite;

    public static bool Matches(CardPreference value, bool rating4, bool rating5, bool favorite)
    {
        if (!Active(rating4, rating5, favorite)) return true;
        return (rating4 && value.Rating == 4)
            || (rating5 && value.Rating == 5)
            || (favorite && value.Favorite);
    }
}
