using System;
using QSOrmProject;
using QSTDI;
using NHibernate;
using QSValidation;
using System.Data.Bindings;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AddInvoiceItemDlg : Gtk.Bin, ITdiDialog, IOrmSlaveDialog
	{
		Adaptor adaptor = new Adaptor ();

		OrmParentReference parentReference;

		public OrmParentReference ParentReference {
			get { return parentReference; }
			set {
				parentReference = value;
				if (!(ParentReference.ParentObject is IncomingInvoice))
					throw new ArgumentException (String.Format ("Родительский объект в parentReference должен являться классом {0}", typeof(IncomingInvoice)));
			}
		}

		IncomingInvoiceItem subject;

		public object Subject {
			get { return subject; }
			set { subject = value as IncomingInvoiceItem; }
		}

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;

		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		string _tabName = "Номенклатура для входящей накладной";

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

		public ITdiTabParent TabParent { get ; set; }

		public bool Save ()
		{
			var valid = new QSValidator<IncomingInvoiceItem> (subject);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			OrmMain.DelayedNotifyObjectUpdated (ParentReference.ParentObject, subject);
			return true;
		}

		public bool HasChanges {
			get { return false; }
		}

		public AddInvoiceItemDlg (OrmParentReference parentReference, IncomingInvoiceItem item)
		{
			this.Build ();
			Subject = item;
			ParentReference = parentReference;
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
		}

		protected void OnReferenceNomenclatureChanged (object sender, EventArgs e)
		{
			if (subject.Nomenclature != null)
				referenceEquipment.Sensitive = subject.Nomenclature.Serial;
		}

		public override void Destroy ()
		{
			adaptor.Disconnect ();
			base.Destroy ();
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			if (!this.HasChanges || Save ())
				OnCloseTab (false);
		}

		protected void OnCloseTab (bool askSave)
		{
			if (CloseTab != null)
				CloseTab (this, new TdiTabCloseEventArgs (askSave));
		}
	}
}

