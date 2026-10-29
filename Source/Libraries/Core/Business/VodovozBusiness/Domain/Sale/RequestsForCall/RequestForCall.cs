using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;

namespace VodovozBusiness.Domain.Sale.RequestsForCall
{
	public class RequestForCall : RequestForCallBase
	{
		public override RequestForCallType Type => RequestForCallType.General;
		
		public static RequestForCall Create(
			Source source,
			string contactName,
			string phoneNumber,
			Nomenclature nomenclature,
			Counterparty counterparty)
		{
			var requestForCall = CreateNew<RequestForCall>(source, contactName, phoneNumber, nomenclature, counterparty);
			return requestForCall;
		}
	}
}
