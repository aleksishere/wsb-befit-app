namespace BeFit.Models;

public class ExerciseStatsViewModel
{
    public string ExerciseTypeName { get; set; }
    public int SessionsCount { get; set; }
    public int TotalRepetitions { get; set; }
    public float AverageLoad { get; set; }
    public float MaxLoad { get; set; }
}