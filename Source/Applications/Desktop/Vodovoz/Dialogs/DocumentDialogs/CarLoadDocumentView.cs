using Autofac;
using Gamma.GtkWidgets;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Journal.GtkUI;
using QS.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Documents;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.TrueMark;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CarLoadDocumentView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private readonly IStockRepository _stockRepository = ScopeProvider.Scope.Resolve<IStockRepository>();
		private readonly IRouteListRepository _routeListRepository = ScopeProvider.Scope.Resolve<IRouteListRepository>();
		private readonly ISubdivisionRepository _subdivisionRepository = ScopeProvider.Scope.Resolve<ISubdivisionRepository>();
		private readonly INavigationManager _navigator = ScopeProvider.Scope.Resolve<INavigationManager>();
		private bool _isCanEditDocument;

		private Menu _itemsPopup = new Menu();
		private MenuItem _itemsOpenOrderCodes = new MenuItem("Просмотр кодов по заказу");

		public CarLoadDocumentView()
		{
			this.Build();

			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<CarLoadDocumentItem>()
				.AddColumn("Номенклатура").AddTextRenderer(x => (x.ExpireDatePercent != null) ? x.Nomenclature.Name + " >" + x.ExpireDatePercent + "% срока годности" : x.Nomenclature.Name)
				.AddColumn("Принадлежность").AddEnumRenderer(x => x.OwnType, true, new Enum[] { OwnTypes.None })
				.AddColumn("С/Н оборудования").AddTextRenderer(x => x.Equipment != null ? x.Equipment.Serial : string.Empty)
				.AddColumn("Кол-во на складе").AddTextRenderer(x => x.Nomenclature.Unit.MakeAmountShortStr(x.AmountInStock))
				.AddColumn("В маршрутнике").AddTextRenderer(x => x.Nomenclature.Unit.MakeAmountShortStr(x.AmountInRouteList))
				.AddColumn("В других отгрузках").AddTextRenderer(x => x.Nomenclature.Unit.MakeAmountShortStr(x.AmountLoaded))
				.AddColumn("Отгружаемое кол-во").AddNumericRenderer(x => x.Amount, new RoundedDecimalToStringConverter()).Editing()
				.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 10))
				.AddSetter((w, x) => w.Digits = (uint)x.Nomenclature.Unit.Digits)
				.AddSetter((w, x) =>
				{
					if(_isCanEditDocument)
					{
						w.ForegroundGdk = CalculateAmountAndColor(x);
					}
				})
				.AddColumn("Заказ").AddNumericRenderer(x => x.OrderId)
				.AddColumn("")
				.Finish();

			ytreeviewItems.Add(_itemsPopup);
			_itemsPopup.Add(_itemsOpenOrderCodes);
			_itemsOpenOrderCodes.Show();
			_itemsPopup.Show();
			_itemsOpenOrderCodes.Activated += OpenOrderCodesDialog;
			ytreeviewItems.ButtonReleaseEvent += OnAddressRightClick;

			ytreeviewItems.Selection.Changed += YtreeviewItems_Selection_Changed;
		}

		private void OnAddressRightClick(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != (uint)GtkMouseButton.Right)
			{
				return;
			}
			var onlyOneSelected = ytreeviewItems.Selection.CountSelectedRows() == 1;
			var selectedItem = ytreeviewItems.GetSelectedObject<CarLoadDocumentItem>();
			var canOpen = selectedItem != null && selectedItem.OrderId.HasValue && onlyOneSelected;
			_itemsOpenOrderCodes.Sensitive = canOpen;
			_itemsPopup.Popup();
		}

		private void OpenOrderCodesDialog(object sender, EventArgs e)
		{
			var onlyOneSelected = ytreeviewItems.Selection.CountSelectedRows() == 1;
			if(!onlyOneSelected)
			{
				return;
			}
			var selectedItem = ytreeviewItems.GetSelectedObject<CarLoadDocumentItem>();
			if(selectedItem == null)
			{
				return;
			}
			_navigator.OpenViewModel<OrderCodesViewModel, int>(null, selectedItem.OrderId.Value, OpenPageOptions.IgnoreHash);
		}

		public void FillItemsByWarehouse()
		{
			if(buttonFillWarehouseItems.Sensitive)
			{
				buttonFillWarehouseItems.Click();
			}
		}

		public void SetIsCanEditDocument(bool isEditable)
		{
			_isCanEditDocument = isEditable;
			buttonFillAllItems.Sensitive = buttonFillWarehouseItems.Sensitive = isEditable;
		}

		void YtreeviewItems_Selection_Changed(object sender, EventArgs e)
		{
			var selected = ytreeviewItems.GetSelectedObject<CarLoadDocumentItem>();
			buttonDelete.Sensitive = selected != null && buttonFillAllItems.Sensitive == true;
		}

		private IUnitOfWorkGeneric<CarLoadDocument> documentUoW;

		public IUnitOfWorkGeneric<CarLoadDocument> DocumentUoW {
			get { return documentUoW; }
			set 
			{
				if(documentUoW == value)
				{
					return;
				}
				documentUoW = value;
				if(DocumentUoW.Root.Items == null)
				{
					DocumentUoW.Root.Items = new List<CarLoadDocumentItem>();
				}

				ytreeviewItems.ItemsDataSource = DocumentUoW.Root.ObservableItems;
				DocumentUoW.Root.PropertyChanged += DocumentUoW_Root_PropertyChanged;
				UpdateButtonState();
			}
		}

		void UpdateButtonState()
		{
			bool routeExist = DocumentUoW.Root.RouteList != null;
			bool warehouseExist = DocumentUoW.Root.Warehouse != null;
			buttonFillAllItems.Sensitive = routeExist;
			buttonFillWarehouseItems.Sensitive = routeExist && warehouseExist;
		}

		void DocumentUoW_Root_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			UpdateButtonState();
		}

		Gdk.Color CalculateAmountAndColor(CarLoadDocumentItem item)
		{
			if(item.Nomenclature.OfficialName == "Терминал для оплаты" && item.Amount > 1)
			{
				item.Amount = decimal.One;
			}

			if(item.Amount > item.AmountInStock)
			{
				return GdkColors.DangerText;
			}

			if(item.Equipment == null)
			{
				if(item.AmountInRouteList < item.AmountLoaded + item.Amount)
				{
					return GdkColors.Orange;
				}

				if(item.AmountInRouteList == item.AmountLoaded + item.Amount)
				{
					return GdkColors.SuccessText;
				}

				if(item.AmountInRouteList > item.AmountLoaded + item.Amount)
				{
					return GdkColors.InfoText;
				}
			}
			else
			{
				if(1 < item.AmountLoaded + item.Amount)
				{
					return GdkColors.Orange;
				}

				if(1 == item.AmountLoaded + item.Amount)
				{
					return GdkColors.SuccessText;
				}

				if(1 > item.AmountLoaded + item.Amount)
				{
					return GdkColors.InfoText;
				}
			}
			return GdkColors.PrimaryText;
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			DocumentUoW.Root.ObservableItems.Remove(ytreeviewItems.GetSelectedObject<CarLoadDocumentItem>());
		}

		protected void OnButtonFillWarehouseItemsClicked(object sender, EventArgs e)
		{
			if(DocumentUoW.Root.Items.Any() && !MessageDialogHelper.RunQuestionDialog("Список будет очищен. Продолжить?"))
			{
				return;
			}

			DocumentUoW.Root.FillFromRouteList(DocumentUoW, _routeListRepository, _subdivisionRepository, true);
			DocumentUoW.Root.UpdateAlreadyLoaded(DocumentUoW, _routeListRepository);

			if(DocumentUoW.Root.Warehouse != null)
			{
				DocumentUoW.Root.UpdateStockAmount(DocumentUoW, _stockRepository);
				DocumentUoW.Root.UpdateAmounts();
			}
		}

		protected void OnButtonFillAllItemsClicked(object sender, EventArgs e)
		{
			if(DocumentUoW.Root.Items.Any() && !MessageDialogHelper.RunQuestionDialog("Список будет очищен. Продолжить?"))
			{
				return;
			}

			DocumentUoW.Root.FillFromRouteList(DocumentUoW, _routeListRepository, null, false);

			var items = DocumentUoW.Root.Items;
			string errorNomenclatures = string.Empty;

			foreach(var item in items)
			{
				if(item.Nomenclature.Unit == null)
				{
					errorNomenclatures += string.Format("{0} код {1}{2}",
						item.Nomenclature.Name, item.Nomenclature.Id, Environment.NewLine);
				}
			}

			if(!string.IsNullOrEmpty(errorNomenclatures))
			{
				errorNomenclatures = "Не указаны единицы измерения для следующих номенклатур:"
					+ Environment.NewLine + errorNomenclatures;
				MessageDialogHelper.RunErrorDialog(errorNomenclatures);
				DocumentUoW.Root.Items.Clear();
				return;
			}

			DocumentUoW.Root.UpdateAlreadyLoaded(DocumentUoW, _routeListRepository);
			if(DocumentUoW.Root.Warehouse != null)
			{
				DocumentUoW.Root.UpdateStockAmount(DocumentUoW, _stockRepository);
				DocumentUoW.Root.UpdateAmounts();
			}
		}
	}
}
