using System;
using System.IO;
using QSTDI;
using QSOrmProject;
using NHibernate;
using System.Data.Bindings;
using NLog;
using Gtk;
using QSValidation;
using System.Collections.Generic;
using QSContacts;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class EmployeeDlg : OrmGtkDialogBase<Employee>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public EmployeeDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Employee> ();
			TabName = "Новый сотрудник";
			ConfigureDlg ();
		}

		public EmployeeDlg (int id)
		{
			this.Build ();
			logger.Info ("Загрузка информации о сотруднике...");
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Employee> (id);
			ConfigureDlg ();
		}

		public EmployeeDlg (Employee sub) : this (sub.Id)
		{
		}

		private void ConfigureDlg ()
		{
			datatableMain.DataSource = subjectAdaptor;
			dataenumcomboCategory.DataSource = subjectAdaptor;
			dataentryPassportSeria.MaxLength = 5;
			dataentryPassportNumber.MaxLength = 6;
			dataentryDrivingNumber.MaxLength = 10;
			UoWGeneric.Root.PropertyChanged += OnPropertyChanged;
			notebookMain.Page = 0;
			notebookMain.ShowTabs = false;
			referenceNationality.SubjectType = typeof(Nationality);
			referenceUser.SubjectType = typeof(User);
			referenceUser.CanEditReference = false;
			attachmentFiles.AttachToTable = OrmMain.GetDBTableName (typeof(Employee));
			if (!UoWGeneric.IsNew) {
				attachmentFiles.ItemId = UoWGeneric.Root.Id;
				attachmentFiles.UpdateFileList ();
			}
			phonesView.Session = Session;
			if (UoWGeneric.Root.Phones == null)
				UoWGeneric.Root.Phones = new List<Phone> ();
			phonesView.Phones = UoWGeneric.Root.Phones;
			buttonSavePhoto.Sensitive = UoWGeneric.Root.Photo != null;
			logger.Info ("Ok");
		}

		public override bool HasChanges { 
			get { return UoWGeneric.HasChanges || attachmentFiles.HasChanges; }
		}

		void OnPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			logger.Debug ("Property {0} changed", e.PropertyName);
		}

		public override bool Save ()
		{
			var valid = new QSValidator<Employee> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;
			phonesView.SaveChanges ();	
			logger.Info ("Сохраняем сотрудника...");
			try {
				UoWGeneric.Save ();
				if (UoWGeneric.IsNew) {
					attachmentFiles.ItemId = UoWGeneric.Root.Id;
				}
				attachmentFiles.SaveChanges ();
			} catch (Exception ex) {
				logger.ErrorException ("Не удалось записать сотрудника.", ex);
				QSProjectsLib.QSMain.ErrorMessage ((Gtk.Window)this.Toplevel, ex);
				return false;
			}
			logger.Info ("Ok");
			return true;

		}

		protected void OnRadioTabInfoToggled (object sender, EventArgs e)
		{
			if (radioTabInfo.Active)
				notebookMain.CurrentPage = 0;
		}

		protected void OnRadioTabAccountsToggled (object sender, EventArgs e)
		{
			if (radioTabFiles.Active)
				notebookMain.CurrentPage = 1;
		}

		protected void OnButtonLoadClicked (object sender, EventArgs e)
		{
			FileChooserDialog Chooser = new FileChooserDialog ("Выберите фото для загрузки...", 
				                            (Window)this.Toplevel,
				                            FileChooserAction.Open,
				                            "Отмена", ResponseType.Cancel,
				                            "Загрузить", ResponseType.Accept);

			FileFilter Filter = new FileFilter ();
			Filter.AddPixbufFormats ();
			Filter.Name = "Все изображения";
			Chooser.AddFilter (Filter);

			if ((ResponseType)Chooser.Run () == ResponseType.Accept) {
				Chooser.Hide ();
				logger.Info ("Загрузка фотографии...");

				FileStream fs = new FileStream (Chooser.Filename, FileMode.Open, FileAccess.Read);
				if (Chooser.Filename.ToLower ().EndsWith (".jpg")) {
					using (MemoryStream ms = new MemoryStream ()) {
						fs.CopyTo (ms);
						UoWGeneric.Root.Photo = ms.ToArray ();
					}
				} else {
					logger.Info ("Конвертация в jpg ...");
					Gdk.Pixbuf image = new Gdk.Pixbuf (fs);
					UoWGeneric.Root.Photo = image.SaveToBuffer ("jpeg");
				}
				fs.Close ();
				buttonSavePhoto.Sensitive = true;
				logger.Info ("Ok");
			}
			Chooser.Destroy ();

		}

		protected void OnButtonSavePhotoClicked (object sender, EventArgs e)
		{
			FileChooserDialog fc =
				new FileChooserDialog ("Укажите файл для сохранения фотографии",
					(Window)this.Toplevel,
					FileChooserAction.Save,
					"Отмена", ResponseType.Cancel,
					"Сохранить", ResponseType.Accept);
			fc.CurrentName = dataentryLastName.Text + " " + dataentryName.Text + " " + dataentryPatronymic.Text + ".jpg";
			fc.Show (); 
			if (fc.Run () == (int)ResponseType.Accept) {
				fc.Hide ();
				FileStream fs = new FileStream (fc.Filename, FileMode.Create, FileAccess.Write);
				fs.Write (UoWGeneric.Root.Photo, 0, UoWGeneric.Root.Photo.Length);
				fs.Close ();
			}
			fc.Destroy ();
		}

		protected void OnDataimageviewerPhotoButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			if (((Gdk.EventButton)args.Event).Type == Gdk.EventType.TwoButtonPress) {
				string filePath = System.IO.Path.Combine (System.IO.Path.GetTempPath (), "temp_img.jpg");
				FileStream fs = new FileStream (filePath, FileMode.Create, FileAccess.Write);
				fs.Write (UoWGeneric.Root.Photo, 0, UoWGeneric.Root.Photo.Length);
				fs.Close ();
				System.Diagnostics.Process.Start (filePath);
			}
		}
	}
}

