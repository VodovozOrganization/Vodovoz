// Этот файл сгенерирован вместе с описанием формы в gui.stetic.
namespace Vodovoz.Views.Warehouse
{
	public partial class TtnDataView
	{
		private global::Gtk.VBox vboxMain;

		private global::Gtk.HBox hboxCargoSender;

		private global::Gtk.Label labelCargoSender;

		private global::QS.Views.Control.EntityEntry entryCargoSender;

		private global::Gtk.HBox hboxCargoReceiver;

		private global::Gtk.Label labelCargoReceiver;

		private global::QS.Views.Control.EntityEntry entryCargoReceiver;

		private global::Gtk.HBox hboxPayer;

		private global::Gtk.Label labelPayer;

		private global::QS.Views.Control.EntityEntry entryPayer;

		private global::Gtk.HBox hboxCar;

		private global::Gtk.Label labelCar;

		private global::QS.Views.Control.EntityEntry entryCar;

		private global::Gtk.HBox hboxTrailer;

		private global::Gtk.Label labelTrailer;

		private global::QS.Views.Control.EntityEntry entryTrailer;

		private global::Gtk.HBox hboxDriver;

		private global::Gtk.Label labelDriver;

		private global::QS.Views.Control.EntityEntry entryDriver;

		private global::Gtk.HBox hboxButtons;

		private global::Gamma.GtkWidgets.yButton buttonAccept;

		private global::Gamma.GtkWidgets.yButton buttonCancel;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			global::Stetic.BinContainer.Attach(this);
			this.Name = "Vodovoz.Views.Warehouse.TtnDataView";
			this.vboxMain = new global::Gtk.VBox();
			this.vboxMain.Name = "vboxMain";
			this.vboxMain.Spacing = 6;
			this.vboxMain.BorderWidth = 6;
			this.hboxCargoSender = new global::Gtk.HBox();
			this.hboxCargoSender.Name = "hboxCargoSender";
			this.hboxCargoSender.Spacing = 6;
			this.labelCargoSender = new global::Gtk.Label();
			this.labelCargoSender.Name = "labelCargoSender";
			this.labelCargoSender.WidthRequest = 150;
			this.labelCargoSender.Xalign = 1F;
			this.labelCargoSender.LabelProp = global::Mono.Unix.Catalog.GetString("Грузоотправитель:");
			this.hboxCargoSender.PackStart(this.labelCargoSender, false, false, 0);
			this.entryCargoSender = new global::QS.Views.Control.EntityEntry();
			this.entryCargoSender.Name = "entryCargoSender";
			this.entryCargoSender.WidthRequest = 420;
			this.hboxCargoSender.PackStart(this.entryCargoSender, true, true, 0);
			this.vboxMain.PackStart(this.hboxCargoSender, false, false, 0);
			this.hboxCargoReceiver = new global::Gtk.HBox();
			this.hboxCargoReceiver.Name = "hboxCargoReceiver";
			this.hboxCargoReceiver.Spacing = 6;
			this.labelCargoReceiver = new global::Gtk.Label();
			this.labelCargoReceiver.Name = "labelCargoReceiver";
			this.labelCargoReceiver.WidthRequest = 150;
			this.labelCargoReceiver.Xalign = 1F;
			this.labelCargoReceiver.LabelProp = global::Mono.Unix.Catalog.GetString("Грузополучатель:");
			this.hboxCargoReceiver.PackStart(this.labelCargoReceiver, false, false, 0);
			this.entryCargoReceiver = new global::QS.Views.Control.EntityEntry();
			this.entryCargoReceiver.Name = "entryCargoReceiver";
			this.entryCargoReceiver.WidthRequest = 420;
			this.hboxCargoReceiver.PackStart(this.entryCargoReceiver, true, true, 0);
			this.vboxMain.PackStart(this.hboxCargoReceiver, false, false, 0);
			this.hboxPayer = new global::Gtk.HBox();
			this.hboxPayer.Name = "hboxPayer";
			this.hboxPayer.Spacing = 6;
			this.labelPayer = new global::Gtk.Label();
			this.labelPayer.Name = "labelPayer";
			this.labelPayer.WidthRequest = 150;
			this.labelPayer.Xalign = 1F;
			this.labelPayer.LabelProp = global::Mono.Unix.Catalog.GetString("Плательщик:");
			this.hboxPayer.PackStart(this.labelPayer, false, false, 0);
			this.entryPayer = new global::QS.Views.Control.EntityEntry();
			this.entryPayer.Name = "entryPayer";
			this.entryPayer.WidthRequest = 420;
			this.hboxPayer.PackStart(this.entryPayer, true, true, 0);
			this.vboxMain.PackStart(this.hboxPayer, false, false, 0);
			this.hboxCar = new global::Gtk.HBox();
			this.hboxCar.Name = "hboxCar";
			this.hboxCar.Spacing = 6;
			this.labelCar = new global::Gtk.Label();
			this.labelCar.Name = "labelCar";
			this.labelCar.WidthRequest = 150;
			this.labelCar.Xalign = 1F;
			this.labelCar.LabelProp = global::Mono.Unix.Catalog.GetString("Автомобиль:");
			this.hboxCar.PackStart(this.labelCar, false, false, 0);
			this.entryCar = new global::QS.Views.Control.EntityEntry();
			this.entryCar.Name = "entryCar";
			this.entryCar.WidthRequest = 420;
			this.hboxCar.PackStart(this.entryCar, true, true, 0);
			this.vboxMain.PackStart(this.hboxCar, false, false, 0);
			this.hboxTrailer = new global::Gtk.HBox();
			this.hboxTrailer.Name = "hboxTrailer";
			this.hboxTrailer.Spacing = 6;
			this.labelTrailer = new global::Gtk.Label();
			this.labelTrailer.Name = "labelTrailer";
			this.labelTrailer.WidthRequest = 150;
			this.labelTrailer.Xalign = 1F;
			this.labelTrailer.LabelProp = global::Mono.Unix.Catalog.GetString("Прицеп:");
			this.hboxTrailer.PackStart(this.labelTrailer, false, false, 0);
			this.entryTrailer = new global::QS.Views.Control.EntityEntry();
			this.entryTrailer.Name = "entryTrailer";
			this.entryTrailer.WidthRequest = 420;
			this.hboxTrailer.PackStart(this.entryTrailer, true, true, 0);
			this.vboxMain.PackStart(this.hboxTrailer, false, false, 0);
			this.hboxDriver = new global::Gtk.HBox();
			this.hboxDriver.Name = "hboxDriver";
			this.hboxDriver.Spacing = 6;
			this.labelDriver = new global::Gtk.Label();
			this.labelDriver.Name = "labelDriver";
			this.labelDriver.WidthRequest = 150;
			this.labelDriver.Xalign = 1F;
			this.labelDriver.LabelProp = global::Mono.Unix.Catalog.GetString("Водитель:");
			this.hboxDriver.PackStart(this.labelDriver, false, false, 0);
			this.entryDriver = new global::QS.Views.Control.EntityEntry();
			this.entryDriver.Name = "entryDriver";
			this.entryDriver.WidthRequest = 420;
			this.hboxDriver.PackStart(this.entryDriver, true, true, 0);
			this.vboxMain.PackStart(this.hboxDriver, false, false, 0);
			this.hboxButtons = new global::Gtk.HBox();
			this.hboxButtons.Name = "hboxButtons";
			this.hboxButtons.Spacing = 6;
			this.buttonAccept = new global::Gamma.GtkWidgets.yButton();
			this.buttonAccept.Name = "buttonAccept";
			this.buttonAccept.CanFocus = true;
			this.buttonAccept.UseUnderline = true;
			this.buttonAccept.Label = global::Mono.Unix.Catalog.GetString("Принять");
			this.hboxButtons.PackStart(this.buttonAccept, false, false, 0);
			this.buttonCancel = new global::Gamma.GtkWidgets.yButton();
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = global::Mono.Unix.Catalog.GetString("Отмена");
			this.hboxButtons.PackStart(this.buttonCancel, false, false, 0);
			this.vboxMain.PackStart(this.hboxButtons, false, false, 0);
			this.Add(this.vboxMain);
			if(this.Child != null)
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}
