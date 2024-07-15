using MouseAnalyser.Domain;

class HostedDatabaseLoader : IHostedService, IDisposable
{
	private readonly AnalysisDb db;
	private readonly IServiceScope scope;

	public HostedDatabaseLoader(IServiceProvider sp)
	{
		this.scope = sp.CreateScope();
		this.db = scope.ServiceProvider.GetRequiredService<AnalysisDb>();
	}

	public void Dispose()
	{
		scope.Dispose();
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		return db.Database.EnsureCreatedAsync(cancellationToken);
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}