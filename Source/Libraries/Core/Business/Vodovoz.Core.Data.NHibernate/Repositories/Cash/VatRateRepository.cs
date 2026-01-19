using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories.Cash;
using Vodovoz.Core.Domain.Cash;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Cash
{
	public class VatRateRepository : IVatRateRepository
	{
		public VatRate GetVatRateByValue(IUnitOfWork unitOfWork, decimal vatRateValue) 
			=> unitOfWork.Session.Query<VatRate>().FirstOrDefault(x => x.VatRateValue == vatRateValue && !x.IsArchive );
	}
}
