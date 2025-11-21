using QS.Project.Journal;
using System;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Goods
{
	public class EquipmentKindJournalNode : JournalEntityNodeBase<EquipmentKind>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public WarrantyCardType WarrantyCardType { get; set; }
	}
}
