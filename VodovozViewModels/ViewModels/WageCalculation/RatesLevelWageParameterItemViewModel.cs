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
	public class RatesLevelWageParameterItemViewModel : EntityWidgetViewModelBase<RatesLevelWageParameterItem>
	{
		private readonly IWageCalculationRepository _wageCalculationRepository;
		public event EventHandler LevelChanged;
		public RatesLevelWageParameterItemViewModel(
			IUnitOfWork uow,
			RatesLevelWageParameterItem entity,
			bool canEdit,
			ICommonServices commonServices,
			IWageCalculationRepository wageCalculationRepository) : base(entity, commonServices)
		{
			_wageCalculationRepository = wageCalculationRepository ?? throw new ArgumentNullException(nameof(wageCalculationRepository));
			UoW = uow;
			CanEdit = canEdit;
			Configure();
			ConfigureEntityPropertyChanges();
		}

		void ConfigureEntityPropertyChanges()
		{
			OnEntityPropertyChanged(
				() => LevelChanged?.Invoke(this, EventArgs.Empty),
				e => e.WageDistrictLevelRates
			);
		}

		void Configure()
		{
			WageLevels = _wageCalculationRepository.AllLevelRates(UoW).ToList();
			if(Entity.WageDistrictLevelRates != null && WageLevels.All(x => x.Id != Entity.WageDistrictLevelRates.Id))
			{
				WageLevels.Add(Entity.WageDistrictLevelRates);
			}
			WageLevels = WageLevels.OrderByDescending(x => x.Id).ToList();
		}

		public bool CanEdit { get; }

		IList<WageDistrictLevelRates> wageLevels;
		public virtual IList<WageDistrictLevelRates> WageLevels {
			get => wageLevels;
			set => SetField(ref wageLevels, value);
		}
	}
}
