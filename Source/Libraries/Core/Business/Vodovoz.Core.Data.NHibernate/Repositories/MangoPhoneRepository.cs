using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Mango;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.NHibernate.Repositories
{
	/*public class MangoPhoneRepository : IMangoPhoneRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public MangoPhoneRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
		}

		public async Task<IEnumerable<MangoPhone>> GetMangoPhones()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				return await uow.Session.QueryOver<MangoPhone>().ListAsync();
			}
		}

		public async Task<IEnumerable<string>> LoadAssignedPhones()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				return await uow.Session.QueryOver<Operator>()
					.OrderBy(x => x.Id).Desc
					.tak





					.WhereRestrictionOn(x => x.PhoneNumber).IsNotNull
					.Where(x => x.State > OperatorState.Connected)
					.Where(x => x.State < OperatorState.Disconnected)
			}
		}
	}*/
}
