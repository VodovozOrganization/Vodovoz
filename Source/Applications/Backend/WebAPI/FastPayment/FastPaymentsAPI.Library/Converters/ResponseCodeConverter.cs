using FastPaymentsApi.Contracts.Responses;

namespace FastPaymentsAPI.Library.Converters
{
	public class ResponseCodeConverter : IResponseCodeConverter
	{
		public ResponseStatus ConvertToResponseStatus(int responseCode)
		{
			switch(responseCode)
			{
				case 0:
					return ResponseStatus.Success;
				case 1:
					return ResponseStatus.ShopIdIsEmpty;
				case 2:
					return ResponseStatus.ShopPasswdIsEmpty;
				case 3:
					return ResponseStatus.ShopIdOrShopPasswdInvalidValue;
				case 4:
					return ResponseStatus.InternalSystemError;
				case 5:
					return ResponseStatus.TicketIsEmpty;
				case 6:
					return ResponseStatus.InvalidIPAddress;
				case 7:
					return ResponseStatus.InvalidXMLRequest;
				case 8:
					return ResponseStatus.XMLRequestIsEmpty;
				case 9:
					return ResponseStatus.UnsupportedRequestEncoding;
				case 10:
					return ResponseStatus.InvalidAmountFormat;
				default:
					return ResponseStatus.UnknownError;
			}
		}
	}
}
