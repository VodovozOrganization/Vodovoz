﻿using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Permissions
{
	public static class InventoryNomenclatureInstance
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
