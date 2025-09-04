using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class StorePermissions
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

			/// <summary>
			/// Разрешение отгрузки самовывоза<br/>
			/// Пользователь может переводить заказ с самовывозом в статус на погрузку
			/// </summary>
			[Display(
				Name = "Разрешение отгрузки самовывоза",
				Description = "Пользователь может переводить заказ с самовывозом в статус на погрузку")]
			public static string CanLoadSelfDeliveryDocument => "allow_load_selfdelivery";

			/// <summary>
			/// Может редактировать данные перевозчика в документах перемещения ТМЦ
			/// </summary>
			public static string CanEditStoreMovementDocumentTransporterData => "can_edit_store_movement_document_transporter_data";

		}
	}
}
