using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.WageCalculation.AdvancedWageParameters
{
	[Appellative(
	Gender = GrammaticalGender.Masculine,
	NominativePlural = "дополнительные параметры расчёта зарплаты",
	Nominative = "дополнительный параметр расчёта зарплаты"
	)]
	public abstract class AdvancedWageParameter : PropertyChangedBase, IAdvancedWageParameter
	{
		public virtual int Id { get; set; }

		private AdvancedWageParameter parentParameter;
		public virtual AdvancedWageParameter ParentParameter {
			get => parentParameter;
			set { 
				SetField(ref parentParameter, value, () => ParentParameter);
				if(value != null)
					wageRate = null;
			}
		}

		private WageRate wageRate;
		[Display(Name = "Тип ставки для расчета зарплаты")]
		public virtual WageRate WageRate { //Устанавливается только у корневого элемента в иерархии дополнительных параметров
			get => wageRate;
			set {
				SetField(ref wageRate, value);
				if(value != null)
					parentParameter = null;
			}
		}

		private IList<IWageHierarchyNode> children;
		[Display(Name = "Вложенные параметры")]
		public virtual IList<IWageHierarchyNode> Children {
			get => children;
			set => SetField(ref children, value);
		}

		//Поле используется только для загрузки из базы списка дополнительных параметров 
		public virtual IList<AdvancedWageParameter> ChildrenParameters {
			get => Children?.OfType<AdvancedWageParameter>()?.ToList();
			set { Children = value?.OfType<IWageHierarchyNode>()?.ToList(); }
		}

		private decimal forDriverWithForwarder;
		[Display(Name = "Величина ставки при наличии экспедитора")]
		public virtual decimal ForDriverWithForwarder {
			get => forDriverWithForwarder;
			set => SetField(ref forDriverWithForwarder, value);
		}

		private decimal forDriverWithoutForwarder;
		[Display(Name = "Величина ставки при отсутствии экспедитора")]
		public virtual decimal ForDriverWithoutForwarder {
			get => forDriverWithoutForwarder;
			set => SetField(ref forDriverWithoutForwarder, value);
		}

		private decimal forForwarder;
		[Display(Name = "Величина ставки для экспедитора")]
		public virtual decimal ForForwarder {
			get => forForwarder;
			set => SetField(ref forForwarder, value);
		}

		public abstract AdvancedWageParameterType AdvancedWageParameterType { get; set; }

		IAdvancedWageParameter IAdvancedWageParameter.ParentParameter => ParentParameter;

		public virtual IWageHierarchyNode Parent {
			get => WageRate as IWageHierarchyNode ?? ParentParameter as IWageHierarchyNode;
			set {

				if(value is WageRate wageRate)
					WageRate = wageRate;
				if(value is AdvancedWageParameter parameter)
					ParentParameter = parameter;
				if(value == null) {
					ParentParameter = null;
					wageRate = null;
				}
			}
		}

		public abstract string Name { get; }


		public virtual string GetUnitName
		{
			get {
				if(WageRate != null)
					return WageRate.GetUnitName;

				var wageParam = this;
				while(wageParam.ParentParameter != null)
					wageParam = wageParam.ParentParameter;

				return wageParam?.WageRate?.GetUnitName ?? string.Empty;
			}
		}

		public abstract bool HasConflicWith(IAdvancedWageParameter advancedWageParameter);

		public abstract override string ToString();
	}
}
