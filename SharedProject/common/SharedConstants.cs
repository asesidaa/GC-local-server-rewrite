namespace SharedProject.common;

public static class SharedConstants
{
    public const int DIFFICULTY_COUNT = 4;

    public const int E_SCORE_THRESHOLD = 0;
    
    public const int D_SCORE_THRESHOLD = 300000;
    
    public const int C_SCORE_THRESHOLD = 500000;
    
    public const int B_SCORE_THRESHOLD = 700000;
    
    public const int A_SCORE_THRESHOLD = 800000;

    public const int S_SCORE_THRESHOLD = 900000;

    public const int S_PLUS_SCORE_THRESHOLD = 950000;

    public const int S_PLUS_PLUS_SCORE_THRESHOLD = 990000;

    public static readonly ScoreGradeMap[] GRADES =
    {
        new(E_SCORE_THRESHOLD, "E"),
        new(D_SCORE_THRESHOLD, "D"),
        new(C_SCORE_THRESHOLD, "C"),
        new(B_SCORE_THRESHOLD, "B"),
        new(A_SCORE_THRESHOLD, "A"),
        new(S_SCORE_THRESHOLD, "S"),
        new(S_PLUS_SCORE_THRESHOLD, "S+"),
        new(S_PLUS_PLUS_SCORE_THRESHOLD, "S++")
    };
}