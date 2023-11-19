using QS.DomainModel.UoW;
using System;
using Vodovoz.Core.Data.Repositories;

namespace Vodovoz.EntityRepositories.Pacs
{
	public class PacsRepository : IPacsRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public PacsRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public bool PacsEnabledFor(int subdivisionId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var result = uow.Session.QueryOver<Subdivision>()
					.Where(x => x.Id == subdivisionId)
					.Select(x => x.PacsTimeManagementEnabled)
					.SingleOrDefault<bool>();
				return result;
			}
		}
	}
}
