using FastPaymentsAPI.Library.DTO_s.Responses;

namespace FastPaymentsAPI.Library.Converters;

public interface IResponseCodeConverter
{
	ResponseStatus ConvertToResponseStatus(int responseCode);
}
