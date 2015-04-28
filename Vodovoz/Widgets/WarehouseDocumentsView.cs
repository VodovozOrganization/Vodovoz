using System;
using QSTDI;
using NHibernate;
using QSOrmProject;
using System.Collections.Generic;
using Gtk;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class WarehouseDocumentsView : Gtk.Bin, ITdiTab
	{
		List<Document> documents;
		ISession session;

		public ISession Session {
			get { 
				if (session == null)
					Session = OrmMain.Sessions.OpenSession ();
				return session;
			}
			set { session = value; }
		}

		public WarehouseDocumentsView ()
		{
			this.Build ();
			documents = new List<Document> ();
			documents.AddRange (Session.CreateCriteria<WriteoffDocument> ().List<WriteoffDocument> ());
			documents.AddRange (Session.CreateCriteria<IncomingInvoice> ().List<IncomingInvoice> ());
			documents.AddRange (Session.CreateCriteria<IncomingWater> ().List<IncomingWater> ());
			documents.AddRange (Session.CreateCriteria<MovementDocument> ().List<MovementDocument> ());
			treeDocuments.ItemsDataSource = documents;
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

		#endregion

		protected void OnButtonAddEnumItemClicked (object sender, EnumItemClickedEventArgs e)
		{
			Document document = new Document ();
			ITdiDialog dlg;

			DocumentType type = (DocumentType)e.ItemEnum;	
			switch (type) {
			case DocumentType.IncomingInvoice:
				document = new IncomingInvoice ();
				dlg = new IncomingInvoiceDlg ();
				break;
			case DocumentType.IncomingWater:
				document = new IncomingWater ();
				dlg = new IncomingWaterDlg ();
				break;
			case DocumentType.MovementDocument:
				document = new MovementDocument ();
				dlg = new MovementDocumentDlg ();
				break;
			case DocumentType.WriteoffDocument:
				document = new WriteoffDocument ();
				dlg = new WriteoffDocumentDlg ();
				break;
			default:
				throw new NotSupportedException ("Тип документа не поддерживается.");
			}
			OrmSimpleDialog.RunSimpleDialog ((Window)this.Toplevel, document.GetType (), document);
		}

	}
}

