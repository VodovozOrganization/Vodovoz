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
		public event EventHandler LevelChanged;
		public RatesLevelWageParameterItemViewModel(IUnitOfWork uow, RatesLevelWageParameterItem entity, bool canEdit, ICommonServices commonServices) : base(entity, commonServices)
		{
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
			WageLevels = WageSingletonRepository.GetInstance().AllLevelRates(UoW).ToList();
		}

		public bool CanEdit { get; }

		IList<WageDistrictLevelRates> wageLevels;
		public virtual IList<WageDistrictLevelRates> WageLevels {
			get => wageLevels;
			set => SetField(ref wageLevels, value);
		}
	}
}