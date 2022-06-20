namespace SharedProject.common;

public record ScoreGradeMap(int Score, string Grade)
{
    public override string ToString()
    {
        return $"{{ Score = {Score}, Grade = {Grade} }}";
    }
}