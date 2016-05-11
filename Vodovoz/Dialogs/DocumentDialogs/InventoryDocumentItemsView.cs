using System;
using QSOrmProject;
using Vodovoz.Domain.Documents;
using System.Collections.Generic;
using Gamma.GtkWidgets;
using Gamma.Utilities;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class InventoryDocumentItemsView : Gtk.Bin
	{
		public InventoryDocumentItemsView()
		{
			this.Build();

			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<InventoryDocumentItem>()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("Кол-во в учёте").AddTextRenderer(x => x.Nomenclature.Unit.MakeAmountShortStr(x.AmountInDB))
				.AddColumn("Кол-во по факту").AddNumericRenderer(x => x.AmountInFact).Editing()
				.AddSetter((w, x) => w.Digits = (uint)x.Nomenclature.Unit.Digits)
				.AddColumn("Разница").AddTextRenderer(x => x.Difference != 0 ? x.Nomenclature.Unit.MakeAmountShortStr(x.Difference) : String.Empty)
				.AddSetter((w, x) => w.Foreground = x.Difference < 0 ? "red" : "blue")
				.AddColumn("Что произошло").AddTextRenderer(x => x.Comment).Editable()
				.Finish();
		}

		private IUnitOfWorkGeneric<InventoryDocument> documentUoW;

		public IUnitOfWorkGeneric<InventoryDocument> DocumentUoW {
			get { return documentUoW; }
			set {
				if (documentUoW == value)
					return;
				documentUoW = value;
				if (DocumentUoW.Root.Items == null)
					DocumentUoW.Root.Items = new List<InventoryDocumentItem> ();
				
				ytreeviewItems.ItemsDataSource = DocumentUoW.Root.ObservableItems;
				UpdateButtonState();
				DocumentUoW.Root.PropertyChanged += DocumentUoW_Root_PropertyChanged;
			}
		}

		void DocumentUoW_Root_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == DocumentUoW.Root.GetPropertyName(x => x.Warehouse))
				UpdateButtonState();
		}

		protected void OnButtonFillItemsClicked(object sender, EventArgs e)
		{
			DocumentUoW.Root.FillItemsFromStock(DocumentUoW);
		}

		private void UpdateButtonState()
		{
			buttonFillItems.Sensitive = DocumentUoW.IsNew && DocumentUoW.Root.Warehouse != null;
		}
	}
}

