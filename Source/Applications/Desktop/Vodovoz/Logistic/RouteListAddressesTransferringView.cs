﻿using Autofac;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;
using Gdk;
using Gtk;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewWidgets.Logistics;
using static Vodovoz.ViewModels.Logistic.RouteListTransferringViewModel;

namespace Vodovoz.Logistic
{
	public partial class RouteListAddressesTransferringView : TabViewBase<RouteListTransferringViewModel>
	{
		private Color _successBaseColor = GdkColors.SuccessBase;
		private Color _primaryBaseColor = GdkColors.PrimaryBase;
		private RouteListJournalFilterViewModel _sourceRouteListsFilter;
		private RouteListJournalFilterViewModel _targetRouteListsFilter;

		public RouteListAddressesTransferringView(RouteListTransferringViewModel viewModel)
			: base(viewModel)
		{
			Build();

			viewModel.OpenLegacyOrderForRouteListJournalViewModelHandler = (filterConfig) =>
			{
				var page = ViewModel.NavigationManager.OpenViewModel<OrderForRouteListJournalViewModel, Action<OrderJournalFilterViewModel>>(ViewModel, filterConfig);

				page.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
				page.ViewModel.OnEntitySelectedResult += ViewModel.OnOrderSelectedResult;

				return page;
			};

			_sourceRouteListsFilter = ViewModel.LifetimeScope.Resolve<RouteListJournalFilterViewModel>();

			_sourceRouteListsFilter.SetAndRefilterAtOnce(filter =>
			{
				filter.DisplayableStatuses = ViewModel.DefaultSourceRouteListStatuses;
				filter.StartDate = ViewModel.DefaultSourceRouteListStartDate;
				filter.EndDate = ViewModel.DefaultSourceRouteListEndDate;
				filter.AddressTypeNodes.ForEach(x => x.Selected = true);
				filter.ExcludeIds = ViewModel.ExcludeIds;
			});

			_sourceRouteListsFilter.DisposeOnDestroy = false;

			ViewModel.SourceRouteListViewModel =
				new LegacyEEVMBuilderFactory<RouteListTransferringViewModel>(ViewModel, Tab, ViewModel, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.LifetimeScope)
				.ForProperty(x => x.SourceRouteList)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<RouteListJournalViewModel, RouteListJournalFilterViewModel>(_sourceRouteListsFilter)
				.Finish();

			_targetRouteListsFilter = ViewModel.LifetimeScope.Resolve<RouteListJournalFilterViewModel>();

			_targetRouteListsFilter.SetAndRefilterAtOnce(filter =>
			{
				filter.DisplayableStatuses = ViewModel.DefaultTargetRouteListStatuses;
				filter.StartDate = ViewModel.DefaultTargetRouteListStartDate;
				filter.EndDate = ViewModel.DefaultTargetRouteListEndDate;
				filter.AddressTypeNodes.ForEach(x => x.Selected = true);
				filter.ExcludeIds = ViewModel.ExcludeIds;
			});

			_targetRouteListsFilter.DisposeOnDestroy = false;

			ViewModel.TargetRouteListViewModel =
				new LegacyEEVMBuilderFactory<RouteListTransferringViewModel>(ViewModel, Tab, ViewModel, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.LifetimeScope)
				.ForProperty(x => x.TargetRouteList)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<RouteListJournalViewModel, RouteListJournalFilterViewModel>(_targetRouteListsFilter)
				.Finish();

			Initialize();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.SourceRouteListId)
				|| e.PropertyName == nameof(ViewModel.TargetRouteListId))
			{
				_targetRouteListsFilter.ExcludeIds = ViewModel.ExcludeIds;
				_sourceRouteListsFilter.ExcludeIds = ViewModel.ExcludeIds;
			}
		}

		private void Initialize()
		{
			hpanedMain.Position = Screen.RootWindow.FrameExtents.Width / 2;

			entityentrySourceRouteList.ViewModel = ViewModel.SourceRouteListViewModel;
			entityentryTargetRouteList.ViewModel = ViewModel.TargetRouteListViewModel;

			//Для каждой TreeView нужен свой экземпляр ColumnsConfig
			ytreeviewSourceAddresses.ColumnsConfig = GetColumnsConfig(false);
			ytreeviewTargetAddresses.ColumnsConfig = GetColumnsConfig(true);

			ytreeviewSourceAddresses.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeviewTargetAddresses.Selection.Mode = Gtk.SelectionMode.Multiple;

			ytreeviewSourceAddresses.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.SourceRouteListAddresses, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedSourceRouteListAddresses, w => w.SelectedRows)
				.InitializeFromSource();

			ytreeviewTargetAddresses.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.TargetRouteListAddresses, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedTargetRouteListAddresses, w => w.SelectedRows)
				.InitializeFromSource();

			ConfigureTreeViewsDriverBalance();

			ybuttonTransfer.Clicked += (_, _2) => ViewModel.TransferAddressesCommand.Execute();
			ybuttonTransfer.Binding
				.AddBinding(ViewModel, vm => vm.CanTransferAddress, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonRevertAddress.Clicked += (_, _2) => ViewModel.RevertTransferAddressesCommand.Execute();
			ybuttonRevertAddress.Binding
				.AddBinding(ViewModel, vm => vm.CanRevertTransferAddresses, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonAddOrder.Clicked += (_, _2) => ViewModel.AddOrderToRouteListEnRouteCommand.Execute();
			ybuttonAddOrder.Binding
				.AddBinding(ViewModel, vm => vm.CanAddOrder, w => w.Sensitive)
				.InitializeFromSource();

			ybtnTransferTerminal.Clicked += (_, _2) => ViewModel.TransferTerminalCommand.Execute();
			ybtnTransferTerminal.Binding
				.AddBinding(ViewModel, vm => vm.CanTransferTerminal, w => w.Sensitive)
				.InitializeFromSource();

			ybtnRevertTerminal.Clicked += (_, _2) => ViewModel.RevertTransferTerminalCommand.Execute();
			ybtnRevertTerminal.Binding
				.AddBinding(ViewModel, vm => vm.CanRevertTransferTerminal, w => w.Sensitive)
				.InitializeFromSource();

			var deliveryfreebalanceviewFrom = new DeliveryFreeBalanceView(ViewModel.SourceRouteListDeliveryFreeBalanceViewModel);
			deliveryfreebalanceviewFrom.ShowAll();
			yhboxDeliveryFreeBalanceFrom.PackStart(deliveryfreebalanceviewFrom, true, true, 0);

			var deliveryfreebalanceviewTo = new DeliveryFreeBalanceView(ViewModel.TargetRouteListDeliveryFreeBalanceViewModel);
			deliveryfreebalanceviewTo.ShowAll();
			yhboxDeliveryFreeBalanceTo.PackStart(deliveryfreebalanceviewTo, true, true, 0);
		}

		private IColumnsConfig GetColumnsConfig(bool isRightPanel)
		{
			var config = ColumnsConfigFactory.Create<RouteListItemNode>()
				.AddColumn("Еж.\nномер").AddTextRenderer(node => node.DalyNumber)
				.AddColumn("Заказ").AddNumericRenderer(node => node.OrderId)
				.AddColumn("Дата").AddTextRenderer(node => node.Date)
				.AddColumn("Адрес").AddTextRenderer(node => node.Address)
				.AddColumn("Бутыли").AddTextRenderer(node => node.FormattedBottlesCount)
				.AddColumn("Статус").AddTextRenderer(node => node.AddressStatus.HasValue ? node.AddressStatus.GetEnumTitle() : "")
				.AddColumn("Доставка\nза час")
					.AddToggleRenderer(x => x.IsFastDelivery).Editing(false);

			if(isRightPanel)
			{
				config.AddColumn("Тип переноса").AddTextRenderer(node => node.AddressTransferType.HasValue ? node.AddressTransferType.GetEnumTitle() : "");
			}
			else
			{
				config.AddColumn("Тип переноса")
					.AddToggleRenderer(x => x.IsNeedToReload).Radio()
						.AddSetter((c, x) => ApplyCellRendererSetter(c, x, true))
					.AddTextRenderer(x => AddressTransferType.NeedToReload.GetEnumTitle())
						.AddSetter((c, x) => ApplyCellRendererSetter(c, x, true))
					.AddToggleRenderer(x => x.IsFromHandToHandTransfer).Radio()
						.AddSetter((c, x) => ApplyCellRendererSetter(c, x, true))
					.AddTextRenderer(x => AddressTransferType.FromHandToHand.GetEnumTitle())
						.AddSetter((c, x) => ApplyCellRendererSetter(c, x, true))
					.AddToggleRenderer(x => x.IsFromFreeBalance).Radio()
						.AddSetter((c, x) => ApplyCellRendererSetter(c, x, false))
					.AddTextRenderer(x => AddressTransferType.FromFreeBalance.GetEnumTitle())
						.AddSetter((c, x) => ApplyCellRendererSetter(c, x, false));
			}

			return config.AddColumn("Нужен\nтерминал").AddToggleRenderer(x => x.NeedTerminal).Editing(false)
						 .AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
						 .RowCells().AddSetter<CellRenderer>((cell, node) => cell.CellBackgroundGdk = node.WasTransfered ? _successBaseColor : _primaryBaseColor)
						 .Finish();
		}

		private void ApplyCellRendererSetter(CellRenderer nodeCellRenderer, RouteListItemNode routeListItemNode, bool isHiddenForNewOrder)
		{
			if(routeListItemNode.RouteListItem == null
				&& isHiddenForNewOrder)
			{
				nodeCellRenderer.Visible = false;

				return;
			}

			var isActive = routeListItemNode.AddressStatus != RouteListItemStatus.Transfered
				&& routeListItemNode.RouteListItem != null;

			nodeCellRenderer.Sensitive = isActive;

			if(nodeCellRenderer is NodeCellRendererToggle<RouteListItemNode> toggle)
			{
				toggle.Activatable = isActive;
			}
		}

		private void ConfigureTreeViewsDriverBalance()
		{
			yTreeViewSourceDriverBalance.CreateFluentColumnsConfig<EmployeeBalanceNode>()
				.AddColumn("Код").AddTextRenderer(n => n.NomenclatureId.ToString())
				.AddColumn("Номенклатура").AddTextRenderer(n => n.NomenclatureName)
				.AddColumn("Количество").AddTextRenderer(n => n.Amount.ToString())
				.Finish();

			yTreeViewSourceDriverBalance.Binding
				.AddBinding(ViewModel, vm => vm.SourceRouteListDriverNomenclatureBalance, w => w.ItemsDataSource)
				.InitializeFromSource();

			yTreeViewTargetDriverBalance.CreateFluentColumnsConfig<EmployeeBalanceNode>()
				.AddColumn("Код").AddTextRenderer(n => n.NomenclatureId.ToString())
				.AddColumn("Номенклатура").AddTextRenderer(n => n.NomenclatureName)
				.AddColumn("Количество").AddTextRenderer(n => n.Amount.ToString())
				.Finish();

			yTreeViewTargetDriverBalance.Binding
				.AddBinding(ViewModel, vm => vm.TargetRouteListDriverNomenclatureBalance, w => w.ItemsDataSource)
				.InitializeFromSource();
		}

		public override void Destroy()
		{
			_targetRouteListsFilter.DisposeOnDestroy = true;
			_targetRouteListsFilter?.Dispose();
			_targetRouteListsFilter = null;
			_sourceRouteListsFilter.DisposeOnDestroy = true;
			_sourceRouteListsFilter?.Dispose();
			_sourceRouteListsFilter = null;

			if(ViewModel != null)
			{
				ViewModel.OpenLegacyOrderForRouteListJournalViewModelHandler = null;
			}
			base.Destroy();
		}
	}
}
