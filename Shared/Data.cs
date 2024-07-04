namespace MouseTracker;
public class Data
{
	public Data()
	{
	}

	public Data(int x, int y, DateTime date)
	{
		X=x;
		Y=y;
		Date=date;
	}

	public int X { get; set; }

	public int Y { get; set; }

	public DateTime Date
	{
		get => new(DateTicks);
		set => DateTicks = value.Ticks;
	}

	public long DateTicks { get; set; }
}

public class Spray
{

	public Spray()
	{
		Items = [];
	}

	public Spray(IEnumerable<Data> items)
	{
		this.Items = items as Data[] ?? items.ToArray();
		if (Items.Length != 0)
		{
			Start = Items.Min(d => d.Date);
			End = Items.Max(d => d.Date);
			Duration = End - Start;
			(AverageXDelta, XStandardDeviation) = Items
				.Skip(1)
				.Select((d, i) => (double)Math.Abs(d.X - Items[i].X))
				.StdDev();

			(AverageYDelta, YStandardDeviation) = Items
				.Skip(1)
				.Select((d, i) => (double)Math.Abs(d.Y - Items[i].Y))
				.StdDev();
		}
	}
	public Data[] Items { get; init; }

	public DateTime Start { get; init; }

	public DateTime End { get; init; }

	public TimeSpan Duration { get; init; }

	public double AverageXDelta { get; init; }

	public double AverageYDelta { get; init; }

	public double XStandardDeviation { get; init; }

	public double YStandardDeviation { get; init; }
}


static class MathUtils
{
	public static (double average, double deviation) StdDev(this IEnumerable<double> values)
	{
		double mean = 0.0;
		double sum = 0.0;
		double stdDev = 0.0;
		double total = 0.0;
		double avg = 0.0;
		int n = 0;
		foreach (double val in values)
		{
			n++;
			total += val;
			double delta = val - mean;
			mean += delta / n;
			sum += delta * (val - mean);
		}
		if (1 < n)
		{
			avg = total / n;
			stdDev = Math.Sqrt(sum / (n - 1));
		}

		return (avg, stdDev);
	}
}