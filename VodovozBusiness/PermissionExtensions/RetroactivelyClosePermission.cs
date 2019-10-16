using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Documents;
using System;

namespace Vodovoz.PermissionExtensions
{
	public class RetroactivelyClosePermission : IPermissionExtension
	{
		public string PermissionId { get => nameof(RetroactivelyClosePermission); }
		public string Name { get => "Изменение документа задним числом"; }
		public string Description { get => "Возможность изменять документы задним числом"; }

		public RetroactivelyClosePermission() {}

		public bool IsValidType(Type typeOfEntity)
		{
			if(typeOfEntity == null)
				return false;

			return ValidTypes().Contains(typeOfEntity.ToString());
		}
	 

		protected IEnumerable<string> ValidTypes()
		{
			yield return nameof(InventoryDocument);
		}
	}
}
