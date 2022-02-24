using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Infrastructure;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class WageDistrictLevelRatesViewModel : EntityTabViewModelBase<WageDistrictLevelRates>
	{
		private readonly IWageCalculationRepository _wageCalculationRepository;
		public ITdiTab ParentTab { get; }

		public WageDistrictLevelRatesViewModel(
			ITdiTab maserTab,
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IUnitOfWork uow,
			IWageCalculationRepository wageCalculationRepository) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			_wageCalculationRepository = wageCalculationRepository ?? throw new ArgumentNullException(nameof(wageCalculationRepository));
			ParentTab = maserTab ?? throw new ArgumentNullException(nameof(maserTab));
			UoW = uow;
			Configure();
		}

		GenericObservableList<WageDistrictLevelRateViewModel> _observableWageDistrictLevelRateViewModels =
			new GenericObservableList<WageDistrictLevelRateViewModel>();

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WageDistrictLevelRateViewModel> ObservableWageDistrictLevelRateViewModels
		{
			get => _observableWageDistrictLevelRateViewModels;
			set => SetField(ref _observableWageDistrictLevelRateViewModels, value);
		}

		void Configure()
		{
			var allWageDistricts = _wageCalculationRepository.AllWageDistricts(UoW).ToList();

			foreach(var carTypeOfUse in Car.GetCarTypesOfUseForRatesLevelWageCalculation())
			{
				foreach(var district in allWageDistricts)
				{
					if(!Entity.ObservableLevelRates.Any(r => r.WageDistrict == district && r.CarTypeOfUse == carTypeOfUse))
					{
						Entity.ObservableLevelRates.Add(
							new WageDistrictLevelRate
							{
								WageDistrict = district,
								CarTypeOfUse = carTypeOfUse,
								WageDistrictLevelRates = Entity
							}
						);
					}
				}
			}
			FillWageDistrictLevelRateViewModels();
		}

		private readonly Dictionary<(int WageDistrictId, CarTypeOfUse CarTypeOfUse), WageDistrictLevelRateViewModel> _viewModelsCache =
			new Dictionary<(int WageDistrictId, CarTypeOfUse CarTypeOfUse), WageDistrictLevelRateViewModel>();

		public void FillWageDistrictLevelRateViewModels()
		{
			foreach(WageDistrictLevelRate distr in Entity.ObservableLevelRates)
			{
				WageDistrictLevelRateViewModel viewModel;
				if(!_viewModelsCache.ContainsKey((distr.WageDistrict.Id, distr.CarTypeOfUse)))
				{
					viewModel = new WageDistrictLevelRateViewModel(distr, CommonServices, UoW, this, new AdvancedWageWidgetFactory());
					_viewModelsCache[(distr.WageDistrict.Id, distr.CarTypeOfUse)] = viewModel;
				}
				else
				{
					viewModel = _viewModelsCache[(distr.WageDistrict.Id, distr.CarTypeOfUse)];
				}

				if(!ObservableWageDistrictLevelRateViewModels.Contains(viewModel))
				{
					ObservableWageDistrictLevelRateViewModels.Add(viewModel);
				}
			}
		}

		public override bool Save(bool close)
		{
			if(Entity.IsDefaultLevel)
			{
				var defaultLevels = UoW.Session.QueryOver<WageDistrictLevelRates>()
					.Where(r => r.IsDefaultLevel)
					.And(r => r.Id != Entity.Id)
					.List();
				foreach(var level in defaultLevels)
				{
					level.IsDefaultLevel = false;
					UoW.Save(level);
				}
			}

			if(Entity.IsDefaultLevelForOurCars)
			{
				var defaultLevels = UoW.Session.QueryOver<WageDistrictLevelRates>()
					.Where(r => r.IsDefaultLevelForOurCars)
					.And(r => r.Id != Entity.Id)
					.List();
				foreach(var level in defaultLevels)
				{
					level.IsDefaultLevelForOurCars = false;
					UoW.Save(level);
				}
			}

			if(Entity.IsDefaultLevelForRaskatCars)
			{
				var defaultLevels = UoW.Session.QueryOver<WageDistrictLevelRates>()
					.Where(r => r.IsDefaultLevelForRaskatCars)
					.And(r => r.Id != Entity.Id)
					.List();
				foreach(var level in defaultLevels)
				{
					level.IsDefaultLevelForRaskatCars = false;
					UoW.Save(level);
				}
			}

			return base.Save(close);
		}
	}
}
