namespace CustomerOrders.Contracts.V5.Carts
{
	/// <summary>
	/// Данные по отзвону за
	/// </summary>
	public sealed class CallBeforeArrivalMinutesDto
	{
		protected CallBeforeArrivalMinutesDto() { }

		protected CallBeforeArrivalMinutesDto(string value, string name)
		{
			Value = value;
			Name = name;
		}
			
		/// <summary>
		/// Значение
		/// </summary>
		public string Value { get; }
		/// <summary>
		/// Название
		/// </summary>
		public string Name { get; }

		public static CallBeforeArrivalMinutesDto Create(string value, string name) => new CallBeforeArrivalMinutesDto(value, name);
	}
}
