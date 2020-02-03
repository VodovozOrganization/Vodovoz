using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.EntityRepositories
{
	public class PhoneRepository : IPhoneRepository
	{

		#region PhoneType

		public IList<PhoneType> GetPhoneTypes(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<PhoneType>().List<PhoneType>();
		}

		public PhoneType PhoneTypeWithPurposeExists(IUnitOfWork uow, PhonePurpose phonePurpose)
		{
			return uow.Session.QueryOver<PhoneType>()
				.Where(x => x.PhonePurpose == phonePurpose)
				.SingleOrDefault<PhoneType>();
		}

		#endregion
	}
}
