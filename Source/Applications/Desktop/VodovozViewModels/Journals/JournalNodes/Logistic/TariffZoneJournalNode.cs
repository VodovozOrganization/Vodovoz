using QS.Project.Journal;
using System;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class TariffZoneJournalNode : JournalEntityNodeBase<TariffZone>
	{
		public string Name { get; set; }
		public bool IsFastDeliveryAvailable { get; set; }
		public TimeSpan FastDeliveryTimeFrom { get; set; }
		public TimeSpan FastDeliveryTimeTo { get; set; }
		public string FastDeliveryAvailableTime => $"с {FastDeliveryTimeFrom:hh\\:mm} до {FastDeliveryTimeTo:hh\\:mm}";
		public override string Title => Name;
	}
}
