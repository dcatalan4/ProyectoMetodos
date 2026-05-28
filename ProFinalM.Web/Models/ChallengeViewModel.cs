namespace ProFinalM.Web.Models;

public class ChallengeViewModel
{
    public List<ChallengeScore> Scores { get; set; } = [];
}

public class ChallengeScore
{
    public string UserName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int Total { get; set; }
    public string Challenge { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SaveScoreRequest
{
    public string UserName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int Total { get; set; }
    public string Challenge { get; set; } = "Reto numerico";
}
