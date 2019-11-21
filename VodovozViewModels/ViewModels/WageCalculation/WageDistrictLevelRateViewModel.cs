using System;
using System.Linq;
using QS.Commands;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;
using QS.DomainModel.UoW;
using QS.Tdi;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels;
using Vodovoz.Infrastructure;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class WageDistrictLevelRateViewModel : EntityWidgetViewModelBase<WageDistrictLevelRate>
	{
		public ITdiTab TdiTab { get; }
		public IAdvancedWageWidgetFactory AdvancedWageWidgetFactory { get; }

		public WageDistrictLevelRateViewModel(WageDistrictLevelRate entity, ICommonServices commonServices, IUnitOfWork uow, ITdiTab tdiTab, IAdvancedWageWidgetFactory advancedWageWidgetFactory) : base(entity, commonServices)
		{
			AdvancedWageWidgetFactory = advancedWageWidgetFactory ?? throw new ArgumentException(nameof(advancedWageWidgetFactory));
			UoW = uow;
			ConfigureViewModel();
			CreateCreateAndFillNewRatesCommand();
			TdiTab = tdiTab;
		}

		void ConfigureViewModel()
		{
			CanFillRates = Entity.Id <= 0 && !Entity.WageRates.Any();
		}

		bool canFillRates;

		public virtual bool CanFillRates {
			get => canFillRates;
			set => SetField(ref canFillRates, value);
		}

		#region CreateAndFillNewRatesCommand

		public DelegateCommand CreateAndFillNewRatesCommand { get; private set; }

		private DelegateCommand<IWageHierarchyNode> openAdvancedParametersCommand;
		public DelegateCommand<IWageHierarchyNode> OpenAdvancedParametersCommand { 
		get{ 	
				if(openAdvancedParametersCommand == null) {
					openAdvancedParametersCommand = new DelegateCommand<IWageHierarchyNode>((selectedNode) => {
						if(selectedNode is AdvancedWageParameter wageParameter)
							TdiTab.TabParent.AddSlaveTab(TdiTab,AdvancedWageWidgetFactory.GetAdvancedWageWidgetViewModel(wageParameter, CommonServices) as TabViewModelBase);
					});
				}
				return openAdvancedParametersCommand;

			} 
		set => openAdvancedParametersCommand = value; }

		private DelegateCommand<IWageHierarchyNode> deleteAdvancedParametersCommand;
		public DelegateCommand<IWageHierarchyNode> DeleteAdvancedParametersCommand {
			get {
				if(deleteAdvancedParametersCommand == null) {
					deleteAdvancedParametersCommand = new DelegateCommand<IWageHierarchyNode>((selectedNode) => {
						if(selectedNode is AdvancedWageParameter wageParameter)
							UoW.Delete(wageParameter);
					});
				}
				return deleteAdvancedParametersCommand;

			}
			set => deleteAdvancedParametersCommand = value;
		}


		void CreateCreateAndFillNewRatesCommand()
		{
			CreateAndFillNewRatesCommand = new DelegateCommand(
				() => {
					foreach(WageRateTypes enumValue in Enum.GetValues(typeof(WageRateTypes))) {
						if(!Entity.ObservableWageRates.Any(r => r.WageRateType == enumValue))
							Entity.ObservableWageRates.Add(
								new WageRate {
									WageRateType = enumValue,
									ForDriverWithForwarder = 0,
									ForDriverWithoutForwarder = 0,
									ForForwarder = 0,
									WageDistrictLevelRate = Entity
								}
							);
					}
					CanFillRates = false;
				},
				() => CanFillRates
			);
		}

		#endregion CreateAndFillNewRatesCommand
	}
}
