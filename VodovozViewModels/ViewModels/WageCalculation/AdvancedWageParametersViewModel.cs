using System;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Infrastructure;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class AdvancedWageParametersViewModel : UoWTabViewModelBase
	{
		public IAdvancedWageWidgetFactory AdvancedWageWidgetFactory { get; }

		public event Action<ViewModelBase> OnParameterViewModelChanged;

		private ViewModelBase parameterViewModel;
		public virtual ViewModelBase ParameterViewModel {
			get => parameterViewModel;
			set {
				SetField(ref parameterViewModel, value);
				OnParameterViewModelChanged?.Invoke(value);
			}
		}

		public AdvancedWageParametersViewModel(IAdvancedWageWidgetFactory advancedWageWidgetFactory,IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService) : base(unitOfWorkFactory, interactiveService)
		{
			AdvancedWageWidgetFactory = advancedWageWidgetFactory ?? throw new ArgumentNullException(nameof(advancedWageWidgetFactory));
		}
	}
}
