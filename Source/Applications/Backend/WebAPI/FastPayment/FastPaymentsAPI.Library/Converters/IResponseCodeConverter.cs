using FastPaymentsApi.Contracts.Responses;

namespace FastPaymentsAPI.Library.Converters
{
	public interface IResponseCodeConverter
	{
		ResponseStatus ConvertToResponseStatus(int responseCode);
	}
}
