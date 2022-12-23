﻿using System.Text.Json.Serialization;

namespace VodovozMangoService.DTO
{
    public class ToCaller
    {
		[JsonPropertyName("extension")]
		public string Extension { get; set; }

		[JsonPropertyName("number")]
		public string Number { get; set; }

		[JsonPropertyName("line_number")]
		public string LineNumber { get; set; }

		[JsonPropertyName("acd_group")]
		public string AcdGroup { get; set; }

		[JsonPropertyName("was_transfered")]
		public bool WasTransfered { get; set; } = false;

		[JsonPropertyName("hold_initiator")]
		public string HoldInitiator { get; set; }

		#region Calculated
		public uint? ExtensionUint => uint.TryParse (Extension, out var i) ? (uint?) i : null;
        #endregion
    }
}
