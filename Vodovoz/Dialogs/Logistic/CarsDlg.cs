using System;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz
{
	public partial class CarsDlg : OrmGtkDialogBase<Car>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public override bool HasChanges { 
			get { return UoWGeneric.HasChanges || attachmentFiles.HasChanges; }
		}

		public CarsDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Car>();
			TabName = "Новый автомобиль";
			ConfigureDlg ();
		}

		public CarsDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Car> (id);
			ConfigureDlg ();
		}

		public CarsDlg (Car sub) : this (sub.Id) {}

		private void ConfigureDlg ()
		{
			notebook1.Page = 0;
			notebook1.ShowTabs = false;
			tableCarData.DataSource = subjectAdaptor;
			dataentryModel.IsEditable = true;
			dataentryRegNumber.IsEditable = true;
			dataentryreferenceDriver.SubjectType = typeof(Employee);
			dataentryFuelType.PropertyMapping<Car> (c => c.FuelType);
			radiobuttonMain.Active = true;

			photoviewCar.Binding.AddBinding(Entity, e => e.Photo, w => w.ImageFile).InitializeFromSource();
			photoviewCar.GetSaveFileName = () => String.Format("{0}({1})", Entity.Model, Entity.RegistrationNumber);

			checkIsCompanyHavings.Binding.AddBinding(Entity, e => e.IsCompanyHavings, w => w.Active).InitializeFromSource();

			attachmentFiles.AttachToTable = OrmMain.GetDBTableName (typeof(Car));
			if (!UoWGeneric.IsNew) {
				attachmentFiles.ItemId = UoWGeneric.Root.Id;
				attachmentFiles.UpdateFileList ();
			}
			OnDataentryreferenceDriverChanged (null, null);
			textDriverInfo.Selectable = true;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<Car> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем автомобиль...");
			try {
				UoWGeneric.Save();
				if (UoWGeneric.IsNew) {
					attachmentFiles.ItemId = UoWGeneric.Root.Id;
				}
				attachmentFiles.SaveChanges ();
			} catch (Exception ex) {
				logger.Error (ex, "Не удалось записать Автомобиль.");
				QSProjectsLib.QSMain.ErrorMessage ((Gtk.Window)this.Toplevel, ex);
				return false;
			}
			logger.Info ("Ok");
			return true;

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

		protected void OnDataentryreferenceDriverChanged (object sender, EventArgs e)
		{
			if (UoWGeneric.Root.Driver != null)
				textDriverInfo.Text = "\tПаспорт: " + UoWGeneric.Root.Driver.PassportSeria + " № " + UoWGeneric.Root.Driver.PassportNumber +
					"\n\tАдрес регистрации: " + UoWGeneric.Root.Driver.AddressRegistration;
		}

		protected void OnCheckIsCompanyHavingsToggled (object sender, EventArgs e)
		{
			Entity.IsCompanyHavings = checkIsCompanyHavings.Active;
			dataentryreferenceDriver.Sensitive = !Entity.IsCompanyHavings;

			if (Entity.IsCompanyHavings)
				Entity.Driver = null;
		}
	}
}

