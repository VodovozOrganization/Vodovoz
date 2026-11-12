namespace VodovozBusiness.Domain.Orders
{
	public interface IDiscountNameAndAmount
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

	public class DiscountNameAndAmount : IDiscountNameAndAmount
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public string Name { get; set; }

		/// <inheritdoc/>
		public decimal Amount { get; set; }

		public static DiscountNameAndAmount Create(int id, string name, decimal amount) =>
			new DiscountNameAndAmount
			{
				Id = id,
				Name = name,
				Amount = amount
			};
	}

	
}
