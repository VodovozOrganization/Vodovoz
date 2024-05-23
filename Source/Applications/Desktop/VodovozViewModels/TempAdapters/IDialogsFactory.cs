using QS.Tdi;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface IDialogsFactory
	{
		ITdiTab CreateReadOnlyOrderDlg(int orderId);
		ITdiDialog CreateCounterpartyDlg(NewCounterpartyParameters parameters);
	}

	public class NewCounterpartyParameters
	{
		public string Name { get; set; }
		public string FullName { get; set; }
		public string INN { get; set; }
		public string KPP { get; set; }
		public PaymentType PaymentMethod { get; set; }
		public string TypeOfOwnership { get; set; }
		public PersonType PersonType { get; set; }
		public string CounterpartyCurrentAcc { get; set; }
		public string CounterpartyBik { get; set; }
		public string CounterpartyBank { get; set; }
		public string CounterpartyCorrespondentAcc { get; set; }
	}
}
