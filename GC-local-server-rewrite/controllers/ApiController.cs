using ChoETL;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using GCLocalServerRewrite.common;
using GCLocalServerRewrite.models;
using SharedProject.enums;
using SharedProject.models;
using SQLite.Net2;
using Swan.Logging;

namespace GCLocalServerRewrite.controllers;

public class ApiController : WebApiController
{
    private readonly SQLiteConnection cardSqLiteConnection;

    private readonly Dictionary<int, Music> musics;
    private readonly Dictionary<int, MusicExtra> musicExtras;

    public ApiController()
    {
        cardSqLiteConnection = DatabaseHelper.ConnectDatabase(Configs.SETTINGS.CardDbName);
        var musicSqLiteConnection = DatabaseHelper.ConnectDatabase(Configs.SETTINGS.MusicDbName);
        musics = musicSqLiteConnection.Table<Music>().ToDictionary(music => music.MusicId);
        musicExtras = musicSqLiteConnection.Table<MusicExtra>().ToDictionary(music => music.MusicId);
    }

    [Route(HttpVerbs.Get, "/Users")]
    // ReSharper disable once UnusedMember.Global
    public List<User> GetUsers()
    {
        var result = cardSqLiteConnection.Table<Card>().ToList().ConvertAll(card => new User
        {
            CardId = card.CardId,
            PlayerName = card.PlayerName
        });

        return result;
    }

    [Route(HttpVerbs.Get, "/UserDetail/{cardId}")]
    // ReSharper disable once UnusedMember.Global
    public UserDetail? GetUserDetail(long cardId)
    {
        var cardResult = cardSqLiteConnection.Table<Card>().Where(card => card.CardId == cardId);

        if (!cardResult.Any())
        {
            return null;
        }

        var card = cardResult.First();

        return ToUserDetail(card);
    }

    private UserDetail ToUserDetail(Card card)
    {
        var userDetail = new UserDetail
        {
            CardId = card.CardId,
            PlayerName = card.PlayerName
        };
        var songPlayDataDict = new Dictionary<int, SongPlayData>();

        ProcessCardDetail(userDetail, songPlayDataDict);

        userDetail.SongPlayDataList = songPlayDataDict.Values.ToList();
        userDetail.TotalSongCount = musics.Count;
        userDetail.TotalStageCount = userDetail.TotalSongCount * 3 + musicExtras.Count;
        userDetail.AverageScore = (int)(userDetail.TotalScore / userDetail.PlayedStageCount);
        userDetail.PlayedSongCount = songPlayDataDict.Count;
        return userDetail;
    }

    private void ProcessCardDetail(UserDetail userDetail, IDictionary<int, SongPlayData> songPlayDataDict)
    {
        var option = cardSqLiteConnection.Table<CardDetail>()
            .FirstOrDefault(detail => detail.CardId == userDetail.CardId && detail.Pcol1 == 0, new CardDetail
            {
                CardId = userDetail.CardId
            });
        SetOptions(option, userDetail);

        var songCounts = cardSqLiteConnection.Table<CardDetail>()
            .Where(detail => detail.CardId == userDetail.CardId && detail.Pcol1 == 20);

        foreach (var detail in songCounts)
        {
            SetCounts(detail, songPlayDataDict, userDetail);
        }

        var songScores = cardSqLiteConnection.Table<CardDetail>()
            .Where(detail => detail.CardId == userDetail.CardId && detail.Pcol1 == 21);

        foreach (var detail in songScores)
        {
            SetDetails(detail, songPlayDataDict, userDetail);
        }

        var favorites = cardSqLiteConnection.Table<CardDetail>()
            .Where(detail => detail.CardId == userDetail.CardId && detail.Pcol1 == 10)
            .ToDictionary(detail => detail.Pcol2);

        foreach (var (musicId, songPlayData) in songPlayDataDict)
        {
            songPlayData.IsFavorite = favorites[musicId].Fcol1 != 0;
        }
    }

    private static void SetOptions(CardDetail cardDetail, UserDetail userDetail)
    {
        var fastSlow = (int)cardDetail.ScoreUi1;
        var feverTrance = (int)cardDetail.ScoreUi2;

        if (!Enum.IsDefined(typeof(PlayOptions.FastSlowIndicator), fastSlow))
        {
            fastSlow = (int)PlayOptions.FastSlowIndicator.NotUsed;
        }

        if (!Enum.IsDefined(typeof(PlayOptions.FeverTranceShow), feverTrance))
        {
            feverTrance = (int)PlayOptions.FeverTranceShow.Show;
        }

        userDetail.PlayOption = new PlayOption
        {
            CardId = cardDetail.CardId,
            FastSlowIndicator = (PlayOptions.FastSlowIndicator)fastSlow,
            FeverTrance = (PlayOptions.FeverTranceShow)feverTrance
        };
    }
    private void SetDetails(CardDetail cardDetail, IDictionary<int, SongPlayData> songPlayDataDict,
        UserDetail userDetail)
    {
        var musicId = cardDetail.Pcol2;
        
        AddSongPlayDataIfNotExist(songPlayDataDict, musicId);

        for (var i = 0; i < 4; i++)
        {
            var songPlayDetailData = songPlayDataDict[musicId].SongPlaySubDataList[i];
            songPlayDetailData.Difficulty = (Difficulty)i;

            if (i != cardDetail.Pcol3)
            {
                continue;
            }

            songPlayDetailData.Score = (int)cardDetail.ScoreUi1;
            songPlayDetailData.MaxChain = (int)cardDetail.ScoreUi3;
            
            userDetail.TotalScore += cardDetail.ScoreUi1;

            if (cardDetail.ScoreUi1 >= 900000)
            {
                userDetail.SAboveStageCount++;
            }

            if (cardDetail.ScoreUi1 >= 950000)
            {
                userDetail.SPlusAboveStageCount++;
            }

            if (cardDetail.ScoreUi1 >= 990000)
            {
                userDetail.SPlusPlusAboveStageCount++;
            }
        }
    }

    private void SetCounts(CardDetail cardDetail, IDictionary<int, SongPlayData> songPlayDataDict, UserDetail userDetail)
    {
        var musicId = cardDetail.Pcol2;

        AddSongPlayDataIfNotExist(songPlayDataDict, musicId);
        
        for (var i = 0; i < 4; i++)
        {
            var songPlayDetailData = songPlayDataDict[musicId].SongPlaySubDataList[i];

            songPlayDetailData.Difficulty = (Difficulty)i;

            if (i != cardDetail.Pcol3)
            {
                continue;
            }

            songPlayDetailData.PlayCount = (int)cardDetail.ScoreUi1;
            songPlayDetailData.ClearState = ClearState.Failed;
            userDetail.PlayedStageCount++;

            if (cardDetail.ScoreUi2 > 0)
            {
                userDetail.ClearedStageCount++;
                songPlayDetailData.ClearState = ClearState.Clear;
            }

            if (cardDetail.ScoreUi3 > 0)
            {
                userDetail.NoMissStageCount++;
                songPlayDetailData.ClearState = ClearState.NoMiss;
            }

            if (cardDetail.ScoreUi4 > 0)
            {
                userDetail.FullChainStageCount++;
                songPlayDetailData.ClearState = ClearState.FullChain;
            }

            if (cardDetail.ScoreUi6 > 0)
            {
                userDetail.PerfectStageCount++;
                songPlayDetailData.ClearState = ClearState.Perfect;
            }
        }
    }
    
    private void AddSongPlayDataIfNotExist(IDictionary<int, SongPlayData> songPlayDataDict, int musicId)
    {
        if (songPlayDataDict.ContainsKey(musicId))
        {
            return;
        }

        var musicData = musics[musicId];
        var songPlayData = new SongPlayData
        {
            Artist = musicData.Artist ?? string.Empty,
            Title = musicData.Title ?? string.Empty,
            SongPlaySubDataList = new SongPlayDetailData[4]
        };

        for (var i = 0; i < 4; i++)
        {
            songPlayData.SongPlaySubDataList[i] = new SongPlayDetailData();
        }
        songPlayDataDict[musicId] = songPlayData;
    }
}