using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MouseAnalyser.DTOs;
using MouseAnalyser.Infrastructure;
using System.Threading;

namespace MouseAnalyser.Features;

[Route("api/[controller]")]
public class AnalysesController(IOptionsMonitor<AnalysisConfig> analysisMonitor) : ControllerBase
{
	private readonly AnalysisConfig analysisConfig = analysisMonitor.CurrentValue;

	[HttpGet("{name}")]
	public async Task<IActionResult> GetAnalysis(string name, CancellationToken cancellationToken)
	{

		var path = Path.Combine(analysisConfig.SqliteDbsPath, name);
		if (!Path.Exists(path))
			return NotFound();

		long distance = TimeSpan.FromMilliseconds(analysisConfig.SpraySizeThresholdInMiliseconds).Ticks;
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
				sprays.Add(new(sprayItems));
				sprayItems = [];
			}
			sprayItems.Add(current);

			last = current.DateTicks;
		}

		return Ok(sprays);
	}


	[HttpGet]
	public Task<IEnumerable<Analysis>> GetAnalyses([FromServices] IOptionsMonitor<AnalysisConfig> analysisOption)
	{
		DirectoryInfo di = new(analysisOption.CurrentValue.SqliteDbsPath);
		if (!di.Exists) di.Create();
		var files = di.EnumerateFiles("*.sqlite")
			.Select(d => new Analysis()
			{
				Name = d.Name,
			});

		return Task.FromResult(files);
	}

	[HttpPost]
	public async Task<IActionResult> AddAnalysis(IFormFile file, CancellationToken cancellation)
	{
		var stream = file.OpenReadStream();

		string path = Path.Combine(analysisConfig.SqliteDbsPath, file.FileName);

		using (var fs = System.IO.File.Open(path, FileMode.Create, FileAccess.Write))
		{
			await stream.CopyToAsync(fs, cancellation);
			fs.Position = 0;
		}
		await stream.DisposeAsync();
		return Ok();
	}

}
