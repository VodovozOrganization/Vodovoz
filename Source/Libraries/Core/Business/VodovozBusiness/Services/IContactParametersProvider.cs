namespace Vodovoz.Services
{
	public interface IContactParametersProvider
	{
		int MinSavePhoneLength { get; }
		string DefaultCityCode { get; }
	}
}
