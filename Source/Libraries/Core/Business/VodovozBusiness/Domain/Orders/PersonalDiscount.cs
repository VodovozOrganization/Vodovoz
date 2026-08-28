using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;
using Vodovoz.Settings.Orders;

namespace VodovozBusiness.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "персональные скидки",
		Nominative = "персональная скидка",
		Prepositional = "персональной скидке",
		PrepositionalPlural = "персональных скидках"
	)]
	public class PersonalDiscount : PropertyChangedBase, IDomainObject
	{
		private readonly DiscountReason _discountReason;
		
		public PersonalDiscount() { }
		
		protected PersonalDiscount(
			DiscountReason discountReason,
			IDiscountReasonSettings discountReasonSettings)
		{
			if(discountReason is null)
			{
				throw new ArgumentNullException(nameof(discountReason), "Нельзя передавать пустое значение основания скидки для персональной скидки");
			}

			if(discountReason.Id != discountReasonSettings.PersonalDiscountReasonId)
			{
				throw new InvalidOperationException("В персональную скидку нельзя передавать основание скидки не Персональная скидка");
			}
			
			_discountReason = discountReason;
		}

		protected PersonalDiscount(PersonalDiscount copyingPersonalDiscount)
		{
			_discountReason = copyingPersonalDiscount.DiscountReason;
			DiscountValue = copyingPersonalDiscount.DiscountValue;
		}
		
		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Основание скидки
		/// </summary>
		[Display(Name = "Основание скидки")]
		public virtual DiscountReason DiscountReason => _discountReason;
		
		/// <summary>
		/// Параметры скидки <see cref="Vodovoz.Core.Domain.Interfaces.IDiscountValue"/>
		/// </summary>
		[Display(Name = "Параметры скидки")]
		public virtual DiscountValue DiscountValue { get; protected set; }

		/// <summary>
		/// Установка новых параметров скидки
		/// </summary>
		/// <param name="receivedDiscountValue">Новые параметры скидки <see cref="DiscountValue"/></param>
		public virtual void SetDiscount(IDiscountValue receivedDiscountValue)
		{
			DiscountValue = receivedDiscountValue as DiscountValue;
		}
		
		/// <summary>
		/// Установка нового значения скидки
		/// </summary>
		/// <param name="discount">Скидка</param>
		/// <param name="isDiscountMoney">Скидка в деньгах</param>
		public virtual void SetDiscount(decimal discount, bool isDiscountMoney)
		{
			DiscountValue.SetDiscount(discount, isDiscountMoney);
		}

		public static PersonalDiscount Create(DiscountReason personalDiscountReason, IDiscountReasonSettings discountReasonSettings) =>
			new PersonalDiscount(personalDiscountReason, discountReasonSettings);
		
		public static PersonalDiscount Copy(PersonalDiscount copyingPersonalDiscount) =>
			new PersonalDiscount(copyingPersonalDiscount);

		public override string ToString()
		{
			if(DiscountValue is null || DiscountReason is null)
			{
				return "Скидка не установлена";
			}
			
			var sb = new StringBuilder();
			
			var discount = Math.Round(DiscountValue.GetDiscount, 2);

			sb.Append(discount);
			sb.Append(DiscountValue.IsDiscountMoney
				? DiscountUnits.money.GetEnumDisplayName()
				: DiscountUnits.percent.GetEnumDisplayName());

			sb.Insert(0, ' ');
			sb.Insert(0, DiscountReason.Name);
			
			return sb.ToString();
		}
	}
}
