using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Roboats;
using Vodovoz.Factories;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

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

			NomenclatureViewModel = new CommonEEVMBuilderFactory<RoboatsWaterType>(this, Entity, UoW, NavigationManager)
				.ForProperty(rwt => rwt.Nomenclature)
				.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel, NomenclatureFilterViewModel>(filter =>
				{
					filter.RestrictCategory = NomenclatureCategory.water;
				})
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();
		}

		public bool CanEdit => _canEdit || _canCreate && UoW.IsNew;

		public RoboatsEntityViewModel RoboatsEntityViewModel
		{
			get => _roboatsEntityViewModel;
			set => SetField(ref _roboatsEntityViewModel, value);
		}

		public IEntityEntryViewModel NomenclatureViewModel { get; private set; }

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
