using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace TrueMark.Contracts
{
	/// <summary>
	/// Статус экземпляров товаров
	/// </summary>
	public class ProductInstanceStatus
	{
		/// <summary>
		/// Код
		/// </summary>
		[JsonPropertyName("IdentificationCode")]
		public string IdentificationCode { get; set; }

		/// <summary>
		/// Идентификатор родительского группового кода
		/// </summary>
		[JsonPropertyName("parent")]
		public string ParentId { get; set; }

		/// <summary>
		/// Уровень вложенности типа упаковки упаковки
		/// </summary>
		[JsonPropertyName("packageType")]
		public PackageType? PackageType { get; set; }

		/// <summary>
		/// Тип упаковки
		/// </summary>
		[JsonPropertyName("generalPackageType")]
		public GeneralPackageType? GeneralPackageType { get; set; }

		/// <summary>
		/// Инн владельца товара
		/// </summary>
		[JsonPropertyName("ownerInn")]
		public string OwnerInn { get; set; }

		/// <summary>
		/// Название владельца
		/// </summary>
		[JsonPropertyName("ownerName")]
		public string OwnerName { get; set; }

		/// <summary>
		/// Статус
		/// </summary>
		[JsonPropertyName("status")]
		public ProductInstanceStatusEnum? Status { get; set; }

		/// <summary>
		/// Дочерние КИ
		/// </summary>
		[JsonPropertyName("child")]
		public IEnumerable<string> Childs { get; set; } = Enumerable.Empty<string>();

		/// <summary>
		/// GTIN
		/// </summary>
		[JsonPropertyName("gtin")]
		public string Gtin { get; set; }

		/// <summary>
		/// Дата производства
		/// </summary>
		[JsonPropertyName("producedDate")]
		public DateTime? ProducedDate { get; set; }

		/// <summary>
		/// Дата истечения срока годности
		/// </summary>
		[JsonPropertyName("expirationDate")]
		public DateTime? ExpirationDate { get; set; }
	}
}
