using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Extensions;

namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class OrderPermissions
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

			/// <summary>
			/// Ручная переотправка УПД<br/>
			/// Пользователь может вручную переотправлять УПД
			/// </summary>
			[Display(
				Name = "Ручная переотправка УПД",
				Description = "Пользователь может вручную переотправлять УПД")]
			public static string CanManuallyResendUpd => nameof(CanManuallyResendUpd).FromPascalCaseToSnakeCase();
		}
	}
}
