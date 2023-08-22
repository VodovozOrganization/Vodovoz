using Gamma.Utilities;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using System.Collections.Specialized;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Employees
{
	public class FineViewModel : EntityTabViewModelBase<Fine>, IAskSaveOnCloseViewModel
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IUndeliveredOrdersJournalOpener _undeliveryViewOpener;
		private readonly IEmployeeService _employeeService;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly IEntitySelectorFactory _employeeSelectorFactory;

		public FineViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			IUndeliveredOrdersJournalOpener undeliveryViewOpener,
			IEmployeeService employeeService,
			IEmployeeJournalFactory employeeJournalFactory,
			IEmployeeSettings employeeSettings,
			ICommonServices commonServices
		) : base(uowBuilder, uowFactory, commonServices)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_undeliveryViewOpener = undeliveryViewOpener ?? throw new ArgumentNullException(nameof(undeliveryViewOpener));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_employeeSelectorFactory = _employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
			CreateCommands();
			ConfigureEntityPropertyChanges();
		}

		private void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(e => e.FineType,
				() => IsFuelOverspendingFine,
				() => IsStandartFine
			);

			SetPropertyChangeRelation(e => e.FineReasonString,
				() => FineReasonString
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

			Entity.Items.CollectionChanged += Items_CollectionChanged; 
		}

		private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if(e.Action == NotifyCollectionChangedAction.Reset)
			{
				OnPropertyChanged(() => CanEditFineType);
			}
		}

		private void CalculateMoneyFromLiters()
		{
			if(Entity.Items.Count() > 1)
			{
				throw new Exception("При типе штрафа \"Перерасход топлива\" недопустимо наличие более одного сотрудника в списке.");
			}
			
			if(RouteList != null)
			{
				decimal fuelCost = RouteList.Car.FuelType.Cost;
				Entity.TotalMoney = Math.Round(Entity.LitersOverspending * fuelCost, 0, MidpointRounding.ToEven);
				var item = Entity.Items.FirstOrDefault();
				
				if(item != null)
				{
					item.Money = Entity.TotalMoney;
					item.LitersOverspending = Entity.LitersOverspending;
				}
			}
		}

		private Employee currentEmployee;
		public Employee CurrentEmployee {
			get {
				if(currentEmployee == null) {
					currentEmployee = _employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				}
				return currentEmployee;
			}
		}

		public FineTypes FineType
		{
			get => Entity.FineType;
			set
			{
				if(Entity.FineType != value)
				{
					Entity.FineType = value;
					UpdateEmployeeList();
				}
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
				if(Entity.RouteList != value)
				{
					Entity.RouteList = value;
					UpdateEmployeeList();
					CalculateMoneyFromLiters();
				}
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

		public bool AskSaveOnClose => CanEdit && HasChanges;

		#endregion

		[PropertyChangedAlso(nameof(CanShowRequestRouteListMessage))]
		public bool IsFuelOverspendingFine => Entity.FineType == FineTypes.FuelOverspending;

		public bool IsStandartFine => Entity.FineType == FineTypes.Standart;

		public bool CanShowRequestRouteListMessage => IsFuelOverspendingFine && Entity.RouteList == null;

		public bool CanEditFineType => CanEdit;

		public decimal Liters
		{
			get => Entity.LitersOverspending;

			set
			{
				Entity.LitersOverspending = value;
				CalculateMoneyFromLiters();
			}
		}

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
			if(driver != null)
			{
				FineItem item = null;
				item = Entity.Items.Where(x => x.Employee == driver).FirstOrDefault();
				Entity.Items.Clear();

				if(item != null)
				{
					Entity.Items.Add(item);
				}
				else
				{
					Entity.AddItem(driver);
				}
			}
		}

		protected override bool BeforeSave()
		{
			Entity.UpdateWageOperations(UoW);
			Entity.UpdateFuelOperations(UoW);
			return base.BeforeSave();
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
				() => _undeliveryViewOpener.OpenFromFine(this, Entity.UndeliveredOrder.OldOrder, Entity.UndeliveredOrder.OldOrder.DeliveryDate, Entity.UndeliveredOrder.UndeliveryStatus),
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
						() => new FineTemplateViewModel(EntityUoWBuilder.ForCreate(), _uowFactory, CommonServices),
						(node) => new FineTemplateViewModel(EntityUoWBuilder.ForOpen(node.Id), _uowFactory, CommonServices),
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
					var employeeSelector = _employeeSelectorFactory.CreateSelector();
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
