namespace Vodovoz.Core.Domain.Interfaces.Sale
{
	/// <summary>
	/// Данные по скидке
	/// </summary>
	public interface IDiscountAmount
	{
		/// <summary>
		/// Идентификатор основания скидки
		/// </summary>
		int Id { get; }
		/// <summary>
		/// Название скидки
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Значение скидки в деньгах
		/// </summary>
		decimal Amount { get; }
	}
}
