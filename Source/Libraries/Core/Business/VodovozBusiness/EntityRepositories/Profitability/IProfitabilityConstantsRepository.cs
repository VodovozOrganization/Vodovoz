using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace Vodovoz.EntityRepositories.Profitability
{
	public interface IProfitabilityConstantsRepository
	{
		ProfitabilityConstants GetLastProfitabilityConstants(IUnitOfWork uow);
		ProfitabilityConstants GetProfitabilityConstantsByCalculatedMonth(IUnitOfWork uow, DateTime calculatedMonth);
		bool ProfitabilityConstantsByCalculatedMonthExists(IUnitOfWork uow, DateTime monthFrom, DateTime monthTo);
		IList<AverageMileageCarsByTypeOfUseNode> GetAverageMileageCarsByTypeOfUse(IUnitOfWork uow, DateTime calculatedMonth);
		ProfitabilityConstants GetNearestProfitabilityConstantsByDate(IUnitOfWork uow, DateTime date);
	}
}
