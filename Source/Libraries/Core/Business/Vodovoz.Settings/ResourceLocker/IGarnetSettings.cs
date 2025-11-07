namespace Vodovoz.Settings.ResourceLocker
{
	/// <summary>
	/// Настройки подключения к Garnet
	/// </summary>
	public interface IGarnetSettings
	{
		string Url { get; }
		string Password { get; }
		string ConnectionString { get; }
	}
}
