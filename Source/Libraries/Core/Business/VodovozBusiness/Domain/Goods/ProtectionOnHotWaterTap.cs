using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	public enum ProtectionOnHotWaterTap
	{
		[Display(Name = "Защита от детей")]
		BabyProtect,
		[Display(Name = "Есть")]
		WithProtect
	}
}
