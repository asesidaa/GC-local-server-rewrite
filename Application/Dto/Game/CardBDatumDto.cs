﻿namespace Application.Dto.Game;

public class CardBDatumDto
{
    [XmlElement(ElementName = "card_id")]
    public long CardId { get; set; }

    [XmlElement(ElementName = "bdata")]
    public string CardBdata { get; set; } = string.Empty;
    
    [XmlElement(ElementName = "bdata_size")]
    public int BDataSize { get; set; }
}