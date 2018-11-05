using System;
using System.Linq;
using Gamma.Utilities;
using Gtk;
using QS.DomainModel.UoW;
using QS.Widgets.Gtk;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Dialogs.Goods
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclatureCharacteristicsView : Gtk.Bin
	{
		private IUnitOfWorkGeneric<Nomenclature> uow;

		public NomenclatureCharacteristicsView()
		{
			this.Build();
		}

		public IUnitOfWorkGeneric<Nomenclature> Uow {
			get => uow; set {
				uow = value;
				RefreshWidgets();
			}
		}

		public void ConfigureView(IUnitOfWorkGeneric<Nomenclature> uow)
		{
			this.Uow = uow;

		}

		public void RefreshWidgets()
		{
			foreach(var wid in tableCharacteristics.AllChildren.Cast<Widget>().ToList()) {
				Remove(wid);
				wid.Destroy();
			}

			if(uow.Root.ProductGroup != null) {
				tableCharacteristics.NRows = (uint)uow.Root.ProductGroup.Characteristics.Count;
				uint row = 0;
				foreach(var characteristic in uow.Root.ProductGroup.Characteristics) {
					var label = new Label(characteristic.GetEnumTitle() + ":");
					label.Xalign = 1;
					tableCharacteristics.Attach(label, 0, 1, row, row + 1, (AttachOptions)0, (AttachOptions)0, 0, 0);
					var charEntry = new FieldCompletionEntry();
					charEntry.Tag = characteristic;
					charEntry.Binding.AddBinding(uow.Root, characteristic.ToString(), w => w.Text).InitializeFromSource();
					tableCharacteristics.Attach(charEntry, 1, 2, row, row + 1, AttachOptions.Fill | AttachOptions.Expand, (AttachOptions)0, 0, 0);
					row++;
				}
				tableCharacteristics.ShowAll();
			}
		}
	}
}
