using System;
using QSTDI;
using QSBanks;
using System.Linq;
using Gtk;
using Vodovoz.Repository;
using QSOrmProject;
using QSBanks.Repository;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class LoadBankTransferDocumentDlg : TdiTabBase
	{
		private BankTransferDocumentParser parser;
		private IUnitOfWork uow;

		private ListStore documents = new ListStore (
			                              typeof(bool), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(TransferDocument),
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string),
			                              typeof(string)
		                              );

		private enum Columns
		{
			CheckCol,
			NumberCol,
			DateCol,
			TotalCol,
			PayerNameCol,
			PayerAccountCol,
			PayerBankCol,
			RecipientNameCol,
			RecipientAccountCol,
			RecipientBankCol,
			TransferDocumentCol,
			PayerNameColorCol,
			PayerAccountColorCol,
			PayerBankColorCol,
			RecipientNameColorCol,
			RecipientAccountColorCol,
			RecipientBankColorCol
		}

		public LoadBankTransferDocumentDlg ()
		{
			this.Build ();
			uow = UnitOfWorkFactory.CreateWithoutRoot ();
			TabName = "Загрузка из банк-клиента";
			labelDescription1.Markup = "<span background=\"light coral\">     </span> - объект будет создан";
			labelDescription2.Markup = "<span background=\"khaki\">     </span> - объект будет исправлен";

			TreeViewColumn checkColumn = new TreeViewColumn ();
			checkColumn.Title = "";
			CellRendererToggle checkCell = new CellRendererToggle ();
			checkCell.Activatable = true;
			checkCell.Toggled += CheckCell_Toggled;
			checkColumn.PackStart (checkCell, true);
			checkColumn.AddAttribute (checkCell, "active", (int)Columns.CheckCol);


			TreeViewColumn numberColumn = new TreeViewColumn ();
			numberColumn.Title = "№";
			CellRendererText numberCell = new CellRendererText ();
			numberColumn.PackStart (numberCell, true);
			numberColumn.AddAttribute (numberCell, "text", (int)Columns.NumberCol);

			TreeViewColumn dateColumn = new TreeViewColumn ();
			dateColumn.Title = "Дата";
			CellRendererText dateCell = new CellRendererText ();
			dateColumn.PackStart (dateCell, true);
			dateColumn.AddAttribute (dateCell, "text", (int)Columns.DateCol);

			TreeViewColumn totalColumn = new TreeViewColumn ();
			totalColumn.Title = "Сумма";
			CellRendererText totalCell = new CellRendererText ();
			totalColumn.PackStart (totalCell, true);
			totalColumn.AddAttribute (totalCell, "text", (int)Columns.TotalCol);

			TreeViewColumn payerColumn = new TreeViewColumn ();
			payerColumn.Title = "Плательщик";
			CellRendererText payerCell = new CellRendererText ();
			payerColumn.PackStart (payerCell, true);
			payerColumn.AddAttribute (payerCell, "text", (int)Columns.PayerNameCol);
			payerColumn.AddAttribute (payerCell, "background", (int)Columns.PayerNameColorCol);

			TreeViewColumn payerAccountColumn = new TreeViewColumn ();
			payerAccountColumn.Title = "Счет плательщика";
			CellRendererText payerAccountCell = new CellRendererText ();
			payerAccountColumn.PackStart (payerAccountCell, true);
			payerAccountColumn.AddAttribute (payerAccountCell, "text", (int)Columns.PayerAccountCol);
			payerAccountColumn.AddAttribute (payerAccountCell, "background", (int)Columns.PayerAccountColorCol);

			TreeViewColumn payerBankColumn = new TreeViewColumn ();
			payerBankColumn.Title = "Банк плательщика";
			CellRendererText payerBankCell = new CellRendererText ();
			payerBankCell.WidthChars = 40;
			payerBankColumn.PackStart (payerBankCell, true);
			payerBankColumn.AddAttribute (payerBankCell, "text", (int)Columns.PayerBankCol);
			payerBankColumn.AddAttribute (payerBankCell, "background", (int)Columns.PayerBankColorCol);
			payerBankColumn.AddAttribute (payerBankCell, "tooltip_column", (int)Columns.PayerBankCol);

			TreeViewColumn recipientColumn = new TreeViewColumn ();
			recipientColumn.Title = "Получатель";
			CellRendererText recipientCell = new CellRendererText ();
			recipientColumn.PackStart (recipientCell, true);
			recipientColumn.AddAttribute (recipientCell, "text", (int)Columns.RecipientNameCol);
			recipientColumn.AddAttribute (recipientCell, "background", (int)Columns.RecipientNameColorCol);

			TreeViewColumn recipientAccountColumn = new TreeViewColumn ();
			recipientAccountColumn.Title = "Счет получателя";
			CellRendererText recipientAccountCell = new CellRendererText ();
			recipientAccountColumn.PackStart (recipientAccountCell, true);
			recipientAccountColumn.AddAttribute (recipientAccountCell, "text", (int)Columns.RecipientAccountCol);
			recipientAccountColumn.AddAttribute (recipientAccountCell, "background", (int)Columns.RecipientAccountColorCol);

			TreeViewColumn recipientBankColumn = new TreeViewColumn ();
			recipientBankColumn.Title = "Банк получателя";
			CellRendererText recipientBankCell = new CellRendererText ();
			recipientBankCell.WidthChars = 40;
			recipientBankColumn.PackStart (recipientBankCell, true);
			recipientBankColumn.AddAttribute (recipientBankCell, "text", (int)Columns.RecipientBankCol);
			recipientBankColumn.AddAttribute (payerBankCell, "tooltip", (int)Columns.RecipientBankCol);
			recipientBankColumn.AddAttribute (recipientBankCell, "background", (int)Columns.RecipientBankColorCol);

			treeDocuments.AppendColumn (checkColumn);
			treeDocuments.AppendColumn (numberColumn);
			treeDocuments.AppendColumn (dateColumn);
			treeDocuments.AppendColumn (totalColumn);
			treeDocuments.AppendColumn (payerColumn);
			treeDocuments.AppendColumn (payerAccountColumn);
			treeDocuments.AppendColumn (payerBankColumn);
			treeDocuments.AppendColumn (recipientColumn);
			treeDocuments.AppendColumn (recipientAccountColumn);
			treeDocuments.AppendColumn (recipientBankColumn);

			treeDocuments.Model = documents;

			buttonUpload.Sensitive = buttonReadFile.Sensitive = false;
			checkButtonAll.Sensitive = false;
		}

		void CheckCell_Toggled (object o, ToggledArgs args)
		{
			TreeIter iter;

			if (documents.GetIter (out iter, new TreePath (args.Path))) {
				bool old = (bool)documents.GetValue (iter, (int)Columns.CheckCol);
				documents.SetValue (iter, (int)Columns.CheckCol, !old);
			}

			checkButtonAll.Toggled -= OnCheckButtonAllToggled;
			checkButtonAll.Active = false;
			checkButtonAll.Toggled += OnCheckButtonAllToggled;
		}

		protected void OnButtonUploadClicked (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}

		protected void OnButtonReadFileClicked (object sender, EventArgs e)
		{
			documents.Clear ();
			parser = new BankTransferDocumentParser (filechooser.Filename);
			parser.Parse ();
			buttonUpload.Sensitive = true;
			checkButtonAll.Sensitive = true;
			foreach (var doc in parser.TransferDocuments) {
				documents.AppendValues (
					true, 
					doc.Number, 
					doc.Date.ToShortDateString (), 
					doc.Total.ToString (), 
					doc.PayerName,
					doc.PayerCheckingAccount,
					doc.PayerBank,
					doc.RecipientName,
					doc.RecipientCheckingAccount,
					doc.RecipientBank,
					doc,
					"white", "white", "white", "white", "white", "white"
				);
			}
			CheckDocuments ();
		}

		protected void OnFilechooserSelectionChanged (object sender, EventArgs e)
		{
			buttonReadFile.Sensitive = !String.IsNullOrWhiteSpace (filechooser.Filename);
			buttonUpload.Sensitive = false;
		}

		protected void CheckDocuments ()
		{
			TreeIter iter;
			if (!documents.GetIterFirst (out iter))
				return;
			do {
				var doc = (documents.GetValue (iter, (int)Columns.TransferDocumentCol) as TransferDocument);

				//Проверяем плательщика
				var payerCounterparty = CounterpartyRepository.GetCounterpartyByINN (uow, doc.PayerInn);
				if (payerCounterparty == null) {
					var organization = OrganizationRepository.GetOrganizationByName (uow, doc.PayerName);
					if (organization == null) {
						documents.SetValue (iter, (int)Columns.PayerNameColorCol, "light coral");
						documents.SetValue (iter, (int)Columns.PayerAccountColorCol, "light coral");
						//TODO Добавить контрагента или организацию.
					} else {
						if (!organization.Accounts.Any (acc => acc.Number == doc.PayerCheckingAccount)) {
							documents.SetValue (iter, (int)Columns.PayerAccountColorCol, "light coral");
							//TODO Добавить счет.
						}
					}
				} else {
					if (payerCounterparty.FullName != doc.PayerName) {
						documents.SetValue (iter, (int)Columns.PayerNameColorCol, "khaki");
						//TODO Исправить имя
					}
					if (!payerCounterparty.Accounts.Any (acc => acc.Number == doc.PayerCheckingAccount)) {
						documents.SetValue (iter, (int)Columns.PayerAccountColorCol, "light coral");
						//TODO Добавить счет
					}
				}

				//Проверяем получателя
				var recipientCounterparty = CounterpartyRepository.GetCounterpartyByINN (uow, doc.PayerInn);
				if (recipientCounterparty == null) {
					documents.SetValue (iter, (int)Columns.RecipientNameColorCol, "light coral");
					documents.SetValue (iter, (int)Columns.RecipientAccountColorCol, "light coral");
					//TODO Добавить контрагента
				} else {
					if (recipientCounterparty.FullName != doc.RecipientName) {
						documents.SetValue (iter, (int)Columns.RecipientNameColorCol, "khaki");
						//TODO Исправить имя
					}
					if (!recipientCounterparty.Accounts.Any (acc => acc.Number == doc.RecipientCheckingAccount)) {
						documents.SetValue (iter, (int)Columns.RecipientAccountColorCol, "light coral");
						//TODO Добавить счет
					}
				}

				//Проверяем банки
				var payerBank = BankRepository.GetBankByBik (uow, doc.PayerBik);
				if (payerBank == null && !String.IsNullOrEmpty (doc.PayerBik)) {
					documents.SetValue (iter, (int)Columns.PayerBankColorCol, "light coral");
					//TODO Сделать что-то с банком, если бик не пустой.
				}
				var recipientBank = BankRepository.GetBankByBik (uow, doc.RecipientBik);
				if (recipientBank == null && !String.IsNullOrEmpty (doc.PayerBik)) {
					documents.SetValue (iter, (int)Columns.RecipientBankColorCol, "light coral");
					//TODO Сделать что-то с банком, если бик не пустой.
				}
			} while (documents.IterNext (ref iter));
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			this.OnCloseTab (false);
		}

		protected void OnCheckButtonAllToggled (object sender, EventArgs e)
		{
			if (!checkButtonAll.HasFocus)
				return;
			TreeIter iter;
			if (!documents.GetIterFirst (out iter))
				return;
			do {
				documents.SetValue (iter, (int)Columns.CheckCol, checkButtonAll.Active);
			} while (documents.IterNext (ref iter));
		}
	}
}