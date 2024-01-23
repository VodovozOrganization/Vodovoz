using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Roboats;
using Vodovoz.Factories;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Dialogs.Roboats
{
	public class RoboatsWaterTypeViewModel : EntityTabViewModelBase<RoboatsWaterType>
	{
		private RoboatsEntityViewModel _roboatsEntityViewModel;
		private readonly INomenclatureJournalFactory _nomenclatureJournalFactory;
		private IEntityAutocompleteSelectorFactory _nomenclatureSelectorFactory;
		private readonly bool _canEdit;
		private readonly bool _canCreate;

		public RoboatsWaterTypeViewModel(IEntityUoWBuilder uowBuilder, INomenclatureJournalFactory nomenclatureJournalFactory, IRoboatsViewModelFactory roboatsViewModelFactory, IUnitOfWorkFactory uowFactory, ICommonServices commonServices) : base(uowBuilder, uowFactory, commonServices)
		{
			if(roboatsViewModelFactory is null)
			{
				throw new ArgumentNullException(nameof(roboatsViewModelFactory));
			}

			_nomenclatureJournalFactory = nomenclatureJournalFactory ?? throw new ArgumentNullException(nameof(nomenclatureJournalFactory));

			RoboatsEntityViewModel = roboatsViewModelFactory.CreateViewModel(Entity);

			NomenclatureSelectorFactory = _nomenclatureJournalFactory.GetRoboatsWaterJournalFactory();

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

		public virtual IEntityAutocompleteSelectorFactory NomenclatureSelectorFactory
		{
			get => _nomenclatureSelectorFactory;
			set => SetField(ref _nomenclatureSelectorFactory, value);
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
