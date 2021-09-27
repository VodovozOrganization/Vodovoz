using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories.WageCalculation;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class SalesPlanWageParameterItemViewModel : EntityWidgetViewModelBase<SalesPlanWageParameterItem>
	{
		private readonly IWageCalculationRepository _wageCalculationRepository;

		public SalesPlanWageParameterItemViewModel(
			IUnitOfWork uow,
			SalesPlanWageParameterItem entity,
			bool canEdit,
			ICommonServices commonServices,
			IWageCalculationRepository wageCalculationRepository) : base(entity, commonServices)
		{
			_wageCalculationRepository = wageCalculationRepository ?? throw new ArgumentNullException(nameof(wageCalculationRepository));
			UoW = uow;
			CanEdit = canEdit;
		}

		public bool CanEdit { get; }

		public IEnumerable<SalesPlan> GetSalesPlans
		{
			get
			{
				var plans = _wageCalculationRepository.AllSalesPlans(UoW, Entity.Id <= 0).ToList();
				
				if(Entity.SalesPlan != null && Entity.SalesPlan.IsArchive)
				{
					plans.Add(Entity.SalesPlan);
				}

				return plans;
			}
		}
	}
}
