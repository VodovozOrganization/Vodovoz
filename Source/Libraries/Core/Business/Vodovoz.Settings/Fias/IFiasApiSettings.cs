namespace Vodovoz.Settings.Fias
{
	public interface IFiasApiSettings
	{
		string FiasApiBaseUrl { get; }
		string FiasApiToken { get; }
	}
}
