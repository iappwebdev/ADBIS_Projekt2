using System.Diagnostics;
using WatDiv.Models;
using WatDiv.WatDivProcessing.Context;

namespace WatDiv.WatDivProcessing.Joins;

internal sealed class WatDivSortMergeJoin
{
    private readonly WatDivContext _watDivContext;
    private readonly StreamWriter _logFile;
    private readonly string? _resultFileDir;
    private readonly string _resultFileExt;
    private readonly string _resultFileName;
    private int _resultFilesCounter;
    private readonly Stopwatch _stopwatch = new();

    public WatDivSortMergeJoin(WatDivContext watDivContext, StreamWriter logFile, string resultFilePath)
    {
        _watDivContext = watDivContext;
        _logFile = logFile;
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
        _resultFileName = timestamp + "_" + Path.GetFileNameWithoutExtension(resultFilePath);
        _resultFileDir = Path.Combine(Path.GetDirectoryName(resultFilePath)!, _resultFileName);
        _resultFileExt = Path.GetExtension(resultFilePath);
        Directory.CreateDirectory(_resultFileDir);
    }

    public void GetSortMergeJoin()
    {
        var joinLikesReviews = GetJoinOverLikesReviews();
        var joinFriendsLikesReviews = GetJoinOverFriendsLikesReviews(joinLikesReviews);
        GetJoinOverFollowsFriendLikesReviews(joinFriendsLikesReviews);
    }

    private List<Tuple<Relation, string>> GetJoinOverLikesReviews()
    {
        long counter = 0;
        var result = new List<Tuple<Relation, string>>();
        int idxLikes = 0;
        int idxReviews = 0;

        long loopCounter = 0;
        int prevGroupStartIdx = 0;
        long prevHashInner = -1;

        var likesList =_watDivContext.Likes.OrderBy(x => x.ObjectHash.Value).ToList();
        var hasReviewList = _watDivContext.HasReview.OrderBy(x => x.SubjectHash.Value).ToList();
        while (idxLikes < likesList.Count && idxReviews < hasReviewList.Count)
        {
            loopCounter++;
            Relation like = likesList[idxLikes];
            Relation review = hasReviewList[idxReviews];

            if (like.ObjectHash.Value < review.SubjectHash.Value)
            {
                idxLikes++;
                idxReviews = prevGroupStartIdx;
                continue;
            }

            if (like.ObjectHash.Value > review.SubjectHash.Value)
            {
                idxReviews++;
                continue;
            }

            result.Add(new Tuple<Relation, string>(like, review.Object));
            counter++;

            if (review.SubjectHash.Value > prevHashInner)
            {
                prevGroupStartIdx = idxReviews;
                prevHashInner = review.SubjectHash.Value;
            }

            idxReviews++;
        }

        _logFile.WriteLine($"{counter:#,#} likeReviews at {DateTime.Now.ToLongTimeString()}, {loopCounter:#,#} Iterations");
        result = result.OrderBy(x => x.Item1.SubjectHash.Value).ToList();
        return result;
    }

    private List<Tuple<Relation, string, string>> GetJoinOverFriendsLikesReviews(List<Tuple<Relation, string>> joinLikesReviews)
    {
        long counter = 0;
        var result = new List<Tuple<Relation, string, string>>();
        int idxLikeReviews = 0;
        int idxFriendOf = 0;

        long loopCounter = 0;
        int prevGroupStartIdx = 0;
        long prevHashInner = -1;

        var friendOfList = _watDivContext.FriendOf.OrderBy(x => x.ObjectHash.Value).ToList();
        while (idxLikeReviews < joinLikesReviews.Count && idxFriendOf < friendOfList.Count)
        {
            loopCounter++;
            (Relation like, string item2) = joinLikesReviews[idxLikeReviews];
            Relation friendOf = friendOfList[idxFriendOf];

            if (like.SubjectHash.Value < friendOf.ObjectHash.Value)
            {
                idxLikeReviews++;
                idxFriendOf = prevGroupStartIdx;
                continue;
            }

            if (like.SubjectHash.Value > friendOf.ObjectHash.Value)
            {
                idxFriendOf++;
                continue;
            }

            result.Add(new Tuple<Relation, string, string>(friendOf, like.Object, item2));
            counter++;

            if (friendOf.ObjectHash.Value > prevHashInner)
            {
                prevGroupStartIdx = idxFriendOf;
                prevHashInner = friendOf.ObjectHash.Value;
            }

            idxFriendOf++;
        }

        _logFile.WriteLine($"{counter:#,#} joinFriendsLikesReviews at {DateTime.Now.ToLongTimeString()}, {loopCounter:#,#} Iterations");
        result = result.OrderBy(x => x.Item1.SubjectHash.Value).ToList();
        joinLikesReviews = null;
        return result;
    }

    private void GetJoinOverFollowsFriendLikesReviews(List<Tuple<Relation, string, string>> joinFriendsLikesReviews)
    {
        long counter = 0;
        var result = new List<string>();
        int idxFriendsLikesReviews = 0;
        int idxFollows = 0;

        long loopCounter = 0;
        int prevGroupStartIdx = 0;
        long prevHashInner = -1;

        var followList = _watDivContext.Follows.OrderBy(x => x.ObjectHash.Value).ToList();
        while (idxFriendsLikesReviews < joinFriendsLikesReviews.Count && idxFollows < followList.Count)
        {
            loopCounter++;
            (Relation friendOf, string item2, string item3) = joinFriendsLikesReviews[idxFriendsLikesReviews];
            Relation follow = followList[idxFollows];

            if (follow.ObjectHash.Value < friendOf.SubjectHash.Value)
            {
                idxFollows++;
                idxFriendsLikesReviews = prevGroupStartIdx;
                continue;
            }

            if (follow.ObjectHash.Value > friendOf.SubjectHash.Value)
            {
                idxFriendsLikesReviews++;
                continue;
            }

            result.Add($"{follow.Subject} follows {follow.Object} friendOf {friendOf.Object} likes {item2} hasReview {item3}");
            counter++;

            if (friendOf.SubjectHash.Value > prevHashInner)
            {
                prevGroupStartIdx = idxFriendsLikesReviews;
                prevHashInner = friendOf.SubjectHash.Value;
            }

            idxFriendsLikesReviews++;

            if (counter % 10000000 != 0) continue;

            WriteIntermediateResult(result, counter);
            result = null;
            result = new List<string>();
        }

        if (result.Count > 0)
        {
            WriteIntermediateResult(result, counter);
        }

        joinFriendsLikesReviews = null;
        _logFile.WriteLine($"{counter:#,#} final entries at {DateTime.Now.ToLongTimeString()}, {loopCounter:#,#} Iterations");
    }

    private void WriteIntermediateResult(IEnumerable<string> result, long counter)
    {
        string resultFilePath = Path.Combine(_resultFileDir!, $"{_resultFileName}_{_resultFilesCounter++:D5}{_resultFileExt}");
        _stopwatch.Restart();
        File.WriteAllLines(resultFilePath, result);
        _stopwatch.Stop();
        _logFile.WriteLine($"{counter:#,#} entries, write intermediate result to {Path.GetFileName(resultFilePath)} in {_stopwatch.Elapsed} ");
    }
}