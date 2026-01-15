using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using QS.Utilities;
using QS.Utilities.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;

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
	public class WageRate : PropertyChangedBase, IDomainObject, IWageHierarchyNode
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
		[Display(Name = "Базовая величина ставки при наличии экспедитора")]
		public virtual decimal ForDriverWithForwarder {
			get => forDriverWithForwarder;
			set => SetField(ref forDriverWithForwarder, value);
		}

		decimal forDriverWithoutForwarder;
		[Display(Name = "Базовая величина ставки при отсутствии экспедитора")]
		public virtual decimal ForDriverWithoutForwarder {
			get => forDriverWithoutForwarder;
			set => SetField(ref forDriverWithoutForwarder, value);
		}

		private decimal forForwarder;
		[Display(Name = "Базовая величина ставки для экспедитора")]
		public virtual decimal ForForwarder {
			get => forForwarder;
			set => SetField(ref forForwarder, value, () => ForForwarder);
		}

		[Display(Name = "Величина ставки при наличии экспедитора(с учетом дополнительных параметров)")]
		public virtual decimal WageForDriverWithForwarder(IRouteListItemWageCalculationSource src) => GetWage(src).ForDriverWithForwarder;

		[Display(Name = "Величина ставки при отсутствии экспедитора(с учетом дополнительных параметров)")]
		public virtual decimal WageForDriverWithoutForwarder(IRouteListItemWageCalculationSource src) => GetWage(src).ForDriverWithoutForwarder;

		[Display(Name = "Величина ставки для экспедитора(с учетом дополнительных параметров)")]
		public virtual decimal WageForForwarder(IRouteListItemWageCalculationSource src) => GetWage(src).ForForwarder;

		public virtual IWageHierarchyNode Parent { get => null; set { } }

		//Для отображение в иерархическом списке
		public virtual IList<IWageHierarchyNode> Children {
			get => ChildrenParameters.OfType<IWageHierarchyNode>().ToList() ?? new List<IWageHierarchyNode>();
		}

		private IList<AdvancedWageParameter> childrenParameters;
		[Display(Name = "Дополнительные параметры расчета зп")]
		public virtual IList<AdvancedWageParameter> ChildrenParameters {
			get {
				if(childrenParameters == null)
					childrenParameters = new List<AdvancedWageParameter>();
				return childrenParameters;
			}
			set { SetField(ref childrenParameters, value);}
		}

		public virtual string Name => WageRateType.GetAttribute<DisplayAttribute>()?.Name ?? WageRateType.ToString();

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

		private RouteListWageNode GetWage(IRouteListItemWageCalculationSource src)
		{
			if(ChildrenParameters?.FirstOrDefault() == null)
				return new RouteListWageNode(ForDriverWithForwarder, forDriverWithoutForwarder, forForwarder);
			foreach(var item in ChildrenParameters) {
				var result = item.CalculateWage(src);
				if(result != null)
					return result;
			}
			return new RouteListWageNode(ForDriverWithForwarder, forDriverWithoutForwarder, forForwarder);
		}

		public virtual string Title => $"{GetType().GetSubjectName().StringToTitleCase()} №{Id}";

		public virtual string GetUnitName {
			get {
				switch(WageRateType) {
					case WageRateTypes.PhoneCompensation:
					case WageRateTypes.Bottle19L:
					case WageRateTypes.EmptyBottle19L:
					case WageRateTypes.Bottle6L:
					case WageRateTypes.PackOfBottles600ml:
					case WageRateTypes.Bottle1500ml:
					case WageRateTypes.Bottle500ml:
					case WageRateTypes.Equipment:
					case WageRateTypes.Address:
					case WageRateTypes.ForeignAddress:
					case WageRateTypes.Bottle19LInBigOrder:
					case WageRateTypes.EmptyBottle19LInBigOrder:
					case WageRateTypes.ContractCancelation:
					case WageRateTypes.FastDelivery:
					case WageRateTypes.FastDeliveryWithLate:
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
					case WageRateTypes.Bottle1500ml:
					case WageRateTypes.Bottle500ml:
					case WageRateTypes.Equipment:
					case WageRateTypes.Address:
					case WageRateTypes.ForeignAddress:
					case WageRateTypes.Bottle19LInBigOrder:
					case WageRateTypes.EmptyBottle19LInBigOrder:
					case WageRateTypes.ContractCancelation:
					case WageRateTypes.FastDelivery:
					case WageRateTypes.FastDeliveryWithLate:
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
					case WageRateTypes.Bottle1500ml:
					case WageRateTypes.Bottle500ml:
					case WageRateTypes.Equipment:
					case WageRateTypes.Address:
					case WageRateTypes.ForeignAddress:
					case WageRateTypes.Bottle19LInBigOrder:
					case WageRateTypes.EmptyBottle19LInBigOrder:
					case WageRateTypes.ContractCancelation:
					case WageRateTypes.FastDelivery:
					case WageRateTypes.FastDeliveryWithLate:
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
					case WageRateTypes.Bottle1500ml:
					case WageRateTypes.Bottle500ml:
					case WageRateTypes.Equipment:
					case WageRateTypes.Address:
					case WageRateTypes.ForeignAddress:
					case WageRateTypes.Bottle19LInBigOrder:
					case WageRateTypes.EmptyBottle19LInBigOrder:
					case WageRateTypes.ContractCancelation:
					case WageRateTypes.FastDelivery:
					case WageRateTypes.FastDeliveryWithLate:
						return ForForwarder.ToShortCurrencyString();
					case WageRateTypes.MinBottlesQtyInBigOrder:
						return string.Format("{0} шт.", ForForwarder);
					default:
						return string.Format("{0} ед.", ForForwarder);
				}
			}
		}

		#endregion Вычисляемые

		public virtual object Clone()
		{
			var wageRate = new WageRate
			{
				WageRateType = WageRateType,
				ForDriverWithForwarder = ForDriverWithForwarder,
				ForDriverWithoutForwarder = ForDriverWithoutForwarder,
				ForForwarder = ForForwarder
			};

			foreach(var child in Children)
			{
				if(child.Clone() is AdvancedWageParameter childParameter)
				{
					childParameter.WageRate = wageRate;
					wageRate.ChildrenParameters.Add(childParameter);
					continue;
				}

				throw new InvalidOperationException("Дочерний узел не является дополнительным параметром расчета зарплаты");
			}

			return wageRate;
		}
	}
}
