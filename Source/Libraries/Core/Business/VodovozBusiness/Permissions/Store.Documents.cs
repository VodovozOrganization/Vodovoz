using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Permissions
{
	public static partial class Store
	{
		public static class Documents
		{
			/// <summary>
			/// Изменение информационнй строки в талоне погрузки<br/>
			/// Пользователь имеет доступ для изменения информационнй строки в талоне погрузки
			/// </summary>
			[Display(
				Name = "Изменение информационнй строки в талоне погрузки",
				Description = "Пользователь имеет доступ для изменения информационнй строки в талоне погрузки")]
			public static string CanEditCarLoadDocumentInfoString => nameof(CanEditCarLoadDocumentInfoString);
		}
	}
}
