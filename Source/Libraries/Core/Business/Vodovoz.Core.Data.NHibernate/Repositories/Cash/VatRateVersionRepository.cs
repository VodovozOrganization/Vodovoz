using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories.Cash;
using Vodovoz.Core.Domain.Cash;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Cash
{
	public class VatRateVersionRepository : IVatRateVersionRepository
	{
		public VatRateVersion GetActualVatRateVersionForNomenclature(IUnitOfWork unitOfWork, int id)
		{
			return unitOfWork.Session.Query<VatRateVersion>().FirstOrDefault(x => x.Nomenclature.Id == id 
			                                                                       && x.StartDate.Date < DateTime.Now
			                                                                       && (x.EndDate > DateTime.Now || x.EndDate == null));
		}
		
		public VatRateVersion GetActualVatRateVersionForOrganization(IUnitOfWork unitOfWork, int id)
		{
			return unitOfWork.Session.Query<VatRateVersion>().FirstOrDefault(x => x.Organization.Id == id 
			                                                                      && x.StartDate.Date < DateTime.Now
			                                                                      && (x.EndDate > DateTime.Now || x.EndDate == null));
		}
		
		public VatRateVersion GetVatRateVersionForNomenclature(IUnitOfWork unitOfWork, int id, DateTime date)
		{
			return unitOfWork.Session.Query<VatRateVersion>().FirstOrDefault(x => x.Nomenclature.Id == id 
			                                                                      && x.StartDate.Date < date
			                                                                      && (x.EndDate > date || x.EndDate == null));
		}
		
		public VatRateVersion GetVatRateVersionForOrganization(IUnitOfWork unitOfWork, int id, DateTime date)
		{
			return unitOfWork.Session.Query<VatRateVersion>().FirstOrDefault(x => x.Organization.Id == id 
			                                                                      && x.StartDate.Date < date
			                                                                      && (x.EndDate > date || x.EndDate == null));
		}
	}
}
