using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DeliveryRulesService.V2.DTO
{
	/// <summary>
	/// Ответ по правилам доставки
	/// </summary>
	public class DeliveryInfoDTO
	{
		private DeliveryRulesResponseStatus statusEnum;
		/// <summary>
		/// Статус
		/// </summary>
		[JsonIgnore]
		public DeliveryRulesResponseStatus StatusEnum
		{
			get => statusEnum;
			set
			{
				statusEnum = value;
				Status = statusEnum.ToString();
			}
		}

		/// <summary>
		/// Статус в строковом варианте
		/// </summary>
		[JsonPropertyOrder(2)]
		public string Status { get; set; }
		
		/// <summary>
		/// Сообщение
		/// </summary>
		[JsonPropertyOrder(1)]
		public string Message { get; set; }

		/// <summary>
		/// Гео группа
		/// </summary>
		[JsonPropertyOrder(0)]
		public string GeoGroup { get; set; }

		/// <summary>
		/// Данные по правилам на день
		/// </summary>
		[JsonPropertyOrder(3)]
		public IList<WeekDayDeliveryInfoDTO> WeekDayDeliveryInfos { get; set; }
	}
}
