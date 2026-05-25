namespace CustomerOrders.Contracts.V5.AdditionalConditions
{
	/// <summary>
	/// Дополнительное условие
	/// </summary>
	public class AdditionalCondition
	{
		/// <summary>
		/// Тип
		/// </summary>
		public string Type { get; set; }
		/// <summary>
		/// Название
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Активность
		/// </summary>
		public bool IsActive { get; set; }
		/// <summary>
		/// Доступность к изменению
		/// </summary>
		public bool Editable { get; set; }
		/// <summary>
		/// Тип виджета
		/// </summary>
		public string WidgetType { get; set; }

		public static AdditionalCondition Create(
			string type,
			string name,
			string widgetType,
			bool isActive,
			bool editable)
		{
			return new AdditionalCondition
			{
				Type = type,
				Name = name,
				WidgetType = widgetType,
				IsActive = isActive,
				Editable = editable
			};
		}
	}
}
