using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Factories;
using Vodovoz.ViewModels.Dialogs.Roboats;

namespace Vodovoz.ViewModels.Dialogs.Counterparties
{
	public class RoboAtsCounterpartyNameViewModel : EntityTabViewModelBase<RoboAtsCounterpartyName>
	{
		private RoboatsEntityViewModel _roboatsEntityViewModel;
		private readonly bool _canEdit;
		private readonly bool _canCreate;

		public RoboAtsCounterpartyNameViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, IRoboatsViewModelFactory roboatsViewModelFactory, ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			if(roboatsViewModelFactory is null)
			{
				throw new ArgumentNullException(nameof(roboatsViewModelFactory));
			}

			TabName = "Имя контрагента Roboats";

			RoboatsEntityViewModel = roboatsViewModelFactory.CreateViewModel(Entity);

			var permissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(RoboAtsCounterpartyName));
			_canEdit = permissionResult.CanUpdate;
			_canCreate = permissionResult.CanCreate;
		}

		public bool CanEdit => _canEdit || (_canCreate && UoW.IsNew);

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
