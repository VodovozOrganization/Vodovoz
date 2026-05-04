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
		public AdditionalConditionType Type { get; set; }
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
		public AdditionalConditionWidgetType WidgetType { get; set; }

		public static AdditionalCondition Create(
			AdditionalConditionType type,
			string name,
			AdditionalConditionWidgetType widgetType,
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
