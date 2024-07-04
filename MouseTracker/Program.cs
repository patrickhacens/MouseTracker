using Gma.System.MouseKeyHook;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using MouseTracker;
using System.Drawing.Design;


string dbPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/GaztoWare/db.sqlite";

HostApplicationBuilder builder = new(args);

builder.Configuration.AddJsonFile("appsettings.json", true, true);

builder.Services.AddDbContext<Db>(options => options
	.UseSqlite($"Data Source = {dbPath}")
	.EnableDetailedErrors());

builder.Services.AddHostedService<MousePoolingBgService>();

var host = builder.Build();

var logger = host.Services.GetService<ILogger<Program>>()!;

await SetupDatabase(dbPath, logger);

await host.RunAsync();

async static Task SetupDatabase(string dbPath, ILogger logger)
{
	var fi = new FileInfo(dbPath);
	if (!fi.Exists)
	{
		logger.LogInformation("Creating database at {dbPath}", dbPath);
		if (!fi.Directory!.Exists)
			fi.Directory.Create();
		var stream = fi.Create();
		stream.Close();

		Db db = new(new DbContextOptionsBuilder<Db>().UseSqlite($"Data Source = {dbPath}").Options);
		await db.Database.EnsureCreatedAsync();
		await db.DisposeAsync();
	}
}


class MousePoolingBgService : BackgroundService, IDisposable
{
	private readonly Db db;
	private readonly ILogger<MousePoolingBgService> logger;
	private readonly int bufferSize = 1000;
	private readonly List<Data> buffer;
	private readonly IMouseEvents hook;
	private readonly SqliteConnection connection;
	private readonly TimeSpan poolingFrequency = TimeSpan.FromMilliseconds(1);

	private DateTime lastPool = DateTime.Now;
	private TimeSpan currentPool = TimeSpan.Zero;

	public MousePoolingBgService(Db db, ILogger<MousePoolingBgService> logger)
	{
		this.hook = Hook.GlobalEvents();
		this.hook.MouseDown += Hook_MouseDown;
		this.hook.MouseUp += Hook_MouseUp;
		this.hook.MouseMove += Hook_MouseMove;
		this.db = db;
		this.logger = logger;
		this.buffer = new(bufferSize);
		this.connection = new(db.Database.GetConnectionString());

	}

	bool isClicking = false;

	private void Hook_MouseMove(object? sender, MouseEventArgs e)
	{
		if (isClicking)
		{
			currentPool += DateTime.Now - lastPool;
			lastPool = DateTime.Now;
			if (currentPool >= poolingFrequency)
			{
				currentPool -= poolingFrequency;
				PoolData(e.X, e.Y);
			}
		}
	}

	private void Hook_MouseUp(object? sender, MouseEventArgs e)
	{
		if (e.Button.HasFlag(MouseButtons.Left))
		{
			isClicking = false;
		}
	}

	private void Hook_MouseDown(object? sender, MouseEventArgs e)
	{
		if (e.Button.HasFlag(MouseButtons.Left))
		{
			isClicking = true;
			PoolData(e.X, e.Y);
		}
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var count = await db.Datum.CountAsync(stoppingToken);
		logger.LogInformation("Starting MacroDetector service db currently has {count} datapoints", count);
		await connection.OpenAsync(stoppingToken);
		Application.Run();
		if (buffer.Count > 0)
		{
			logger.LogInformation("There are {count} items on buffer, saving", buffer.Count);
			await Save([.. buffer], CancellationToken.None);
		}
		logger.LogInformation("Stopping");
	}

	async Task Save(Data[] data, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Saving {count} records...", data.Length);

		try
		{
			using (var transaction = await connection.BeginTransactionAsync(cancellationToken))
			{
				var command = connection.CreateCommand();
				command.CommandText = """
					INSERT INTO Datum VALUES ($x, $y, $date)
				""";
				var x = command.CreateParameter();
				var y = command.CreateParameter();
				var date = command.CreateParameter();
				x.ParameterName = "$x";
				y.ParameterName = "$y";
				date.ParameterName = "$date";

				command.Parameters.Add(x);
				command.Parameters.Add(y);
				command.Parameters.Add(date);

				for (var i = 0; i < data.Length; i++)
				{
					x.Value = data[i].X;
					y.Value = data[i].Y;
					date.Value = data[i].DateTicks;
					await command.ExecuteNonQueryAsync(cancellationToken);
				}
				await transaction.CommitAsync(cancellationToken);
			}
			logger.LogInformation("{count} data saved", data.Length);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Could not save data");
		}
	}

	void PoolData(int x, int y)
	{
		logger.LogTrace("{x}, {y}", x, y);
		buffer.Add(new(x, y, DateTime.UtcNow));
		if (buffer.Count >= bufferSize)
		{
			Save([.. buffer], CancellationToken.None);
			buffer.Clear();
		}
	}

	public override void Dispose()
	{
		this.hook.MouseUp -= Hook_MouseUp;
		this.hook.MouseDown -= Hook_MouseDown;
		this.hook.MouseMove -= Hook_MouseMove;
		this.connection.Dispose();
		base.Dispose();
	}
}