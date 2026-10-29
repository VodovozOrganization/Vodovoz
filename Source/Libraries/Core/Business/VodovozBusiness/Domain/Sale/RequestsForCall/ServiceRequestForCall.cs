using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;

namespace VodovozBusiness.Domain.Sale.RequestsForCall
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Заявки на звонок по услугам СЦ",
		Nominative = "Заявка на звонок по услугам СЦ",
		Prepositional = "Заявке на звонок по услугам СЦ",
		PrepositionalPlural = "Заявках на звонок по услугам СЦ"
	)]
	public class ServiceRequestForCall : RequestForCallBase
	{
		public override RequestForCallType Type => RequestForCallType.Service;
		
		public static ServiceRequestForCall Create(
			Source source,
			string contactName,
			string phoneNumber,
			Nomenclature nomenclature,
			Counterparty counterparty)
		{
			var requestForCall = CreateNew<ServiceRequestForCall>(source, contactName, phoneNumber, nomenclature, counterparty);
			return requestForCall;
		}
	}
}
