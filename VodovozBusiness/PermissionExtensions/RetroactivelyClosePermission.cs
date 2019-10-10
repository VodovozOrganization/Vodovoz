using System.Collections.Generic;
using QS.Project.Domain;
using System.Linq;
using Vodovoz.Domain.Documents;

namespace Vodovoz.PermissionExtensions
{
	public class RetroactivelyClosePermission : IPermissionExtension
	{
		public string PermissionId { get => nameof(RetroactivelyClosePermission); }
		public string Name { get => "Изменение документа задним числом"; }
		public string Description { get => "Возможность изменять документы задним числом"; }

		public bool IsValidType(TypeOfEntity typeOfEntity)
		{
			if(typeOfEntity == null)
				return false;

			return ValidTypes().Contains(typeOfEntity.Type);
		}

		public IEnumerable<string> ValidTypes()
		{
			yield return nameof(InventoryDocument);
		}
	}
}
