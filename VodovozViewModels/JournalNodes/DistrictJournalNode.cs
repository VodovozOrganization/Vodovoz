using System;
using QS.Project.Journal;
using Vodovoz.Domain.Sale;

namespace Vodovoz.JournalNodes
{
	public class DistrictJournalNode : JournalEntityNodeBase
	{
		protected DistrictJournalNode() : base(typeof(District))
		{
		}

		protected DistrictJournalNode(Type entityType) : base(entityType)
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
