using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	/// <summary>
	/// Права Экземпляр номенклатуры
	/// </summary>
	public static class InventoryNomenclatureInstancePermissions
	{
		/// <summary>
		/// Может снимать и устанавливать параметр б/у
		/// </summary>
		[Display(
			Name = "Редактирование параметра б/у",
			Description = "Пользователь может снимать и устанавливать параметр б/у")]
		public static string CanEditUsedParameter => "InventoryNomenclatureInstance.CanEditUsedParameter";
	}
}
