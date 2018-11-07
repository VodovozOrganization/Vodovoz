using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NLog;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Operations;
using Vodovoz.Repository.Operations;
using Vodovoz.ViewModel;

namespace Vodovoz.Dialogs.DocumentDialogs
{
	public partial class TransferOperationDocumentDlg : QS.Dialog.Gtk.EntityDialogBase<TransferOperationDocument>
	{
		static Logger logger = LogManager.GetCurrentClassLogger();

		public TransferOperationDocumentDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<TransferOperationDocument>();
			TabName = "Новый перенос между точками доставки";
			ConfigureDlg();
			Entity.Author = Entity.ResponsiblePerson = Repository.EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogWorks.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}
		}

		public TransferOperationDocumentDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<TransferOperationDocument>(id);
			ConfigureDlg();
		}

		public TransferOperationDocumentDlg(TransferOperationDocument sub) : this (sub.Id)
		{
		}

		void ConfigureDlg()
		{
			textComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
		//	datepickerDate.Date = Entity.TimeStamp;
			datepickerDate.Binding.AddBinding(Entity, e => e.TimeStamp, w => w.Date).InitializeFromSource();

			var counterpartyFilter = new CounterpartyFilter(UoW);
			referenceCounterpartyFrom.RepresentationModel = new ViewModel.CounterpartyVM(counterpartyFilter);
			referenceCounterpartyFrom.Binding.AddBinding(Entity, e => e.FromClient, w => w.Subject).InitializeFromSource();

			counterpartyFilter = new CounterpartyFilter(UoW);
			referenceCounterpartyTo.RepresentationModel = new ViewModel.CounterpartyVM(counterpartyFilter);
			referenceCounterpartyTo.Binding.AddBinding(Entity, e => e.ToClient, w => w.Subject).InitializeFromSource();

			referenceDeliveryPointTo.CanEditReference = false;
			referenceDeliveryPointTo.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPointTo.Binding.AddBinding(Entity, e => e.ToDeliveryPoint, w => w.Subject).InitializeFromSource();
			referenceDeliveryPointFrom.CanEditReference = false;
			referenceDeliveryPointFrom.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPointFrom.Binding.AddBinding(Entity, e => e.FromDeliveryPoint, w => w.Subject).InitializeFromSource();
			referenceEmployee.RepresentationModel = new EmployeesVM(new EmployeeFilter(UoW));
			referenceEmployee.Binding.AddBinding(Entity, e => e.ResponsiblePerson, w => w.Subject).InitializeFromSource();

			transferoperationdocumentitemview1.DocumentUoW = UoWGeneric;
		}

		public override bool Save()
		{
			var messages = new List<string>();

			var valid = new QSValidator<TransferOperationDocument>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;
			

			Entity.LastEditor = Repository.EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.TimeStamp = datepickerDate.Date;
		//	Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null) {
				MessageDialogWorks.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
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
			referenceDeliveryPointFrom.Sensitive = referenceCounterpartyFrom.Subject != null;
			if(referenceCounterpartyFrom.Subject != null) {
				var points = ((Counterparty)referenceCounterpartyFrom.Subject).DeliveryPoints.Select(o => o.Id).ToList();
				referenceDeliveryPointFrom.ItemsCriteria = UoWGeneric.Session.CreateCriteria<DeliveryPoint>()
					.Add(Restrictions.In("Id", points));
			}
		}

		protected void OnReferenceCounterpartyToChanged(object sender, EventArgs e)
		{
			referenceDeliveryPointTo.Sensitive = referenceCounterpartyTo.Subject != null;
			if(referenceCounterpartyTo.Subject != null) {
				var points = ((Counterparty)referenceCounterpartyTo.Subject).DeliveryPoints.Select(o => o.Id).ToList();
				referenceDeliveryPointTo.ItemsCriteria = UoWGeneric.Session.CreateCriteria<DeliveryPoint>()
					.Add(Restrictions.In("Id", points));
			}
		}

		protected void OnReferenceDeliveryPointFromChanged(object sender, EventArgs e)
		{
			RefreshSpinButtons();
		}

		protected void OnCheckbuttonLockToggled(object sender, EventArgs e)
		{
			if(referenceDeliveryPointFrom.Subject != null)
				RefreshSpinButtons();
		}

		protected void RefreshSpinButtons()
		{
			int bottlesMax = BottlesRepository.GetBottlesAtDeliveryPoint(UoWGeneric, Entity.FromDeliveryPoint, Entity.TimeStamp);
			decimal depositsBottlesMax = DepositRepository.GetDepositsAtDeliveryPoint(UoWGeneric, Entity.FromDeliveryPoint, DepositType.Bottles, Entity.TimeStamp);
			decimal depositsEquipmentMax = DepositRepository.GetDepositsAtDeliveryPoint(UoWGeneric, Entity.FromDeliveryPoint, DepositType.Equipment, Entity.TimeStamp);

			if(Entity.OutBottlesOperation != null) {
				spinBottles.Value = Entity.OutBottlesOperation.Returned != 0 ? Entity.OutBottlesOperation.Returned : (Entity.OutBottlesOperation.Delivered * -1);
			} else
				spinBottles.Value = 0;

			if(Entity.OutBottlesDepositOperation != null) {
				spinDepositsBottles.Value = (double)(Entity.OutBottlesDepositOperation.RefundDeposit != 0 ? Entity.OutBottlesDepositOperation.RefundDeposit : (Entity.OutBottlesDepositOperation.ReceivedDeposit * -1));
			} else
				spinDepositsBottles.Value = 0;

			if(Entity.OutEquipmentDepositOperation != null) {
				spinDepositsEquipment.Value = (double)(Entity.OutEquipmentDepositOperation.RefundDeposit != 0 ? Entity.OutEquipmentDepositOperation.RefundDeposit : (Entity.OutEquipmentDepositOperation.ReceivedDeposit * -1));
			} else
				spinDepositsEquipment.Value = 0;

			if(Math.Abs(bottlesMax) < Math.Abs(spinBottles.Value)
			   || Math.Abs(depositsBottlesMax) < Math.Abs((decimal)spinDepositsBottles.Value)
				|| Math.Abs(depositsEquipmentMax) < Math.Abs((decimal)spinDepositsEquipment.Value))
			{
				checkbuttonLock.Active = false;
			}

			spinBottles.Sensitive = referenceDeliveryPointFrom.Subject != null;
			labelBottlesMax.LabelProp = bottlesMax.ToString();

			spinDepositsBottles.Sensitive = referenceDeliveryPointFrom.Subject != null;
			labelDepositsBottlesMax.LabelProp = depositsBottlesMax.ToString();

			spinDepositsEquipment.Sensitive = referenceDeliveryPointFrom.Subject != null;
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
			if(Entity.OutEquipmentDepositOperation == null)
			{
				this.HasChanges = spinDepositsEquipment.Value != 0;
				return;
			}
			this.HasChanges = (decimal)spinDepositsBottles.Value != (Entity.OutEquipmentDepositOperation.RefundDeposit != 0 ? Entity.OutEquipmentDepositOperation.RefundDeposit : (Entity.OutEquipmentDepositOperation.ReceivedDeposit * -1));
		}
	}
}
