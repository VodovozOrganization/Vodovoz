using System;
namespace Vodovoz.Core.Journal
{
	public abstract class MultipleEntityVMNodeBase
	{
		public abstract Type EntityType { get; set; }

		public abstract int DocumentId { get; set; }

		public abstract string DisplayName { get; }
	}
}
