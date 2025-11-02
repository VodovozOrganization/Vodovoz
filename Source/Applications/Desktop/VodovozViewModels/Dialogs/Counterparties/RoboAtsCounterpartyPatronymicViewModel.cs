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
	public class RoboAtsCounterpartyPatronymicViewModel : EntityTabViewModelBase<RoboAtsCounterpartyPatronymic>
	{
		private RoboatsEntityViewModel _roboatsEntityViewModel;
		private readonly bool _canEdit;
		private readonly bool _canCreate;

		public RoboAtsCounterpartyPatronymicViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, IRoboatsViewModelFactory roboatsViewModelFactory, ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			if(roboatsViewModelFactory is null)
			{
				throw new ArgumentNullException(nameof(roboatsViewModelFactory));
			}

			TabName = "Отчество контрагента Roboats";

			RoboatsEntityViewModel = roboatsViewModelFactory.CreateViewModel(Entity);

			var permissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(RoboAtsCounterpartyPatronymic));
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
