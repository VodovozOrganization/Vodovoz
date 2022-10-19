using System;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParametersViewModels;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class AdvancedWageParametersViewModel : UoWWidgetViewModelBase
	{
		public event Action CancelCreation;
		public event Action<IWageHierarchyNode> AcceptCreation; 

		public IAdvancedWageWidgetFactory AdvancedWageWidgetFactory { get; }

		public IWageHierarchyNode RootNode { get; }

		public ICommonServices CommonServices { get; }

		private ViewModelBase parameterViewModel;
		public virtual ViewModelBase ParameterViewModel {
			get => parameterViewModel;
			set => SetField(ref parameterViewModel, value);
		}

		private AdvancedWageParameterType parameterType;
		public virtual AdvancedWageParameterType ParameterType {
			get => parameterType;
			set {
				SetField(ref parameterType, value);
				OnParameterTypeChanged(value);
			}
		}

		public AdvancedWageParametersViewModel(IWageHierarchyNode rootNode, IAdvancedWageWidgetFactory advancedWageWidgetFactory, ICommonServices commonServices)
		{
			RootNode = rootNode;
			AdvancedWageWidgetFactory = advancedWageWidgetFactory ?? throw new ArgumentNullException(nameof(advancedWageWidgetFactory));
			CommonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
		}

		protected void OnParameterTypeChanged(AdvancedWageParameterType newType)
		{
			ParameterViewModel = AdvancedWageWidgetFactory.GetAdvancedWageWidgetViewModel(newType, RootNode, CommonServices);
		}

		#region Commans

		private DelegateCommand cancelCreationCommand;
		public DelegateCommand CancelCreationCommand {
			get {
				if(cancelCreationCommand == null)
					cancelCreationCommand = new DelegateCommand(() => { CancelCreation?.Invoke();},() => true);
				return cancelCreationCommand;

			}
			set => cancelCreationCommand = value;
		}

		private DelegateCommand addCommand;
		public DelegateCommand AddCommand {
			get {
				if(addCommand == null) {
					AddCommand = new DelegateCommand(() => {
						var parameter = (ParameterViewModel as IWageParameterViewModel).GetParameter();
						if(!CommonServices.ValidationService.Validate(parameter))
							return;
						AcceptCreation.Invoke((ParameterViewModel as IWageParameterViewModel).GetParameter());
					}, () => true);
				}
				return addCommand;

			}
			set => addCommand = value;
		}

		#endregion Commands

	}
}
