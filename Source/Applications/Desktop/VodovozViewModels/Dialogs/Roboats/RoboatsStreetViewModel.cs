using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Roboats;
using Vodovoz.Factories;

namespace Vodovoz.ViewModels.Dialogs.Roboats
{
	public class RoboatsStreetViewModel : EntityTabViewModelBase<RoboatsStreet>
	{
		private RoboatsEntityViewModel _roboatsEntityViewModel;
		private readonly IRoboatsViewModelFactory _roboatsViewModelFactory;
		private readonly bool _canEdit;
		private readonly bool _canCreate;

		public RoboatsStreetViewModel(IEntityUoWBuilder uowBuilder, IRoboatsViewModelFactory roboatsViewModelFactory, IUnitOfWorkFactory uowFactory, ICommonServices commonServices) : base(uowBuilder, uowFactory, commonServices)
		{
			_roboatsViewModelFactory = roboatsViewModelFactory ?? throw new ArgumentNullException(nameof(roboatsViewModelFactory));

			RoboatsEntityViewModel = _roboatsViewModelFactory.CreateViewModel(Entity);

			var permissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(RoboatsStreet));
			_canEdit = permissionResult.CanUpdate;
			_canCreate = permissionResult.CanCreate;
		}

		public bool CanEdit => _canEdit || _canCreate && UoW.IsNew;

		public RoboatsEntityViewModel RoboatsEntityViewModel
		{
			get => _roboatsEntityViewModel;
			set => SetField(ref _roboatsEntityViewModel, value);
		}
		protected override bool BeforeSave()
		{
			return RoboatsEntityViewModel.Save();
		}

		public void Cancel()
		{
			Close(false, CloseSource.Cancel);
		}

		public override void Close(bool askSave, CloseSource source)
		{
			if(TabParent == null)
			{
				OnTabClosed();
			}
			else
			{
				base.Close(askSave, source);
			}
		}
	}
}
