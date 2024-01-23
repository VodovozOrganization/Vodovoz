using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Roboats;
using Vodovoz.Factories;

namespace Vodovoz.ViewModels.Dialogs.Roboats
{
	public class RoboatsWaterTypeViewModel : EntityTabViewModelBase<RoboatsWaterType>
	{
		private RoboatsEntityViewModel _roboatsEntityViewModel;
		private IEntityAutocompleteSelectorFactory _nomenclatureSelectorFactory;
		private readonly bool _canEdit;
		private readonly bool _canCreate;

		public RoboatsWaterTypeViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IRoboatsViewModelFactory roboatsViewModelFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(roboatsViewModelFactory is null)
			{
				throw new ArgumentNullException(nameof(roboatsViewModelFactory));
			}

			RoboatsEntityViewModel = roboatsViewModelFactory.CreateViewModel(Entity);

			var permissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(RoboatsWaterType));
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
