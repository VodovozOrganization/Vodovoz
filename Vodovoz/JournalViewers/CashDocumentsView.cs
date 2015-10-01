using System;
using Gtk;
using NLog;
using QSOrmProject;
using QSOrmProject.UpdateNotification;
using QSTDI;
using Vodovoz.ViewModel;
using Vodovoz.Domain.Cash;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CashDocumentsView : TdiTabBase
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		public CashDocumentsView ()
		{
			this.Build ();
			this.TabName = "Кассовые документы";
			buttonAdd.ItemsEnum = typeof(CashDocumentType);
			tableDocuments.RepresentationModel = new CashDocumentsVM ();
			hboxFilter.Add (tableDocuments.RepresentationModel.RepresentationFilter as Widget);
			(tableDocuments.RepresentationModel.RepresentationFilter as Widget).Show ();
			tableDocuments.RepresentationModel.UpdateNodes ();
			tableDocuments.Selection.Changed += OnSelectionChanged;
			buttonEdit.Sensitive = buttonDelete.Sensitive = false;
		}

		void OnRefObjectUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{
			tableDocuments.RepresentationModel.UpdateNodes ();

		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			buttonEdit.Sensitive = buttonDelete.Sensitive = tableDocuments.Selection.CountSelectedRows () > 0;
		}

		protected void OnButtonAddEnumItemClicked (object sender, EnumItemClickedEventArgs e)
		{
			ITdiDialog dlg;

			CashDocumentType type = (CashDocumentType)e.ItemEnum;	
			switch (type) {
			case CashDocumentType.Income:
				dlg = new CashIncomeDlg ();
				break;
			case CashDocumentType.Expense:
				dlg = new CashExpenseDlg ();
				break;
			case CashDocumentType.AdvanceReport:
				dlg = new AdvanceReportDlg ();
				break;
			default:
				throw new NotSupportedException ("Тип документа не поддерживается.");
			}
			if (TabParent.BeforeCreateNewTab ((object)null, null).HasFlag (TdiBeforeCreateResultFlag.Canceled))
				return;
			TabParent.AddTab (dlg, this);
		}

		protected void OnTableDocumentsRowActivated (object o, RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			if (tableDocuments.Selection.CountSelectedRows () == 0)
				return;
			
			int id = tableDocuments.GetSelectedId ();
			var DocType = (tableDocuments.GetSelectedObject () as ViewModel.CashDocumentsVMNode).DocTypeEnum;
			ITdiDialog dlg;
			switch (DocType) {
			case CashDocumentType.Income:
				dlg = new CashIncomeDlg (id);
				break;
			case CashDocumentType.Expense:
				dlg = new CashExpenseDlg (id);
				break;
			case CashDocumentType.AdvanceReport: 
				dlg = new AdvanceReportDlg (id);
				break;
			default:
				return;
			}
			TabParent.AddTab (dlg, this);
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			OrmMain.DeleteObject (tableDocuments.GetSelectedObjects () [0]);
		}

		protected void OnButtonFilterToggled (object sender, EventArgs e)
		{
			hboxFilter.Visible = buttonFilter.Active;
		}

	}
}

