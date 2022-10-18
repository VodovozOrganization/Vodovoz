using QS.Project.Journal;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Client
{
	public class PhonesJournalNode : JournalEntityNodeBase<Phone>
	{
		public string Phone { get; set; }
		public int? CounterpartyId { get;set; }
		public int? DeliveryPointId { get; set; }
		public string PhoneType { get; set; }

		public string Type
		{
			get 
			{ 
				if(DeliveryPointId != null)
				{
					return $"ТД: {PhoneType}";
				}

				if(CounterpartyId != null)
				{
					return $"Контрагент: {PhoneType}";
				}

				return null;
			}
		}
	}
}
