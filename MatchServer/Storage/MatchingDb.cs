using SharedProject.models;

namespace MatchServer.Storage;

public class MatchingDb
{
    public Dictionary<long, List<OnlineMatchingData>> MatchingDictionary;

    public object DbLock = new();

    static MatchingDb()
    {
    }

    private MatchingDb()
    {
        MatchingDictionary = new Dictionary<long, List<OnlineMatchingData>>();
    }

    public static MatchingDb GetInstance { get; } = new MatchingDb();

}