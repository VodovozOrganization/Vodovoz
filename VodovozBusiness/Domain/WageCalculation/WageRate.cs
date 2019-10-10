using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using QS.Utilities;
using QS.Utilities.Text;

namespace Vodovoz.Domain.WageCalculation
{
	[
		Appellative(
			Gender = GrammaticalGender.Feminine,
			NominativePlural = "ставки расчёта ЗП",
			Nominative = "ставка расчёта ЗП",
			Accusative = "ставки расчёта ЗП",
			Genitive = "ставки расчёта ЗП"
		)
	]
	[HistoryTrace]
	[EntityPermission]
	public class WageRate : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		WageDistrictLevelRate wageDistrictLevelRate;
		[Display(Name = "Набор ставок")]
		public virtual WageDistrictLevelRate WageDistrictLevelRate {
			get => wageDistrictLevelRate;
			set => SetField(ref wageDistrictLevelRate, value);
		}

		WageRateTypes wageRateTypes;
		[Display(Name = "Тип ставки")]
		public virtual WageRateTypes WageRateType {
			get => wageRateTypes;
			set => SetField(ref wageRateTypes, value);
		}

		decimal forDriverWithForwarder;
		[Display(Name = "Величина ставки при наличии экспедитора")]
		public virtual decimal ForDriverWithForwarder {
			get => forDriverWithForwarder;
			set => SetField(ref forDriverWithForwarder, value);
		}

		decimal forDriverWithoutForwarder;
		[Display(Name = "Величина ставки при отсутствии экспедитора")]
		public virtual decimal ForDriverWithoutForwarder {
			get => forDriverWithoutForwarder;
			set => SetField(ref forDriverWithoutForwarder, value);
		}

		private decimal forForwarder;
		[Display(Name = "Величина ставки для экспедитора")]
		public virtual decimal ForForwarder {
			get => forForwarder;
			set => SetField(ref forForwarder, value, () => ForForwarder);
		}

		#endregion Свойства

		public WageRate()
		{
		}

		public WageRate(WageRateTypes wageRateType, decimal forDriverWithForwarder, decimal forDriverWithoutForwarder, decimal forForwarder)
		{
			WageRateType = wageRateType;
			ForDriverWithForwarder = forDriverWithForwarder;
			ForDriverWithoutForwarder = forDriverWithoutForwarder;
			ForForwarder = forForwarder;
		}

		#region Вычисляемые

		public virtual string Title => $"{GetType().GetSubjectName().StringToTitleCase()} №{Id}";

		public virtual string GetUnitName {
			get {
				switch(WageRateType) {
					case WageRateTypes.PhoneCompensation:
					case WageRateTypes.Bottle19L:
					case WageRateTypes.EmptyBottle19L:
					case WageRateTypes.Bottle6L:
					case WageRateTypes.PackOfBottles600ml:
					case WageRateTypes.Equipment:
					case WageRateTypes.Address:
					case WageRateTypes.Bottle19LInBigOrder:
					case WageRateTypes.EmptyBottle19LInBigOrder:
					case WageRateTypes.ContractCancelation:
						return CurrencyWorks.CurrencyShortName;
					case WageRateTypes.MinBottlesQtyInBigOrder:
						return "шт.";
					default:
						return "ед.";
				}
			}
		}

		public virtual string GetForDriverWithForwarderString {
			get {
				switch(WageRateType) {
					case WageRateTypes.PhoneCompensation:
					case WageRateTypes.Bottle19L:
					case WageRateTypes.EmptyBottle19L:
					case WageRateTypes.Bottle6L:
					case WageRateTypes.PackOfBottles600ml:
					case WageRateTypes.Equipment:
					case WageRateTypes.Address:
					case WageRateTypes.Bottle19LInBigOrder:
					case WageRateTypes.EmptyBottle19LInBigOrder:
					case WageRateTypes.ContractCancelation:
						return ForDriverWithForwarder.ToShortCurrencyString();
					case WageRateTypes.MinBottlesQtyInBigOrder:
						return string.Format("{0} шт.", ForDriverWithForwarder);
					default:
						return string.Format("{0} ед.", ForDriverWithForwarder);
				}
			}
		}

		public virtual string GetForDriverWithoutForwarderString {
			get {
				switch(WageRateType) {
					case WageRateTypes.PhoneCompensation:
					case WageRateTypes.Bottle19L:
					case WageRateTypes.EmptyBottle19L:
					case WageRateTypes.Bottle6L:
					case WageRateTypes.PackOfBottles600ml:
					case WageRateTypes.Equipment:
					case WageRateTypes.Address:
					case WageRateTypes.Bottle19LInBigOrder:
					case WageRateTypes.EmptyBottle19LInBigOrder:
					case WageRateTypes.ContractCancelation:
						return ForDriverWithoutForwarder.ToShortCurrencyString();
					case WageRateTypes.MinBottlesQtyInBigOrder:
						return string.Format("{0} шт.", ForDriverWithoutForwarder);
					default:
						return string.Format("{0} ед.", ForDriverWithoutForwarder);
				}
			}
		}

		public virtual string GetForForwarderString {
			get {
				switch(WageRateType) {
					case WageRateTypes.PhoneCompensation:
					case WageRateTypes.Bottle19L:
					case WageRateTypes.EmptyBottle19L:
					case WageRateTypes.Bottle6L:
					case WageRateTypes.PackOfBottles600ml:
					case WageRateTypes.Equipment:
					case WageRateTypes.Address:
					case WageRateTypes.Bottle19LInBigOrder:
					case WageRateTypes.EmptyBottle19LInBigOrder:
					case WageRateTypes.ContractCancelation:
						return ForForwarder.ToShortCurrencyString();
					case WageRateTypes.MinBottlesQtyInBigOrder:
						return string.Format("{0} шт.", ForForwarder);
					default:
						return string.Format("{0} ед.", ForForwarder);
				}
			}
		}
		#endregion Вычисляемые
	}
}