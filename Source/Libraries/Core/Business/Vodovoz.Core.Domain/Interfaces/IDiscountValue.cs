namespace Vodovoz.Core.Domain.Interfaces
{
	/// <summary>
	/// Значение скидки
	/// </summary>
	public interface IDiscountValue
	{
		/// <summary>
		/// Скидка в деньгах
		/// </summary>
		bool IsDiscountMoney { get; }
		/// <summary>
		/// Скидка в процентах
		/// </summary>
		decimal Discount { get; }
		/// <summary>
		/// Скидка в денежном эквиваленте
		/// </summary>
		decimal DiscountMoney { get; }
		/// <summary>
		/// Нулевая скидка
		/// </summary>
		bool IsZeroDiscount { get; }
		/// <summary>
		/// Получение значения скидки.
		/// Если IsDiscountMoney - false, то возвращаем Discount, иначе DiscountMoney
		/// </summary>
		decimal GetDiscount { get; }
		/// <summary>
		/// Добавление значения скидки к текущему
		/// </summary>
		/// <param name="discountValue">Добавляемое значение скидки</param>
		void AddDiscountValue(IDiscountValue discountValue);
		/// <summary>
		/// Установка скидки
		/// Если isDiscountMoney - false, то устанавливаем Discount, иначе DiscountMoney
		/// </summary>
		/// <param name="discount">Скидка</param>
		/// <param name="isDiscountMoney">Скидка в деньгах</param>
		void SetDiscount(decimal discount, bool isDiscountMoney);
	}
}
