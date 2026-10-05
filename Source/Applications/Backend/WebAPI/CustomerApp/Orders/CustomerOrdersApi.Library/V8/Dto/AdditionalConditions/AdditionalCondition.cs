using System.Collections.Generic;

namespace CustomerOrdersApi.Library.V8.Dto.AdditionalConditions
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
		/// <summary>
		/// Параметры в виде ключ-значение
		/// </summary>
		public IEnumerable<AdditionalParameter> Parameters { get; set; }

		public static AdditionalCondition Create(
			string type,
			string name,
			string widgetType,
			bool isActive,
			bool editable,
			IEnumerable<AdditionalParameter> parameters = null
			)
		{
			return new AdditionalCondition
			{
				Type = type,
				Name = name,
				WidgetType = widgetType,
				IsActive = isActive,
				Editable = editable,
				Parameters = parameters
			};
		}

		/// <summary>
		/// Параметры
		/// </summary>
		public class AdditionalParameter
		{
			/// <summary>
			/// Значение
			/// </summary>
			public string Value { get; set; }
			/// <summary>
			/// Название
			/// </summary>
			public string Name { get; set; }

			public static AdditionalParameter Create(string value, string name)
			{
				return new AdditionalParameter
				{
					Value = value,
					Name = name
				};
			}
		}
	}
}
