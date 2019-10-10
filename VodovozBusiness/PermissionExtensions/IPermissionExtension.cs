using System;
using QS.Project.Domain;

namespace Vodovoz.Core.Permissions
{
	public interface IPermissionExtension
	{
		string PermissionId { get; }

		string Name { get; }

		string Description { get; }

		bool IsValidType(TypeOfEntity typeOfEntity);
	}
}
