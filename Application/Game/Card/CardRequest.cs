using Microsoft.AspNetCore.Mvc;

namespace Application.Game.Card;

public class CardRequest
{
    [ModelBinder(Name = "mac_addr")]
    public string Mac { get; set; } = string.Empty;

    [ModelBinder(Name = "cmd_str")]
    public int CardCommandType { get; set; }

    [ModelBinder(Name = "type")]
    public int CardRequestType { get; set; }

    [ModelBinder(Name = "card_no")]
    public long CardId { get; set; }

    [ModelBinder(Name = "data")]
    public string Data { get; set; } = string.Empty;
}