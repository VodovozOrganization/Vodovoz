using System;
using QSOrmProject;
using NLog;
using NHibernate;
using QSTDI;
using System.Data.Bindings;
using QSValidation;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CarsDlg : Gtk.Bin, QSTDI.ITdiDialog, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession session;
		private Adaptor adaptorCar = new Adaptor();
		private Car subject;
		private bool NewItem = false;

		public ITdiTabParent TabParent { set; get;}

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;
		public bool HasChanges { 
			get{return NewItem || Session.IsDirty() || attachmentFiles.HasChanges;}
		}

		private string _tabName = "Новый автомобиль";
		public string TabName
		{
			get{return _tabName;}
			set{
				if (_tabName == value)
					return;
				_tabName = value;
				if (TabNameChanged != null)
					TabNameChanged(this, new TdiTabNameChangedEventArgs(value));
			}

		}

		public ISession Session
		{
			get
			{
				if (session == null)
					Session = OrmMain.Sessions.OpenSession();
				return session;
			}
			set
			{
				session = value;
			}
		}

		public object Subject
		{
			get {return subject;}
			set {
				if (value is Car)
					subject = value as Car;
			}
		}

		public CarsDlg()
		{
			this.Build();
			NewItem = true;
			subject = new Car();
			ConfigureDlg();
		}

		public CarsDlg(int id)
		{
			this.Build();
			subject = Session.Load<Car>(id);
			TabName = subject.Model + " - " + subject.RegistrationNumber;
			ConfigureDlg();
		}

		public CarsDlg(Car sub)
		{
			this.Build();
			subject = Session.Load<Car>(sub.Id);
			TabName = subject.Model + " - " + subject.RegistrationNumber;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			notebook1.Page = 0;
			notebook1.ShowTabs = false;
			adaptorCar.Target = subject;
			tableCarData.DataSource = adaptorCar;
			dataentryModel.IsEditable = true;
			dataentryRegNumber.IsEditable = true;
			dataentryreferenceDriver.SubjectType = typeof(Employee);
			radiobuttonMain.Active = true;

			attachmentFiles.AttachToTable = OrmMain.GetDBTableName(typeof(Car));
			if(!NewItem)
			{
				attachmentFiles.ItemId = subject.Id;
				attachmentFiles.UpdateFileList();
			}
		}

		public bool Save()
		{
			var valid = new QSValidator<Car> (subject);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info("Сохраняем автомобиль...");
			try
			{
				Session.SaveOrUpdate(subject);
				Session.Flush();
				if(NewItem)
				{
					attachmentFiles.ItemId = subject.Id;
				}
				attachmentFiles.SaveChanges();
			}
			catch( Exception ex)
			{
				logger.ErrorException("Не удалось записать Автомобиль.", ex);
				QSProjectsLib.QSMain.ErrorMessage((Gtk.Window)this.Toplevel, ex);
				return false;
			}
			OrmMain.NotifyObjectUpdated(subject);
			logger.Info("Ok");
			return true;

		}

		public override void Destroy()
		{
			Session.Close();
			adaptorCar.Disconnect();
			base.Destroy();
		}

		protected void OnCloseTab(bool askSave)
		{
			if (CloseTab != null)
				CloseTab(this, new TdiTabCloseEventArgs(askSave));
		}
			
		protected void OnRadiobuttonFilesToggled (object sender, EventArgs e)
		{
			if (radiobuttonFiles.Active)
				notebook1.CurrentPage = 1;
		}

		protected void OnRadiobuttonMainToggled (object sender, EventArgs e)
		{
			if (radiobuttonMain.Active)
				notebook1.CurrentPage = 0;
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			OnCloseTab (false);
		}
			
		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			if (!this.HasChanges || Save())
				OnCloseTab (false);
		}
	}
}

