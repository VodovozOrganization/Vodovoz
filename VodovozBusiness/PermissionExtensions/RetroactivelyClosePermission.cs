using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Documents;
using System;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;

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
			return typeOfEntity.IsSubclassOf(typeof(Document));
		}
	}
}
