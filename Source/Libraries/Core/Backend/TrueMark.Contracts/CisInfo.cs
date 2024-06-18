using System.Text.Json.Serialization;

namespace TrueMark.Contracts
{
	/// <summary>
	/// Общедоступная информации о КИ
	/// </summary>
	public class CisInfo
	{
		/// <summary>
		/// КИ запрашиваемых потребительских/групповых/транспортных упаковок
		/// </summary>
		[JsonPropertyName("requestedCis")]
		public string RequestedCis { get; set; }

		/// <summary>
		/// Тип упаковки
		/// </summary>
		[JsonPropertyName("packageType")]
		public string PackageType { get; set; }

		/// <summary>
		/// Уровень упаковки
		/// </summary>
		[JsonPropertyName("status")]
		public string Status { get; set; }

		/// <summary>
		/// ИНН собственника товара
		/// </summary>
		[JsonPropertyName("ownerInn")]
		public string OwnerInn { get; set; }

		/// <summary>
		/// Наименование владельца товара
		/// </summary>
		[JsonPropertyName("ownerName")]
		public string OwnerName { get; set; }
	}
}
