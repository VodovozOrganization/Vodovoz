using System;
using System.Data.Bindings;
using Gtk.DataBindings;
using NLog;
using NHibernate;
using System.Data.Bindings.Collections.Generic;
using System.Data.Bindings.Collections;
using System.Collections.Generic;
using Gtk;
using QSOrmProject;
using QSWidgetLib;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PricesView : Gtk.Bin
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession session;
		private GenericObservableList<NomenclaturePrice> PricesList;

		public ISession Session
		{
			get {
				return session;
			}
			set {
				session = value;
			}
		}

		private IList<NomenclaturePrice> prices;
		public IList<NomenclaturePrice> Prices
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
					PricesList = new GenericObservableList<NomenclaturePrice> (prices);
					PricesList.ElementAdded += OnEmailListElementAdded;
					PricesList.ElementRemoved += OnEmailListElementRemoved;
					if (PricesList.Count == 0)
						PricesList.Add (new NomenclaturePrice ());
					else {
						foreach (NomenclaturePrice price in PricesList)
							AddPriceRow (price);
					}
				}
			}
		}

		void OnEmailListElementRemoved (object aList, int[] aIdx, object aObject)
		{
			Widget foundWidget = null;
			foreach(Widget wid in datatablePrices.AllChildren)
			{
				if(wid is IAdaptableContainer && (wid as IAdaptableContainer).Adaptor.Adaptor.FinalTarget == aObject)
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

		void OnEmailListElementAdded (object aList, int[] aIdx)
		{
			foreach(int i in aIdx)
			{
				AddPriceRow(PricesList[i]);
			}
		}

		uint RowNum;

		public PricesView ()
		{
			this.Build ();
			datatablePrices.NRows = RowNum = 0;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			PricesList.Add(new NomenclaturePrice());
		}

		private void AddPriceRow(NomenclaturePrice newPrice) 
		{
			datatablePrices.NRows = RowNum + 1;
			Adaptor rowAdaptor = new Adaptor(newPrice);

			Gtk.Label textFromLabel = new Gtk.Label ("от (шт.)");
			datatablePrices.Attach (textFromLabel, (uint)0, (uint)1, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			DataSpinButton countDataEntry = new DataSpinButton(1, 9999, 1);
			countDataEntry.DataSource = rowAdaptor;
			countDataEntry.Mappings = "MinCount";
			datatablePrices.Attach (countDataEntry, (uint)1, (uint)2, RowNum, RowNum + 1, AttachOptions.Expand | AttachOptions.Fill, (AttachOptions)0, (uint)0, (uint)0);

			Gtk.Label textCplLabel = new Gtk.Label (" - ");
			datatablePrices.Attach (textCplLabel, (uint)2, (uint)3, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			DataSpinButton priceDataEntry = new DataSpinButton (0, 999999, 1);
			priceDataEntry.Digits = 2;
			priceDataEntry.DataSource = rowAdaptor;
			priceDataEntry.Mappings = "Price";
			datatablePrices.Attach (priceDataEntry, (uint)3, (uint)4, RowNum, RowNum + 1, AttachOptions.Expand | AttachOptions.Fill, (AttachOptions)0, (uint)0, (uint)0);

			Gtk.Label textCurrencyLabel = new Gtk.Label ("руб.");
			datatablePrices.Attach (textCurrencyLabel, (uint)4, (uint)5, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			Gtk.Button deleteButton = new Gtk.Button ();
			Gtk.Image image = new Gtk.Image ();
			image.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-delete", global::Gtk.IconSize.Menu);
			deleteButton.Image = image;
			deleteButton.Clicked += OnButtonDeleteClicked;
			datatablePrices.Attach (deleteButton, (uint)5, (uint)6, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			datatablePrices.ShowAll ();

			RowNum++;
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			Table.TableChild delButtonInfo = ((Table.TableChild)(this.datatablePrices [(Widget)sender]));
			Widget foundWidget = null;
			foreach(Widget wid in datatablePrices.AllChildren)
			{
				if(wid is IAdaptableContainer && delButtonInfo.TopAttach == (datatablePrices[wid] as Table.TableChild).TopAttach)
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

			PricesList.Remove((NomenclaturePrice)(foundWidget as IAdaptableContainer).Adaptor.Adaptor.FinalTarget);
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
					if (w.GetType() == typeof(DataComboBox))
						datatablePrices.Attach (w, Left, Right, Row - 1, Row, AttachOptions.Fill | AttachOptions.Expand, (AttachOptions)0, (uint)0, (uint)0);
					else
						datatablePrices.Attach (w, Left, Right, Row - 1, Row, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);
				}
		}

		private void CleanList()
		{
			while (PricesList.Count > 0)
			{
				PricesList.RemoveAt(0);
			}
		}

		public void SaveChanges()
		{
			foreach(NomenclaturePrice price in PricesList)
			{
					Session.SaveOrUpdate(price);
			}
		}
	}
}
