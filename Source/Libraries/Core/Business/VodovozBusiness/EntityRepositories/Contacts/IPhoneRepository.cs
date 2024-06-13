using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories.Contacts;

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
	}
}
