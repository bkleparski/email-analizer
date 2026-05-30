namespace EmailAnalyzer.Application;

public class AnalysisCounterService
{
    private int _totalAnalyses;
    public int TotalAnalyses => _totalAnalyses;
    public event Action? OnChanged;

    public void Increment()
    {
        Interlocked.Increment(ref _totalAnalyses);
        OnChanged?.Invoke();
    }
}
