namespace TrueMark.ProductInstanceInfoCheck.Worker;

public class TrueMarkProductInstanceInfoCheckOptions
{
	public int CodesPerRequestLimit { get; set; }
	public TimeSpan RequestsDelay { get; internal set; } = TimeSpan.FromSeconds(1);
}
