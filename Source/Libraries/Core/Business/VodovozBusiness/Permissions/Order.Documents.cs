using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Permissions
{
	public static partial class Order
	{
		public static class Documents
		{
			/// <summary>
			/// Изменение дополнительной информации в счете<br/>
			/// Пользователь имеет доступ для изменения дополнительной информации в счете
			/// </summary>
			[Display(
				Name = "Изменение дополнительной информации в счете",
				Description = "Пользователь имеет доступ для изменения дополнительной информации в счете")]
			public static string CanEditBillAdditionalInfo => nameof(CanEditBillAdditionalInfo);
		}
	}
}
