namespace WatDiv.Config.PropConfig;

internal sealed class PropConfig : IPropConfig
{
    public string Follows => "follows";

    public string FriendOf => "friendOf";

    public string Likes => "likes";

    public string HasReview => "hasReview";

    public string[] GetPropKeys() => new[] { Follows, FriendOf, Likes, HasReview };
}