using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Navigation;
using QS.Tdi;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Dialogs.OrderWidgets
{
	/// <summary>
	/// Класс для управления различных вариантов копирования заказа в desktop приложении
	/// </summary>
	public class CopyOrderDlgOpener
	{
		private readonly ILogger<CopyOrderDlgOpener> _logger;
		private readonly ITdiCompatibilityNavigation _navigator;
		private readonly IInteractiveService _interactiveService;

		public CopyOrderDlgOpener(
			ILogger<CopyOrderDlgOpener> logger,
			ITdiCompatibilityNavigation navigator,
			IInteractiveService interactiveService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}
		
		/// <summary>
		/// Открытие OrderDlg для повтора заказа
		/// </summary>
		/// <param name="master">Мастер вкладка</param>
		/// <param name="copiedOrder">Копируемый заказ</param>
		public void OpenCopyLesserOrderDlg(ITdiTab master, Order copiedOrder)
		{
			if(!CheckDistrict(copiedOrder.DeliveryPoint))
			{
				return;
			}
			
			_logger.LogInformation("Нажата кнопка повторить заказ {CopingOrderId}", copiedOrder.Id);
			var dlg = new OrderDlg();
			dlg.CopyLesserOrderFrom(copiedOrder.Id);

			master.TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<Order>(65656),
				() => dlg
			);
		}
		
		/// <summary>
		/// Открытие OrderDlg для повтора заказа из недовоза
		/// </summary>
		/// <param name="master">Мастер вкладка</param>
		/// <param name="orderDlg">Открытый диалог заказа, если есть</param>
		/// <param name="copiedOrder">Копируемый заказ</param>
		public bool TryOpenCopyOrderDlg(ITdiTab master, Order copiedOrder, out ITdiTab orderDlg)
		{
			orderDlg = null;
			
			if(!CheckDistrict(copiedOrder.DeliveryPoint))
			{
				return false;
			}
			
			var tag = $"NewCopyFromOrder_{copiedOrder.Id}_Dlg";
			var existsTab = FindTabByTag(tag);

			if(existsTab == null)
			{
				var dlg = new OrderDlg();
				dlg.CopyOrderFrom(copiedOrder.Id);
				dlg.Tag = tag;
				master.TabParent.OpenTab(() => dlg, master);
				orderDlg = FindTabByTag(tag);
				
				return true;
			}

			TDIMain.MainNotebook.CurrentPage = TDIMain.MainNotebook.PageNum(existsTab as OrderDlg);
			orderDlg = existsTab;
			return true;
		}

		/// <summary>
		/// Открытие OrderDlg для повтора заказа с диалога звонка Манго <see cref="CounterpartyOrderViewModel"/>
		/// Мастер вкладка DialogViewModelBase
		/// </summary>
		/// <param name="master">Мастер вкладка</param>
		/// <param name="copiedOrder">Копируемый заказ</param>
		/// <param name="openPageOptions">Настройки открытия</param>
		public void OpenOrderDlgForCopyOrderFromMangoByNavigator(
			DialogViewModelBase master, Order copiedOrder, OpenPageOptions openPageOptions = OpenPageOptions.IgnoreHash)
		{
			if(!CheckDistrict(copiedOrder.DeliveryPoint))
			{
				return;
			}
			
			_logger.LogInformation("Нажата кнопка повторить заказ {CopingOrderId} в Манго", copiedOrder.Id);
			_navigator.OpenTdiTab<OrderDlg, int, bool>(master, copiedOrder.Id, true, openPageOptions);
		}
		
		/// <summary>
		/// Открытие OrderDlg для повтора заказа с диалога звонка Манго <see cref="CounterpartyOrderViewModel"/>
		/// Мастер вкладка ITdiTab
		/// </summary>
		/// <param name="master">Мастер вкладка</param>
		/// <param name="copiedOrder">Копируемый заказ</param>
		/// <param name="openPageOptions">Настройки открытия</param>
		public void OpenOrderDlgForCopyOrderFromMangoByNavigator(
			ITdiTab master, Order copiedOrder, OpenPageOptions openPageOptions = OpenPageOptions.IgnoreHash)
		{
			if(!CheckDistrict(copiedOrder.DeliveryPoint))
			{
				return;
			}
			
			_navigator.OpenTdiTabOnTdi<OrderDlg, int, bool>(master, copiedOrder.Id, true, openPageOptions);
		}
		
		/// <summary>
		/// Открытие OrderDlg для создания заказа из онлайн заказа
		/// Мастер вкладка DialogViewModelBase
		/// </summary>
		/// <param name="master">Мастер вкладка</param>
		/// <param name="onlineOrder">Онлайн заказ</param>
		/// <param name="openPageOptions">Настройки открытия</param>
		/// <returns></returns>
		public ITdiPage OpenOrderDlgForCopyOrderFromOnlineOrderByNavigator(
			DialogViewModelBase master, OnlineOrder onlineOrder, OpenPageOptions openPageOptions = OpenPageOptions.AsSlave)
		{
			if(!CheckDistrict(onlineOrder.DeliveryPoint))
			{
				return null;
			}
			
			return _navigator.OpenTdiTab<OrderDlg, OnlineOrder>(master, onlineOrder, openPageOptions);
		}
		
		private ITdiTab FindTabByTag(string tag) =>
			TDIMain.MainNotebook.Tabs.FirstOrDefault(x => x.TdiTab is TdiTabBase tab && tab.Tag?.ToString() == tag)?.TdiTab;
		
		private bool CheckDistrict(DeliveryPoint deliveryPoint)
		{
			if(deliveryPoint != null && deliveryPoint.District is null)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					$"Невозможно открыть карточку создания заказа, т.к. в точке доставки {deliveryPoint.Id} не указан логистический район," +
					" поэтому нельзя рассчитать доставку. Проверьте правильность установки и наличие координат");
				
				return false;
			}

			return true;
		}
	}
}
