using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using Gtk;
using NLog;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class MovementDocumentItemsView : Gtk.Bin
	{
		GenericObservableList<MovementDocumentItem> items;

		static Logger logger = LogManager.GetCurrentClassLogger ();

		public MovementDocumentItemsView ()
		{
			this.Build ();
			treeItemsList.Selection.Changed += OnSelectionChanged;
		}

		private IUnitOfWorkGeneric<MovementDocument> documentUoW;

		public IUnitOfWorkGeneric<MovementDocument> DocumentUoW {
			get { return documentUoW; }
			set {
				if (documentUoW == value)
					return;
				documentUoW = value;
				if (DocumentUoW.Root.Items == null)
					DocumentUoW.Root.Items = new List<MovementDocumentItem> ();
				items = DocumentUoW.Root.ObservableItems;

				treeItemsList.ColumnsConfig = Gamma.ColumnConfig.FluentColumnsConfig<MovementDocumentItem>.Create()
					.AddColumn("Наименование").AddTextRenderer(i => i.Name)
					.AddColumn("С/Н оборудования").AddTextRenderer(i => i.EquipmentString)

					.AddColumn("Количество")
					.AddNumericRenderer(i => i.Amount)
					.AddSetter((c, i) => c.Editable = i.CanEditAmount)
					.WidthChars(10)
					.AddSetter((c, i) => c.Digits = (uint)i.Nomenclature.Unit.Digits)
					.AddSetter((c, i) => c.Adjustment = new Adjustment(0, 0, (double)i.AmountOnSource, 1, 100, 0))
					.AddTextRenderer(i => i.Nomenclature.Unit.Name, false)
					.AddColumn("")
					.Finish();

				treeItemsList.ItemsDataSource = items;
			}
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			items.Remove (treeItemsList.GetSelectedObjects () [0] as MovementDocumentItem);
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			buttonDelete.Sensitive = treeItemsList.Selection.CountSelectedRows () > 0;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			if (DocumentUoW.Root.FromWarehouse == null)
			{
				MessageDialogWorks.RunErrorDialog ("Не добавлен склад отправителя.");
				return;
			}

			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}

			var filter = new StockBalanceFilter (UnitOfWorkFactory.CreateWithoutRoot ());
			filter.SetAndRefilterAtOnce(x => x.RestrictWarehouse = DocumentUoW.Root.FromWarehouse);

			ReferenceRepresentation SelectDialog = new ReferenceRepresentation (new ViewModel.StockBalanceVM (filter));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.None;
			SelectDialog.ObjectSelected += NomenclatureSelected;

			mytab.TabParent.AddSlaveTab (mytab, SelectDialog);
		}

		void NomenclatureSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var nomenctature = DocumentUoW.GetById<Nomenclature> (e.ObjectId);
			DocumentUoW.Root.AddItem (nomenctature, 0, (e.VMNode as ViewModel.StockBalanceVMNode).Amount);
		}
	}
}

