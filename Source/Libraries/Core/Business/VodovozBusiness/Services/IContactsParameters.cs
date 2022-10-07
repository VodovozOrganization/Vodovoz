namespace Vodovoz.Services
{
	public interface IContactsParameters
	{
		int MinSavePhoneLength { get; }
		string DefaultCityCode { get; }
	}
}
