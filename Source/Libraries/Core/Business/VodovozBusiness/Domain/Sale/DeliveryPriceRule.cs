using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Parameters;

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
	[EntityPermission]
	public class DeliveryPriceRule : BusinessObjectBase<DeliveryPriceRule>, IDomainObject, IDeliveryPriceRule, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		public virtual string Title => $"{this}";
		
		private int water19LCount;
		[Display(Name = "Количество 19л бутылей в заказе")]
		public virtual int Water19LCount {
			get => water19LCount;
			set => SetField(ref water19LCount, value);
		}
		
		decimal orderMinSumEShopGoods;
		[Display(Name = "Минимальная сумма товаров ИМ")]
		public virtual decimal OrderMinSumEShopGoods {
			get => orderMinSumEShopGoods;
			set => SetField(ref orderMinSumEShopGoods, value);
		}

		private int water6LCount;
		[Display(Name = "Количество 6л бутылей в заказе")]
		public virtual int Water6LCount
		{
			get => water6LCount;
			set => SetField(ref water6LCount, value);
		}

		private int water1500mlCount;
		[Display(Name = "Количество 1,5л бутылей в заказе")]
		public virtual int Water1500mlCount
		{
			get => water1500mlCount;
			set => SetField(ref water1500mlCount, value);
		}

		private int water600mlCount;
		[Display(Name = "Количество 0,6л бутылей в заказе")]
		public virtual int Water600mlCount
		{
			get => water600mlCount;
			set => SetField(ref water600mlCount, value);
		}

		private int water500mlCount;
		[Display(Name = "Количество 0,5л бутылей в заказе")]
		public virtual int Water500mlCount
		{
			get => water500mlCount;
			set => SetField(ref water500mlCount, value);
		}

		private string ruleName;
		[Display(Name = "Название или краткое описание правила")]
		public virtual string RuleName
		{
			get => ruleName;
			set => SetField(ref ruleName, value);
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!(validationContext.ServiceContainer.GetService(
				typeof(IDistrictRuleRepository)) is IDistrictRuleRepository districtRuleRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий { nameof(districtRuleRepository) }");
			}
			
			if(districtRuleRepository.GetAllDeliveryPriceRules(UoW).Where(r => r.Id != Id).Contains(this))
			{
				yield return new ValidationResult("Такое правило уже существует и нельзя его создавать");
			}
		}

		#endregion

		#region переопределённые методы

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if(Water19LCount > 0 
				|| Water6LCount > 0 
				|| Water1500mlCount > 0 
				|| Water600mlCount > 0 
				|| Water500mlCount > 0) 
			{
				sb.Append("Если");
				sb.Append($" 19л б. < {Water19LCount}шт.");
				sb.Append($" или 6л б. < {water6LCount}шт.");
				sb.Append($" или 1500мл б. < {water1500mlCount}шт.");
				sb.Append($" или 500мл б. < {Water500mlCount}шт.");
			}

			return sb.ToString().Trim(' ', ',', 'и');
		}

		public override bool Equals(object obj)
		{
			if(obj == null || this.GetType() != obj.GetType())
				return false;

			DeliveryPriceRule rule = (DeliveryPriceRule)obj;
			bool result = this.Water19LCount == rule.Water19LCount
				&& this.Water6LCount == rule.Water6LCount
				&& this.Water1500mlCount == rule.Water1500mlCount
				&& this.Water600mlCount == rule.Water600mlCount
				&& this.Water500mlCount == rule.Water500mlCount
				&& this.OrderMinSumEShopGoods == rule.OrderMinSumEShopGoods;
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
			return 31 * Water19LCount.GetHashCode() 
				+ 31 * Water6LCount.GetHashCode() 
				+ 31 * Water1500mlCount.GetHashCode() 
				+ 31 * Water600mlCount.GetHashCode() 
				+ 31 * water500mlCount.GetHashCode();
		}

		#endregion
	}
}
