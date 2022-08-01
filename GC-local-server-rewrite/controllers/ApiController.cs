using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using GCLocalServerRewrite.common;
using GCLocalServerRewrite.models;
using SharedProject.common;
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

    [Route(HttpVerbs.Post, "/Users/SetPlayerName")]
    public bool SetPlayerName([JsonData] User data)
    {
        var existing = cardSqLiteConnection.Table<Card>().Where(card => card.CardId == data.CardId);
        if (!existing.Any())
        {
            $"Trying to update non existing user's name! Card id {data.CardId}".Warn();
            return false;
        }
        var user = existing.First();
        user.PlayerName = data.PlayerName;
        return cardSqLiteConnection.Update(user) == 1;
    }

    [Route(HttpVerbs.Post, "/UserDetail/SetMusicFavorite")]
    // ReSharper disable once UnusedMember.Global
    public bool SetFavorite([JsonData] MusicFavoriteData data)
    {
        var existing = cardSqLiteConnection.Table<CardDetail>()
            .Where(detail => detail.CardId == data.CardId
                             && detail.Pcol1 == Configs.FAVORITE_PCOL1
                             && detail.Pcol2 == data.MusicId);

        if (!existing.Any())
        {
            $"Trying to update non existing song's favorite! Card id {data.CardId}, music id {data.MusicId}".Warn();
            return false;
        }

        var cardDetail = existing.First();
        cardDetail.Fcol1 = data.IsFavorite ? 1 : 0;
        var result = cardSqLiteConnection.Update(cardDetail);

        return result == 1;
    }

    [Route(HttpVerbs.Post, "/UserDetail/SetPlayOption")]
    // ReSharper disable once UnusedMember.Global
    public bool SetPlayOption([JsonData] PlayOption data)
    {
        var firstConfig = cardSqLiteConnection.Table<CardDetail>()
            .Where(detail => detail.CardId == data.CardId
                             && detail.Pcol1 == Configs.FIRST_CONFIG_PCOL1
                             && detail.Pcol2 == Configs.CONFIG_PCOL2
                             && detail.Pcol3 == Configs.CONFIG_PCOL3);
        
        var secondConfig = cardSqLiteConnection.Table<CardDetail>()
            .Where(detail => detail.CardId == data.CardId
                             && detail.Pcol1 == Configs.SECOND_CONFIG_PCOL1
                             && detail.Pcol2 == Configs.CONFIG_PCOL2
                             && detail.Pcol3 == Configs.CONFIG_PCOL3);

        if (!firstConfig.Any() || !secondConfig.Any())
        {
            $"Trying to update non existing card's config! Card id {data.CardId}".Warn();

            return false;
        }

        var firstDetail = firstConfig.First();
        firstDetail.ScoreUi1 = (long)data.FastSlowIndicator;
        firstDetail.ScoreUi2 = (long)data.FeverTrance;
        firstDetail.ScoreI1 = data.AvatarId;
        firstDetail.Fcol2 = (int)data.TitleId;

        var secondDetail = secondConfig.First();
        secondDetail.ScoreI1 = data.NavigatorId;
        
        var firstResult = cardSqLiteConnection.Update(firstDetail);
        var secondResult = cardSqLiteConnection.Update(secondDetail);

        return firstResult == 1 && secondResult == 1;
    }

    [Route(HttpVerbs.Get, "/UserDetail/{cardId}")]
    // ReSharper disable once UnusedMember.Global
    public UserDetail? GetUserDetail(long cardId)
    {
        var cardResult = cardSqLiteConnection.Table<Card>().Where(card => card.CardId == cardId);

        if (!cardResult.Any())
        {
            $"Getting detail for non exisisting card! Card id is {cardId}".Warn();
            return null;
        }

        var card = cardResult.First();

        return ToUserDetail(card);
    }

    private UserDetail? ToUserDetail(Card card)
    {
        if (!cardSqLiteConnection.Table<CardDetail>().Select(detail => detail.CardId == card.CardId).Any())
        {
            return null;
        }
        
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
        var firstOption = cardSqLiteConnection.Table<CardDetail>()
            .FirstOrDefault(detail => detail.CardId == userDetail.CardId
                                      && detail.Pcol1 == Configs.FIRST_CONFIG_PCOL1
                                      && detail.Pcol2 == Configs.CONFIG_PCOL2
                                      && detail.Pcol3 == Configs.CONFIG_PCOL3
                            , new CardDetail
                            {
                                CardId = userDetail.CardId
                            });
        var secondOption = cardSqLiteConnection.Table<CardDetail>()
            .FirstOrDefault(detail => detail.CardId == userDetail.CardId
                                      && detail.Pcol1 == Configs.SECOND_CONFIG_PCOL1
                                      && detail.Pcol2 == Configs.CONFIG_PCOL2
                                      && detail.Pcol3 == Configs.CONFIG_PCOL3
                            , new CardDetail
                            {
                                CardId = userDetail.CardId
                            });
        
        SetOptions(firstOption, secondOption, userDetail);

        var songCounts = cardSqLiteConnection.Table<CardDetail>()
            .Where(detail => detail.CardId == userDetail.CardId && detail.Pcol1 == Configs.COUNT_PCOL1);

        foreach (var detail in songCounts)
        {
            SetCounts(detail, songPlayDataDict, userDetail);
        }

        var songScores = cardSqLiteConnection.Table<CardDetail>()
            .Where(detail => detail.CardId == userDetail.CardId && detail.Pcol1 == Configs.SCORE_PCOL1);

        foreach (var detail in songScores)
        {
            SetDetails(detail, songPlayDataDict, userDetail);
        }

        var favorites = cardSqLiteConnection.Table<CardDetail>()
            .Where(detail => detail.CardId == userDetail.CardId && detail.Pcol1 == Configs.FAVORITE_PCOL1)
            .ToDictionary(detail => detail.Pcol2);

        foreach (var (musicId, songPlayData) in songPlayDataDict)
        {
            songPlayData.IsFavorite = favorites[musicId].Fcol1 != 0;
        }
    }

    private static void SetOptions(CardDetail firstOptionCardDetail, CardDetail secondOptionCardDetail, UserDetail userDetail)
    {
        var fastSlow = (int)firstOptionCardDetail.ScoreUi1;
        var feverTrance = (int)firstOptionCardDetail.ScoreUi2;

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
            CardId = firstOptionCardDetail.CardId,
            FastSlowIndicator = (PlayOptions.FastSlowIndicator)fastSlow,
            FeverTrance = (PlayOptions.FeverTranceShow)feverTrance,
            AvatarId = firstOptionCardDetail.ScoreI1,
            TitleId = firstOptionCardDetail.Fcol2,
            NavigatorId = secondOptionCardDetail.ScoreI1
        };
    }
    private void SetDetails(CardDetail cardDetail, IDictionary<int, SongPlayData> songPlayDataDict,
        UserDetail userDetail)
    {
        var musicId = cardDetail.Pcol2;
        
        AddSongPlayDataIfNotExist(songPlayDataDict, musicId);

        for (var i = 0; i < SharedConstants.DIFFICULTY_COUNT; i++)
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

            if (cardDetail.ScoreUi1 >= SharedConstants.S_SCORE_THRESHOLD)
            {
                userDetail.SAboveStageCount++;
            }

            if (cardDetail.ScoreUi1 >= SharedConstants.S_PLUS_SCORE_THRESHOLD)
            {
                userDetail.SPlusAboveStageCount++;
            }

            if (cardDetail.ScoreUi1 >= SharedConstants.S_PLUS_PLUS_SCORE_THRESHOLD)
            {
                userDetail.SPlusPlusAboveStageCount++;
            }
        }
    }

    private void SetCounts(CardDetail cardDetail, IDictionary<int, SongPlayData> songPlayDataDict, UserDetail userDetail)
    {
        var musicId = cardDetail.Pcol2;

        AddSongPlayDataIfNotExist(songPlayDataDict, musicId);
        
        for (var i = 0; i < SharedConstants.DIFFICULTY_COUNT; i++)
        {
            var songPlayDetailData = songPlayDataDict[musicId].SongPlaySubDataList[i];

            songPlayDetailData.Difficulty = (Difficulty)i;

            if (i != cardDetail.Pcol3)
            {
                continue;
            }

            songPlayDetailData.PlayCount = (int)cardDetail.ScoreUi1;
            songPlayDetailData.LastPlayTime = cardDetail.LastPlayTime;
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
            MusicId = musicId,
            SongPlaySubDataList = new SongPlayDetailData[SharedConstants.DIFFICULTY_COUNT]
        };

        for (var i = 0; i < SharedConstants.DIFFICULTY_COUNT; i++)
        {
            songPlayData.SongPlaySubDataList[i] = new SongPlayDetailData();
        }
        songPlayDataDict[musicId] = songPlayData;
    }
}