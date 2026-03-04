using QS.Project.Journal;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.ViewModels.Organizations
{
	public class OrganizationJournalNode : JournalEntityNodeBase<Organization>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public bool HasTaxcomEdoAccountId { get; set; }
		public bool HasAvangardShopId { get; set; }
		public bool HasCashBoxId { get; set; }
		public bool SendDebtLetters { get; set; }
		public bool SendDebtLettersWithASignatureAndSeal { get; set; }
	}
}
