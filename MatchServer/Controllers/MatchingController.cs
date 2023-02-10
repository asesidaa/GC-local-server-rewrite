using MatchServer.Storage;
using Microsoft.AspNetCore.Mvc;
using SharedProject.models;

namespace MatchServer.Controllers;

[ApiController]
[Route("[controller]")]
public class MatchingController : ControllerBase
{
    private readonly ILogger<MatchingController> logger;


    public MatchingController(ILogger<MatchingController> logger) {
        this.logger = logger;
    }

    [HttpPost("Start")]
    public ActionResult<IEnumerable<OnlineMatchingData>> StartOnlineMatching(OnlineMatchingData startData)
    {
        logger.LogInformation("Start matching, card id {CardId}, player name {PlayerName}", startData.CardId, startData.PlayerName);
        var matchingDb = MatchingDb.GetInstance;
        
        lock (matchingDb.DbLock)
        {
            foreach (var (matchingId, entries) in matchingDb.MatchingDictionary)
            {
                if (entries.Count >= 4)
                {
                    continue;
                }

                if (entries.Any(data => data.CardId == startData.CardId))
                {
                    logger.LogWarning("Card id {CardId} already exists in matching id {MatchingId}", startData.CardId, matchingId);
                    continue;
                }

                entries.Add(startData);
                startData.MatchingId = matchingId;
                startData.EntryNo = entries.Count - 1;
                break;
            }
            
            if (startData.MatchingId != 0)
            {
                return Ok(matchingDb.MatchingDictionary[startData.MatchingId]);
            }
            
            startData.MatchingId = matchingDb.MatchingDictionary.Count + 1;
            startData.EntryNo = 0;
            matchingDb.MatchingDictionary[startData.MatchingId] = new List<OnlineMatchingData>
            {
                startData
            };

            return Ok(matchingDb.MatchingDictionary[startData.MatchingId]);
        }
    }

    [HttpPost("Update")]
    public ActionResult<IEnumerable<OnlineMatchingUpdateRequest>> UpdateOnlineMatching(OnlineMatchingUpdateRequest updateData)
    {
        var matchingDb = MatchingDb.GetInstance;
        var matchingId = updateData.MatchingId;
        
        logger.LogInformation("Update matching, card id is {CardId}, matching id is {MatchingId}", updateData.CardId, updateData.MatchingId);
        
        if (!matchingDb.MatchingDictionary.ContainsKey(matchingId))
        {
            return BadRequest();
        }

        var dataList = matchingDb.MatchingDictionary[matchingId];

        var data = dataList.Find(data => data.CardId == updateData.CardId);
        if (data is null)
        {
            return BadRequest();
        }

        data.MessageId = updateData.MessageId;
        if (data.MatchingRemainingTime <= 0)
        {
            data.Status = 3;
            return Ok(dataList);
        }

        data.MatchingRemainingTime--;
        return Ok(dataList); 
    }

    [HttpPost("Finish")]
    public ActionResult<bool> FinishOnlineMatching(OnlineMatchingFinishRequest request)
    {
        var matchingDb = MatchingDb.GetInstance;
        var matchingId = request.MatchingId;

        lock (matchingDb.DbLock)
        {
            if (!matchingDb.MatchingDictionary.ContainsKey(matchingId))
            {
                return BadRequest();
            }
            var dataList = matchingDb.MatchingDictionary[matchingId];

            var index = dataList.FindIndex(data => data.CardId == request.CardId);
            dataList.RemoveAt(index);
        }
        return Ok(true);
    } 

    [HttpGet("Debug")]
    public ActionResult<Dictionary<long, List<OnlineMatchingUpdateRequest>>> InspectOnlineMatching()
    {
        var matchingDb = MatchingDb.GetInstance;

        return Ok(matchingDb.MatchingDictionary);
    }

    [HttpGet("Clear")]
    public ActionResult<bool> Clear()
    {
        var matchingDb = MatchingDb.GetInstance;
        lock (matchingDb.DbLock)
        {
            matchingDb.MatchingDictionary.Clear();
        }
        return Ok(true);
    }
}