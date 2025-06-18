namespace Vodovoz.Permissions
{
	public static partial class Documents
	{
		/// <summary>
		/// Права документы перемещения
		/// </summary>
		public static class MovementDocument
		{
			/// <summary>
			/// Может редактировать данные перевозчика в документах перемещения ТМЦ
			/// </summary>
			public static string CanEditStoreMovementDocumentTransporterData => "can_edit_store_movement_document_transporter_data";
		}
	}
}
