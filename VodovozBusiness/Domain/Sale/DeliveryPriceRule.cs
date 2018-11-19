using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using QS.DomainModel.Entity;
using QSSupportLib;
using Vodovoz.Repositories.Sale;

namespace Vodovoz.Domain.Sale
{
	[
		Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "правила цен доставки",
			Nominative = "правило цены доставки",
			Prepositional = "правиле цены доставки",
			PrepositionalPlural = "правилах цен доставки"
		)
	]
	public class DeliveryPriceRule : BusinessObjectBase<DeliveryPriceRule>, IDomainObject, IDeliveryPriceRule, IValidatableObject
	{
		public DeliveryPriceRule(){}

		#region Свойства

		public virtual int Id { get; set; }

		int water19LCount;
		[Display(Name = "Количество 19л бутылей в заказе")]
		public virtual int Water19LCount {
			get => water19LCount;
			set {
				if(SetField(ref water19LCount, value, () => Water19LCount)) {
					OnPropertyChanged(() => Water6LCount);
					OnPropertyChanged(() => Water600mlCount);
				}
			}
		}

		[Display(Name = "Количество 6л бутылей на одну 19л бутыль")]
		public virtual int EqualsCount6LFor19L => int.Parse(MainSupport.BaseParameters.All["эквивалент_6л_на_1бутыль_19л"]);

		int water600mlCount;
		[Display(Name = "Количество 0,6л бутылей на одну 19л бутыль")]
		public virtual int EqualsCount600mlFor19L => int.Parse(MainSupport.BaseParameters.All["эквивалент_0,6л_на_1бутыль_19л"]);

		[Display(Name = "Количество 6л бутылей в заказе")]
		public virtual string Water6LCount => (water19LCount * EqualsCount6LFor19L).ToString();

		[Display(Name = "Количество 0,6л бутылей в заказе")]
		public virtual string Water600mlCount => (water19LCount * EqualsCount600mlFor19L).ToString();

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(ScheduleRestrictedDistrictRuleRepository.GetAllDeliveryPriceRules(UoW).Where(r => r.Id != Id).Contains(this))
				yield return new ValidationResult("Такое правило уже существует и нельзя его создавать");
		}

		#endregion

		#region переопределённые методы

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if(Water19LCount > 0) {
				sb.Append("Если");
				sb.Append(String.Format(" 19л б. < {0}шт.", Water19LCount));
				sb.Append(String.Format(" или 6л б. < {0}шт.", water19LCount * EqualsCount6LFor19L));
				sb.Append(String.Format(" или 600мл б. < {0}шт.", Water19LCount * EqualsCount600mlFor19L));
			}

			return sb.ToString().Trim(new []{' ', ',', 'и'});
		}

		public override bool Equals(object obj)
		{
			if(obj == null || this.GetType() != obj.GetType())
				return false;

			DeliveryPriceRule rule = (DeliveryPriceRule)obj;
			bool result = this.Water19LCount == rule.Water19LCount;
			return result;
		}

		public static bool operator ==(DeliveryPriceRule x, DeliveryPriceRule y)
		{
			return x.Equals(y);
		}

		public static bool operator !=(DeliveryPriceRule x, DeliveryPriceRule y)
		{
			return !(x == y);
		}

		public override int GetHashCode()
		{
			int result = 0;
			result += 31 * result + this.Water19LCount.GetHashCode();
			return result;
		}

		#endregion
	}
}