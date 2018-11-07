using System;
using System.Data.Bindings.Collections.Generic;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QS.Tdi;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Dialogs.DocumentDialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TransferOperationDocumentItemView : Gtk.Bin
	{
		GenericObservableList<MovementDocumentItem> items;

		static Logger logger = LogManager.GetCurrentClassLogger();

		public TransferOperationDocumentItemView()
		{
			this.Build();
			treeItemsList.Selection.Changed += OnSelectionChanged;
		}

		private IUnitOfWorkGeneric<TransferOperationDocument> documentUoW;

		public IUnitOfWorkGeneric<TransferOperationDocument> DocumentUoW {
			get { return documentUoW; }
			set {
				if(documentUoW == value)
					return;
				documentUoW = value;
		//		if(DocumentUoW.Root.Items == null)
		//			DocumentUoW.Root.Items = new List<TransferOperationDocument>();
		//		items = DocumentUoW.Root.ObservableItems;

				treeItemsList.ColumnsConfig = Gamma.ColumnConfig.FluentColumnsConfig<MovementDocumentItem>.Create()
					.AddColumn("Наименование").AddTextRenderer(i => i.Name)
					.AddColumn("С/Н оборудования").AddTextRenderer(i => i.EquipmentString)
					.AddColumn("Количество")
					.AddNumericRenderer(i => i.Amount).Editing().WidthChars(10)
					.AddSetter((c, i) => c.Digits = (uint)i.Nomenclature.Unit.Digits)
					.AddSetter((c, i) => c.Editable = i.CanEditAmount)
					.AddSetter((c, i) => c.Adjustment = new Adjustment(0, 0, (double)i.AmountOnSource, 1, 100, 0))
					.AddTextRenderer(i => i.Nomenclature.Unit.Name, false)
					.Finish();

				treeItemsList.ItemsDataSource = items;
			}
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			items.Remove(treeItemsList.GetSelectedObjects()[0] as MovementDocumentItem);
		}

		void OnSelectionChanged(object sender, EventArgs e)
		{
			buttonDelete.Sensitive = treeItemsList.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			if(DocumentUoW.Root.FromClient == null) {
				MessageDialogWorks.RunErrorDialog("Не добавлен отправитель.");
				return;
			}

			if(DocumentUoW.Root.FromDeliveryPoint == null) {
				MessageDialogWorks.RunErrorDialog("Не добавлена точка доставки отправителя.");
			}

			ITdiTab mytab = DialogHelper.FindParentTab(this);
			if(mytab == null) {
				logger.Warn("Родительская вкладка не найдена.");
				return;
			}

			var filter = new StockBalanceFilter(UnitOfWorkFactory.CreateWithoutRoot());
		//	filter.RestrictWarehouse = DocumentUoW.Root.FromWarehouse;

			ReferenceRepresentation SelectDialog = new ReferenceRepresentation(new ViewModel.StockBalanceVM(filter));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.None;
			SelectDialog.ObjectSelected += NomenclatureSelected;

			mytab.TabParent.AddSlaveTab(mytab, SelectDialog);
		}

		void NomenclatureSelected(object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var nomenctature = DocumentUoW.GetById<Nomenclature>(e.ObjectId);
		//	DocumentUoW.Root.AddItem(nomenctature, 0, (e.VMNode as ViewModel.StockBalanceVMNode).Amount);
		}
	}
}
