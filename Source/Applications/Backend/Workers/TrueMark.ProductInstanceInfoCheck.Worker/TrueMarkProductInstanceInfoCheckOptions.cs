namespace TrueMark.ProductInstanceInfoCheck.Worker;

public class TrueMarkProductInstanceInfoCheckOptions
{
	public int CodesPerRequestLimit { get; set; }
	public TimeSpan RequestsDelay { get; set; } = TimeSpan.FromSeconds(1);
	public TimeSpan RequestsTimeOut { get; set; } = TimeSpan.FromSeconds(2);
}
