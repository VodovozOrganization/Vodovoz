using System;
using System.IO;
using QSTDI;
using QSOrmProject;
using NHibernate;
using System.Data.Bindings;
using NLog;
using Gtk;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeDlg : Gtk.Bin, QSTDI.ITdiDialog, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession session;
		private Adaptor adaptorEmployee = new Adaptor();
		private Employee subject;
		private bool NewItem = false;

		public ITdiTabParent TabParent { set; get;}

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;
		public bool HasChanges { 
			get{return NewItem || Session.IsDirty();}
		}

		private string _tabName = "Новый сотрудник";
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
				if (value is Employee)
					subject = value as Employee;
			}
		}

		public EmployeeDlg()
		{
			this.Build();
			NewItem = true;
			subject = new Employee();
			ConfigureDlg();
		}

		public EmployeeDlg(int id)
		{
			this.Build();
			subject = Session.Load<Employee>(id);
			TabName = subject.Name;
			ConfigureDlg();
		}

		public EmployeeDlg(Employee sub)
		{
			this.Build();
			subject = Session.Load<Employee>(sub.Id);
			TabName = subject.Name;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			adaptorEmployee.Target = subject;
			datatableMain.DataSource = adaptorEmployee;
			dataenumcomboCategory.DataSource = adaptorEmployee;
			subject.PropertyChanged += OnPropertyChanged;
			notebookMain.Page = 0;
			notebookMain.ShowTabs = false;
			referenceNationality.SubjectType = typeof(Nationality);
			referenceUser.SubjectType = typeof(User);
			referenceUser.CanEditReference = false;
		}

		void OnPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			logger.Debug("Property {0} changed", e.PropertyName);
		}

		public bool SaveButton()
		{
			logger.Info("Сохраняем сотрудника...");
			try
			{
				Session.SaveOrUpdate(subject);
				Session.Flush();
			}
			catch( Exception ex)
			{
				logger.ErrorException("Не удалось записать сотрудника.", ex);
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
			//adaptorOrg.Disconnect();
			base.Destroy();
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			if (!this.HasChanges || SaveButton())
				OnCloseTab(false);
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}

		protected void OnCloseTab(bool askSave)
		{
			if (CloseTab != null)
				CloseTab(this, new TdiTabCloseEventArgs(askSave));
		}

		protected void OnRadioTabInfoToggled(object sender, EventArgs e)
		{
			if (radioTabInfo.Active)
				notebookMain.CurrentPage = 0;
		}

		protected void OnRadioTabAccountsToggled(object sender, EventArgs e)
		{
			if (radioTabFiles.Active)
				notebookMain.CurrentPage = 1;
		}

		protected void OnButtonLoadClicked(object sender, EventArgs e)
		{
			FileChooserDialog Chooser = new FileChooserDialog("Выберите фото для загрузки...", 
				(Window) this.Toplevel,
				FileChooserAction.Open,
				"Отмена", ResponseType.Cancel,
				"Загрузить", ResponseType.Accept );

			FileFilter Filter = new FileFilter();
			Filter.AddPixbufFormats ();
			Filter.Name = "Все изображения";
			Chooser.AddFilter(Filter);

			if((ResponseType) Chooser.Run () == ResponseType.Accept)
			{
				Chooser.Hide();
				logger.Info("Загрузка фотографии...");

				FileStream fs = new FileStream(Chooser.Filename, FileMode.Open, FileAccess.Read);
				if(Chooser.Filename.ToLower().EndsWith (".jpg"))
				{
					using (MemoryStream ms = new MemoryStream())
					{
						fs.CopyTo(ms);
						subject.Photo = ms.ToArray();
					}
				}
				else 
				{
					logger.Info("Конвертация в jpg ...");
					Gdk.Pixbuf image = new Gdk.Pixbuf(fs);
					subject.Photo = image.SaveToBuffer("jpeg");
				}
				fs.Close();
				//ImageChanged = true;
				//ReadImage();
				buttonSavePhoto.Sensitive = true;
				logger.Info("Ok");
			}
			Chooser.Destroy ();

		}
	}
}

