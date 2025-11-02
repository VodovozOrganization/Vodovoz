using System;
using System.Collections.Generic;
using System.Linq;
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
		/// GTIN
		/// </summary>
		[JsonPropertyName("gtin")]
		public string Gtin { get; set; }

		/// <summary>
		/// Уровень вложенности типа упаковки упаковки
		/// </summary>
		[JsonPropertyName("packageType")]
		public string PackageType { get; set; }

		/// <summary>
		/// Тип упаковки
		/// </summary>
		[JsonPropertyName("generalPackageType")]
		public string GeneralPackageType { get; set; }

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

		/// <summary>
		/// Уровень упаковки
		/// </summary>
		[JsonPropertyName("status")]
		public string Status { get; set; }

		/// <summary>
		/// Дочерние КИ
		/// </summary>
		[JsonPropertyName("child")]
		public IEnumerable<string> Childs { get; set; } = Enumerable.Empty<string>();

		/// <summary>
		/// Родительский КИ
		/// </summary>
		[JsonPropertyName("parent")]
		public string Parent { get; set; }

		/// <summary>
		/// Дата производства
		/// </summary>
		[JsonPropertyName("producedDate")]
		public string ProducedDate { get; set; }

		/// <summary>
		/// Дата истечения срока годности
		/// </summary>
		[JsonPropertyName("expirationDate")]
		public string ExpirationDate { get; set; }
	}
}
