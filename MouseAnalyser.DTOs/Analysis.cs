namespace MouseAnalyser.DTOs;

public class Analysis
{
	public Analysis()
	{
	}

	public Analysis(string name)
	{
		Name=name;
	}

	public string Name { get; set; } = null!;
}
