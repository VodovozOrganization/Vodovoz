using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class WageDistrictLevelRatesViewModel : EntityTabViewModelBase<WageDistrictLevelRates>
	{
		public ITdiTab ParentTab { get;}

		public WageDistrictLevelRatesViewModel(ITdiTab maserTab, IEntityUoWBuilder uoWBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, IUnitOfWork uow) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			ParentTab = maserTab ?? throw new ArgumentNullException(nameof(maserTab));
			UoW = uow;
			Configure();
		}

		GenericObservableList<WageDistrictLevelRateViewModel> observableWageDistrictLevelRateViewModels = new GenericObservableList<WageDistrictLevelRateViewModel>();
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WageDistrictLevelRateViewModel> ObservableWageDistrictLevelRateViewModels {
			get => observableWageDistrictLevelRateViewModels;
			set => SetField(ref observableWageDistrictLevelRateViewModels, value, () => ObservableWageDistrictLevelRateViewModels);
		}

		public WageDistrictLevelRateViewModel WageDistrictLevelRateVM { get; set; }

		void Configure()
		{
			foreach(var district in WageSingletonRepository.GetInstance().AllWageDistricts(UoW)) {
				if(!Entity.ObservableLevelRates.Any(r => r.WageDistrict == district))
					Entity.ObservableLevelRates.Add(
						new WageDistrictLevelRate {
							WageDistrict = district,
							WageDistrictLevelRates = Entity
						}
					);
			}
			FillWageDistrictLevelRateViewModels();
		}

		Dictionary<int, WageDistrictLevelRateViewModel> viewModelsCache = new Dictionary<int, WageDistrictLevelRateViewModel>();
		public void FillWageDistrictLevelRateViewModels()
		{
			foreach(WageDistrictLevelRate distr in Entity.ObservableLevelRates) {
				WageDistrictLevelRateViewModel viewModel = null;
				if(!viewModelsCache.ContainsKey(distr.WageDistrict.Id)) {
					viewModel = new WageDistrictLevelRateViewModel(distr, CommonServices, UoW, this, new AdvancedWageWidgetFactory());
					viewModelsCache[distr.WageDistrict.Id] = viewModel;
				} else {
					viewModel = viewModelsCache[distr.WageDistrict.Id];
				}

				if(!ObservableWageDistrictLevelRateViewModels.Contains(viewModel))
					ObservableWageDistrictLevelRateViewModels.Add(viewModel);
			}
		}

		protected override void BeforeSave()
		{
			var defaultLevels = UoW.Session.QueryOver<WageDistrictLevelRates>()
								   .Where(r => r.IsDefaultLevel)
								   .List();
			foreach(var level in defaultLevels) {
				level.IsDefaultLevel = false;
				UoW.Save(level);
			}
		}
	}
}