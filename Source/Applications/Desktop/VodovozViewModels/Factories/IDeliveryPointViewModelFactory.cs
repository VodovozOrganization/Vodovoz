using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Dialogs.Counterparties;

namespace Vodovoz.Factories
{
	public interface IDeliveryPointViewModelFactory
	{
		DeliveryPointViewModel GetForOpenDeliveryPointViewModel(int id);
		DeliveryPointViewModel GetForCreationDeliveryPointViewModel(Counterparty client);
	}
}
