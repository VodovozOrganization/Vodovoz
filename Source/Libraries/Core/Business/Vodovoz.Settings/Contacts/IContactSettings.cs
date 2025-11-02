namespace Vodovoz.Settings.Contacts
{
	public interface IContactSettings
	{
		int MinSavePhoneLength { get; }
		string DefaultCityCode { get; }
	}
}
