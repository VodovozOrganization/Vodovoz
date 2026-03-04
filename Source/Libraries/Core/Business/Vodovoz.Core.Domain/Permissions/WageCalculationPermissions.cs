using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static class WageCalculationPermissions
	{
		/// <summary>
		/// Работа со справочником Коэффициенты мотивации КЦ
		/// </summary>
		[Display(
			Name = "Работа со справочником Коэффициенты мотивации КЦ",
			Description = "Пользователь имеет доступ к справочнику Коэффициенты мотивации КЦ")]
		public static string CanEditCallCenterMotivationCoefficient => nameof(CanEditCallCenterMotivationCoefficient);
	}
}
