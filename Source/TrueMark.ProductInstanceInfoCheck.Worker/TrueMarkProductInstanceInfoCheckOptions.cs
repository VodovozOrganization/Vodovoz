namespace TrueMark.ProductInstanceInfoCheck.Worker;

public class TrueMarkProductInstanceInfoCheckOptions
{
	public TimeSpan RequestsDelay { get; internal set; } = TimeSpan.FromSeconds(1);
}
