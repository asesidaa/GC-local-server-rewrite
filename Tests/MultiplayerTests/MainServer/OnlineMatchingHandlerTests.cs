using System.Xml.Linq;
using Application.Game.Card.OnlineMatching;
using Application.Interfaces;
using Domain.Config;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace MultiplayerTests.MainServer;

public class OnlineMatchingHandlerTests
{
    private static readonly DateTimeOffset Start = new(2026, 8, 25, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Start_UsesOuterCardIdentityAndConfiguredRoomTiming()
    {
        var fixture = CreateFixture();
        var handler = new StartOnlineMatchingCommandHandler(fixture.Service, fixture.Clock);

        var result = await handler.Handle(
            new StartOnlineMatchingCommand(123456789, EntryXml(1001, 7)),
            TestContext.Current.CancellationToken);

        Assert.True(result.Succeeded);
        var record = XDocument.Parse(result.Data!).Root!.Element("online_matching")!.Element("record")!;
        Assert.Equal("123456789", record.Element("card_id")!.Value);
        Assert.Equal("1001", record.Element("machine_id")!.Value);
        Assert.Equal("99", record.Element("matching_timeout")!.Value);
        Assert.Equal("10", record.Element("matching_wait_time")!.Value);
        Assert.Equal("89", record.Element("matching_remaining_time")!.Value);
    }

    [Fact]
    public async Task Start_InvalidProtocolIntegerReturnsServiceFailure()
    {
        var fixture = CreateFixture();
        var handler = new StartOnlineMatchingCommandHandler(fixture.Service, fixture.Clock);

        var result = await handler.Handle(
            new StartOnlineMatchingCommand(1, EntryXml(0, 7)),
            TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.Contains("machine_id", result.Error!.Message);
    }

    [Fact]
    public async Task Update_ForwardsActionEventMatchAndMessage()
    {
        var fixture = CreateFixture();
        var start = new StartOnlineMatchingCommandHandler(fixture.Service, fixture.Clock);
        var first = await start.Handle(
            new StartOnlineMatchingCommand(1, EntryXml(1001, 7)),
            TestContext.Current.CancellationToken);
        var second = await start.Handle(
            new StartOnlineMatchingCommand(2, EntryXml(1002, 7)),
            TestContext.Current.CancellationToken);
        var matchingId = ReadMatchingId(first.Data!);
        Assert.Equal(matchingId, ReadMatchingId(second.Data!));
        var handler = new UpdateOnlineMatchingCommandHandler(
            fixture.Service,
            NullLogger<UpdateOnlineMatchingCommandHandler>.Instance);

        var result = await handler.Handle(
            new UpdateOnlineMatchingCommand(2, UpdateXml(2, 7, matchingId, 777)),
            TestContext.Current.CancellationToken);
        var invalidAction = await handler.Handle(
            new UpdateOnlineMatchingCommand(2, UpdateXml(1, 7, matchingId, 888)),
            TestContext.Current.CancellationToken);

        Assert.True(result.Succeeded);
        var records = XDocument.Parse(result.Data!).Root!.Element("online_matching")!.Elements("record").ToArray();
        Assert.Equal("70", records.Single(record => record.Element("card_id")!.Value == "1").Element("message_id")!.Value);
        Assert.Equal("777", records.Single(record => record.Element("card_id")!.Value == "2").Element("message_id")!.Value);
        Assert.False(invalidAction.Succeeded);
        Assert.Contains("action", invalidAction.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Result_ParsesAssociatesAndAcknowledgesOnlyValidMembers()
    {
        var fixture = CreateFixture(maxPlayers: 2);
        var start = new StartOnlineMatchingCommandHandler(fixture.Service, fixture.Clock);
        var first = await start.Handle(
            new StartOnlineMatchingCommand(1, EntryXml(1001, 7)),
            TestContext.Current.CancellationToken);
        await start.Handle(
            new StartOnlineMatchingCommand(2, EntryXml(1002, 7)),
            TestContext.Current.CancellationToken);
        var matchingId = ReadMatchingId(first.Data!);
        var handler = new UploadOnlineMatchingResultCommandHandler(fixture.Service);

        var valid = await handler.Handle(
            new UploadOnlineMatchingResultCommand(1, ResultXml(7, matchingId, 100000)),
            TestContext.Current.CancellationToken);
        var unknownCard = await handler.Handle(
            new UploadOnlineMatchingResultCommand(99, ResultXml(7, matchingId, 100001)),
            TestContext.Current.CancellationToken);

        Assert.True(valid.Succeeded);
        Assert.Equal("1", XDocument.Parse(valid.Data!).Root!.Element("online_battle_result")!.Element("status")!.Value);
        Assert.False(unknownCard.Succeeded);
        Assert.Contains("does not belong", unknownCard.Error!.Message);
    }

    private static Fixture CreateFixture(int maxPlayers = 4)
    {
        var options = new OnlineMatchingConfig
        {
            SearchTimeoutSeconds = 99,
            FormationTimeoutSeconds = 40,
            PollIntervalSeconds = 10,
            RoomIdleExpirySeconds = 600,
            TerminalRetentionSeconds = 120,
            MatchingIdReuseGuardSeconds = 600,
            MaxPlayers = maxPlayers
        };
        var clock = new ManualTimeProvider(Start);
        var service = new OnlineMatchingService(
            clock,
            Options.Create(options),
            new SequenceMatchingIdGenerator(),
            NullLogger<OnlineMatchingService>.Instance);
        return new Fixture(service, clock);
    }

    private static string EntryXml(uint machineId, uint eventId)
    {
        return $"""
                <root><data>
                  <machine_id>{machineId}</machine_id><event_id>{eventId}</event_id>
                  <player_name>PLAYER</player_name><avatar_id>10</avatar_id><title_id>20</title_id>
                  <class_id>30</class_id><group_id>40</group_id><tenpo_id>50</tenpo_id>
                  <tenpo_name>SHOP</tenpo_name><pref_id>60</pref_id><pref>PREF</pref><message_id>70</message_id>
                </data></root>
                """;
    }

    private static string UpdateXml(uint action, uint eventId, uint matchingId, uint messageId)
    {
        return $"""
                <root><data><action>{action}</action><event_id>{eventId}</event_id>
                <matching_id>{matchingId}</matching_id><message_id>{messageId}</message_id></data></root>
                """;
    }

    private static string ResultXml(uint eventId, uint matchingId, uint score)
    {
        return $"""
                <root><data>
                  <event_id>{eventId}</event_id><matching_id>{matchingId}</matching_id>
                  <class_id>30</class_id><group_id>40</group_id>
                  <result_score>{score}</result_score><result_star>500</result_star><result_rank>1</result_rank>
                  <music_id_1>11</music_id_1><music_id_2>12</music_id_2><music_id_3>13</music_id_3>
                  <difficulty_lv_1>0</difficulty_lv_1><difficulty_lv_2>1</difficulty_lv_2><difficulty_lv_3>2</difficulty_lv_3>
                  <item_id_1>21</item_id_1><item_id_2>22</item_id_2><item_id_3>23</item_id_3>
                </data></root>
                """;
    }

    private static uint ReadMatchingId(string xml)
    {
        return uint.Parse(XDocument.Parse(xml).Root!.Element("online_matching")!.Element("record")!
            .Element("matching_id")!.Value);
    }

    private sealed record Fixture(OnlineMatchingService Service, ManualTimeProvider Clock);

    private sealed class SequenceMatchingIdGenerator : IMatchingIdGenerator
    {
        private uint next = 100;

        public uint Next() => ++next;
    }
}
