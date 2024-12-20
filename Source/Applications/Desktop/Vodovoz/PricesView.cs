using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Bindings;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Widgets;
using Gtk;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PricesView : Gtk.Bin
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private IList<NomenclaturePriceBase> prices;
		private uint RowNum;

		public IList<NomenclaturePriceBase> Prices
		{
			get {
				return prices;
			}
			set {
				if (prices == value)
					return;
				if (PricesList != null)
					CleanList();
				prices = value;
				buttonAdd.Sensitive = prices != null;
				if (value != null) {
					PricesList = new GenericObservableList<NomenclaturePriceBase> (prices);
					PricesList.ElementAdded += OnEmailListElementAdded;
					PricesList.ElementRemoved += OnEmailListElementRemoved;

					foreach (NomenclaturePriceBase price in PricesList)
						AddPriceRow (price);
				}
			}
		}

		private void OnEmailListElementRemoved (object aList, int[] aIdx, object aObject)
		{
			Widget foundWidget = null;
			foreach(Widget wid in datatablePrices.AllChildren)
			{
				if(wid is yValidatedEntry && (wid as yValidatedEntry).Tag == aObject)
				{
					foundWidget = wid;
					break;
				}
			}
			if(foundWidget == null)
			{
				logger.Warn("Не найден виджет ассоциированный с удаленной ценой.");
				return;
			}

			Table.TableChild child = ((Table.TableChild)(this.datatablePrices [foundWidget]));
			RemoveRow(child.TopAttach);
		}

		private void OnEmailListElementAdded (object aList, int[] aIdx)
		{
			foreach(int i in aIdx)
			{
				AddPriceRow(PricesList[i]);
			}
		}

		public PricesView ()
		{
			this.Build ();
			datatablePrices.NRows = RowNum = 0;
		}
		
		public GenericObservableList<NomenclaturePriceBase> PricesList { get; private set; }

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			NomenclaturePriceBase price;

			switch(NomenclaturePriceType)
			{
				case NomenclaturePriceBase.NomenclaturePriceType.General:
					price = new NomenclaturePrice();
					break;
				case NomenclaturePriceBase.NomenclaturePriceType.Alternative:
					price = new AlternativeNomenclaturePrice();
					break;
				default:
					throw new NotSupportedException($"{NomenclaturePriceType} не поддерживается");
			}

			PricesList.Add(price);
		}

		private void AddPriceRow(NomenclaturePriceBase newPrice) 
		{
			datatablePrices.NRows = RowNum + 1;

			Gtk.Label textFromLabel = new Gtk.Label ("от (шт.)");
			datatablePrices.Attach (textFromLabel, (uint)0, (uint)1, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);
			textFromLabel.Show();

			var countDataEntry = new ySpinButton(1, 9999, 1);
			countDataEntry.Numeric = true;
			countDataEntry.Binding.AddBinding (newPrice, e => e.MinCount, w => w.ValueAsInt).InitializeFromSource ();
			datatablePrices.Attach (countDataEntry, (uint)1, (uint)2, RowNum, RowNum + 1, AttachOptions.Expand | AttachOptions.Fill, (AttachOptions)0, (uint)0, (uint)0);
			countDataEntry.Show();

			Gtk.Label textCplLabel = new Gtk.Label (" - ");
			datatablePrices.Attach (textCplLabel, (uint)2, (uint)3, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);
			textCplLabel.Show();

			var priceDataEntry = new ySpinButton (0, 999999, 1);
			priceDataEntry.Digits = 2;
			priceDataEntry.Binding.AddBinding (newPrice, e => e.Price, w => w.ValueAsDecimal).InitializeFromSource ();
			datatablePrices.Attach (priceDataEntry, (uint)3, (uint)4, RowNum, RowNum + 1, AttachOptions.Expand | AttachOptions.Fill, (AttachOptions)0, (uint)0, (uint)0);
			priceDataEntry.Show();

			Gtk.Label textCurrencyLabel = new Gtk.Label ("руб.");
			datatablePrices.Attach (textCurrencyLabel, (uint)4, (uint)5, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);
			textCurrencyLabel.Show();

			Gtk.Button deleteButton = new Gtk.Button ();
			Gtk.Image image = new Gtk.Image ();
			image.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-delete", global::Gtk.IconSize.Menu);
			deleteButton.Image = image;
			deleteButton.Clicked += OnButtonDeleteClicked;
			datatablePrices.Attach (deleteButton, (uint)5, (uint)6, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);
			deleteButton.Show();

			yValidatedEntry objectStore = new yValidatedEntry();
			objectStore.Tag = newPrice;
			objectStore.Visibility = false;
			datatablePrices.Attach(objectStore, (uint)6, (uint)7, RowNum, RowNum + 1, AttachOptions.Shrink, (AttachOptions)0, (uint)10, (uint)0);

			RowNum++;
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			Table.TableChild delButtonInfo = ((Table.TableChild)(this.datatablePrices [(Widget)sender]));
			Widget foundWidget = null;
			foreach(Widget wid in datatablePrices.AllChildren)
			{
				if(wid is yValidatedEntry && delButtonInfo.TopAttach == (datatablePrices[wid] as Table.TableChild).TopAttach)
				{
					foundWidget = wid;
					break;
				}
			}
			if(foundWidget == null)
			{
				logger.Warn("Не найден виджет ассоциированный с удаленной ценой.");
				return;
			}

			var found = (NomenclaturePriceBase)(foundWidget as yValidatedEntry).Tag;
			PricesList.Remove(found);
		}

		private void RemoveRow(uint Row)
		{
			foreach (Widget w in datatablePrices.Children)
				if (((Table.TableChild)(this.datatablePrices [w])).TopAttach == Row) {
					datatablePrices.Remove (w);
					w.Destroy ();
				}
			for (uint i = Row + 1; i < datatablePrices.NRows; i++)
				MoveRowUp (i);
			datatablePrices.NRows = --RowNum;
		}

		protected void MoveRowUp(uint Row)
		{
			foreach (Widget w in datatablePrices.Children)
				if (((Table.TableChild)(this.datatablePrices [w])).TopAttach == Row) {
					uint Left = ((Table.TableChild)(this.datatablePrices [w])).LeftAttach;
					uint Right = ((Table.TableChild)(this.datatablePrices [w])).RightAttach;
					datatablePrices.Remove (w);
					if(w.GetType() == typeof(ySpinButton))
					{
						datatablePrices.Attach(w, Left, Right, Row - 1, Row, AttachOptions.Fill | AttachOptions.Expand, (AttachOptions)0, (uint)0, (uint)0);
					}
					else
					{
						datatablePrices.Attach(w, Left, Right, Row - 1, Row, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);
					}
				}
		}

		private void CleanList()
		{
			while (PricesList.Count > 0)
			{
				PricesList.RemoveAt(0);
			}
		}

		public NomenclaturePriceBase.NomenclaturePriceType NomenclaturePriceType { get; set; }

	}
}
