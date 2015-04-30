using System;
using QSTDI;
using NHibernate;
using QSOrmProject;
using QSOrmProject.Deletion;
using System.Collections.Generic;
using Gtk;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class WarehouseDocumentsView : Gtk.Bin, ITdiJournal
	{
		List<Document> documents;
		ISession session;

		public ISession Session {
			get { 
				if (session == null)
					Session = OrmMain.OpenSession ();
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
			treeDocuments.Selection.Changed += OnSelectionChanged;
			buttonEdit.Sensitive = buttonDelete.Sensitive = false;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			buttonEdit.Sensitive = buttonDelete.Sensitive = treeDocuments.Selection.CountSelectedRows () > 0;
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

		#region ITdiJournal implementation

		public event EventHandler<TdiOpenObjDialogEventArgs> OpenObjDialog;
		public event EventHandler<TdiOpenObjDialogEventArgs> DeleteObj;

		#endregion

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


		protected void OnTreeDocumentsRowActivated (object o, RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}


		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			if (TabParent.BeforeCreateNewTab ((object)null, null).HasFlag (TdiBeforeCreateResultFlag.Canceled))
				return;
			TabParent.AddTab (OrmMain.CreateObjectDialog (treeDocuments.GetSelectedObjects () [0]), this);
		}


		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			
			DeleteDlg delete = new DeleteDlg ();
			delete.RunDeletion (treeDocuments.GetSelectedObjects () [0].GetType (), (treeDocuments.GetSelectedObjects () [0] as IDomainObject).Id);
		}

		#endregion
	}
}

