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
using System.Collections.Generic;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class WageDistrictLevelRateViewModel : EntityWidgetViewModelBase<WageDistrictLevelRate>
	{
		public ITdiTab TdiTab { get; }
		public IAdvancedWageWidgetFactory AdvancedWageWidgetFactory { get; }
		public event Action WageRatesUpdate;
		public event Action WageRatesFill;

		private ViewModelBase advancedWidgetViewModel;
		public virtual ViewModelBase AdvancedWidgetViewModel {
			get => advancedWidgetViewModel;
			set => SetField(ref advancedWidgetViewModel, value);
		}

		private IWageHierarchyNode selectedNode;
		public virtual IWageHierarchyNode SelectedNode {
			get => selectedNode;
			set {
				SetField(ref selectedNode, value);
				OnPropertyChanged(nameof(IsAdvancedParameterSelected));
				OnPropertyChanged(nameof(IsNodeSelected));
			}
		}

		public bool IsNodeSelected => selectedNode != null;
		public bool IsAdvancedParameterSelected  => selectedNode is AdvancedWageParameter;

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
			ConfigureEntityPropertyChanges();
		}

		private void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(e => this.SelectedNode, () => IsAdvancedParameterSelected);
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
						if(!(selectedNode is AdvancedWageParameter wageParameter))
							return;
						var widgetVM = AdvancedWageWidgetFactory.GetAdvancedWageWidgetViewModel(wageParameter, CommonServices);
						AdvancedWidgetViewModel = widgetVM;
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
						if(!(selectedNode is AdvancedWageParameter wageParameter))
							return;
						(wageParameter.Parent as WageRate)?.ChildrenParameters.Remove(wageParameter);
						(wageParameter.Parent as AdvancedWageParameter)?.ChildrenParameters.Remove(wageParameter);
						WageRatesUpdate?.Invoke();
					});
				}
				return deleteAdvancedParametersCommand;

			}
			set => deleteAdvancedParametersCommand = value;
		}

		private DelegateCommand<IWageHierarchyNode> addNewParameterCommand;
		public DelegateCommand<IWageHierarchyNode> AddNewParameterCommand {
			get {
				if(addNewParameterCommand == null) {
					addNewParameterCommand = new DelegateCommand<IWageHierarchyNode>((selectedNode) => {
						if(selectedNode == null)
							return;
						var widgetVM = new AdvancedWageParametersViewModel(selectedNode, new AdvancedWageWidgetFactory(), CommonServices);
						widgetVM.AcceptCreation += (obj) => {
							(selectedNode as WageRate)?.ChildrenParameters.Add(obj as AdvancedWageParameter);
							(selectedNode as AdvancedWageParameter)?.ChildrenParameters.Add(obj as AdvancedWageParameter);
							AdvancedWidgetViewModel = null;
							WageRatesUpdate?.Invoke();
						};
						widgetVM.CancelCreation += () => AdvancedWidgetViewModel = null;
						AdvancedWidgetViewModel = widgetVM;
						},
					(obj) => obj != null);
				}
				return addNewParameterCommand;

			}
			set => addNewParameterCommand = value;
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
					WageRatesFill?.Invoke();
					CanFillRates = false;
				},
				() => CanFillRates
			);
		}

		private DelegateCommand<IWageHierarchyNode> selectionChangedCommand;
		public DelegateCommand<IWageHierarchyNode> SelectionChangedCommand {
			get {
				if(selectionChangedCommand == null) {
					selectionChangedCommand = new DelegateCommand<IWageHierarchyNode>(
						(node) => {
							SelectedNode = node;
						},
						(node) => true
					);
				}
				return selectionChangedCommand;
			}
		}

		#endregion CreateAndFillNewRatesCommand
	}
}
