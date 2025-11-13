using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Data.Counterparties;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using VodovozBusiness.Domain.Contacts;

namespace Vodovoz.EntityRepositories
{
	public interface IPhoneRepository
	{
		#region PhoneType

		IList<PhoneType> GetPhoneTypes(IUnitOfWork uow);
		PhoneType PhoneTypeWithPurposeExists(IUnitOfWork uow, PhonePurpose phonePurpose);

		#endregion

		IList<IncomingCallsAnalysisReportNode> GetLastOrderIdAndDeliveryDateByPhone(
			IUnitOfWork uow, IEnumerable<string> incomingCallsNumbers);
		IList<Phone> GetPhonesByNumber(IUnitOfWork uow, string digitsPhone);
		IEnumerable<PhoneInfo> GetPhoneInfoByCounterpartiesIds(IUnitOfWork uow, IEnumerable<int> counterpartiesIds);
	}
}
