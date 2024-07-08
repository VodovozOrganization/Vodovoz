using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;
using Vodovoz.EntityRepositories.WageCalculation;

namespace Vodovoz.Infrastructure.Persistance.WageCalculation
{
	internal sealed class AdvancedWageRepository : IAdvancedWageRepository
	{
		public IEnumerable<AdvancedWageParameter> GetRootParameter(IUnitOfWork uow, WageRate wageRate)
		{
			if(uow == null)
				throw new ArgumentNullException(nameof(uow));
			if(wageRate == null)
				throw new ArgumentNullException(nameof(wageRate));
			if(wageRate.Id < 1)
				throw new ArgumentException(nameof(wageRate));

			return uow.Session.QueryOver<AdvancedWageParameter>()
						.Where(x => x.WageRate.Id == wageRate.Id)
						.List();
		}

		public IEnumerable<AdvancedWageParameter> GetChildParameters(IUnitOfWork uow, AdvancedWageParameter parentParameter)
		{
			if(uow == null)
				throw new ArgumentNullException(nameof(uow));
			if(parentParameter == null)
				throw new ArgumentNullException(nameof(parentParameter));
			if(parentParameter.Id < 1)
				throw new ArgumentException(nameof(parentParameter));

			return uow.Session.QueryOver<AdvancedWageParameter>()
						.Where(x => x.ParentParameter.Id == parentParameter.Id)
						.List();
		}
	}
}
