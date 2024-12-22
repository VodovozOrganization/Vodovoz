using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Защита на кране с горячей водой
	/// </summary>
	public enum ProtectionOnHotWaterTap
	{
		/// <summary>
		/// Защита от детей
		/// </summary>
		[Display(Name = "Защита от детей")]
		BabyProtect,
		/// <summary>
		/// Есть
		/// </summary>
		[Display(Name = "Есть")]
		WithProtect
	}
}
