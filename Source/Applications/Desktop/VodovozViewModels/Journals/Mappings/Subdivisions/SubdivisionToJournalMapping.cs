using Vodovoz.Journals.JournalViewModels.Organizations;

namespace Vodovoz.ViewModels.Journals.Mappings.Subdivisions
{
	/// <summary>
	/// Маппинг для подразделения(какой журнал открывать)
	/// </summary>
	public class SubdivisionToJournalMapping : EntityToJournalMapping<Subdivision>
	{
		public SubdivisionToJournalMapping()
		{
			Journal(typeof(SubdivisionsJournalViewModel));
		}
	}
}
