using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories.Document;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.Documents;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Document
{
	public class DocumentOrganizationCounterRepository : IDocumentOrganizationCounterRepository
	{
		public DocumentOrganizationCounter GetMaxDocumentOrganizationCounterOnYear(
			IUnitOfWork unitOfWork,
			DateTime date,
			OrganizationEntity organizationEntity)
		{
			var year = date.Year;

			return unitOfWork.Session.Query<DocumentOrganizationCounter>()
				.Where(d => d.CounterDateYear == year && d.Organization == organizationEntity)
				.OrderByDescending(d => d.Counter)
				.FirstOrDefault();
		}

		public async Task<DocumentOrganizationCounter> GetMaxDocumentOrganizationCounterOnYearAsync(
			IUnitOfWork unitOfWork,
			DateTime date,
			OrganizationEntity organizationEntity,
			CancellationToken cancellationToken)
		{
			var year = date.Year;
			
			return await unitOfWork.Session.Query<DocumentOrganizationCounter>()
				.Where(d => d.CounterDateYear == year && d.Organization == organizationEntity)
				.OrderByDescending(d => d.Counter)
				.FirstOrDefaultAsync(cancellationToken);
		}

		public DocumentOrganizationCounter GetDocumentOrganizationCounterByOrder(IUnitOfWork unitOfWork, OrderEntity order, int organizationId)
		{
			return unitOfWork.Session.Query<DocumentOrganizationCounter>()
				.FirstOrDefault(d => d.Order.Id == order.Id && d.Organization.Id == organizationId);
		}

		public async Task<string> GetDocumentNumberByOrderId(IUnitOfWork unitOfWork, int orderId, CancellationToken cancellationToken)
		{
			return await unitOfWork.Session.Query<OrderDocumentEntity>()
				.Where(od => od.Order.Id == orderId
					&& (od.Type == OrderDocumentType.SpecialUPD || od.Type == OrderDocumentType.UPD)
					&& od.DocumentOrganizationCounter != null)
				.Select(od => od.DocumentOrganizationCounter.DocumentNumber)
				.FirstOrDefaultAsync(cancellationToken);
		}
	}
}
