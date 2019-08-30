using System.Linq;
using QS.Project.Domain;
using QS.Services;
using QS.Utilities;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class WageParameterViewModel : EntityTabViewModelBase<WageParameter>
	{
		public WageParameterViewModel(IEntityConstructorParam ctorParam, ICommonServices commonServices) : base(ctorParam, commonServices)
		{
			ConfigureEntityPropertyChanges();
			SetVisibility();
		}

		bool isWageCalcRateVisible;
		public virtual bool IsWageCalcRateVisible {
			get => isWageCalcRateVisible;
			set => SetField(ref isWageCalcRateVisible, value);
		}

		bool areQuantitiesForSalesPlanVisible;
		public virtual bool AreQuantitiesForSalesPlanVisible {
			get => areQuantitiesForSalesPlanVisible;
			set => SetField(ref areQuantitiesForSalesPlanVisible, value);
		}

		public WageCalculationType[] AvailableWageCalcTypes => WageParameter.WageCalculationTypesForEmployeeCategory();

		public double WageCalcRateMaxValue => Entity.WageCalcType == WageCalculationType.percentage ? 100 : 100000;
		public string WageCalcRateUnit => Entity.WageCalcType == WageCalculationType.percentage ? "%" : CurrencyWorks.CurrencyShortName;

		void SetVisibility()
		{
			IsWageCalcRateVisible = WageParameter.WageCalculationTypesWithRates.Contains(Entity.WageCalcType);
			AreQuantitiesForSalesPlanVisible = Entity.WageCalcType == WageCalculationType.salesPlan;
		}

		void FixValues()
		{
			switch(Entity.WageCalcType) {
				case WageCalculationType.percentage:
				case WageCalculationType.fixedRoute:
				case WageCalculationType.fixedDay:
					Entity.QuantityOfFullBottlesToSell = 0;
					Entity.QuantityOfEmptyBottlesToTake = 0;
					break;
				case WageCalculationType.salesPlan:
					Entity.WageCalcRate = 0;
					break;
				default:
					Entity.WageCalcRate = 0;
					Entity.QuantityOfFullBottlesToSell = 0;
					Entity.QuantityOfEmptyBottlesToTake = 0;
					break;
			}
		}

		//decimal wageCalcRate;
		//public virtual decimal WageCalcRate {
		//	get => Entity.WageCalcRate;
		//	set {
		//		if(Entity.WageCalcType == WageCalculationType.percentage)
		//			value = value > 100 ? 100 : value;
		//		if(SetField(ref wageCalcRate, value))
		//			Entity.WageCalcRate = value;
		//	}
		//}

		void ConfigureEntityPropertyChanges()
		{
			OnEntityPropertyChanged(
				SetVisibility,
				e => e.WageCalcType
			);

			OnEntityPropertyChanged(
				FixValues,
				e => e.WageCalcType
			);

			OnEntityPropertyChanged(
				() => Entity.Comment = Entity.Title,
				e => e.WageCalcType,
				e => e.WageCalcRate,
				e => e.QuantityOfFullBottlesToSell,
				e => e.QuantityOfEmptyBottlesToTake
			);

			SetPropertyChangeRelation(
				e => e.WageCalcType,
				() => WageCalcRateMaxValue
			);

			SetPropertyChangeRelation(
				e => e.WageCalcType,
				() => WageCalcRateUnit
			);
		}
	}
}
