using System;
using System.Collections.Generic;
using Autofac;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Validation;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.TempAdapters;

namespace Vodovoz.Dialogs.DocumentDialogs
{
	public partial class TransferOperationDocumentDlg : QS.Dialog.Gtk.EntityDialogBase<TransferOperationDocument>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private IEmployeeRepository _employeeRepository;
		private IBottlesRepository _bottlesRepository;
		private IDepositRepository _depositRepository;

		public TransferOperationDocumentDlg()
		{
			ResolveDependencies();
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<TransferOperationDocument>();
			TabName = "Новый перенос между точками доставки";
			ConfigureDlg();

			var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoW);

			Entity.AuthorId = currentEmployee?.Id;
			Entity.ResponsiblePerson = currentEmployee;

			if(Entity.AuthorId == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}
		}

		public TransferOperationDocumentDlg(int id)
		{
			ResolveDependencies();
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<TransferOperationDocument>(id);
			ConfigureDlg();
		}

		public TransferOperationDocumentDlg(TransferOperationDocument sub) : this(sub.Id)
		{
		}

		private void ResolveDependencies()
		{
			_employeeRepository = _lifetimeScope.Resolve<IEmployeeRepository>();
			_bottlesRepository = _lifetimeScope.Resolve<IBottlesRepository>();
			_depositRepository = _lifetimeScope.Resolve<IDepositRepository>();
		}

		void ConfigureDlg()
		{
			ySpecCmbDeliveryPointFrom.SetSizeRequest(250, 30);
			ySpecCmbDeliveryPointTo.SetSizeRequest(250, 30);

			textComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			datepickerDate.Binding.AddBinding(Entity, e => e.TimeStamp, w => w.Date).InitializeFromSource();

			var clientFactory = new CounterpartyJournalFactory();
			referenceCounterpartyFrom.SetEntitySelectorFactory(clientFactory.CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope));
			referenceCounterpartyFrom.Binding.AddBinding(Entity, e => e.FromClient, w => w.Subject).InitializeFromSource();

			referenceCounterpartyTo.SetEntitySelectorFactory(clientFactory.CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope));
			referenceCounterpartyTo.Binding.AddBinding(Entity, e => e.ToClient, w => w.Subject).InitializeFromSource();

			var employeeFactory = _lifetimeScope.Resolve<IEmployeeJournalFactory>();
			evmeEmployee.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeEmployee.Binding.AddBinding(Entity, e => e.ResponsiblePerson, w => w.Subject).InitializeFromSource();

			transferoperationdocumentitemview1.DocumentUoW = UoWGeneric;

			if(Entity.FromClient != null)
				RefreshSpinButtons(_bottlesRepository, _depositRepository);
		}

		public override bool Save()
		{
			var messages = new List<string>();

			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			Entity.LastEditorId = _employeeRepository.GetEmployeeForCurrentUser(UoW)?.Id;
			if(Entity.LastEditorId == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}
			if(spinBottles.Value == 0f && spinDepositsBottles.Value == 0f && spinDepositsEquipment.Value == 0f) {
				MessageDialogHelper.RunErrorDialog("Вы ничего не указали для перемещения.");
				return false;
			}

			messages.AddRange(Entity.SaveOperations(UoWGeneric, (int)spinBottles.Value, (decimal)spinDepositsBottles.Value, (decimal)spinDepositsEquipment.Value));

			logger.Info("Сохраняем документ переноса...");
			UoWGeneric.Save();
			logger.Info("Ok.");
			return true;
		}

		protected void OnReferenceCounterpartyFromChanged(object sender, EventArgs e)
		{
			ySpecCmbDeliveryPointFrom.Sensitive = Entity.FromClient != null;
			if(Entity.FromClient != null) {
				ySpecCmbDeliveryPointFrom.SetRenderTextFunc<DeliveryPoint>(d => string.Format("{1}: {0}", d.ShortAddress, d.Id));
				ySpecCmbDeliveryPointFrom.ItemsList = Entity.FromClient.DeliveryPoints;
				ySpecCmbDeliveryPointFrom.Binding.AddBinding(Entity, t => t.FromDeliveryPoint, w => w.SelectedItem).InitializeFromSource();
				RefreshSpinButtons(_bottlesRepository, _depositRepository);
				ySpecCmbDeliveryPointFrom.Changed += (s, ea) => RefreshSpinButtons(_bottlesRepository, _depositRepository);
			}
		}

		protected void OnReferenceCounterpartyToChanged(object sender, EventArgs e)
		{
			ySpecCmbDeliveryPointTo.Sensitive = Entity.ToClient != null;
			if(Entity.ToClient != null) {
				ySpecCmbDeliveryPointTo.SetRenderTextFunc<DeliveryPoint>(d => string.Format("{1}: {0}", d.ShortAddress, d.Id));
				ySpecCmbDeliveryPointTo.ItemsList = Entity.ToClient.DeliveryPoints;
				ySpecCmbDeliveryPointTo.Binding.AddBinding(Entity, t => t.ToDeliveryPoint, w => w.SelectedItem).InitializeFromSource();
			}
		}

		protected void OnCheckbuttonLockToggled(object sender, EventArgs e)
		{
			if(Entity.FromClient != null)
				RefreshSpinButtons(_bottlesRepository, _depositRepository);
		}

		protected void RefreshSpinButtons(IBottlesRepository bottlesRepository, IDepositRepository depositRepository)
		{
			if(depositRepository == null)
				throw new ArgumentNullException(nameof(depositRepository));
			if(bottlesRepository == null)
				throw new ArgumentNullException(nameof(bottlesRepository));

			int bottlesMax = bottlesRepository.GetBottlesDebtAtCounterpartyAndDeliveryPoint(
				UoWGeneric,
				Entity.FromClient,
				Entity.FromDeliveryPoint,
				Entity.TimeStamp);
			decimal depositsBottlesMax = depositRepository.GetDepositsAtCounterpartyAndDeliveryPoint(UoWGeneric, Entity.FromClient, Entity.FromDeliveryPoint, DepositType.Bottles, Entity.TimeStamp);
			decimal depositsEquipmentMax = depositRepository.GetDepositsAtCounterpartyAndDeliveryPoint(UoWGeneric, Entity.FromClient, Entity.FromDeliveryPoint, DepositType.Equipment, Entity.TimeStamp);

			if(Entity.OutBottlesOperation != null)
				spinBottles.Value = Entity.OutBottlesOperation.Returned != 0 ? Entity.OutBottlesOperation.Returned : (Entity.OutBottlesOperation.Delivered * -1);
			else
				spinBottles.Value = 0;

			if(Entity.OutBottlesDepositOperation != null)
				spinDepositsBottles.Value = (double)(Entity.OutBottlesDepositOperation.RefundDeposit != 0 ? Entity.OutBottlesDepositOperation.RefundDeposit : (Entity.OutBottlesDepositOperation.ReceivedDeposit * -1));
			else
				spinDepositsBottles.Value = 0;

			if(Entity.OutEquipmentDepositOperation != null)
				spinDepositsEquipment.Value = (double)(Entity.OutEquipmentDepositOperation.RefundDeposit != 0 ? Entity.OutEquipmentDepositOperation.RefundDeposit : (Entity.OutEquipmentDepositOperation.ReceivedDeposit * -1));
			else
				spinDepositsEquipment.Value = 0;

			if(Math.Abs(bottlesMax) < Math.Abs(spinBottles.Value)
			   || Math.Abs(depositsBottlesMax) < Math.Abs((decimal)spinDepositsBottles.Value)
				|| Math.Abs(depositsEquipmentMax) < Math.Abs((decimal)spinDepositsEquipment.Value)) {
				checkbuttonLock.Active = false;
			}

			spinBottles.Sensitive = Entity.FromClient != null;
			labelBottlesMax.LabelProp = bottlesMax.ToString();

			spinDepositsBottles.Sensitive = Entity.FromClient != null;
			labelDepositsBottlesMax.LabelProp = depositsBottlesMax.ToString();

			spinDepositsEquipment.Sensitive = Entity.FromClient != null;
			labelDepositsEquipmentMax.LabelProp = depositsEquipmentMax.ToString();

			if(checkbuttonLock.Active) {
				spinBottles.Adjustment.Upper = bottlesMax > 0 ? bottlesMax : 0;
				spinBottles.Adjustment.Lower = bottlesMax < 0 ? bottlesMax : 0;

				spinDepositsBottles.Adjustment.Upper = (double)(depositsBottlesMax > 0 ? depositsBottlesMax : 0);
				spinDepositsBottles.Adjustment.Lower = (double)(depositsBottlesMax < 0 ? depositsBottlesMax : 0);

				spinDepositsEquipment.Adjustment.Upper = (double)(depositsEquipmentMax > 0 ? depositsEquipmentMax : 0);
				spinDepositsEquipment.Adjustment.Lower = (double)(depositsEquipmentMax < 0 ? depositsEquipmentMax : 0);
			} else {
				spinBottles.Adjustment.Upper = 1000;
				spinBottles.Adjustment.Lower = -1000;

				spinDepositsBottles.Adjustment.Upper = 100000;
				spinDepositsBottles.Adjustment.Lower = -100000;

				spinDepositsEquipment.Adjustment.Upper = 100000;
				spinDepositsEquipment.Adjustment.Lower = -100000;
			}
		}

		protected void OnSpinBottlesChanged(object sender, EventArgs e)
		{
			if(Entity.OutBottlesOperation == null) {
				this.HasChanges = spinBottles.Value != 0;
				return;
			}
			this.HasChanges = (int)spinBottles.Value != (Entity.OutBottlesOperation.Returned != 0 ? Entity.OutBottlesOperation.Returned : (Entity.OutBottlesOperation.Delivered * -1));
		}

		protected void OnSpinDepositsBottlesChanged(object sender, EventArgs e)
		{
			if(Entity.OutBottlesDepositOperation == null) {
				this.HasChanges = spinDepositsBottles.Value != 0;
				return;
			}
			this.HasChanges = (decimal)spinDepositsBottles.Value != (Entity.OutBottlesDepositOperation.RefundDeposit != 0 ? Entity.OutBottlesDepositOperation.RefundDeposit : (Entity.OutBottlesDepositOperation.ReceivedDeposit * -1));
		}

		protected void OnSpinDepositsEquipmentChanged(object sender, EventArgs e)
		{
			if(Entity.OutEquipmentDepositOperation == null) {
				this.HasChanges = spinDepositsEquipment.Value != 0;
				return;
			}
			this.HasChanges = (decimal)spinDepositsBottles.Value != (Entity.OutEquipmentDepositOperation.RefundDeposit != 0 ? Entity.OutEquipmentDepositOperation.RefundDeposit : (Entity.OutEquipmentDepositOperation.ReceivedDeposit * -1));
		}

		protected override void OnDestroyed()
		{
			if(_lifetimeScope != null)
			{
				_lifetimeScope.Dispose();
				_lifetimeScope = null;
			}
			base.OnDestroyed();
		}
	}
}
