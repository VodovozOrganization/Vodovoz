using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories.Document;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Document
{
	public class DocumentOrganizationCounterRepository : IDocumentOrganizationCounterRepository
	{
		public DocumentOrganizationCounter GetMaxDocumentOrganizationCounterOnYear(IUnitOfWork unitOfWork, DateTime date)
		{
			var year = date.Year;
			var startDate = new DateTime(year, 1, 1);
			var endDate = startDate.AddYears(1);

			return unitOfWork.Session.Query<DocumentOrganizationCounter>()
				.Where(d => d.CounterDate >= startDate && d.CounterDate < endDate)
				.OrderByDescending(d => d.Counter)
				.FirstOrDefault();
		}

		public int? GetMaxCounterOnYear(IUnitOfWork unitOfWork, DateTime date)
		{
			var year = date.Year;
			var startDate = new DateTime(year, 1, 1);
			var endDate = startDate.AddYears(1);

			return unitOfWork.Session.Query<DocumentOrganizationCounter>()
				.Where(d => d.CounterDate >= startDate && d.CounterDate < endDate)
				.OrderByDescending(d => d.Counter)
				.FirstOrDefault()?.Counter;
		}
	}
}
