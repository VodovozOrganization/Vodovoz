using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Complaints;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Complaints
{
	public class GuiltyItemsViewModel : EntityWidgetViewModelBase<Complaint>
	{
		readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		readonly ICommonServices _commonServices;
		readonly IEntityAutocompleteSelectorFactory _employeeSelectorFactory;
		private readonly ISubdivisionParametersProvider _subdivisionParametersProvider;

		public GuiltyItemsViewModel(
			Complaint entity,
			IUnitOfWork uow,
			ICommonServices commonServices,
			ISubdivisionRepository subdivisionRepository,
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			bool isForSalesDepartment = false
		) : base(entity, commonServices)
		{
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_employeeSelectorFactory = _employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_commonServices = commonServices;
			_subdivisionParametersProvider = subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			CreateCommands();
		}

		GuiltyItemViewModel currentGuiltyVM;
		public virtual GuiltyItemViewModel CurrentGuiltyVM
		{
			get => currentGuiltyVM;
			set
			{
				SetField(ref currentGuiltyVM, value, () => CurrentGuiltyVM);
				OnPropertyChanged(nameof(CanAddGuilty));
			}
		}

		private bool canRemoveGuilty;
		public virtual bool CanRemoveGuilty {
			get => canRemoveGuilty;
			set => SetField(ref canRemoveGuilty, value, () => CanRemoveGuilty);
		}


		bool canEditGuilty;
		public bool CanEditGuilty {
			get => canEditGuilty;
			set => SetField(ref canEditGuilty, value, () => CanEditGuilty);
		}

        public bool CanAddGuilty => ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_add_guilty_in_complaints")
			&& CurrentGuiltyVM == null;

		void UpdateAcessibility()
		{
			CanEditGuilty = !CanAddGuilty;
		}

		void CreateItem()
		{
			CurrentGuiltyVM = new GuiltyItemViewModel(
				new ComplaintGuiltyItem(),
				_commonServices,
				_subdivisionRepository,
				_employeeJournalFactory,
				_subdivisionParametersProvider,
				UoW
			);
			UpdateAcessibility();
		}

		void ClearItem()
		{
			CurrentGuiltyVM = null;
			UpdateAcessibility();
		}

		#region Commands

		void CreateCommands()
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
