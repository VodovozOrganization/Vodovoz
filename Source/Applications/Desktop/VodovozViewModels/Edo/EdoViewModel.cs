using QS.DomainModel.UoW;
using System;
using QS.ViewModels.Dialog;
using QS.Navigation;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoViewModel : DialogViewModelBase, IDisposable
	{
		private readonly IUnitOfWork _uow;

		public EdoViewModel(
			IUnitOfWork uow,
			int orderId,
			EdoInOrderViewModel edoInOrderViewModel,
			INavigationManager navigation
			) : base(navigation)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			EdoInOrderViewModel = edoInOrderViewModel ?? throw new ArgumentNullException(nameof(edoInOrderViewModel));

			Title = $"ЭДО для заказа {orderId}";

			EdoInOrderViewModel.Setup(_uow, orderId);
			EdoInOrderViewModel.Load();
		}

		public EdoInOrderViewModel EdoInOrderViewModel { get; }

		public void Dispose()
		{
			_uow?.Dispose();
		}
	}
}
