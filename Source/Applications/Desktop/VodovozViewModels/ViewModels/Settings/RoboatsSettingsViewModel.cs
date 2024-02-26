using System;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Settings.Roboats;

namespace Vodovoz.ViewModels.ViewModels.Settings
{
	public class RoboatsSettingsViewModel : WidgetViewModelBase
	{
		private readonly IRoboatsSettings _roboatsSettings;
		public RoboatsSettingsViewModel(IRoboatsSettings roboatsSettings, ICurrentPermissionService currentPermissionService)
		{
			if(currentPermissionService is null)
			{
				throw new ArgumentNullException(nameof(currentPermissionService));
			}
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));

			CanEdit = currentPermissionService.ValidatePresetPermission("can_manage_roboats");
		}

		public virtual bool CanEdit { get; set; }

		public virtual bool RoboatsServiceEnabled
		{
			get => _roboatsSettings.RoboatsEnabled;
			set
			{
				if(!CanEdit)
				{
					return;
				}
				_roboatsSettings.RoboatsEnabled = value;
			}
		}
	}
}
