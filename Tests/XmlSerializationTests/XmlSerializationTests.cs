using System.Xml.Linq;
using System.Xml.Serialization;
using Application.Common.Extensions;
using Application.Dto.Game;
using Application.Game.Rank;

namespace XmlSerializationTests;

public class XmlSerializationTests
{
    [Fact]
    public void SerializeCardData_WithXPath_ProducesCorrectStructure()
    {
        var dto = new CardDto
        {
            CardId = 12345,
            PlayerName = "TestPlayer",
            ScoreI1 = 100,
            Fcol1 = 1,
            Fcol2 = 2,
            Fcol3 = 3,
            AchieveStatus = "status",
            Created = "2024-01-01",
            Modified = "2024-01-02"
        };

        var xml = dto.SerializeCardData("/root/card");

        // Must contain XML declaration
        Assert.Contains("<?xml", xml);

        var doc = XDocument.Parse(xml);
        Assert.NotNull(doc.Root);
        Assert.Equal("root", doc.Root.Name.LocalName);

        var card = doc.Root.Element("card");
        Assert.NotNull(card);
        Assert.Equal("12345", card.Element("card_id")?.Value);
        Assert.Equal("TestPlayer", card.Element("player_name")?.Value);
        Assert.Equal("100", card.Element("score_i1")?.Value);
        Assert.Equal("1", card.Element("fcol1")?.Value);
        Assert.Equal("2", card.Element("fcol2")?.Value);
        Assert.Equal("3", card.Element("fcol3")?.Value);
    }

    [Fact]
    public void SerializeCardData_NoNamespaces()
    {
        var dto = new CardDto { CardId = 1, PlayerName = "Test" };
        var xml = dto.SerializeCardData("/root/card");

        // Must not contain xmlns
        Assert.DoesNotContain("xmlns", xml);
    }

    [Fact]
    public void SerializeCardDetailDto_WithXPath_IncludesAllFields()
    {
        var dto = new CardDetailDto
        {
            Id = 5,
            CardId = 999,
            Pcol1 = 10,
            Pcol2 = 20,
            Pcol3 = 30,
            ScoreI1 = 50000,
            ScoreUi1 = 1,
            ScoreUi2 = 2,
            ScoreUi3 = 3,
            ScoreUi4 = 4,
            ScoreUi5 = 5,
            ScoreUi6 = 6,
            ScoreBi1 = 100000,
            LastPlayTenpoId = "1337",
            Fcol1 = 11,
            Fcol2 = 22,
            Fcol3 = 33
        };

        var xml = dto.SerializeCardData("/root/card_detail");
        var doc = XDocument.Parse(xml);
        var detail = doc.Root!.Element("card_detail");
        Assert.NotNull(detail);

        // id should be an attribute
        Assert.Equal("5", detail.Attribute("id")?.Value);
        Assert.Equal("999", detail.Element("card_id")?.Value);
        Assert.Equal("10", detail.Element("pcol1")?.Value);
        Assert.Equal("20", detail.Element("pcol2")?.Value);
        Assert.Equal("30", detail.Element("pcol3")?.Value);
        Assert.Equal("11", detail.Element("fcol1")?.Value);
        Assert.Equal("22", detail.Element("fcol2")?.Value);
        Assert.Equal("33", detail.Element("fcol3")?.Value);
    }

    [Fact]
    public void SerializeCardDetailDto_IdMinusOne_ShouldNotSerializeId()
    {
        var dto = new CardDetailDto
        {
            Id = -1,
            CardId = 999,
            Pcol1 = 10
        };

        var xml = dto.SerializeCardData("/root/card_detail");
        var doc = XDocument.Parse(xml);
        var detail = doc.Root!.Element("card_detail");
        Assert.NotNull(detail);

        // When Id is -1, ShouldSerializeId returns false, so no id attribute
        Assert.Null(detail.Attribute("id"));
    }

    [Fact]
    public void SerializeCardDataList_ProducesMultipleRecords()
    {
        var dtos = new List<MusicDto>
        {
            new() { Id = 0, MusicId = 100, Title = "Song A", Artist = "Artist A", NewFlag = 1, UseFlag = 1, CalcFlag = 0 },
            new() { Id = 1, MusicId = 200, Title = "Song B", Artist = "Artist B", NewFlag = 0, UseFlag = 1, CalcFlag = 1 }
        };

        var xml = dtos.SerializeCardDataList("/root/music/record");
        var doc = XDocument.Parse(xml);

        Assert.Equal("root", doc.Root!.Name.LocalName);
        var music = doc.Root.Element("music");
        Assert.NotNull(music);

        var records = music.Elements("record").ToList();
        Assert.Equal(2, records.Count);

        Assert.Equal("0", records[0].Attribute("id")?.Value);
        Assert.Equal("100", records[0].Element("music_id")?.Value);
        Assert.Equal("Song A", records[0].Element("title")?.Value);

        Assert.Equal("1", records[1].Attribute("id")?.Value);
        Assert.Equal("200", records[1].Element("music_id")?.Value);
        Assert.Equal("Song B", records[1].Element("title")?.Value);
    }

    [Fact]
    public void SerializeCardData_ContainerWithXmlRoot_UsesOwnRoot()
    {
        var container = new GlobalScoreRankContainer
        {
            Ranks = new List<ScoreRankDto>
            {
                new()
                {
                    Id = 0,
                    CardId = 123,
                    PlayerName = "Player1",
                    Rank = 1,
                    Rank2 = 1,
                    TotalScore = 999999
                }
            },
            Status = new RankStatus
            {
                TableName = "GlobalScoreRank",
                StartDate = "2024-01-01",
                EndDate = "2024-12-31",
                Rows = 1,
                Status = 1
            }
        };

        var xml = container.SerializeCardData();

        // Must contain XML declaration
        Assert.Contains("<?xml", xml);
        Assert.DoesNotContain("xmlns", xml);

        var doc = XDocument.Parse(xml);
        Assert.Equal("root", doc.Root!.Name.LocalName);

        // Check score_rank array
        var scoreRank = doc.Root.Element("score_rank");
        Assert.NotNull(scoreRank);
        var record = scoreRank.Element("record");
        Assert.NotNull(record);
        Assert.Equal("0", record.Attribute("id")?.Value);
        Assert.Equal("123", record.Element("card_id")?.Value);

        // Check ranking_status
        var status = doc.Root.Element("ranking_status");
        Assert.NotNull(status);
        Assert.Equal("GlobalScoreRank", status.Element("table_name")?.Value);
    }

    [Fact]
    public void DeserializeCardData_ParsesDataElement()
    {
        var xml = """
            <root>
                <data>
                    <card_id>12345</card_id>
                    <player_name>TestPlayer</player_name>
                    <score_i1>100</score_i1>
                    <fcol1>1</fcol1>
                    <fcol2>2</fcol2>
                    <fcol3>3</fcol3>
                    <achieve_status>done</achieve_status>
                    <created>2024-01-01</created>
                    <modified>2024-01-02</modified>
                </data>
            </root>
            """;

        var result = xml.DeserializeCardData<CardDto>();

        Assert.Equal(12345, result.CardId);
        Assert.Equal("TestPlayer", result.PlayerName);
        Assert.Equal(100, result.ScoreI1);
        Assert.Equal(1, result.Fcol1);
        Assert.Equal(2, result.Fcol2);
        Assert.Equal(3, result.Fcol3);
    }

    [Fact]
    public void DeserializeCardData_CardDetail_IncludesAllFields()
    {
        var xml = """
            <root>
                <data>
                    <card_id>999</card_id>
                    <pcol1>10</pcol1>
                    <pcol2>20</pcol2>
                    <pcol3>30</pcol3>
                    <score_i1>50000</score_i1>
                    <score_ui1>1</score_ui1>
                    <score_ui2>2</score_ui2>
                    <score_ui3>3</score_ui3>
                    <score_ui4>4</score_ui4>
                    <score_ui5>5</score_ui5>
                    <score_ui6>6</score_ui6>
                    <score_bi1>100000</score_bi1>
                    <last_play_tenpo_id>1337</last_play_tenpo_id>
                    <fcol1>11</fcol1>
                    <fcol2>22</fcol2>
                    <fcol3>33</fcol3>
                </data>
            </root>
            """;

        var dto = xml.DeserializeCardData<CardDetailDto>();
        Assert.Equal(999, dto.CardId);
        Assert.Equal(10, dto.Pcol1);
        Assert.Equal(20, dto.Pcol2);
        Assert.Equal(30, dto.Pcol3);
        Assert.Equal(50000, dto.ScoreI1);
        Assert.Equal(11, dto.Fcol1);
        Assert.Equal(22, dto.Fcol2);
        Assert.Equal(33, dto.Fcol3);
    }

    [Fact]
    public void RoundTrip_SerializeAndDeserialize()
    {
        var original = new CardDto
        {
            CardId = 42,
            PlayerName = "RoundTrip",
            ScoreI1 = 777,
            Fcol1 = 10,
            Fcol2 = 20,
            Fcol3 = 30,
            AchieveStatus = "test",
            Created = "2024-06-15",
            Modified = "2024-06-16"
        };

        // Serialize
        var xml = original.SerializeCardData("/root/data");

        // Deserialize (our method looks for /root/data)
        var deserialized = xml.DeserializeCardData<CardDto>();

        Assert.Equal(original.CardId, deserialized.CardId);
        Assert.Equal(original.PlayerName, deserialized.PlayerName);
        Assert.Equal(original.ScoreI1, deserialized.ScoreI1);
        Assert.Equal(original.Fcol1, deserialized.Fcol1);
        Assert.Equal(original.Fcol2, deserialized.Fcol2);
        Assert.Equal(original.Fcol3, deserialized.Fcol3);
    }

    [Fact]
    public void SerializeEmptyObject_ProducesEmptyElement()
    {
        var empty = new object();
        var xml = empty.SerializeCardData("/root/cond");

        var doc = XDocument.Parse(xml);
        Assert.Equal("root", doc.Root!.Name.LocalName);
        var cond = doc.Root.Element("cond");
        Assert.NotNull(cond);
    }

    [Fact]
    public void SerializeCardDataList_ThreeLevelXPath()
    {
        var dtos = new List<MusicAouDto>
        {
            new() { Id = 0, MusicId = 1, UseFlag = 1 },
            new() { Id = 1, MusicId = 2, UseFlag = 0 }
        };

        var xml = dtos.SerializeCardDataList("/root/music_aou/record");
        var doc = XDocument.Parse(xml);

        Assert.Equal("root", doc.Root!.Name.LocalName);
        var musicAou = doc.Root.Element("music_aou");
        Assert.NotNull(musicAou);
        var records = musicAou.Elements("record").ToList();
        Assert.Equal(2, records.Count);
        Assert.Equal("1", records[0].Element("music_id")?.Value);
        Assert.Equal("2", records[1].Element("music_id")?.Value);
    }

    [Fact]
    public void SerializeScoreRankDto_WithAttributes()
    {
        var dto = new ScoreRankDto
        {
            Id = 3,
            CardId = 456,
            PlayerName = "RankPlayer",
            Rank = 5,
            Rank2 = 5,
            TotalScore = 500000,
            AvatarId = 10,
            TitleId = 20,
            PrefId = 1,
            Pref = "Tokyo",
            AreaId = 2,
            Area = "Shibuya",
            LastPlayTenpoId = 100,
            TenpoName = "Arcade1",
            Title = "Champion"
        };

        var xml = dto.SerializeCardData("/root/score_rank");
        var doc = XDocument.Parse(xml);
        var rank = doc.Root!.Element("score_rank");
        Assert.NotNull(rank);
        Assert.Equal("3", rank.Attribute("id")?.Value);
        Assert.Equal("456", rank.Element("card_id")?.Value);
        Assert.Equal("RankPlayer", rank.Element("player_name")?.Value);
        Assert.Equal("500000", rank.Element("score_bi1")?.Value);
    }

    [Fact]
    public void SerializeCardBDatumDto()
    {
        var dto = new CardBDatumDto
        {
            CardId = 789,
            CardBdata = "base64encodeddata",
            BDataSize = 1024
        };

        var xml = dto.SerializeCardData("/root/card_bdata");
        var doc = XDocument.Parse(xml);
        var bdata = doc.Root!.Element("card_bdata");
        Assert.NotNull(bdata);
        Assert.Equal("789", bdata.Element("card_id")?.Value);
        Assert.Equal("base64encodeddata", bdata.Element("bdata")?.Value);
        Assert.Equal("1024", bdata.Element("bdata_size")?.Value);
    }

    [Fact]
    public void SerializeMusicExtraDto()
    {
        var dto = new MusicExtraDto
        {
            Id = 0,
            MusicId = 555,
            UseFlag = 1
        };

        var xml = dto.SerializeCardData("/root/music_extra");
        var doc = XDocument.Parse(xml);
        var extra = doc.Root!.Element("music_extra");
        Assert.NotNull(extra);
        Assert.Equal("0", extra.Attribute("id")?.Value);
        Assert.Equal("555", extra.Element("music_id")?.Value);
        Assert.Equal("1", extra.Element("use_flag")?.Value);
    }
}
