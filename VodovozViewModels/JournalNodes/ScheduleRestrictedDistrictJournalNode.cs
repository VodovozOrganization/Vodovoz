using System;
using QS.Project.Journal;
using Vodovoz.Domain.Sale;

namespace Vodovoz.JournalNodes
{
	public class ScheduleRestrictedDistrictJournalNode : JournalEntityNodeBase
	{
		protected ScheduleRestrictedDistrictJournalNode() : base(typeof(ScheduleRestrictedDistrict))
		{
		}

		protected ScheduleRestrictedDistrictJournalNode(Type entityType) : base(entityType)
		{
		}

		public override string Title {
			get => Name;
			protected set { }
		}

		public string Name { get; set; }
		public string WageDistrict { get; set; }
	}
}
