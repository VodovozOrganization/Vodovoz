namespace Vodovoz.Settings.Common
{
	public interface IErrorSendSettings
	{
		string DefaultBaseForErrorSend { get; }

		int RowCountForErrorLog { get; }
	}
}
