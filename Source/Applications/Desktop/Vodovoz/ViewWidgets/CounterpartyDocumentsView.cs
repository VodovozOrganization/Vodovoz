using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Binding;
using Gamma.ColumnConfig;
using NHibernate.Proxy;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Tdi;
using QSOrmProject;
using QSReport;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CounterpartyDocumentsView : Gtk.Bin
	{
		public IUnitOfWork UoW { get; set; }
		public Counterparty Counterparty { get; set; }
		public List<CounterpartyDocumentNode> CounterpartyDocs { get; private set; } = new List<CounterpartyDocumentNode>();


		public CounterpartyDocumentsView()
		{
			this.Build();
		}

		public void Config(IUnitOfWork uow, Counterparty counterparty, bool selectable = false)
		{
			UoW = uow;
			Counterparty = counterparty;
			ytreeDocuments.Selection.Mode = Gtk.SelectionMode.Single;
			ytreeDocuments.Selection.Changed += Selection_Changed;
			ytreeDocuments.RowActivated += (o, args) => buttonViewDocument.Click();

			var columnConfig = FluentColumnsConfig<CounterpartyDocumentNode>.Create();

			if(selectable) {
				columnConfig.AddColumn("Выбрать").AddToggleRenderer(x => x.Selected).Editing();
			}

			ytreeDocuments.ColumnsConfig = columnConfig
				.AddColumn("Документ").AddTextRenderer(x => x.Title)
				.AddColumn("Номер").AddTextRenderer(x => x.Number)
				.AddColumn("Дата").AddTextRenderer(x => x.Date)
				.AddColumn("Точка доставки").AddTextRenderer(x => x.DeliveryPoint != null ? x.DeliveryPoint.ShortAddress : String.Empty)
				.Finish();

			LoadData();
		}

		private void LoadData()
		{
			CounterpartyContract contractAlias = null;

			//получаем список контрактов
			var contracts = UoW.Session.QueryOver<CounterpartyContract>(() => contractAlias)
				.Where(() => contractAlias.Counterparty.Id == Counterparty.Id)
				.List();

			foreach(var contract in contracts) {
				CounterpartyDocumentNode contractNode = new CounterpartyDocumentNode();
				contractNode.Document = contract;
				contractNode.Documents = new List<CounterpartyDocumentNode>();

				CounterpartyDocs.Add(contractNode);
			}

			ytreeDocuments.YTreeModel = new RecursiveTreeModel<CounterpartyDocumentNode>(CounterpartyDocs, x => x.Parent, x => x.Documents);
		}

		protected void OnButtonViewDocumentClicked(object sender, EventArgs e)
		{
			ITdiTab mytab = DialogHelper.FindParentTab(this);
			if(mytab == null)
				return;

			CounterpartyDocumentNode selectedPrintableDocuments = (ytreeDocuments.GetSelectedObject() as CounterpartyDocumentNode);

			if(selectedPrintableDocuments.Document is CounterpartyContract) {
				int contractID = (selectedPrintableDocuments.Document as CounterpartyContract).Id;
				ITdiDialog dlg = new CounterpartyContractDlg(contractID);
				mytab.TabParent.AddTab(dlg, mytab);
			}

			if(selectedPrintableDocuments.Document is OrderDocument) {
				var rdlDoc = (selectedPrintableDocuments.Document as IPrintableRDLDocument);
				if(rdlDoc != null) {
					mytab.TabParent.AddTab(DocumentPrinter.GetPreviewTab(rdlDoc), mytab);
				}
			}
		}

		public List<CounterpartyDocumentNode> GetSelectedDocuments() {
			var result = new List<CounterpartyDocumentNode>();
			foreach(var item in CounterpartyDocs) {
				result.AddRange(item.GetSelectedDocuments());
			}
			return result;
		}

		void Selection_Changed(object sender, EventArgs e)
		{
			CounterpartyDocumentNode selectedDocument = (ytreeDocuments.GetSelectedObject() as CounterpartyDocumentNode);
			buttonViewDocument.Sensitive = selectedDocument != null;
		}
	}

	public class CounterpartyDocumentNode
	{
		public bool Selected { get; set; }

		public string Title {
			get{
				if(Document is CounterpartyContract) {
					return "Договор";
				}

				return "";
			}
		}

		public string Number {
			get {
				if(Document is CounterpartyContract contract) {
					return contract.Number;
				}

				return "";
			}
		}

		public string Date {
			get {
				if(Document is CounterpartyContract contract) {
					return contract.IssueDate.ToShortDateString();
				}

				if(Document is OrderDocument orderDocument) {
					return orderDocument.DocumentDateText;
				}

				return "";
			}
		}

		public object Document { get; set; }

		public DeliveryPoint DeliveryPoint { get; set; }

		public CounterpartyDocumentNode Parent { get; set; }

		public List<CounterpartyDocumentNode> Documents { get; set; }

		public List<CounterpartyDocumentNode> GetSelectedDocuments()
		{
			List<CounterpartyDocumentNode> result = new List<CounterpartyDocumentNode>();
			if(Selected) {
				result.Add(this);
			}
			if(Documents != null && Documents.Count > 0) {
				foreach(var item in Documents) {
					result.AddRange(item.GetSelectedDocuments().Where(x => x.Selected));
				}
			}
			return result;
		}
	}
}
