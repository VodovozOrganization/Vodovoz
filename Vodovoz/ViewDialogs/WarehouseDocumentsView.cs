using System;
using System.Collections.Generic;
using Gtk;
using NHibernate;
using NLog;
using QSOrmProject;
using QSOrmProject.UpdateNotification;
using QSTDI;
using Vodovoz.Domain.Documents;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class WarehouseDocumentsView : Gtk.Bin, ITdiJournal
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();
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
			treeDocuments.Selection.Changed += OnSelectionChanged;
			buttonEdit.Sensitive = buttonDelete.Sensitive = false;
			UpdateTable ();
			IOrmObjectMapping map = OrmMain.GetObjectDiscription (typeof(IncomingInvoice));
			if (map != null)
				map.ObjectUpdated += OnRefObjectUpdated;
			map = OrmMain.GetObjectDiscription (typeof(IncomingWater));
			if (map != null)
				map.ObjectUpdated += OnRefObjectUpdated;
			map = OrmMain.GetObjectDiscription (typeof(MovementDocument));
			if (map != null)
				map.ObjectUpdated += OnRefObjectUpdated;
			map = OrmMain.GetObjectDiscription (typeof(WriteoffDocument));
			if (map != null)
				map.ObjectUpdated += OnRefObjectUpdated;
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

		void OnRefObjectUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{
			logger.Info ("Получаем таблицу справочника<{0}>...", typeof(Document).Name);
			UpdateTable ();
			logger.Info ("Ok.");
		}

		void UpdateTable ()
		{
			documents = new List<Document> ();
			Session.Clear ();
			documents.AddRange (Session.CreateCriteria<WriteoffDocument> ().List<WriteoffDocument> ());
			documents.AddRange (Session.CreateCriteria<IncomingInvoice> ().List<IncomingInvoice> ());
			documents.AddRange (Session.CreateCriteria<IncomingWater> ().List<IncomingWater> ());
			documents.AddRange (Session.CreateCriteria<MovementDocument> ().List<MovementDocument> ());
			treeDocuments.ItemsDataSource = documents;
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
			OrmMain.DeleteObject(treeDocuments.GetSelectedObjects () [0]);
		}

		#endregion
	}
}

