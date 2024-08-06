using QS.Commands;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using ClientPaymentType = Vodovoz.Domain.Client.PaymentType;

namespace Vodovoz.Presentation.ViewModels.PaymentTypes
{
	public partial class SelectPaymentTypeViewModel : WindowDialogViewModelBase
	{
		public event EventHandler<PaymentTypeSelectedEventArgs> PaymentTypeSelected;

		public SelectPaymentTypeViewModel(INavigationManager navigation) : base(navigation)
		{
			Title = "Укажите форму оплаты";

			WindowPosition = QS.Dialog.WindowGravity.None;

			SelectPaymentTypeCommand = new DelegateCommand<ClientPaymentType>(SelectPaymentType);
		}

		public GenericObservableList<ClientPaymentType> ExcludedPaymentTypes { get; } = new GenericObservableList<ClientPaymentType>();

		public void AddExcludedPaymentTypes(params ClientPaymentType[] paymentTypes)
		{
			var paymentTypesToAdd = paymentTypes.Where(x => !ExcludedPaymentTypes.Contains(x)).ToArray();

			foreach(var paymentTypeToAdd in paymentTypesToAdd)
			{
				ExcludedPaymentTypes.Add(paymentTypeToAdd);
			}
		}

		public void RemoveExcludedPaymentTypes(params ClientPaymentType[] paymentTypes)
		{
			var paymentTypesToRemove = paymentTypes.Where(x => ExcludedPaymentTypes.Contains(x)).ToArray();

			foreach(var paymentTypeToAdd in paymentTypesToRemove)
			{
				ExcludedPaymentTypes.Remove(paymentTypeToAdd);
			}
		}

		public DelegateCommand<ClientPaymentType> SelectPaymentTypeCommand { get; }

		private void SelectPaymentType(ClientPaymentType type)
		{
			PaymentTypeSelected?.Invoke(this, new PaymentTypeSelectedEventArgs(type));
			Close(false, CloseSource.Self);
		}

		public bool IsPaymentTypeCashVisible => !ExcludedPaymentTypes.Contains(ClientPaymentType.Cash);
		public bool IsPaymentTypeTerminalVisible => !ExcludedPaymentTypes.Contains(ClientPaymentType.Terminal);
		public bool IsPaymentTypeDriverApplicationVisible => !ExcludedPaymentTypes.Contains(ClientPaymentType.DriverApplicationQR);
		public bool IsPaymentTypeSmsQrVisible => !ExcludedPaymentTypes.Contains(ClientPaymentType.SmsQR);
		public bool IsPaymentTypePaidOnlineVisible => !ExcludedPaymentTypes.Contains(ClientPaymentType.PaidOnline);
		public bool IsPaymentTypeBarterVisible => !ExcludedPaymentTypes.Contains(ClientPaymentType.Barter);
		public bool IsPaymentTypeContractDocumentationVisible => !ExcludedPaymentTypes.Contains(ClientPaymentType.ContractDocumentation);
		public bool IsPaymentTypeCashlessVisible => !ExcludedPaymentTypes.Contains(ClientPaymentType.Cashless);
	}
}
