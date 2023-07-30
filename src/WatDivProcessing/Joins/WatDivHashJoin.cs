using System.Collections;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Text;
using WatDiv.Models;
using WatDiv.WatDivProcessing.Context;

namespace WatDiv.WatDivProcessing.Joins;

internal sealed class WatDivHashJoin
{
    private readonly WatDivContext _context;
    private readonly StreamWriter _logFile;
    private readonly string? _resultFileDir;
    private readonly string _resultFileExt;
    private readonly string _resultFileName;
    private int _resultFilesCounter;
    private readonly Stopwatch _stopwatch = new();

    public WatDivHashJoin(WatDivContext context, StreamWriter logFile, string resultFilePath)
    {
        _context = context;
        _logFile = logFile;
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
        _resultFileName = timestamp + "_" +  Path.GetFileNameWithoutExtension(resultFilePath);
        _resultFileDir = Path.Combine(Path.GetDirectoryName(resultFilePath)!, _resultFileName);
        _resultFileExt = Path.GetExtension(resultFilePath);
        Directory.CreateDirectory(_resultFileDir);
    }

    /// <summary>
    /// Join over like, review, friend and follow. Solution für required hash join for Ex. 2b (optimization by selecting smallest tables first).
    /// Implemented by myself.
    /// </summary>
    /// <returns></returns>
    public void GetHashJoinOptimized()
    {
        var joinLikesReviews = GetJoinOverLikesReviews();
        var joinFriendsLikesReviews = GetJoinOverFriendsLikesReviews(joinLikesReviews);
        GetJoinOverFollowsFriendLikesReviews(joinFriendsLikesReviews);
    }

    private List<Tuple<Relation, string>> GetJoinOverLikesReviews()
    {
        long counter = 0;
        var result = new List<Tuple<Relation, string>>();
        var likesObj = _context.Likes.GroupBy(x => x.ObjectHash)
            .Select(x => new KeyValuePair<Hash, IEnumerable>(x.Key, x))
            .ToFrozenDictionary();

        foreach (Relation review in _context.HasReview)
        {
            if (!likesObj.ContainsKey(review.SubjectHash)) continue;

            foreach (Relation item in likesObj[review.SubjectHash])
            {
                counter++;
                result.Add(
                    new Tuple<Relation, string>(
                        item,
                        review.Object
                    )
                );
            }
        }

        _logFile.WriteLine($"{counter:#,#} likeReviews at {DateTime.Now.ToLongTimeString()}");
        return result;
    }

    private List<Tuple<Relation, string, string>> GetJoinOverFriendsLikesReviews(IEnumerable<Tuple<Relation, string>> likesReviews)
    {
        long counter = 0;
        var result = new List<Tuple<Relation, string, string>>();
        var likesSubj = likesReviews.GroupBy(x => x.Item1.SubjectHash)
            .Select(x => new KeyValuePair<Hash, IEnumerable<Tuple<Relation, string>>>(x.Key, x))
            .ToFrozenDictionary();


        foreach (Relation friend in _context.FriendOf)
        {
            if (!likesSubj.ContainsKey(friend.ObjectHash)) continue;

            foreach (var item in likesSubj[friend.ObjectHash])
            {
                counter++;
                result.Add(
                    new Tuple<Relation, string, string>(
                        friend,
                        item.Item1.Object,
                        item.Item2
                    )
                );
            }
        }

        _logFile.WriteLine($"{counter:#,#} joinFriendsLikesReviews at {DateTime.Now.ToLongTimeString()}");
        return result;
    }

    private void GetJoinOverFollowsFriendLikesReviews(IEnumerable<Tuple<Relation, string, string>> friendsLikesReviews)
    {
        long counter = 0;
        var result = new List<string>();
        var followedSubj = friendsLikesReviews.GroupBy(x => x.Item1.SubjectHash)
            .Select(x => new KeyValuePair<Hash, IEnumerable<Tuple<Relation, string, string>>>(x.Key, x))
            .ToFrozenDictionary();

        foreach (Relation follow in _context.Follows)
        {
            if (!followedSubj.ContainsKey(follow.ObjectHash)) continue;

            foreach (var item in followedSubj[follow.ObjectHash])
            {
                counter++;
                result.Add($"{follow.Subject} follows {follow.Object} friendOf {item.Item1.Object} likes {item.Item2} hasReview {item.Item3}");
                if (counter % 10000000 != 0) continue;

                WriteIntermediateResult(result, counter);
                result = null;
                result = new List<string>();
            }
        }

        if (result.Count > 0)
        {
            WriteIntermediateResult(result, counter);
        }

        _logFile.WriteLine($"{counter:#,#} final entries at {DateTime.Now.ToLongTimeString()}");
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