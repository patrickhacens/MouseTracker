using Microsoft.EntityFrameworkCore;

namespace MouseAnalyser.Domain;

public class AnalysisDb : DbContext
{
	public AnalysisDb(DbContextOptions options) : base(options)
	{
	}

	public DbSet<Player> Players { get; set; }
}
