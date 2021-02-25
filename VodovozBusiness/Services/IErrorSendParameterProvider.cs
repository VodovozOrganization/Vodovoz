namespace Vodovoz.Services
{
	public interface IErrorSendParameterProvider
	{
		string GetDefaultBaseForErrorSend();

		int GetRowCountForErrorLog();
	}
}
