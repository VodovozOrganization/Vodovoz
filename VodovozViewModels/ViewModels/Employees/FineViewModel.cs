using System;
using System.Linq;
using QS.Commands;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure.Services;
using Vodovoz.TempAdapters;
using QS.Project.Journal;
using Vodovoz.Domain;
using QS.DomainModel.Entity;
using QS.Project.Journal.EntitySelector;
using Gamma.Utilities;
using Vodovoz.Domain.Logistic;
using QS.DomainModel.UoW;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Services;
using Vodovoz.Domain.Cash;
using QS.Dialog;

namespace Vodovoz.ViewModels.Employees
{
	public class FineViewModel : EntityTabViewModelBase<Fine>, IAskSaveOnCloseViewModel
	{
		private readonly IUnitOfWorkFactory uowFactory;
		private readonly IUndeliveredOrdersJournalOpener undeliveryViewOpener;
		private readonly IEmployeeService employeeService;
		private readonly IEntitySelectorFactory employeeSelectorFactory;
		private readonly IEmployeeSettings _employeeSettings;
		private readonly ICategoryRepository _categoryRepository;

		public FineViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			IUndeliveredOrdersJournalOpener undeliveryViewOpener,
			IEmployeeService employeeService,
			IEntitySelectorFactory employeeSelectorFactory,
			IEmployeeSettings employeeSettings,
			ICommonServices commonServices
		) : base(uowBuilder, uowFactory, commonServices)
		{
			this.uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			this.undeliveryViewOpener = undeliveryViewOpener ?? throw new ArgumentNullException(nameof(undeliveryViewOpener));
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.employeeSelectorFactory = employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory));
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			CreateCommands();
			ConfigureEntityPropertyChanges();
			UpdateEmployeeList();
		}

		private void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(e => e.FineType,
				() => IsFuelOverspendingFine,
				() => IsStandartFine
			);

			SetPropertyChangeRelation(e => e.RouteList,
				() => CanShowRequestRouteListMessage,
				() => DateEditable
			);

			OnEntityPropertyChanged(SetDefaultReason,
				x => x.FineType
			);

			OnEntityPropertyChanged(Entity.UpdateItems,
				x => x.FineType
			);

			Entity.ObservableItems.ListChanged += aList => OnPropertyChanged(() => CanEditFineType);
		}

		private Employee currentEmployee;
		public Employee CurrentEmployee {
			get {
				if(currentEmployee == null) {
					currentEmployee = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				}
				return currentEmployee;
			}
		}

		public FineTypes FineType
		{
			get => Entity.FineType;
			set
			{
				Entity.FineType = value;
				UpdateEmployeeList();
			}
		}

		public string FineReasonString
		{
			get => Entity.FineReasonString;
			set => Entity.FineReasonString = value;
		}

		public virtual RouteList RouteList
		{
			get => Entity.RouteList;
			set
			{
				Entity.RouteList = value;
				UpdateEmployeeList();
			}
		}
		
		public virtual UndeliveredOrder UndeliveredOrder
		{
			get => Entity.UndeliveredOrder;
			set => Entity.UndeliveredOrder = value;
		}

		public bool CanEdit => PermissionResult.CanUpdate || (PermissionResult.CanCreate && Entity.Id == 0);

		public bool DateEditable => UoW.IsNew && Entity.RouteList == null;

		#region IAskSaveOnCloseViewModel

		public bool AskSaveOnClose => CanEdit;

		#endregion

		[PropertyChangedAlso(nameof(CanShowRequestRouteListMessage))]
		public bool IsFuelOverspendingFine => Entity.FineType == FineTypes.FuelOverspending;

		public bool IsStandartFine => Entity.FineType == FineTypes.Standart;

		public bool CanShowRequestRouteListMessage => IsFuelOverspendingFine && Entity.RouteList == null;

		public bool CanEditFineType => CanEdit;

		private void SetDefaultReason()
		{
			Entity.FineReasonString = Entity.FineType.GetEnumTitle();
		}

		private void UpdateEmployeeList()
		{
			if(Entity.RouteList != null)
			{
				ClearItems(Entity.RouteList.Driver);
			}
			else
			{
				ClearItems();
			}
		}

		private void ClearItems(Employee driver = null)
		{
			FineItem item = null;
			if(driver != null)
			{
				item = Entity.ObservableItems.Where(x => x.Employee == driver).FirstOrDefault();
			}
			Entity.ObservableItems.Clear();
			if(driver != null)
			{
				if(item != null)
				{
					Entity.ObservableItems.Add(item);
				}
				else
				{
					Entity.AddItem(driver);
				}
			}
		}

		protected override void BeforeSave()
		{
			Entity.UpdateWageOperations(UoW);
			Entity.UpdateFuelOperations(UoW);
			base.BeforeSave();
		}

		public override bool Save(bool close)
		{
			if(Entity.Author == null)
			{
				Entity.Author = CurrentEmployee;
			}

			return base.Save(close);
		}		

		#region Commands

		private void CreateCommands()
		{
			CreateAttachFineCommand();
			CreateAddFineItemCommand();
			CreateDivideByAllCommand();
			CreateSelectReasonTemplateCommand();
			CreateDeleteFineItemCommand();
		}

		#region AttachFineCommand

		public DelegateCommand OpenUndeliveryCommand { get; private set; }

		private void CreateAttachFineCommand()
		{
			OpenUndeliveryCommand = new DelegateCommand(
				() => undeliveryViewOpener.OpenFromFine(this, Entity.UndeliveredOrder.OldOrder, Entity.UndeliveredOrder.OldOrder.DeliveryDate, Entity.UndeliveredOrder.UndeliveryStatus),
				() => true
			);
		}

		#endregion AttachFineCommand

		#region DivideByAllCommand

		public DelegateCommand DivideByAllCommand { get; private set; }

		private void CreateDivideByAllCommand()
		{
			DivideByAllCommand = new DelegateCommand(
				Entity.DivideAtAll,
				() => true
			);
		}

		#endregion DivideByAllCommand

		#region SelectReasonTemplateCommand

		public DelegateCommand SelectReasonTemplateCommand { get; private set; }

		private void CreateSelectReasonTemplateCommand()
		{
			SelectReasonTemplateCommand = new DelegateCommand(
				() => {
					var fineTemplatesJournalViewModel = new SimpleEntityJournalViewModel<FineTemplate, FineTemplateViewModel>(x => x.Reason,
						() => new FineTemplateViewModel(EntityUoWBuilder.ForCreate(), uowFactory, CommonServices),
						(node) => new FineTemplateViewModel(EntityUoWBuilder.ForOpen(node.Id), uowFactory, CommonServices),
						QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
						CommonServices
					);
					fineTemplatesJournalViewModel.SelectionMode = JournalSelectionMode.Single;
					fineTemplatesJournalViewModel.OnEntitySelectedResult += (sender, args) => {
						var selectedNode = args.SelectedNodes.FirstOrDefault();
						if(selectedNode == null || Entity.FineType == FineTypes.FuelOverspending) {
							return;
						}
						var selectedFineTemplate = UoW.GetById<FineTemplate>(selectedNode.Id);
						Entity.FineReasonString = selectedFineTemplate.Reason;
						Entity.TotalMoney = selectedFineTemplate.FineMoney;
					};
					TabParent.AddSlaveTab(this, fineTemplatesJournalViewModel);
				},
				() => true
			);
		}

		#endregion SelectReasonTemplateCommand

		#region AddFineItemCommand

		public DelegateCommand AddFineItemCommand { get; private set; }

		private void CreateAddFineItemCommand()
		{
			AddFineItemCommand = new DelegateCommand(
				() => {
					var employeeSelector = employeeSelectorFactory.CreateSelector();
					employeeSelector.OnEntitySelectedResult += (sender, e) => {
						var node = e.SelectedNodes.FirstOrDefault();
						if(node == null) {
							return;
						}

						Entity.AddItem(UoW.GetById<Employee>(node.Id));
					};
					TabParent.AddSlaveTab(this, employeeSelector);
				},
				() => Entity.RouteList == null && IsStandartFine
			);
			AddFineItemCommand.CanExecuteChangedWith(Entity, x => x.RouteList);
			AddFineItemCommand.CanExecuteChangedWith(this, x => x.IsStandartFine, x => x.CanEdit);
		}

		#endregion AddFineItemCommand

		#region DeleteFineItemCommand

		public DelegateCommand<FineItem> DeleteFineItemCommand { get; private set; }

		private void CreateDeleteFineItemCommand()
		{
			DeleteFineItemCommand = new DelegateCommand<FineItem>(
				Entity.RemoveItem,
				(item) => item != null && CanEdit && IsStandartFine && Entity.RouteList == null
			);
			DeleteFineItemCommand.CanExecuteChangedWith(Entity, x => x.RouteList);
			DeleteFineItemCommand.CanExecuteChangedWith(this, x => x.IsStandartFine, x => x.CanEdit);
		}

		#endregion DeleteFineItemCommand

		#endregion Commands
	}
}
