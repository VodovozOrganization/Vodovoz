using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories.Goods;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Goods
{
	public class NomenclatureRepository : INomenclatureRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public NomenclatureRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public async Task<NomenclatureEntity> GetNomenclatureByGtinAsync(
			IUnitOfWork uow,
			string gtin,
			CancellationToken cancellationToken)
		{
			GtinEntity gtinAlias = null;

			var nomenclatures = await uow.Session.QueryOver<NomenclatureEntity>()
				.Left.JoinAlias(x => x.Gtins, () => gtinAlias)
				.Where(() => gtinAlias.GtinNumber == gtin)
				.ListAsync(cancellationToken);

			return nomenclatures.FirstOrDefault();
		}

		public async Task<IEnumerable<GtinEntity>> GetGtinsAsync(CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var result = await uow.Session.QueryOver<GtinEntity>()
					.OrderBy(g => g.Priority).Asc
					.ListAsync(cancellationToken);

				return result;
			}
		}

		public async Task<IEnumerable<GroupGtinEntity>> GetGroupGtinsAsync(CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var result = await uow.Session.QueryOver<GroupGtinEntity>()
					.ListAsync(cancellationToken);

				return result;
			}
		}
	}
}
