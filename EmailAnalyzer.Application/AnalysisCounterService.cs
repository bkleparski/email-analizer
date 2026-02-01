namespace EmailAnalyzer.Application;

public class AnalysisCounterService
{
    public int TotalAnalyses { get; private set; }
    public void Increment() => TotalAnalyses++;
}
