using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DeliveryRulesService.DTO
{
	public class DeliveryInfoDTO
	{
		private DeliveryRulesResponseStatus statusEnum;
		
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

		[JsonPropertyOrder(2)]
		public string Status { get; set; }

		[JsonPropertyOrder(1)]
		public string Message { get; set; }

		[JsonPropertyOrder(0)]
		public string GeoGroup { get; set; }

		[JsonPropertyOrder(3)]
		public IList<WeekDayDeliveryInfoDTO> WeekDayDeliveryInfos { get; set; }
	}
}
