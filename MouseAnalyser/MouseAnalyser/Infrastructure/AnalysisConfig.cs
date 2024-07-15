namespace MouseAnalyser.Infrastructure;

public class AnalysisConfig
{
    public string SqliteDbsPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GaztoWare", "Dbs");
    public long SpraySizeThresholdInMiliseconds { get; set; } = 500;

}
