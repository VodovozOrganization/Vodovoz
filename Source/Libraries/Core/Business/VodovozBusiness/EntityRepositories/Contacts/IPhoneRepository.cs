using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;

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
	}
}
