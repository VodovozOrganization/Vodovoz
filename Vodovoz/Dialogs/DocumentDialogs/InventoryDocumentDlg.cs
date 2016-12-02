using System;
using System.Collections.Generic;
using System.Linq;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	public partial class InventoryDocumentDlg : OrmGtkDialogBase<InventoryDocument>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public InventoryDocumentDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<InventoryDocument> ();
			Entity.Author = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if(Entity.Author == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}
			if (CurrentUserSettings.Settings.DefaultWarehouse != null)
				Entity.Warehouse = UoWGeneric.GetById<Warehouse>(CurrentUserSettings.Settings.DefaultWarehouse.Id);

			ConfigureDlg ();
		}

		public InventoryDocumentDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<InventoryDocument> (id);
			ConfigureDlg ();
		}

		public InventoryDocumentDlg (InventoryDocument sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			ydatepickerDocDate.Binding.AddBinding(Entity, e => e.TimeStamp, w => w.Date).InitializeFromSource();
			yentryrefWarehouse.SubjectType = typeof(Warehouse);
			yentryrefWarehouse.Binding.AddBinding(Entity, e => e.Warehouse, w => w.Subject).InitializeFromSource();
			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			string errorMessage = "Не установлены единицы измерения у следующих номенклатур :" + Environment.NewLine;
			int wrongNomenclatures = 0;
			foreach (var item in UoWGeneric.Root.Items)
			{
				if(item.Nomenclature.Unit == null) {
					errorMessage += string.Format("Номер: {0}. Название: {1}{2}",
						item.Nomenclature.Id, item.Nomenclature.Name, Environment.NewLine);
					wrongNomenclatures++;
				}
			}
			if (wrongNomenclatures > 0) {
				MessageDialogWorks.RunErrorDialog(errorMessage);
				FailInitialize = true;
				return;
			}

			inventoryitemsview.DocumentUoW = UoWGeneric;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<InventoryDocument> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			Entity.LastEditor = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			Entity.UpdateOperations(UoW);

			logger.Info ("Сохраняем акт списания...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if (UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint (typeof(InventoryDocument), "акта инвентаризации"))
				Save ();

			var reportInfo = new QSReport.ReportInfo {
				Title = String.Format ("Акт инвентаризации №{0} от {1:d}", Entity.Id, Entity.TimeStamp),
				Identifier = "Store.InventoryDoc",
				Parameters = new Dictionary<string, object> {
					{ "inventory_id",  Entity.Id }
				}
			};

			TabParent.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName(reportInfo),
				() => new QSReport.ReportViewDlg (reportInfo)
			);
		}

		protected void OnYentryrefWarehouseBeforeChangeByUser(object sender, EntryReferenceBeforeChangeEventArgs e)
		{
			if(Entity.Warehouse != null && Entity.Items.Count > 0)
			{
				if (MessageDialogWorks.RunQuestionDialog("При изменении склада табличная часть документа будет очищена. Продолжить?"))
					Entity.ObservableItems.Clear();
				else
					e.CanChange = false;
			}
		}
	}
}

