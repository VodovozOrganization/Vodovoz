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
			ylabelDate.Binding.AddFuncBinding(Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp).InitializeFromSource();
			yentryrefWarehouse.SubjectType = typeof(Warehouse);
			yentryrefWarehouse.Binding.AddBinding(Entity, e => e.Warehouse, w => w.Subject).InitializeFromSource();
			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			inventoryitemsview.DocumentUoW = UoWGeneric;
			if (Entity.Items.Any(x => x.AmountInDB != 0))
				yentryrefWarehouse.Sensitive = false;
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

		public void SetSensitiveWarehouse(bool sensitive)
		{
			yentryrefWarehouse.Sensitive = sensitive;
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
	}
}

