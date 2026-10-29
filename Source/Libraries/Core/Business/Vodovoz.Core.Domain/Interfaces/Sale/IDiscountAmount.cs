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
		/// <summary>
		/// Обновление названия и самой скидки
		/// </summary>
		/// <param name="name">Название скидки</param>
		/// <param name="amount">Скидка в деньгах</param>
		void Update(string name, decimal amount);
	}
}
