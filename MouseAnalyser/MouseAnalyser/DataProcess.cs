using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MouseTracker;

namespace MouseAnalyser;

[ApiController]
public class DataProcessController : ControllerBase
{
	[HttpPost("/process")]
	public async Task<IActionResult> PreProcessFile(IFormFile file, CancellationToken cancellationToken)
	{
		long distance = TimeSpan.FromMilliseconds(500).Ticks;

		var stream = file.OpenReadStream();
		var path = Path.GetTempFileName();

		using (var fs = System.IO.File.OpenWrite(path))
		{
			await stream.CopyToAsync(fs, cancellationToken);
			fs.Position = 0;
		}
		await stream.DisposeAsync();

		var builder = new DbContextOptionsBuilder<Db>()
			.UseSqlite($"Data Source={path}")
			.EnableDetailedErrors()
			.EnableSensitiveDataLogging();

		Db db = new(builder.Options);

		var data = await db.Datum.ToArrayAsync(cancellationToken);

		List<Spray> sprays = [];
		long last = data[0].DateTicks;

		List<Data> sprayItems = [];
		for (int i = 0; i < data.Length; i++)
		{
			Data current = data[i];


			if (Math.Abs(last - current.DateTicks) > distance)
			{
				sprays.Add(new (sprayItems));
				sprayItems = [];
			}
			sprayItems.Add(current);

			last = current.DateTicks;
		}

		return Ok(sprays);
	}
}
