namespace Vodovoz.Services
{
	public interface IFiasApiParametersProvider
	{
		string FiasApiBaseUrl { get; }
		string FiasApiToken { get; }
	}
}
