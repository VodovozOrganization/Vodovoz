namespace Vodovoz.EntityRepositories.Permissions
{
	public class EntityExtendedPermission
	{
		private bool? _canRead;
		private bool? _canCreate;
		private bool? _canUpdate;
		private bool? _canDelete;

		public bool? CanRead
		{
			get => _canRead;
			set
			{
				_canRead = value;
				Initialized = true;
			}
		}

		public bool? CanCreate
		{
			get => _canCreate;
			set
			{
				_canCreate = value;
				Initialized = true;
			}
		}

		public bool? CanUpdate
		{
			get => _canUpdate;
			set
			{
				_canUpdate = value;
				Initialized = true;
			}
		}

		public bool? CanDelete
		{
			get => _canDelete;
			set
			{
				_canDelete = value;
				Initialized = true;
			}
		}

		public bool? ExtendedPermissionValue { get; set; }

		public bool HasPermission => CanRead.HasValue && CanCreate.HasValue && CanUpdate.HasValue && CanDelete.HasValue;

		public bool Initialized { get; set; }
	}
}
