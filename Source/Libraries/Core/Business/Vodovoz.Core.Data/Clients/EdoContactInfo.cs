namespace Vodovoz.Core.Data.Clients
{
	public class EdoContactInfo
	{
		protected EdoContactInfo(string edxClientId, string inn, EdoContactStateCode stateCode)
		{
			EdxClientId = edxClientId;
			Inn = inn;
			StateCode = stateCode;
		}
		
		public string EdxClientId { get; }
		public string Inn { get; }
		public EdoContactStateCode StateCode { get; }

		public static EdoContactInfo Create(string edxClientId, string inn, EdoContactStateCode stateCode)
		{
			return new EdoContactInfo(edxClientId, inn, stateCode);
		}
	}
}
