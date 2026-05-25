namespace CustomerOrders.Contracts.V5.Carts
{
	/// <summary>
	/// Метод оплаты
	/// </summary>
	public sealed class PaymentMethod
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Форма оплаты
		/// </summary>
		public string MethodType { get; set; }
		/// <summary>
		/// Доступность
		/// </summary>
		public bool Available { get; set; }

		public static PaymentMethod Create(
			int id,
			string methodType,
			bool available = true)
		{
			return new PaymentMethod
			{
				Id = id,
				MethodType = methodType,
				Available = available
			};
		}
	}
}
