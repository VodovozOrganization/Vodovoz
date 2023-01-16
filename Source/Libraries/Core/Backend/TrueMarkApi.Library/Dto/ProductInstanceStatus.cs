using System;
using System.Text.Json.Serialization;

namespace TrueMarkApi.Library.Dto
{
	public class ProductInstanceStatus
	{
		[JsonPropertyName("IdentificationCode")]
		public string IdentificationCode { get; set; }

		[JsonPropertyName("status")]
		public ProductInstanceStatusEnum Status { get; set; }
	}
}
