using System;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;
using System.Linq;
using QS.Project.Journal.EntitySelector;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using QS.Commands;
using Vodovoz.JournalViewModels.Employees;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.TempAdapters;
using Vodovoz.Infrastructure.Services;
using QS.DomainModel.Config;
using QS.Project.Journal;
using Vodovoz.ViewModels.Employees;
using QS.DomainModel.Entity;
using Vodovoz.EntityRepositories.Subdivisions;
using QS.Project.Services;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintViewModel : EntityTabViewModelBase<Complaint>
	{
		private readonly ICommonServices commonServices;
		private readonly IUndeliveriesViewOpener undeliveryViewOpener;
		private readonly IEmployeeService employeeService;
		private readonly IEntitySelectorFactory employeeSelectorFactory;
		private readonly IEntityConfigurationProvider entityConfigurationProvider;
		private readonly IFilePickerService filePickerService;
		private readonly ISubdivisionRepository subdivisionRepository;

		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory OrderSelectorFactory { get; }

		public ComplaintViewModel(
			IEntityConstructorParam ctorParam, 
			ICommonServices commonServices, 
			IUndeliveriesViewOpener undeliveryViewOpener,
			IEmployeeService employeeService,
			IEntitySelectorFactory employeeSelectorFactory,
			IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
			IEntityAutocompleteSelectorFactory orderSelectorFactory,
			IEntityConfigurationProvider entityConfigurationProvider,
			IFilePickerService filePickerService,
			ISubdivisionRepository subdivisionRepository
			) : base(ctorParam, commonServices)
		{
			OrderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			this.entityConfigurationProvider = entityConfigurationProvider ?? throw new ArgumentNullException(nameof(entityConfigurationProvider));
			this.filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));
			this.subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			CounterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.undeliveryViewOpener = undeliveryViewOpener ?? throw new ArgumentNullException(nameof(undeliveryViewOpener));
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.employeeSelectorFactory = employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory));
			Entity.ObservableComplaintDiscussions.ElementChanged += ObservableComplaintDiscussions_ElementChanged;
			Entity.ObservableFines.ListContentChanged += ObservableFines_ListContentChanged;

			if(ctorParam.IsNewEntity) {
				AbortOpening("Невозможно создать новую жалобу из текущего диалога, необходимо использовать диалоги создания");
			}

			ConfigureEntityChangingRelations();

			CreateCommands();
			TabName = $"Жалоба №{Entity.Id} от {Entity.CreationDate.ToShortDateString()}";
		}

		protected void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.ComplaintType,
				() => IsInnerComplaint,
				() => IsClientComplaint
			);

			SetPropertyChangeRelation(e => e.Status,
				() => Status
			);
		}

		void ObservableComplaintDiscussions_ElementChanged(object aList, int[] aIdx)
		{
			OnPropertyChanged(() => SubdivisionsInWork);
			Entity.UpdateComplaintStatus();
		}

		void ObservableFines_ListContentChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(() => FineItems);
		}

		public virtual ComplaintStatuses Status {
			get => Entity.Status;
			set {
				Entity.Status = value;
				Entity.ActualCompletionDate = DateTime.Now;
			}
		}

		private ComplaintDiscussionsViewModel discussionsViewModel;
		public ComplaintDiscussionsViewModel DiscussionsViewModel {
			get {
				if(discussionsViewModel == null) {
					discussionsViewModel = new ComplaintDiscussionsViewModel(Entity, this, UoW, entityConfigurationProvider, filePickerService, CommonServices);
				}
				return discussionsViewModel;
			}
		}

		private GuiltyItemsViewModel guiltyItemsViewModel;
		public GuiltyItemsViewModel GuiltyItemsViewModel {
			get {
				if(guiltyItemsViewModel == null) {
					guiltyItemsViewModel = new GuiltyItemsViewModel(Entity, UoW, CommonServices, subdivisionRepository);
				}

				return guiltyItemsViewModel;
			}
		}


		private FilesViewModel filesViewModel;
		public FilesViewModel FilesViewModel {
			get {
				if(filesViewModel == null) {
					filesViewModel = new FilesViewModel(CommonServices.InteractiveService, filePickerService, UoW);
				}

				return filesViewModel;
			}
		}

		public string SubdivisionsInWork {
			get {
				string inWork = string.Join(", ", Entity.ComplaintDiscussions.Where(x => x.Status == ComplaintStatuses.InProcess).Select(x => x.Subdivision.ShortName));
				string okk = Entity.ComplaintDiscussions.Any(x => x.Status == ComplaintStatuses.Checking) ? "OKK" : null;
				string result;
				if(!string.IsNullOrWhiteSpace(inWork) && !string.IsNullOrWhiteSpace(okk)) {
					result = string.Join(", ", inWork, okk);
				} else if(!string.IsNullOrWhiteSpace(inWork)) {
					result = inWork;
				} else if(!string.IsNullOrWhiteSpace(okk)) {
					result = okk;
				} else {
					return "";
				}
				return $"В работе у: {result}";
			}
		}

		private List<ComplaintSource> complaintSources;
		public IEnumerable<ComplaintSource> ComplaintSources {
			get {
				if(complaintSources == null) {
					complaintSources = UoW.GetAll<ComplaintSource>().ToList();
				}
				return complaintSources;
			}
		}

		private List<ComplaintResult> complaintResults;
		public IEnumerable<ComplaintResult> ComplaintResults {
			get {
				if(complaintResults == null) {
					complaintResults = UoW.GetAll<ComplaintResult>().ToList();
				}
				return complaintResults;
			}
		}

		public IList<FineItem> FineItems => Entity.Fines.SelectMany(x => x.Items).ToList();

		public bool IsInnerComplaint => Entity.ComplaintType == ComplaintType.Inner;

		public bool IsClientComplaint => Entity.ComplaintType == ComplaintType.Client;

		[PropertyChangedAlso(nameof(CanAddFine), nameof(CanAttachFine))]
		public bool CanEdit => PermissionResult.CanUpdate;

		public bool CanAddFine => CanEdit;
		public bool CanAttachFine => CanEdit;


		#region Commands

		private void CreateCommands()
		{
			CreateAttachFineCommand();
			CreateAddFineCommand();
		}

		#region AttachFineCommand

		public DelegateCommand AttachFineCommand { get; private set; }

		private void CreateAttachFineCommand()
		{
			AttachFineCommand = new DelegateCommand(
				() => {
					var fineFilter = new FineFilterViewModel(commonServices.InteractiveService);
					fineFilter.ExcludedIds = Entity.Fines.Select(x => x.Id).ToArray();
					var fineJournalViewModel = new FinesJournalViewModel(
						fineFilter,
						undeliveryViewOpener,
						employeeService,
						employeeSelectorFactory,
						entityConfigurationProvider,
						CommonServices
					);
					fineJournalViewModel.SelectionMode = JournalSelectionMode.Single;
					fineJournalViewModel.OnEntitySelectedResult += (sender, e) => {
						var selectedNode = e.SelectedNodes.FirstOrDefault();
						if(selectedNode == null) {
							return;
						}
						Entity.AddFine(UoW.GetById<Fine>(selectedNode.Id));
					};
					TabParent.AddSlaveTab(this, fineJournalViewModel);
				},
				() => CanAttachFine
			);
			AttachFineCommand.CanExecuteChangedWith(this, x => CanAttachFine);
		}

		#endregion AttachFineCommand

		#region AddFineCommand

		public DelegateCommand AddFineCommand { get; private set; }

		private void CreateAddFineCommand()
		{
			AddFineCommand = new DelegateCommand(
				() => {
					FineViewModel fineViewModel = new FineViewModel(
						EntityConstructorParam.ForCreate(),
						undeliveryViewOpener,
						employeeService,
						employeeSelectorFactory,
						entityConfigurationProvider,
						CommonServices
					);
					fineViewModel.EntitySaved += (sender, e) => {
						Entity.AddFine(e.Entity as Fine);
					};
					TabParent.AddSlaveTab(this, fineViewModel);
				},
				() => CanAddFine
			);
			AddFineCommand.CanExecuteChangedWith(this, x => CanAddFine);
		}

		#endregion AddFineCommand

		#endregion Commands
	}
}
