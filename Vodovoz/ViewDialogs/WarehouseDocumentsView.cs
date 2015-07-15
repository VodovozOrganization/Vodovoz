using System;
using System.Collections.Generic;
using Gtk;
using NHibernate;
using NLog;
using QSOrmProject;
using QSOrmProject.UpdateNotification;
using QSTDI;
using Vodovoz.Domain.Documents;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class WarehouseDocumentsView : Gtk.Bin, ITdiJournal
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		public WarehouseDocumentsView ()
		{
			this.Build ();
			tableDocuments.RepresentationModel = new DocumentsVM ();
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
			Document document;

			DocumentType type = (DocumentType)e.ItemEnum;	
			switch (type) {
			case DocumentType.IncomingInvoice:
				document = new IncomingInvoice ();
				break;
			case DocumentType.IncomingWater:
				document = new IncomingWater ();
				break;
			case DocumentType.MovementDocument:
				document = new MovementDocument ();
				break;
			case DocumentType.WriteoffDocument:
				document = new WriteoffDocument ();
				break;
			default:
				throw new NotSupportedException ("Тип документа не поддерживается.");
			}
			if (TabParent.BeforeCreateNewTab ((object)null, null).HasFlag (TdiBeforeCreateResultFlag.Canceled))
				return;
			TabParent.AddTab (OrmMain.CreateObjectDialog (document.GetType ()), this);
		}

		#region ITdiTab implementation

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;

		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public ITdiTabParent TabParent { get ; set ; }

		protected string _tabName = "Журнал документов";

		public string TabName {
			get { return _tabName; }
			set {
				if (_tabName == value)
					return;
				_tabName = value;
				if (TabNameChanged != null)
					TabNameChanged (this, new TdiTabNameChangedEventArgs (value));
			}
		}
			
		protected void OnTableDocumentsRowActivated (object o, RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			if (tableDocuments.GetSelectedObjects ().GetLength (0) > 0) {
				int id = (tableDocuments.GetSelectedObjects () [0] as ViewModel.DocumentVMNode).Id;
				string DocType = (tableDocuments.GetSelectedObjects () [0] as ViewModel.DocumentVMNode).DocType;
				ITdiDialog dlg;
				switch (DocType) {
				case "Входящая накладная":
					dlg = new IncomingInvoiceDlg (id);
					break;
				case "Документ производства":
					dlg = new IncomingWaterDlg (id);
					break;
				case "Документ перемещения": 
					dlg = new MovementDocumentDlg (id);
					break;
				case "Акт списания":
					dlg = new WriteoffDocumentDlg (id);
					break;
				default:
					return;
				}
				TabParent.AddTab (dlg, this);
			}
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			OrmMain.DeleteObject(tableDocuments.GetSelectedObjects () [0]);
		}

		#endregion
	}
}

