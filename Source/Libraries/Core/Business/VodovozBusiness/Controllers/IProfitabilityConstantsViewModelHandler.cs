using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain;

namespace Vodovoz.Controllers
{
	public interface IProfitabilityConstantsViewModelHandler
	{
		IUnitOfWorkGeneric<ProfitabilityConstants> GetLastCalculatedProfitabilityConstants();
		IUnitOfWorkGeneric<ProfitabilityConstants> GetProfitabilityConstantsByCalculatedMonth(IUnitOfWork uow, DateTime calculatedMonth);
	}
}
