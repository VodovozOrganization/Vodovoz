using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.Domain.Complaints;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Settings.Organizations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.ViewModels.Complaints
{
	public class GuiltyItemsViewModel : EntityWidgetViewModelBase<Complaint>
	{
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly DialogViewModelBase _container;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly ICommonServices _commonServices;
		private readonly IEntityAutocompleteSelectorFactory _employeeSelectorFactory;
		private readonly ISubdivisionSettings _subdivisionSettings;

		public GuiltyItemsViewModel(
			Complaint entity,
			IUnitOfWork uow,
			DialogViewModelBase container,
			ILifetimeScope lifetimeScope,
			ICommonServices commonServices,
			ISubdivisionRepository subdivisionRepository,
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionSettings subdivisionSettings,
			bool isForSalesDepartment = false
		) : base(entity, commonServices)
		{
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_employeeSelectorFactory = _employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_container = container ?? throw new ArgumentNullException(nameof(container));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_subdivisionSettings = subdivisionSettings ?? throw new ArgumentNullException(nameof(subdivisionSettings));
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			CreateCommands();
		}

		private GuiltyItemViewModel _currentGuiltyVM;
		public virtual GuiltyItemViewModel CurrentGuiltyVM
		{
			get => _currentGuiltyVM;
			set
			{
				SetField(ref _currentGuiltyVM, value);
				OnPropertyChanged(nameof(CanAddGuilty));
			}
		}

		private bool _canRemoveGuilty;
		public virtual bool CanRemoveGuilty {
			get => _canRemoveGuilty;
			set => SetField(ref _canRemoveGuilty, value);
		}

		private bool _canEditGuilty;
		public bool CanEditGuilty {
			get => _canEditGuilty;
			set => SetField(ref _canEditGuilty, value);
		}

        public bool CanAddGuilty => ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_add_guilty_in_complaints")
			&& CurrentGuiltyVM == null;

		private void UpdateAcessibility()
		{
			CanEditGuilty = !CanAddGuilty;
		}

		private void CreateItem()
		{
			CurrentGuiltyVM = new GuiltyItemViewModel(
				new ComplaintGuiltyItem(),
				_commonServices,
				_subdivisionRepository,
				_employeeJournalFactory,
				_subdivisionSettings,
				UoW
			);

			CurrentGuiltyVM.SubdivisionViewModel = new CommonEEVMBuilderFactory<ComplaintGuiltyItem>(_container, CurrentGuiltyVM.Entity, UoW, _container.NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Subdivision)
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel>()
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();

			UpdateAcessibility();
		}

		private void ClearItem()
		{
			CurrentGuiltyVM = null;
			UpdateAcessibility();
		}

		#region Commands

		private void CreateCommands()
		{
			CreateAddGuiltyCommand();
			CreateRemoveGuiltyCommand();
			CreateSaveGuiltyCommand();
			CreateCancelCommand();
		}

		#region AddGuiltyCommand

		public DelegateCommand AddGuiltyCommand { get; private set; }
		private void CreateAddGuiltyCommand()
		{
			AddGuiltyCommand = new DelegateCommand(
				CreateItem,
				() => CanAddGuilty
			);
		}

		#endregion AddGuiltyCommand

		#region SaveGuiltyCommand

		public DelegateCommand SaveGuiltyCommand { get; private set; }

		private void CreateSaveGuiltyCommand()
		{
			SaveGuiltyCommand = new DelegateCommand(
				() => {
					if(!CurrentGuiltyVM.Entity.Responsible.IsEmployeeResponsible)
						CurrentGuiltyVM.Entity.Employee = null;
					if(!CurrentGuiltyVM.Entity.Responsible.IsSubdivisionResponsible)
						CurrentGuiltyVM.Entity.Subdivision = null;
					CurrentGuiltyVM.Entity.Complaint = Entity;
					Entity.ObservableGuilties.Add(CurrentGuiltyVM.Entity);
					ClearItem();
				},
				() => CanEditGuilty
			);
		}

		#endregion SaveGuiltyCommand

		#region CancelCommand

		public DelegateCommand CancelCommand { get; private set; }

		private void CreateCancelCommand()
		{
			CancelCommand = new DelegateCommand(
				ClearItem,
				() => CanEditGuilty
			);
		}

		#endregion CancelCommand

		#region RemoveGuiltyCommand

		public DelegateCommand<ComplaintGuiltyItem> RemoveGuiltyCommand { get; private set; }
		private void CreateRemoveGuiltyCommand()
		{
			RemoveGuiltyCommand = new DelegateCommand<ComplaintGuiltyItem>(
				g => Entity.ObservableGuilties.Remove(g),
				g => CanRemoveGuilty
			);
			RemoveGuiltyCommand.CanExecuteChangedWith(this, x => x.CanRemoveGuilty);
		}

		#endregion RemoveGuiltyCommand

		#endregion Commands
	}
}
