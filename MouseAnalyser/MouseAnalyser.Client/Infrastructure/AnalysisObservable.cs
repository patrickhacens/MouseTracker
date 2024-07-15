using MouseAnalyser.DTOs;

namespace MouseAnalyser.Client.Infrastructure;

public class AnalysisObservable
{
	public event EventHandler<Analysis> AnalysisAdded;

	public void InformAnalysisChanged(Analysis analysis)
	{
		AnalysisAdded?.Invoke(this, analysis);
	}
}
