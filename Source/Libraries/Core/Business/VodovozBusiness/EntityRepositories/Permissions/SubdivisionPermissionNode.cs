using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.Project.Domain;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.EntityRepositories.Permissions
{
	public class SubdivisionPermissionNode : IPermissionNode
	{
		public TypeOfEntity TypeOfEntity { get; set; }
		public EntitySubdivisionOnlyPermission EntitySubdivisionOnlyPermission { get; set; }
		public IList<EntitySubdivisionPermissionExtended> EntityPermissionExtended { get; set; }
		public EntityPermissionBase EntityPermission => EntitySubdivisionOnlyPermission;

		IList<EntityPermissionExtendedBase> IPermissionNode.EntityPermissionExtended
		{
			get => EntityPermissionExtended.OfType<EntityPermissionExtendedBase>().ToList();
			set => EntityPermissionExtended = value.OfType<EntitySubdivisionPermissionExtended>().ToList();
		}

		public void CopySettingsFromNodeExceptSubdivision(SubdivisionPermissionNode souceNode)
		{
			if(EntitySubdivisionOnlyPermission is null
				|| EntitySubdivisionOnlyPermission.Subdivision is null)
			{
				throw new InvalidOperationException(
					$"Свойство {nameof(EntitySubdivisionOnlyPermission)} должно быть определено и установлено подразделение");
			}

			EntitySubdivisionOnlyPermission.TypeOfEntity = souceNode.TypeOfEntity;
			EntitySubdivisionOnlyPermission.CanCreate = souceNode.EntitySubdivisionOnlyPermission.CanCreate;
			EntitySubdivisionOnlyPermission.CanDelete = souceNode.EntitySubdivisionOnlyPermission.CanDelete;
			EntitySubdivisionOnlyPermission.CanRead = souceNode.EntitySubdivisionOnlyPermission.CanRead;
			EntitySubdivisionOnlyPermission.CanUpdate = souceNode.EntitySubdivisionOnlyPermission.CanUpdate;

			TypeOfEntity = souceNode.TypeOfEntity;

			EntityPermissionExtended = new List<EntitySubdivisionPermissionExtended>();

			foreach(var item in souceNode.EntityPermissionExtended)
			{
				var nodePermissionExtended = new EntitySubdivisionPermissionExtended
				{
					Subdivision = souceNode.EntitySubdivisionOnlyPermission.Subdivision
				};

				nodePermissionExtended.CopySettingsFromPermissionExceptSubdivision(item);

				EntityPermissionExtended.Add(nodePermissionExtended);
			}
		}
	}
}
