using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Repositories.Contacts
{
	public static class PhoneRepository
	{
		public static IList<PhoneType> GetPhoneTypes(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<PhoneType>().List<PhoneType>();
		}
	}
}
