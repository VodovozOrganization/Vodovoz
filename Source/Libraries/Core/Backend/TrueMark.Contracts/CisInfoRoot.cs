using System.Text.Json.Serialization;

namespace TrueMark.Contracts
{
	/// <summary>
	/// Общедоступная информации о КИ
	/// </summary>
	public class CisInfoRoot
	{
		[JsonPropertyName("cisInfo")]
		public CisInfo CisInfo { get; set; }
	}
}
