using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using QS.Utilities;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.WageCalculation
{
	[
		Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "параметры расчёта ЗП",
			Nominative = "параметр расчёта ЗП",
			Accusative = "параметра расчёта ЗП",
			Genitive = "параметра расчёта ЗП"
		)
	]
	[HistoryTrace]
	[EntityPermission]
	public class WageParameter : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}

		EmployeeCategory employeeCategory;
		[Display(Name = "Категория для расчёта")]
		public virtual EmployeeCategory EmployeeCategory {
			get => employeeCategory;
			set {
				if(NHibernate.NHibernateUtil.IsInitialized(EmployeeCategory))
					WageCalcType = WageCalculationType.normal;
				SetField(ref employeeCategory, value);
			}
		}

		WageCalculationType wageCalcType;
		[Display(Name = "Тип расчёта зарплаты")]
		public virtual WageCalculationType WageCalcType {
			get => wageCalcType;
			set => SetField(ref wageCalcType, value);
		}

		bool isArchive;
		[Display(Name = "В архиве")]
		public virtual bool IsArchive {
			get => isArchive;
			set => SetField(ref isArchive, value);
		}

		#region Для водил и экспедиторов

		decimal wageCalcRate;
		[Display(Name = "Ставка для расчёта зарплаты")]
		public virtual decimal WageCalcRate {
			get => wageCalcRate;
			set {
				if(WageCalcType == WageCalculationType.percentage)
					value = value > 100 ? 100 : value;
				SetField(ref wageCalcRate, value);
			}
		}

		#endregion Для водил и экспедиторов

		#region для офисных

		int quantityOfFullBottlesToSell;
		[Display(Name = "План по продажам")]
		public virtual int QuantityOfFullBottlesToSell {
			get => quantityOfFullBottlesToSell;
			set => SetField(ref quantityOfFullBottlesToSell, value);
		}

		int quantityOfEmptyBottlesToTake;
		[Display(Name = "План по забору")]
		public virtual int QuantityOfEmptyBottlesToTake {
			get => quantityOfEmptyBottlesToTake;
			set => SetField(ref quantityOfEmptyBottlesToTake, value);
		}

		#endregion для офисных

		#endregion Свойства

		#region Calculating

		public virtual string Title {
			get {
				var title = WageCalcType.GetEnumTitle();
				switch(WageCalcType) {
					case WageCalculationType.normal:
					case WageCalculationType.withoutPayment:
					case WageCalculationType.percentageForService:
						return title;
					case WageCalculationType.percentage:
						return string.Format("{0} - {1}%", title, WageCalcRate);
					case WageCalculationType.fixedRoute:
					case WageCalculationType.fixedDay:
						return string.Format("{0} - {1}", title, WageCalcRate.ToShortCurrencyString());
					case WageCalculationType.salesPlan:
						return string.Format(
							"{0} (продажа - {1} бут., забор - {2} бут.)",
							title,
							QuantityOfFullBottlesToSell,
							QuantityOfEmptyBottlesToTake
						);
					default:
						return string.Format("Неизвестный параметр №{0}", Id);
				}
			}
		}

		public static WageCalculationType[] WageCalculationTypesForEmployeeCategory(EmployeeCategory? eCategory = null)
		{
			if(!eCategory.HasValue)
				return Enum.GetValues(typeof(WageCalculationType))
						   .OfType<WageCalculationType>()
						   .ToArray();

			switch(eCategory) {
				case EmployeeCategory.office:
					return new WageCalculationType[] {
						WageCalculationType.normal,
						WageCalculationType.salesPlan
					};
				case EmployeeCategory.driver:
					return Enum.GetValues(typeof(WageCalculationType))
							   .OfType<WageCalculationType>()
							   .Where(e => e != WageCalculationType.salesPlan)
							   .ToArray();
				case EmployeeCategory.forwarder:
					return Enum.GetValues(typeof(WageCalculationType))
							   .OfType<WageCalculationType>()
							   .Where(e => e != WageCalculationType.salesPlan)
							   .ToArray();
				default:
					return new WageCalculationType[] {
						WageCalculationType.normal
					};
			}
		}

		public static WageCalculationType[] WageCalculationTypesWithRates {
			get {
				return new[] {
					WageCalculationType.fixedDay,
					WageCalculationType.fixedRoute,
					WageCalculationType.percentage
				};
			}
		}

		#endregion Calculating

		#region Validation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(WageCalculationTypesWithRates.Contains(WageCalcType) && WageCalcRate <= 0)
				yield return new ValidationResult(
					"Укажите ставку",
					new[] {
						this.GetPropertyName(o => o.WageCalcType),
						this.GetPropertyName(o => o.WageCalcRate)
					}
				);
			if(WageCalcType == WageCalculationType.salesPlan && (QuantityOfFullBottlesToSell <= 0 || QuantityOfEmptyBottlesToTake <= 0))
				yield return new ValidationResult(
					"Укажите планируемое количество на продажу и забор",
					new[] {
						this.GetPropertyName(o => o.WageCalcType),
						this.GetPropertyName(o => o.QuantityOfFullBottlesToSell),
						this.GetPropertyName(o => o.QuantityOfEmptyBottlesToTake)
					}
				);
		}

		#endregion Validation

	}
}
