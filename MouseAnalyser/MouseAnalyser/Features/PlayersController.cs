using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MouseAnalyser.Domain;

namespace MouseAnalyser.Features;

[Route("api/[controller]")]
public class PlayersController(AnalysisDb db) : ControllerBase
{
	private readonly AnalysisDb db = db;

	[HttpGet]
	public async Task<IEnumerable<Player>> GetPlayers(CancellationToken cancellation)
	{
		return await db.Players.ToListAsync(cancellation);
	}


	[HttpPost]
	public async Task<Player> AddPlayer([FromBody] Player player, CancellationToken cancellation)
	{
		db.Players.Add(player);
		await db.SaveChangesAsync(cancellation);
		return player;
	}

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> RemovePlayer([FromRoute] Guid id, CancellationToken cancellation)
	{
		db.Entry(new Player()
		{
			Id = id,
		}).State= EntityState.Deleted;

		await db.SaveChangesAsync(cancellation);
		return Ok();
	}
}
