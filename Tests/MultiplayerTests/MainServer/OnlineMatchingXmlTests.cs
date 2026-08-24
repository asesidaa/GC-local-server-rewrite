using System.Xml.Linq;
using Application.Common.Extensions;
using Application.Dto.Game;
using Application.Game.Card.OnlineMatching;
using Domain.Enums;
using Domain.Models.OnlineMatching;

namespace MultiplayerTests.MainServer;

public class OnlineMatchingXmlTests
{
    private static readonly DateTimeOffset EntryStart = new(2026, 8, 25, 1, 2, 3, TimeSpan.Zero);

    [Fact]
    public void EntryRequest_DeserializesOnly8705Fields()
    {
        const string xml = """
                           <root><data>
                             <machine_id>1001</machine_id><event_id>7</event_id>
                             <player_name>PLAYER</player_name><avatar_id>11</avatar_id>
                             <title_id>12</title_id><class_id>13</class_id><group_id>14</group_id>
                             <tenpo_id>15</tenpo_id><tenpo_name>SHOP</tenpo_name>
                             <pref_id>16</pref_id><pref>PREF</pref><message_id>17</message_id>
                           </data></root>
                           """;

        var dto = xml.DeserializeCardData<OnlineMatchingEntryRequestDto>();
        var participant = OnlineMatchingXml.ToParticipant(dto, 1234567890123456, EntryStart);

        Assert.Equal(1001u, participant.MachineId);
        Assert.Equal(7u, participant.EventId);
        Assert.Equal(1234567890123456, participant.CardId);
        Assert.Equal("PLAYER", participant.PlayerName);
        Assert.Equal(17u, participant.MessageId);
        Assert.Equal(EntryStart, participant.EntryStart);
    }

    [Fact]
    public void UpdateRequest_DeserializesOnly8961Fields()
    {
        const string xml = """
                           <root><data>
                             <action>2</action><event_id>7</event_id>
                             <matching_id>305419896</matching_id><message_id>99</message_id>
                           </data></root>
                           """;

        var dto = xml.DeserializeCardData<OnlineMatchingUpdateRequestDto>();
        var update = OnlineMatchingXml.ToUpdate(dto);

        Assert.Equal(new OnlineMatchingUpdateValues(2, 7, 305419896, 99), update);
    }

    [Fact]
    public void ResultRequest_DeserializesEvery8709Field()
    {
        const string xml = """
                           <root><data>
                             <event_id>7</event_id><matching_id>101</matching_id>
                             <class_id>3</class_id><group_id>4</group_id>
                             <result_score>100000</result_score><result_star>500</result_star><result_rank>2</result_rank>
                             <music_id_1>11</music_id_1><music_id_2>12</music_id_2><music_id_3>13</music_id_3>
                             <difficulty_lv_1>0</difficulty_lv_1><difficulty_lv_2>1</difficulty_lv_2><difficulty_lv_3>3</difficulty_lv_3>
                             <item_id_1>21</item_id_1><item_id_2>22</item_id_2><item_id_3>23</item_id_3>
                           </data></root>
                           """;

        var dto = xml.DeserializeCardData<OnlineMatchingResultRequestDto>();
        var result = OnlineMatchingXml.ToResult(dto);

        Assert.Equal(7u, result.EventId);
        Assert.Equal(101u, result.MatchingId);
        Assert.Equal(100000u, result.ResultScore);
        Assert.Equal([11u, 12u, 13u], new[] { result.MusicId1, result.MusicId2, result.MusicId3 });
        Assert.Equal([0u, 1u, 3u], new[] { result.DifficultyLevel1, result.DifficultyLevel2, result.DifficultyLevel3 });
        Assert.Equal([21u, 22u, 23u], new[] { result.ItemId1, result.ItemId2, result.ItemId3 });
    }

    [Fact]
    public void Snapshot_SerializesNativeRecordNamesAndSharedTiming()
    {
        var participants = new[]
        {
            CreateParticipantSnapshot(0, 1, 1001, 70),
            CreateParticipantSnapshot(1, 2, 1002, 71)
        };
        var snapshot = new OnlineMatchRoomSnapshot(
            0x10203040,
            7,
            OnlineMatchPhase.Forming,
            new OnlineMatchTiming(40, 10, 30),
            participants);

        var xml = OnlineMatchingXml.SerializeSnapshot(snapshot);
        var document = XDocument.Parse(xml);
        var records = document.Root!.Element("online_matching")!.Elements("record").ToArray();

        Assert.Equal(2, records.Length);
        Assert.Equal("0", records[0].Attribute("id")!.Value);
        Assert.Equal("1", records[1].Attribute("id")!.Value);
        Assert.Equal("270544960", records[0].Element("matching_id")!.Value);
        Assert.Equal("0", records[0].Element("entry_no")!.Value);
        Assert.Equal("1", records[1].Element("entry_no")!.Value);
        Assert.All(records, record =>
        {
            Assert.Equal("1", record.Element("status")!.Value);
            Assert.Equal("40", record.Element("matching_timeout")!.Value);
            Assert.Equal("10", record.Element("matching_wait_time")!.Value);
            Assert.Equal("30", record.Element("matching_remaining_time")!.Value);
            Assert.Equal("2026/08/25 01:02:03", record.Element("entry_start")!.Value);
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(4294967296)]
    public void EntryRequest_RejectsMachineIdsOutsideNonzeroUInt32(long machineId)
    {
        var dto = CreateEntryDto();
        dto.MachineId = machineId;

        Assert.Throws<OnlineMatchingProtocolException>(() =>
            OnlineMatchingXml.ToParticipant(dto, 1, EntryStart));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(-1)]
    [InlineData(4294967296)]
    public void UpdateRequest_RejectsInvalidAction(long action)
    {
        var dto = new OnlineMatchingUpdateRequestDto
        {
            Action = action,
            EventId = 7,
            MatchingId = 101,
            MessageId = 1
        };

        Assert.Throws<OnlineMatchingProtocolException>(() => OnlineMatchingXml.ToUpdate(dto));
    }

    [Fact]
    public void ResultRequest_RejectsNegativeProtocolIntegers()
    {
        var dto = new OnlineMatchingResultRequestDto
        {
            EventId = 7,
            MatchingId = 101,
            ResultScore = -1
        };

        Assert.Throws<OnlineMatchingProtocolException>(() => OnlineMatchingXml.ToResult(dto));
    }

    private static OnlineMatchingEntryRequestDto CreateEntryDto()
    {
        return new OnlineMatchingEntryRequestDto
        {
            MachineId = 1001,
            EventId = 7,
            PlayerName = "PLAYER",
            AvatarId = 11,
            TitleId = 12,
            ClassId = 13,
            GroupId = 14,
            TenpoId = 15,
            TenpoName = "SHOP",
            PrefId = 16,
            Pref = "PREF",
            MessageId = 17
        };
    }

    private static OnlineMatchParticipantSnapshot CreateParticipantSnapshot(
        uint entryNumber,
        long cardId,
        uint machineId,
        uint messageId)
    {
        return new OnlineMatchParticipantSnapshot(
            entryNumber,
            cardId,
            machineId,
            7,
            $"Player {cardId}",
            11,
            12,
            13,
            14,
            15,
            "SHOP",
            16,
            "PREF",
            messageId,
            EntryStart,
            false);
    }
}
