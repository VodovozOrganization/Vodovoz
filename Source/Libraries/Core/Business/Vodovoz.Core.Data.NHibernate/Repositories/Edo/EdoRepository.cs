using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo
{
	public class EdoRepository : IEdoRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public EdoRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public async Task<IEnumerable<OrganizationEntity>> GetEdoOrganizationsAsync(CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var result = await uow.Session.QueryOver<OrganizationEntity>()
					.Where(x => x.OrganizationEdoType != OrganizationEdoType.WithoutEdo)
					.ListAsync(cancellationToken);

				return result;
			}
		}

		public async Task<IEnumerable<GtinEntity>> GetGtinsAsync(CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var result = await uow.Session.QueryOver<GtinEntity>()
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

		public async Task<bool> HasReceiptOnSumToday(decimal sum, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				FiscalMoneyPosition fiscalMoneyPositionAlias = null;
				EdoFiscalDocument fiscalDocumentAlias = null;

				var query = uow.Session.QueryOver(() => fiscalDocumentAlias)
					.JoinAlias(() => fiscalDocumentAlias.MoneyPositions, () => fiscalMoneyPositionAlias, JoinType.LeftOuterJoin)
					.Where(() => fiscalMoneyPositionAlias.Sum == sum)
					.Where(() => fiscalDocumentAlias.Index == 0)
					.WhereRestrictionOn(() => fiscalDocumentAlias.Status).IsIn(new[] { FiscalDocumentStatus.Printed, FiscalDocumentStatus.WaitForCallback, FiscalDocumentStatus.Completed })
					.Where(Restrictions.Eq(
						Projections.SqlFunction("DATE", NHibernateUtil.Date, Projections.Property(() => fiscalDocumentAlias.CheckoutTime)),
						DateTime.Today)
					)
					.ToRowCountQuery();

				var count = await query.SingleOrDefaultAsync<int>(cancellationToken);
				return count > 0;
			}
		}

		public IEnumerable<OrderEdoTask> GetEdoTaskByOrderAsync(
			IUnitOfWork uow,
			int orderId
			)
		{
			OrderEdoRequest orderEdoRequestAlias = null;
			OrderEdoTask orderEdoTaskAlias = null;

			var edoTasks = uow.Session.QueryOver(() => orderEdoTaskAlias)
				.Left.JoinAlias(() => orderEdoTaskAlias.OrderEdoRequest, () => orderEdoRequestAlias)
				.Where(() => orderEdoRequestAlias.Order.Id == orderId)
				.Where(() => orderEdoRequestAlias.DocumentType == EdoDocumentType.UPD)
				.List()
				;
			return edoTasks;
		}
	}
}
