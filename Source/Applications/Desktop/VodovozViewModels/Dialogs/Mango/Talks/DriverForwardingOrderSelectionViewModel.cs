using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using VodovozBusiness.EntityRepositories.Nodes;

namespace Vodovoz.ViewModels.Dialogs.Mango.Talks
{
	/// <summary>
	/// Диалог выбора заказа клиента для перевода звонка на водителя, доставляющего этот заказ
	/// </summary>
	public class DriverForwardingOrderSelectionViewModel : WindowDialogViewModelBase
	{
		private DriverForwardingOrderNode _selectedOrder;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="navigation">Навигация</param>
		/// <param name="orders">Заказы в пути, среди которых выбирается заказ для перевода звонка</param>
		public DriverForwardingOrderSelectionViewModel(
			INavigationManager navigation,
			IList<DriverForwardingOrderNode> orders) : base(navigation)
		{
			Orders = orders ?? throw new ArgumentNullException(nameof(orders));

			Title = DriverCallForwardingMessages.DialogTitle;
			WindowPosition = WindowGravity.None;

			if(!Orders.Any())
			{
				throw new AbortCreatingPageException(
					"У контрагента нет заказов в пути",
					Title,
					ImportanceLevel.Warning);
			}

			ForwardCallCommand = new DelegateCommand(ForwardCall, () => CanForwardCall);
			ForwardCallCommand.CanExecuteChangedWith(this, x => x.CanForwardCall);

			CancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
		}

		/// <summary>
		/// Обработчик перевода звонка на водителя выбранного заказа
		/// Возвращает <c>true</c>, если звонок переведён — в этом случае окно закрывается
		/// Задаётся диалогом разговора, открывающим это окно
		/// </summary>
		public Func<DriverForwardingOrderNode, bool> ForwardCallHandler { get; set; }

		/// <summary>
		/// Команда перевода звонка на водителя выбранного заказа
		/// </summary>
		public DelegateCommand ForwardCallCommand { get; }

		/// <summary>
		/// Команда закрытия окна без перевода звонка
		/// </summary>
		public DelegateCommand CancelCommand { get; }

		/// <summary>
		/// Заказы клиента в пути
		/// </summary>
		public IList<DriverForwardingOrderNode> Orders { get; }

		/// <summary>
		/// Выбранный заказ
		/// </summary>
		[PropertyChangedAlso(nameof(CanForwardCall))]
		public DriverForwardingOrderNode SelectedOrder
		{
			get => _selectedOrder;
			set => SetField(ref _selectedOrder, value);
		}

		/// <summary>
		/// Можно ли перевести звонок на водителя по выбранному заказу
		/// </summary>
		public bool CanForwardCall => SelectedOrder?.CanForwardCall == true;

		private void ForwardCall()
		{
			if(!CanForwardCall)
			{
				return;
			}

			if(ForwardCallHandler?.Invoke(SelectedOrder) == true)
			{
				Close(false, CloseSource.Self);
			}
		}
	}
}
