using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CustomerApp.Contracts.Sale.Templates
{
	/// <summary>
	/// Информация по возможным еженедельным повторениям 
	/// </summary>
	public class RepeatTemplateEveryWeeks
	{
		/// <summary>
		/// Список повторений по умолчанию
		/// </summary>
		public IEnumerable<int> Options { get; set; }
		/// <summary>
		/// Минимум для «Указать вручную»
		/// </summary>
		[JsonPropertyName("min")]
		public int ManualSettingsMin { get; set; }
		/// <summary>
		/// Максимум для «Указать вручную»
		/// </summary>
		[JsonPropertyName("max")]
		public int ManualSettingsMax { get; set; }
	}
}
