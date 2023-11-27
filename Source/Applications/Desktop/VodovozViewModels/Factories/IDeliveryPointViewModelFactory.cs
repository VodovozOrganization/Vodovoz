using System;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Dialogs.Counterparty;

namespace Vodovoz.Factories
{
	public interface IDeliveryPointViewModelFactory
	{
		DeliveryPointViewModel GetForOpenDeliveryPointViewModel(int id);
		DeliveryPointViewModel GetForCreationDeliveryPointViewModel(Counterparty client);
	}
}
