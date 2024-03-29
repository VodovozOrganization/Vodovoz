﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrueMarkApi.Library.Dto
{
	public class ProductInstancesInfo
	{
		[JsonPropertyName("instanceStatuses")]
		public IEnumerable<ProductInstanceStatus> InstanceStatuses { get; set; }

		[JsonPropertyName("errorMessage")]
		public string ErrorMessage { get; set; }
	}

	
}
