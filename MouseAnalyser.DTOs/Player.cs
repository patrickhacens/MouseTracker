using System;

namespace MouseAnalyser.DTOs;

public class Player
{
	public Guid Id { get; set; }

	public string Name { get; set; } = null!;

	public string PubgId { get; set; } = null!;
}
