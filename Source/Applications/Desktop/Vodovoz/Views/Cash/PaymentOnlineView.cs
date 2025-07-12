using QS.Dialog;
using QS.Views.GtkUI;
using QSWidgetLib;
using System;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Cash;

namespace Vodovoz.Views.Cash
{
	public partial class PaymentOnlineView : TabViewBase<PaymentOnlineViewModel>
	{
		private readonly IInteractiveService _interactiveService;
		public PaymentOnlineView(PaymentOnlineViewModel viewModel,
			IInteractiveService interactiveService) : base(viewModel)
		{
			_interactiveService = interactiveService;
			Build();
			Configure();
		}

		private void Configure()
		{
			ybuttonSave.Clicked += SaveHandler;
			ybuttonCancel.BindCommand(ViewModel.CloseCommand);

			entryOnlineOrder.ValidationMode = (QS.Widgets.ValidationType)ValidationType.numeric;
			entryOnlineOrder.Binding.AddBinding(ViewModel.Entity,
				e => e.OnlinePaymentNumber,
				w => w.Text,
				new NullableIntToStringConverter()).InitializeFromSource();

			comboPaymentFrom.SetRenderTextFunc<PaymentFrom>(x => x.Name);

			comboPaymentFrom.Binding.AddBinding(ViewModel,
				vm => vm.ItemsList, w => w.ItemsList)
				.InitializeFromSource();

			comboPaymentFrom.Binding.AddBinding(ViewModel,
				vm => vm.PaymentOnlineFrom, w => w.SelectedItem)
				.InitializeFromSource();
		}

		private void SaveHandler(object sender, EventArgs e)
		{
			if(ViewModel.RequiresShipmentConfirmationWarning())
			{
				var result = _interactiveService.Question(new[] { "Продолжить", "Отмена" },
					"Заказ ещё не отгружен. При продолжении оплаты заказ будет закрыт, и отгрузка станет невозможной. Продолжить?", "Ошибка");

				if(string.IsNullOrEmpty(result) || result == "Отмена")
				{
					return;
				}
			}
			if(ViewModel.SaveCommand.CanExecute())
			{
				ViewModel.SaveCommand.Execute();
			}
		}
	}
}

