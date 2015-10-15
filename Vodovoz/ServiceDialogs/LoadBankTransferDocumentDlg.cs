using System;
using QSTDI;
using QSBanks;
using Gtk;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class LoadBankTransferDocumentDlg : TdiTabBase
	{
		private BankTransferDocumentParser parser;
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
			                              typeof(TransferDocument));

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
			TransferDocumentCol
		}


		public LoadBankTransferDocumentDlg ()
		{
			this.Build ();
			TabName = "Загрузка из банк-клиента";

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

			TreeViewColumn payerAccountColumn = new TreeViewColumn ();
			payerAccountColumn.Title = "Счет плательщика";
			CellRendererText payerAccountCell = new CellRendererText ();
			payerAccountColumn.PackStart (payerAccountCell, true);
			payerAccountColumn.AddAttribute (payerAccountCell, "text", (int)Columns.PayerAccountCol);

			TreeViewColumn payerBankColumn = new TreeViewColumn ();
			payerBankColumn.Title = "Банк плательщика";
			CellRendererText payerBankCell = new CellRendererText ();
			payerBankColumn.PackStart (payerBankCell, true);
			payerBankColumn.AddAttribute (payerBankCell, "text", (int)Columns.PayerBankCol);

			TreeViewColumn recipientColumn = new TreeViewColumn ();
			recipientColumn.Title = "Получатель";
			CellRendererText recipientCell = new CellRendererText ();
			recipientColumn.PackStart (recipientCell, true);
			recipientColumn.AddAttribute (recipientCell, "text", (int)Columns.RecipientNameCol);

			TreeViewColumn recipientAccountColumn = new TreeViewColumn ();
			recipientAccountColumn.Title = "Счет получателя";
			CellRendererText recipientAccountCell = new CellRendererText ();
			recipientAccountColumn.PackStart (recipientAccountCell, true);
			recipientAccountColumn.AddAttribute (recipientAccountCell, "text", (int)Columns.RecipientAccountCol);

			TreeViewColumn recipientBankColumn = new TreeViewColumn ();
			recipientBankColumn.Title = "Банк получателя";
			CellRendererText recipientBankCell = new CellRendererText ();
			recipientBankColumn.PackStart (recipientBankCell, true);
			recipientBankColumn.AddAttribute (recipientBankCell, "text", (int)Columns.RecipientBankCol);

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
		}

		void CheckCell_Toggled (object o, ToggledArgs args)
		{
			TreeIter iter;

			if (documents.GetIter (out iter, new TreePath (args.Path))) {
				bool old = (bool)documents.GetValue (iter, (int)Columns.CheckCol);
				documents.SetValue (iter, (int)Columns.CheckCol, !old);
			}
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
			foreach (var doc in parser.TransferDocuments) {
				documents.AppendValues (
					true, 
					doc.Number, 
					doc.Date.ToShortDateString (), 
					doc.Total.ToString (), 
					doc.PayerName,
					doc.PayerAccount,
					doc.PayerBank,
					doc.RecipientName,
					doc.RecipientAccount,
					doc.RecipientBank,
					doc
				);
			}
		}

		protected void OnFilechooserSelectionChanged (object sender, EventArgs e)
		{
			buttonReadFile.Sensitive = !String.IsNullOrWhiteSpace (filechooser.Filename);
			buttonUpload.Sensitive = false;
		}
	}
}

