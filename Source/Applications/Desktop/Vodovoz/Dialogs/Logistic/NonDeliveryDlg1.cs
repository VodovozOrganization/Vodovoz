using System;
namespace Vodovoz.Dialogs.Logistic
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class Widget : Gtk.Bin
	{

		//#region поля

		////private static Logger logger = LogManager.GetCurrentClassLogger ();

		//private Track track = null;
		//private decimal balanceBeforeOp = default (decimal);

		////List<RouteListRepository.ReturnsNode> allReturnsToWarehouse;
		//int bottlesReturnedToWarehouse;
		//int bottlesReturnedTotal;

		////enum RouteListActions
		////{
		////	[Display (Name = "Новый штраф")]
		////	CreateNewFine,
		////	[Display (Name = "Перенести разгрузку в другой МЛ")]
		////	TransferReceptionToAnotherRL,
		////	[Display (Name = "Перенести разгрузку в этот МЛ")]
		////	TransferReceptionToThisRL,
		////	[Display (Name = "Перенести адреса в этот МЛ")]
		////	TransferAddressesToThisRL,
		////	[Display (Name = "Перенести адреса из этого МЛ")]
		////	TransferAddressesToAnotherRL

		////}

		//#endregion

		//#region Конструкторы и конфигурирование диалога

		//public NonDeliveryDlg (NonDelivery nDeliveryList) : this (nDeliveryList.Id) { }

		//public NonDeliveryDlg (int id)
		//{
		//	this.Build ();
		//	UoWGeneric = UnitOfWorkFactory.CreateForRoot<NonDelivery> (id);
		//	TabName = String.Format ("Создание недовоза{0}", Entity.Id);
		//	ConfigureDlg ();
		//}

		//public override bool Save ()
		//{
		//	throw new NotImplementedException ();
		//}

		//void ConfigureDlg ()
		//{
		//	//speccomboShift.ItemsList = GuilPerson.				

		//	//enumWarrantyType.Binding
		//	//	.AddBinding (UoWGeneric.Root, equipmentType => equipmentType.WarrantyCardType, widget => widget.SelectedItem)
		//	//	.InitializeFromSource ();

		//	//speccomboShift.ItemsList =   DeliveryShiftRepository.ActiveShifts (UoW);
		//	//speccomboShift.Binding.AddBinding (Entity, rl => rl.Guilt, widget => widget.SelectedItem).InitializeFromSource ();
		//	//speccomboShift.Sensitive = editing;

		//	//speccomboShift.Binding.AddBinding (Entity, e => e.Guilt, w => w.Text).InitializeFromSource ();
		//	//dataentryRegNumber.Binding.AddBinding (Entity, e => e.RegistrationNumber, w => w.Text).InitializeFromSource ();

		//	//dataentryreferenceDriver.SubjectType = typeof (Employee);
		//	//dataentryreferenceDriver.Binding.AddBinding (Entity, e => e.Driver, w => w.Subject).InitializeFromSource ();

		//	//dataentryFuelType.SubjectType = typeof (FuelType);
		//	//dataentryFuelType.Binding.AddBinding (Entity, e => e.FuelType, w => w.Subject).InitializeFromSource ();
		//	//radiobuttonMain.Active = true;

		//	//dataspinbutton1.Binding.AddBinding (Entity, e => e.FuelConsumption, w => w.Value).InitializeFromSource ();

		//	//photoviewCar.Binding.AddBinding (Entity, e => e.Photo, w => w.ImageFile).InitializeFromSource ();
		//	//photoviewCar.GetSaveFileName = () => String.Format ("{0}({1})", Entity.Model, Entity.RegistrationNumber);
		//}
		//public Widget ()
		//{
		//	this.Build ();
		//}
	}
}
