using System;
using System.Linq;
using Gtk;
using NLog;
using QSOrmProject;
using QSOrmProject.UpdateNotification;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class CashDocumentsView : TdiTabBase, ITdiJournal
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		private IUnitOfWork uow;

		public CashDocumentsView ()
		{
			this.Build ();
			this.TabName = "Кассовые документы";
			buttonAdd.ItemsEnum = typeof(CashDocumentType);
			tableDocuments.RepresentationModel = new CashDocumentsVM ();
			uow = tableDocuments.RepresentationModel.UoW;
			tableDocuments.RepresentationModel.ItemsListUpdated += TableDocuments_RepresentationModel_ItemsListUpdated;
			hboxFilter.Add (tableDocuments.RepresentationModel.RepresentationFilter as Widget);
			(tableDocuments.RepresentationModel.RepresentationFilter as Widget).Show ();
			tableDocuments.RepresentationModel.UpdateNodes ();
			tableDocuments.Selection.Changed += OnSelectionChanged;
			buttonEdit.Sensitive = buttonDelete.Sensitive = false;

			//Подписываемся на обновления для общей кассы
			OrmMain.GetObjectDescription<Expense> ().ObjectUpdated += OnCashUpdated;
			OrmMain.GetObjectDescription<Income> ().ObjectUpdated += OnCashUpdated;
			UpdateCurrentCash ();
		}

		void OnCashUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{
			UpdateCurrentCash ();
		}

		void TableDocuments_RepresentationModel_ItemsListUpdated (object sender, EventArgs e)
		{
			CalculateTotal ();
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
			CashDocumentType type = (CashDocumentType)e.ItemEnum;	
			switch (type) {
				case CashDocumentType.Income:
					TabParent.OpenTab(OrmMain.GenerateDialogHashName<Income>(0),
						() => new CashIncomeDlg(), this
					);
				break;
			case CashDocumentType.Expense:
					TabParent.OpenTab(OrmMain.GenerateDialogHashName<Expense>(0),
						() => new CashExpenseDlg(), this
					);
				break;
			case CashDocumentType.AdvanceReport:
					TabParent.OpenTab(OrmMain.GenerateDialogHashName<AdvanceReport>(0),
						() => new AdvanceReportDlg(), this
					);
				break;
			default:
				throw new NotSupportedException ("Тип документа не поддерживается.");
			}
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
			switch (DocType) {
			case CashDocumentType.Income:
					TabParent.OpenTab(OrmMain.GenerateDialogHashName<Income>(id),
						() => new CashIncomeDlg(id), this
					);
				break;
			case CashDocumentType.Expense:
					TabParent.OpenTab(OrmMain.GenerateDialogHashName<Expense>(id),
						() => new CashExpenseDlg(id), this
					);
				break;
			case CashDocumentType.AdvanceReport: 
					TabParent.OpenTab(OrmMain.GenerateDialogHashName<AdvanceReport>(id),
						() => new AdvanceReportDlg(id), this
					);
				break;
			default:
				throw new NotSupportedException ("Тип документа не поддерживается.");
			}
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			var node = (ViewModel.CashDocumentsVMNode)tableDocuments.GetSelectedNode ();
			Type docType = null;

			switch(node.DocTypeEnum)
			{
			case CashDocumentType.Income:
				docType = typeof(Income);
				break;
			case CashDocumentType.Expense:
				docType = typeof(Expense);
				break;
			case CashDocumentType.AdvanceReport:
				docType = typeof(AdvanceReport);
				break;
			}

			if (OrmMain.DeleteObject (docType, tableDocuments.GetSelectedId ()))
			{
				tableDocuments.RepresentationModel.UpdateNodes ();
				UpdateCurrentCash ();
			}
		}

		protected void OnButtonFilterToggled (object sender, EventArgs e)
		{
			hboxFilter.Visible = buttonFilter.Active;
		}

		void CalculateTotal()
		{
			decimal total = 0;
			foreach(var node in tableDocuments.RepresentationModel.ItemsList.Cast<ViewModel.CashDocumentsVMNode> ())
			{
				total += node.Money;
			}
			labelDocsSum.LabelProp = String.Format ("Сумма документов: {0}",
				CurrencyWorks.GetShortCurrencyString (total));
		}

		void UpdateCurrentCash ()
		{
			labelCurrentCash.LabelProp = String.Format ("Сейчас денег в кассе: {0}", 
				CurrencyWorks.GetShortCurrencyString (
					Repository.Cash.CashRepository.CurrentCash (uow)
				));
		}
	}
}

