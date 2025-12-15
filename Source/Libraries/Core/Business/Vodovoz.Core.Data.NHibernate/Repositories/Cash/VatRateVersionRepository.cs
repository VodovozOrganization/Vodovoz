using System;
using System.Collections;
using System.Collections.Generic;
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
		
		public IEnumerable<VatRateVersion> GetVatRateVersionsForOrganization(IUnitOfWork unitOfWork, decimal targetVatRate)
		{
			return unitOfWork.Session.Query<VatRateVersion>()
				.Where(x =>  x.EndDate == null
				             && x.VatRate.VatRateValue == targetVatRate
				             && x.Nomenclature == null
				             && x.VatRate.Vat1cTypeValue != Vat1cType.IndividualEntrepreneur
				             && x.Organization != null);
		}
		
		public IEnumerable<VatRateVersion> GetVatRateVersionsForNomenclature(IUnitOfWork unitOfWork, decimal targetVatRate)
		{
			return unitOfWork.Session.Query<VatRateVersion>()
				.Where(x => x.EndDate == null
				             && x.VatRate.VatRateValue == targetVatRate
				             && x.Nomenclature != null
				             && x.VatRate.Vat1cTypeValue != Vat1cType.Reduced
				             && x.Organization == null);
		}
	}
}
