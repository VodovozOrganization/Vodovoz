using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using QS.Utilities.Text;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace Vodovoz.Domain.WageCalculation
{
	[
		Appellative(
			Gender = GrammaticalGender.Feminine,
			NominativePlural = "строки ставок по зарплатным районам и уровням",
			Nominative = "строка ставки по зарплатным районам и уровням"
		)
	]
	[HistoryTrace]
	[EntityPermission]
	public class WageDistrictLevelRate : PropertyChangedBase, IDomainObject, IValidatableObject, ICloneable
	{
		#region Свойства

		public virtual int Id { get; set; }

		WageDistrict wageDistrict;
		[Display(Name = "Зарплатный район")]
		public virtual WageDistrict WageDistrict {
			get => wageDistrict;
			set => SetField(ref wageDistrict, value);
		}

		CarTypeOfUse _carTypeOfUse;
		[Display(Name = "Тип ТС")]
		public virtual CarTypeOfUse CarTypeOfUse {
			get => _carTypeOfUse;
			set => SetField(ref _carTypeOfUse, value);
		}

		WageDistrictLevelRates wageDistrictLevelRates;
		[Display(Name = "Набор ставок по уровню")]
		public virtual WageDistrictLevelRates WageDistrictLevelRates {
			get => wageDistrictLevelRates;
			set => SetField(ref wageDistrictLevelRates, value);
		}

		IList<WageRate> wageRates = new List<WageRate>();
		[Display(Name = "Ставки")]
		public virtual IList<WageRate> WageRates {
			get => wageRates;
			set => SetField(ref wageRates, value);
		}

		GenericObservableList<WageRate> observableWageRates;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WageRate> ObservableWageRates {
			get {
				if(observableWageRates == null)
				{
					observableWageRates = new GenericObservableList<WageRate>(WageRates);
				}

				return observableWageRates;
			}
		}

		#endregion Свойства

		#region Вычисляемые

		public virtual string Title => $"{GetType().GetSubjectName().StringToTitleCase()} №{Id}";

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(WageRates.Count < Enum.GetValues(typeof(WageRateTypes)).Length)
				yield return new ValidationResult(
					$"Не заполнены ставки для зарплатной группы '{CarTypeOfUse.GetEnumTitle()} {WageDistrict.Name}'",
					new[] { this.GetPropertyName(o => o.WageRates) }
				);
			foreach(var wageRate in WageRates) {
				if(wageRate.ChildrenParameters.FirstOrDefault() != null)
					foreach(var item in ValidateParameters(wageRate.ChildrenParameters,validationContext))
						yield return item;
			}
		}

		private IEnumerable<ValidationResult> ValidateParameters(IList<AdvancedWageParameter> wageParameters, ValidationContext validationContext)
		{
			foreach(var item in wageParameters) {
				if(item.ChildrenParameters.FirstOrDefault() != null)
					foreach(var newItem in ValidateParameters(item.ChildrenParameters, validationContext))
						yield return newItem;
				foreach(var validResult in item.Validate(validationContext))
					yield return validResult;
			}
		}

		#endregion Вычисляемые

		public virtual object Clone()
		{
			var wageDistrictLevelRate = new WageDistrictLevelRate
			{
				WageDistrict = WageDistrict,
				CarTypeOfUse = CarTypeOfUse,
				WageRates = new List<WageRate>()
			};

			foreach(var wageRate in WageRates)
			{
				var clonedWageRate = (WageRate)wageRate.Clone();
				clonedWageRate.WageDistrictLevelRate = wageDistrictLevelRate;
				wageDistrictLevelRate.WageRates.Add(clonedWageRate);
			}

			return wageDistrictLevelRate;
		}
	}
}
