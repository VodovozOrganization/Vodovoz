using System;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class WageParameterViewModel : TabViewModelBase, ISingleUoWDialog
	{
		private readonly WageParameterTargets wageParameterTarget;
		private readonly ICommonServices commonServices;

		public IUnitOfWork UoW { get; private set; }
		public event EventHandler<WageParameter> OnWageParameterCreated;
		private readonly bool isNewEntity;

		public WageParameterViewModel(IUnitOfWork uow, WageParameterTargets wageParameterTarget, ICommonServices commonServices) : base(commonServices.InteractiveService)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			this.wageParameterTarget = wageParameterTarget;
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			isNewEntity = true;
			TabName = "Новый расчет зарплаты";
			CreateWageParameter();
		}

		public WageParameterViewModel(WageParameter wageParameter, IUnitOfWork uow, ICommonServices commonServices) : base(commonServices.InteractiveService)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			isNewEntity = false;
			TabName = wageParameter.Title;
			WageParameter = wageParameter ?? throw new ArgumentNullException(nameof(wageParameter));
			WageParameterType = WageParameter.WageParameterType;
		}

		public bool CanEdit => isNewEntity;

		private WageParameter wageParameter;
		public virtual WageParameter WageParameter {
			get => wageParameter;
			set => SetField(ref wageParameter, value, () => WageParameter);
		}

		private WageParameterTypes wageParameterType;
		public virtual WageParameterTypes WageParameterType {
			get => wageParameterType;
			set {
				if(SetField(ref wageParameterType, value, () => WageParameterType)) {
					UpdateWageParameter();
				}
			}
		}

		private WidgetViewModelBase typedWageParameterViewModel;
		public virtual WidgetViewModelBase TypedWageParameterViewModel {
			get => typedWageParameterViewModel;
			set => SetField(ref typedWageParameterViewModel, value, () => TypedWageParameterViewModel);
		}

		private void UpdateWageParameter()
		{
			if(isNewEntity) {
				CreateWageParameter();
			}
			CreateWageParameterViewModel();
		}

		private void CreateWageParameterViewModel()
		{
			(TypedWageParameterViewModel as IDisposable)?.Dispose();

			switch(WageParameterType) {
				case WageParameterTypes.Fixed:
					TypedWageParameterViewModel = new FixedWageParameterViewModel((FixedWageParameter)WageParameter, CanEdit, commonServices);
					break;
				case WageParameterTypes.Percent:
					TypedWageParameterViewModel = new PercentWageParameterViewModel((PercentWageParameter)WageParameter, CanEdit, commonServices);
					break;
				case WageParameterTypes.RatesLevel:
					TypedWageParameterViewModel = new RatesLevelWageParameterViewModel(
						UoW,
						(RatesLevelWageParameter)WageParameter,
						CanEdit,
						commonServices
					);
					break;
				case WageParameterTypes.SalesPlan:
					TypedWageParameterViewModel = new SalesPlanWageParameterViewModel(
						UoW,
						(SalesPlanWageParameter)WageParameter,
						CanEdit,
						commonServices
					);
					break;
				case WageParameterTypes.OldRates:
					TypedWageParameterViewModel = new OldRatesWageParameterViewModel((OldRatesWageParameter)WageParameter, commonServices);
					break;
				case WageParameterTypes.Manual:
					TypedWageParameterViewModel = null;
					break;				
				default:
					break;
			}
		}

		private void CreateWageParameter()
		{
			if(!isNewEntity || (WageParameter != null && WageParameterType == WageParameter.WageParameterType)) {
				return;
			}

			switch(WageParameterType) {
				case WageParameterTypes.Fixed:
					WageParameter = new FixedWageParameter();
					break;
				case WageParameterTypes.Percent:
					WageParameter = new PercentWageParameter();
					break;
				case WageParameterTypes.RatesLevel:
					WageParameter = new RatesLevelWageParameter();
					break;
				case WageParameterTypes.SalesPlan:
					WageParameter = new SalesPlanWageParameter();
					break;
				case WageParameterTypes.Manual:
					WageParameter = new ManualWageParameter();
					break;
				case WageParameterTypes.OldRates:
					WageParameter = new OldRatesWageParameter();
					break;
				default:
					throw new NotImplementedException($"Не описано какой параметер должен создаваться для типа {WageParameterType.GetEnumTitle()}");
			}

			WageParameter.WageParameterTarget = wageParameterTarget;
		}

		public void Save()
		{
			if(WageParameter == null || !isNewEntity) {
				return;
			}

			var validator = commonServices.ValidationService.GetValidator();
			if(!validator.Validate(WageParameter)) {
				return;
			}

			OnWageParameterCreated?.Invoke(this, WageParameter);
			Close(false);
		}
	}
}
