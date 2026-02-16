using Autofac;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Navigation;
using QS.Print;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Profitability;
using Vodovoz.ViewModels.Infrastructure.Print;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.Widgets;
using Vodovoz.ViewWidgets.Logistics;

namespace Vodovoz.Logistic
{
	public partial class RouteListCreateView : TabViewBase<RouteListCreateViewModel>
	{
		private AdditionalLoadingItemsView _additionalLoadingItemsView;

		public RouteListCreateView(RouteListCreateViewModel viewModel) : base(viewModel)
		{
			Build();
			Initialize();
		}

		private void Initialize()
		{
			ViewModel.DisableItemsUpdateDelegate = DisableItemsUpdate;

			createroutelistitemsview1.NavigationManager = ViewModel.NavigationManager;
			createroutelistitemsview1.ParentViewModel = ViewModel;

			ynotebook1.ShowTabs = false;
			radioBtnInformation.Toggled += OnInformationToggled;
			radioBtnInformation.Active = true;

			buttonSave.Clicked += (_, _2) => ViewModel.SaveCommand.Execute();
			btnCancel.Clicked += (_, _2) => ViewModel.CancelCommand.Execute();

			printTimeButton.Clicked += (_, _2) => ViewModel.ShowPrintTimeCommand.Execute();
			ybuttonAddAdditionalLoad.Clicked += OnButtonAddAdditionalLoadClicked;
			ybuttonRemoveAdditionalLoad.Clicked += OnButtonRemoveAdditionalLoadClicked;

			datepickerDate.Binding
				.AddBinding(ViewModel.Entity, e => e.Date, w => w.Date)
				.InitializeFromSource();

			datepickerDate.DateChangedByUser += (s, e) =>
			{
				ViewModel.OnDatepickerDateDateChangedByUser(s, e);
				createroutelistitemsview1.UpdateInfo();
			};

			InitializeSpecialConditions();

			entryCar.ViewModel = ViewModel.CarViewModel;
			entryCar.Binding
				.AddBinding(ViewModel, vm => vm.CanAccept, w => w.Sensitive)
				.InitializeFromSource();

			entryCar.ViewModel.ChangedByUser += ViewModel.OnCarChangedByUser;

			entryDriver.ViewModel = ViewModel.DriverViewModel;
			entryDriver.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeDriver, w => w.Sensitive)
				.InitializeFromSource();

			entryDriver.ViewModel.ChangedByUser += ViewModel.OnDriverChangedByUser;

			lblDriverComment.Binding
				.AddSource(ViewModel.Entity)
				.AddFuncBinding(
					routeList => routeList.Driver != null
						? routeList.Driver.Comment
						: "",
					w => w.Text)
				.InitializeFromSource();

			hboxDriverComment.Binding
				.AddFuncBinding(ViewModel.Entity,
					e => e.Driver != null
						&& !string.IsNullOrWhiteSpace(e.Driver.Comment),
					w => w.Visible)
				.InitializeFromSource();

			entryForwarder.ViewModel = ViewModel.ForwarderViewModel;

			entryForwarder.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeForwarder, w => w.Sensitive)
				.InitializeFromSource();

			entryForwarder.ViewModel.Changed += OnForwarderChanged;

			hboxForwarderComment.Binding
				.AddFuncBinding(
					ViewModel.Entity,
					routeList => routeList.Forwarder != null
						&& !string.IsNullOrWhiteSpace(routeList.Forwarder.Comment),
					w => w.Visible)
				.InitializeFromSource();

			lblForwarderComment.Binding
				.AddFuncBinding(
					ViewModel.Entity,
					routeList => routeList.Forwarder != null
						? routeList.Forwarder.Comment
						: "",
					w => w.Text);

			entryLogistician.ViewModel = ViewModel.LogisticianViewModel;

			speccomboShift.ItemsList = ViewModel.DeliveryShiftsCache;
			speccomboShift.Binding
				.AddBinding(ViewModel.Entity, e => e.Shift, w => w.SelectedItem)
				.InitializeFromSource();

			labelStatus.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.Status.GetEnumTitle(), w => w.LabelProp)
				.InitializeFromSource();

			createroutelistitemsview1.RouteListUoW = ViewModel.UoWGeneric;
			createroutelistitemsview1.SetPermissionParameters(
				ViewModel.CanCreate,
				ViewModel.CanUpdate,
				ViewModel.IsLogistician);

			createroutelistitemsview1.IsEditable(ViewModel.CanAccept, ViewModel.CanOpenOrder);

			InitializeAdditionalLoading();

			ybuttonAddAdditionalLoad.Binding
				.AddBinding(ViewModel, vm => vm.CanAddAdditionalLoad, w => w.Visible)
				.InitializeFromSource();

			ybuttonRemoveAdditionalLoad.Binding
				.AddBinding(ViewModel, vm => vm.HaveAdditionalLoad, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanRemoveAdditionalLoad, w => w.Sensitive)
				.InitializeFromSource();

			ggToStringWidget.UoW = ViewModel.UoW;
			ggToStringWidget.Label = "Район города:";
			ggToStringWidget.Binding
				.AddBinding(ViewModel.Entity, x => x.ObservableGeographicGroups, x => x.Items)
				.InitializeFromSource();

			buttonAccept.Clicked += (_, _2) => ViewModel.AcceptCommand.Execute();
			buttonAccept.Binding
				.AddBinding(ViewModel, vm => vm.CanAccept, w => w.Visible)
				.InitializeFromSource();

			buttonRevertToNew.Clicked += (_, _2) => ViewModel.RevertToNewCommand.Execute();
			buttonRevertToNew.Binding
				.AddBinding(ViewModel, vm => vm.CanRevertToNew, w => w.Visible)
				.InitializeFromSource();

			enumPrint.ItemsEnum = typeof(RouteListPrintableDocuments);
			enumPrint.SetVisibility(RouteListPrintableDocuments.TimeList, false);
			enumPrint.SetVisibility(RouteListPrintableDocuments.OrderOfAddresses, false);
			enumPrint.EnumItemClicked += (_, e) => ViewModel.PrintCommand.Execute((RouteListPrintableDocuments)e.ItemEnum);
			enumPrint.Binding
				.AddBinding(ViewModel, vm => vm.CanPrint, w => w.Sensitive)
				.InitializeFromSource();

			//Телефон
			var mangoManager = Startup.MainWin.MangoManager;
			phoneLogistican.MangoManager = mangoManager;
			phoneDriver.MangoManager = mangoManager;
			phoneForwarder.MangoManager = mangoManager;

			phoneLogistican.Binding
				.AddBinding(ViewModel.Entity, e => e.Logistician, w => w.Employee)
				.InitializeFromSource();

			phoneDriver.Binding
				.AddBinding(ViewModel.Entity, e => e.Driver, w => w.Employee)
				.InitializeFromSource();

			phoneForwarder.Binding
				.AddBinding(ViewModel.Entity, e => e.Forwarder, w => w.Employee)
				.InitializeFromSource();

			labelTerminalCondition.Binding
				.AddBinding(ViewModel, vm => vm.DriverTerminalCondition, w => w.Text)
				.InitializeFromSource();

			fixPriceSpin.Binding
				.AddBinding(ViewModel.Entity, e => e.FixedShippingPrice, w => w.ValueAsDecimal)
				.AddBinding(ViewModel.Entity, e => e.HasFixedShippingPrice, w => w.Sensitive)
				.InitializeFromSource();

			checkIsFixPrice.Binding
				.AddBinding(ViewModel.Entity, e => e.HasFixedShippingPrice, w => w.Active)
				.InitializeFromSource();

			speccomboShift.Binding
				.AddBinding(ViewModel, vm => vm.CanAccept, w => w.Sensitive)
				.InitializeFromSource();

			ggToStringWidget.Binding
				.AddBinding(ViewModel, vm => vm.CanAccept, w => w.Sensitive)
				.InitializeFromSource();

			datepickerDate.Binding
				.AddBinding(ViewModel, vm => vm.CanAccept, w => w.Sensitive)
				.InitializeFromSource();

			ylabelCashSubdivision.Binding
				.AddBinding(ViewModel, vm => vm.ClosingSubdivisionName, w => w.Text)
				.InitializeFromSource();

			fixPriceSpin.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeFixedPrice, w => w.Sensitive)
				.InitializeFromSource();

			checkIsFixPrice.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeIsFixPrice, w => w.Sensitive)
				.InitializeFromSource();

			InitializeProfitability();

			btnCopyEntityId.Binding
				.AddBinding(ViewModel, vm => vm.CanCopyId, w => w.Sensitive)
				.InitializeFromSource();

			btnCopyEntityId.Clicked += OnBtnCopyEntityIdClicked;
			ViewModel.DocumentPrinted += OnDocumentsPrinted;
			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnForwarderChanged(object sender, EventArgs e)
		{
			createroutelistitemsview1.OnForwarderChanged();
			createroutelistitemsview1.UpdateInfo();
		}

		private void InitializeAdditionalLoading()
		{
			var additionalLoadingItemsViewModel =
				new AdditionalLoadingItemsViewModel(
					ViewModel.UoW,
					Tab,
					ViewModel.NavigationManager as ITdiCompatibilityNavigation,
					ViewModel.InteractiveService);

			additionalLoadingItemsViewModel
				.BindWithSource(ViewModel.Entity, e => e.AdditionalLoadingDocument);

			additionalLoadingItemsViewModel.CanEdit = ViewModel.Entity.Status == RouteListStatus.New;
			_additionalLoadingItemsView = new AdditionalLoadingItemsView(additionalLoadingItemsViewModel);
			_additionalLoadingItemsView.WidthRequest = 300;
			_additionalLoadingItemsView.ShowAll();
			hboxAdditionalLoading.PackStart(_additionalLoadingItemsView, false, false, 0);

			ybuttonAddAdditionalLoad.Binding
				.AddBinding(ViewModel, vm => vm.CanAddAdditionalLoad, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonRemoveAdditionalLoad.Binding
				.AddBinding(ViewModel, vm => vm.CanRemoveAdditionalLoad, w => w.Sensitive)
				.InitializeFromSource();

			UpdateAdditionalLoadDocumentsVisibility();
		}

		private void InitializeProfitability()
		{
			radioBtnProfitability.Sensitive = ViewModel.CanReadRouteListProfitability;
			radioBtnProfitability.Toggled += OnProfitabilityToggled;
			createroutelistitemsview1.UpdateProfitabilityInfo();

			ConfigureTreeRouteListProfitability();
		}

		private void InitializeSpecialConditions()
		{
			ylblSpecialConditionsConfirmed.Binding
				.AddBinding(ViewModel.Entity, e => e.SpecialConditionsAccepted, w => w.Visible)
				.InitializeFromSource();

			ylblSpecialConditionsConfirmedDateTime.Binding
				.AddBinding(ViewModel.Entity, e => e.SpecialConditionsAccepted, w => w.Visible)
				.AddFuncBinding(e => e.SpecialConditionsAcceptedAt.HasValue
						? e.SpecialConditionsAcceptedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
						: "",
					w => w.Text)
				.InitializeFromSource();

			radioBtnSprcialConditions.Visible = ViewModel.SpecialConditions.Any();
			radioBtnSprcialConditions.Toggled += OnButtonSpecialConditionsToggled;

			ConfigureTreeRouteListSpecialConditions();
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.CanAccept))
			{
				_additionalLoadingItemsView.ViewModel.CanEdit = ViewModel.CanAccept;
			}

			if(e.PropertyName == nameof(ViewModel.CanAccept)
				|| e.PropertyName == nameof(ViewModel.CanOpenOrder))
			{
				createroutelistitemsview1.IsEditable(ViewModel.CanAccept, ViewModel.CanOpenOrder);
			}

			if(e.PropertyName == nameof(ViewModel.AdditionalLoadItemsVisible))
			{
				UpdateAdditionalLoadDocumentsVisibility();
			}
		}

		private void UpdateAdditionalLoadDocumentsVisibility()
		{
			_additionalLoadingItemsView.Visible = ViewModel.AdditionalLoadItemsVisible;
		}

		private void ConfigureTreeRouteListSpecialConditions()
		{
			ytreeviewSpecialConditions.CreateFluentColumnsConfig<RouteListSpecialCondition>()
				.AddColumn("Название")
				.AddTextRenderer(x => ViewModel.SpecialConditionsTypes
					.Where(sct => sct.Id == x.RouteListSpecialConditionTypeId)
					.Select(sct => sct.Name)
					.FirstOrDefault() ?? "")
				.AddColumn("Принято")
				.AddTextRenderer(x => x.Accepted ? "Да" : "Нет")
				.AddColumn("Создано")
				.AddTextRenderer(x => x.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
				.AddColumn("")
				.Finish();

			ytreeviewSpecialConditions.ItemsDataSource = ViewModel.SpecialConditions;
		}

		private void ConfigureTreeRouteListProfitability()
		{
			treeRouteListProfitability.ColumnsConfig = FluentColumnsConfig<RouteListProfitability>.Create()
				.AddColumn("№ МЛ")
					.AddNumericRenderer(x => ViewModel.Entity.Id)
				.AddColumn("Фактический пробег,\nкм")
					.AddNumericRenderer(x => x.Mileage)
				.AddColumn("Амортизация,\nруб")
					.AddNumericRenderer(x => x.Amortisation)
					.Digits(2)
				.AddColumn("Ремонт,\nруб")
					.AddNumericRenderer(x => x.RepairCosts)
					.Digits(2)
				.AddColumn("Топливо,\nруб")
					.AddNumericRenderer(x => x.FuelCosts)
					.Digits(2)
				.AddColumn("Затраты ЗП\nвод + эксп, руб")
					.AddNumericRenderer(x => x.DriverAndForwarderWages)
					.Digits(2)
				.AddColumn("Оплата доставки\nклиентом: Доставка за\nчас, платная доставка, руб")
					.AddNumericRenderer(x => x.PaidDelivery)
					.Digits(2)
				.AddColumn("Затраты на МЛ,\nруб")
					.AddNumericRenderer(x => x.RouteListExpenses)
					.Digits(2)
				.AddColumn("Вывезено,\nкг")
					.AddNumericRenderer(x => x.TotalGoodsWeight)
					.Digits(2)
				.AddColumn("Затраты\nна кг")
					.AddNumericRenderer(x => x.RouteListExpensesPerKg)
					.Digits(2)
				.AddColumn("Сумма\nпродаж,\nруб")
					.AddNumericRenderer(x => x.SalesSum)
					.Digits(2)
				.AddColumn("Сумма затрат,\nруб")
					.AddNumericRenderer(x => x.ExpensesSum)
					.Digits(2)
				.AddColumn("Валовая\nмаржа,\nруб")
					.AddNumericRenderer(x => x.GrossMarginSum)
					.Digits(2)
				.AddColumn("Валовая маржа, %")
					.AddNumericRenderer(x => x.GrossMarginPercents)
					.Digits(2)
				.AddColumn("")
				.Finish();

			treeRouteListProfitability.ItemsDataSource = ViewModel.RouteListProfitabilities;
		}

		private void OnButtonAddAdditionalLoadClicked(object sender, EventArgs args)
		{
			ViewModel.AddAdditionalLoadingCommand.Execute();
			createroutelistitemsview1.UpdateInfo();
		}

		private void OnButtonRemoveAdditionalLoadClicked(object sender, EventArgs e)
		{
			ViewModel.RemoveAdditionalLoadingCommand.Execute();
			createroutelistitemsview1.UpdateInfo();
		}

		private void OnDocumentsPrinted(object sender, EventArgs e)
		{
			if(e is EndPrintArgs printArgs)
			{
				if(printArgs.Args.Cast<IPrintableDocument>().Any(d => d.Name == RouteListPrintableDocuments.RouteList.GetEnumTitle()))
				{
					ViewModel.Entity.AddPrintHistory();
					ViewModel.Save();
				}
			}
		}

		private void OnButtonSpecialConditionsToggled(object sender, EventArgs e)
		{
			if(radioBtnSprcialConditions.Active)
			{
				ynotebook1.Page = 2;
			}
		}

		protected void OnBtnCopyEntityIdClicked(object sender, EventArgs e)
		{
			if(ViewModel.Entity.Id > 0)
			{
				GetClipboard(Gdk.Selection.Clipboard).Text = ViewModel.Entity.Id.ToString();
			}
		}

		private void OnInformationToggled(object sender, EventArgs e)
		{
			if(radioBtnInformation.Active)
			{
				ynotebook1.Page = 0;
			}
		}

		private void OnProfitabilityToggled(object sender, EventArgs e)
		{
			if(radioBtnProfitability.Active)
			{
				ynotebook1.Page = 1;
			}
		}

		private void DisableItemsUpdate(bool isDisable) => createroutelistitemsview1.DisableColumnsUpdate = isDisable;

		public override void Destroy()
		{
			ViewModel.DocumentPrinted -= OnDocumentsPrinted;
			ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
			base.Destroy();
		}
	}
}
