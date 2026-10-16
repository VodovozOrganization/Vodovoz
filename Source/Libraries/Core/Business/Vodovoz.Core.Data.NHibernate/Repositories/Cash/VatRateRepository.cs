using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories.Cash;
using Vodovoz.Core.Domain.Cash;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Cash
{
	public class VatRateRepository : IVatRateRepository
	{
		public VatRate GetVatRateByValue(IUnitOfWork unitOfWork, decimal vatRateValue) 
			=> unitOfWork.Session.Query<VatRate>().FirstOrDefault(x => x.VatRateValue == vatRateValue && !x.IsArchive);

		public VatRate GetActualVatRateFromOrganization(IUnitOfWork uow, int organizationId, DateTime? date)
		{
			var query =
				from vatVersion in uow.Session.Query<VatRateVersion>()
				join vatRate in uow.Session.Query<VatRate>()
					on vatVersion.VatRate.Id equals vatRate.Id
				where vatVersion.Organization.Id == organizationId
					&& vatVersion.StartDate.Date <= date
					&& (vatVersion.EndDate == null || vatVersion.EndDate >= date)
					&& !vatRate.IsArchive
				select vatVersion.VatRate;

			return query.FirstOrDefault();
		}
		
		public VatRate GetActualVatRateFromNomenclature(IUnitOfWork uow, int nomenclatureId, DateTime? date)
		{
			var query =
				from vatVersion in uow.Session.Query<VatRateVersion>()
				join vatRate in uow.Session.Query<VatRate>()
					on vatVersion.VatRate.Id equals vatRate.Id
				where vatVersion.Nomenclature.Id == nomenclatureId
					&& vatVersion.StartDate.Date <= date
					&& (vatVersion.EndDate == null || vatVersion.EndDate >= date)
					&& !vatRate.IsArchive
				select vatVersion.VatRate;

			return query.FirstOrDefault();
		}
	}
}
