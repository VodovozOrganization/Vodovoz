using QS.Project.Services.FileDialog;
using QS.Services;
using System;
using Vodovoz.Domain.Roboats;
using Vodovoz.ViewModels.Dialogs.Roboats;

namespace Vodovoz.Factories
{
	public class RoboatsViewModelFactory : IRoboatsViewModelFactory
	{
		private readonly IRoboatsFileStorageFactory _roboatsFileStorageFactory;
		private readonly IFileDialogService _fileDialogService;
		private readonly ICurrentPermissionService _currentPermissionService;

		public RoboatsViewModelFactory(IRoboatsFileStorageFactory roboatsFileStorageFactory, IFileDialogService fileDialogService, ICurrentPermissionService currentPermissionService)
		{
			_roboatsFileStorageFactory = roboatsFileStorageFactory ?? throw new ArgumentNullException(nameof(roboatsFileStorageFactory));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
		}
		public RoboatsEntityViewModel CreateViewModel(IRoboatsEntity entity)
		{
			return new RoboatsEntityViewModel(entity, _roboatsFileStorageFactory, _fileDialogService, _currentPermissionService);
		}
	}
}
