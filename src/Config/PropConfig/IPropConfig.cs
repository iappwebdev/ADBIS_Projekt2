namespace WatDiv.Config.PropConfig;

internal interface IPropConfig
{
    string Follows { get; }

    string FriendOf { get; }

    string Likes { get; }

    string HasReview { get; }

    string[] GetPropKeys();
}