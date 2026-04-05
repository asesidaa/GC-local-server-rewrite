using Application.Common.Helpers;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Game.Card.Management;

public record CardRegisterCommand(long CardId, string Data) : IRequestWrapper<string>;

public class RegisterCommandHandler : RequestHandlerBase<CardRegisterCommand, string>
{
    private readonly ILogger<RegisterCommandHandler> logger;
    public RegisterCommandHandler(ICardDependencyAggregate aggregate, ILogger<RegisterCommandHandler> logger) : base(aggregate)
    {
        this.logger = logger;
    }

    public override async Task<ServiceResult<string>> Handle(CardRegisterCommand request, CancellationToken cancellationToken)
    {
        var exists = CardDbContext.CardMains.Any(card => card.CardId == request.CardId);
        if (exists)
        {
            return ServiceResult.Failed<string>(ServiceError.CustomMessage($"Card {request.CardId} already exists!"));
        }

        var card = request.Data.DeserializeCardData<CardDto>().CardDtoToCardMain();
        card.CardId = request.CardId;
        card.Created = TimeHelper.CurrentTimeToString();
        card.Modified = card.Created;
        logger.LogInformation("New card {{Id: {Id}, Player Name: {Name}}} registered", card.CardId, card.PlayerName);
        CardDbContext.CardMains.Add(card);

        if (UnlockConfig.EnableLockSystem)
        {
            await InitializeUnlockState(request.CardId, cancellationToken);
        }

        await CardDbContext.SaveChangesAsync(cancellationToken);

        return new ServiceResult<string>(request.Data);
    }

    private async Task InitializeUnlockState(long cardId, CancellationToken cancellationToken)
    {
        var defaults = await CardDbContext.DefaultUnlockStates
            .AsNoTracking()
            .ToDictionaryAsync(d => d.ItemType, d => d.UnlockedBitset, cancellationToken);

        var itemTypes = new (UnlockItemType Type, int Count)[]
        {
            (UnlockItemType.Avatar, Config.AvatarCount),
            (UnlockItemType.Navigator, Config.NavigatorCount),
            (UnlockItemType.Item, Config.ItemCount),
            (UnlockItemType.Skin, Config.SkinCount),
            (UnlockItemType.SoundEffect, Config.SeCount),
            (UnlockItemType.Title, Config.TitleCount),
        };

        foreach (var (type, count) in itemTypes)
        {
            var typeInt = (int)type;
            var bitset = defaults.TryGetValue(typeInt, out var json)
                ? BitsetHelper.EnsureLength(BitsetHelper.Deserialize(json), count)
                : BitsetHelper.CreateAllZeroes(count);

            CardDbContext.PlayerUnlockStates.Add(new PlayerUnlockState
            {
                CardId = cardId,
                ItemType = typeInt,
                UnlockedBitset = BitsetHelper.Serialize(bitset)
            });
        }

        // Music: bitset indexed by max MusicId
        var maxMusicId = await MusicDbContext.MusicUnlocks.AnyAsync(cancellationToken)
            ? await MusicDbContext.MusicUnlocks.MaxAsync(m => (int)m.MusicId, cancellationToken)
            : 0;

        if (maxMusicId > 0)
        {
            var musicTypeInt = (int)UnlockItemType.Music;
            var musicBitset = defaults.TryGetValue(musicTypeInt, out var musicJson)
                ? BitsetHelper.EnsureLength(BitsetHelper.Deserialize(musicJson), maxMusicId)
                : BitsetHelper.CreateAllZeroes(maxMusicId);

            CardDbContext.PlayerUnlockStates.Add(new PlayerUnlockState
            {
                CardId = cardId,
                ItemType = musicTypeInt,
                UnlockedBitset = BitsetHelper.Serialize(musicBitset)
            });
        }

        // Initialize coin balance
        CardDbContext.PlayerCoins.Add(new PlayerCoin
        {
            CardId = cardId,
            CurrentCoins = UnlockConfig.DefaultCoins,
            TotalCoins = UnlockConfig.DefaultCoins,
            MonthlyCoins = UnlockConfig.DefaultCoins
        });
    }
}
