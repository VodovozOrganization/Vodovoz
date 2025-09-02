namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class OrderPermissions
	{
		/// <summary>
		/// Права промонаборы
		/// </summary>
		public static class PromotionalSet
		{
			/// <summary>
			/// Право на изменение типа промонабора
			/// </summary>
			public static string CanChangeTypePromoSet => "can_change_the_type_of_promo_set";
		}
	}
}
