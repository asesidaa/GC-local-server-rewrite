using SharedProject.models;
using GenFu;
using SharedProject.enums;

namespace MudAdmin.Utils;

public class MockDataRepo
{
    private static readonly MockDataRepo INSTANCE = new MockDataRepo();

    public List<User> Users { get; }

    public List<UserDetail> UserDetails { get; private set; } = null!;

    public List<SongPlayData> SongPlayDataList { get; private set; } = null!;

    private MockDataRepo()
    {
        ConfigureGenFu();
        Users = GenFu.GenFu.ListOf<User>(10);
        GenerateUserDetails();
        GenerateSongPlayData();
    }

    private void GenerateSongPlayData()
    {
        SongPlayDataList = GenFu.GenFu.ListOf<SongPlayData>();

        foreach (var songPlayData in SongPlayDataList)
        {
            var subDataList = new List<SongPlayDetailData>();
            var random = new Random();

            foreach (var difficulty in Enum.GetValues<Difficulty>())
            {
                if (random.Next() <= int.MaxValue / 2)
                {
                    continue;
                }

                var subData = GenFu.GenFu.New<SongPlayDetailData>();
                subData.Difficulty = difficulty;

                if (subData.ClearState == ClearState.Perfect)
                {
                    subData.ClearState = ClearState.FullChain;
                }
                subDataList.Add(subData);
            }

            songPlayData.SongPlaySubDataList = subDataList;
        }

    }

    private void GenerateUserDetails()
    {
        UserDetails = new List<UserDetail>();

        foreach (var user in Users)
        {
            var detail = GenFu.GenFu.New<UserDetail>();
            detail.CardId = user.CardId;
            detail.PlayerName = user.PlayerName;
            detail.PlayOption = new PlayOption
            {
                CardId = user.CardId,
                FeverTrance = PlayOptions.FeverTranceShow.Show,
                FastSlowIndicator = PlayOptions.FastSlowIndicator.NotUsed
            };
            detail.AverageScore = 900000;
            detail.TotalScore = 10000000;
            detail.TotalSongCount = 123;
            detail.TotalStageCount = 390;
            UserDetails.Add(detail);
        }
    }

    public static MockDataRepo GetMockDataRepo()
    {
        return INSTANCE;
    }

    private void ConfigureGenFu()
    {
        GenFu.GenFu.Configure<User>()
            .Fill(user => user.CardId, () => new Random().NextInt64(7000000000000000, 8000000000000000));
        
        GenFu.GenFu.Configure<UserDetail>()
            .Fill(detail => detail.PlayedSongCount).WithinRange(100, 123)
            .Fill(detail => detail.ClearedStageCount).WithinRange(300, 390)
            .Fill(detail => detail.NoMissStageCount).WithinRange(200, 300)
            .Fill(detail => detail.FullChainStageCount).WithinRange(100, 200)
            .Fill(detail => detail.PerfectStageCount).WithinRange(0, 100)
            .Fill(detail => detail.SAboveStageCount).WithinRange(200, 300)
            .Fill(detail => detail.SPlusAboveStageCount).WithinRange(100, 200)
            .Fill(detail => detail.SPlusPlusAboveStageCount).WithinRange(0, 100);

        GenFu.GenFu.Configure<SongPlayDetailData>()
            .Fill(data => data.Score).WithinRange(0, 1000001);
        GenFu.GenFu.Configure<SongPlayData>()
            .Fill(data => data.ShowDetails, false);

    }
}