using Microsoft.EntityFrameworkCore;
namespace MouseTracker;
public class Db(DbContextOptions options) : DbContext(options)
{
	public DbSet<Data> Datum { get; set; }

	protected override void OnModelCreating(ModelBuilder mb)
	{
		base.OnModelCreating(mb);
		mb.Entity<Data>(t =>
		{
			t.HasNoKey();
			t.HasIndex(d => d.DateTicks);
			t.Ignore(d => d.Date);
		});
	}
}