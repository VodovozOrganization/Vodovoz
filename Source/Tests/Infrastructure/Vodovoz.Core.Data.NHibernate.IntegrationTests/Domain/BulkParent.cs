using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System.Collections.Generic;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	/// <summary>
	/// Родитель со всеми вариантами коллекций, поддерживаемыми и неподдерживаемыми.
	/// </summary>
	public class BulkParent : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; } = "parent";
		public virtual int Code { get; set; }

		public virtual IObservableList<BulkChild> Items { get; set; } = new ObservableList<BulkChild>();
		public virtual IList<NonInverseChild> NonInverseItems { get; set; } = new List<NonInverseChild>();
		public virtual ISet<SetChild> SetItems { get; set; } = new HashSet<SetChild>();
		public virtual IList<OrderedChild> OrderedItems { get; set; } = new List<OrderedChild>();
		public virtual IList<FilteredChild> FilteredItems { get; set; } = new List<FilteredChild>();
		public virtual IList<WideChild> WideItems { get; set; } = new List<WideChild>();
		public virtual IList<BulkTag> Tags { get; set; } = new List<BulkTag>();
		public virtual IList<string> Values { get; set; } = new List<string>();
		public virtual IList<ChildComponent> Components { get; set; } = new List<ChildComponent>();
		public virtual IList<ListChild> ListItems { get; set; } = new List<ListChild>();
		public virtual IList<WhereChild> WhereItems { get; set; } = new List<WhereChild>();
		public virtual IList<ImmutableChild> ImmutableItems { get; set; } = new List<ImmutableChild>();
		public virtual IList<JoinedChildBase> JoinedItems { get; set; } = new List<JoinedChildBase>();
		public virtual IList<SecondaryTableChild> SecondaryTableItems { get; set; } = new List<SecondaryTableChild>();
		public virtual IList<VersionedChild> VersionedItems { get; set; } = new List<VersionedChild>();
		public virtual IList<PropertyRefChild> PropertyRefItems { get; set; } = new List<PropertyRefChild>();
		public virtual IList<AssignedChild> AssignedItems { get; set; } = new List<AssignedChild>();
		public virtual IList<GuidChild> GuidItems { get; set; } = new List<GuidChild>();
		public virtual IList<GuidCombChild> GuidCombItems { get; set; } = new List<GuidCombChild>();
		public virtual IList<HiLoChild> HiLoItems { get; set; } = new List<HiLoChild>();
		public virtual IList<IncrementChild> IncrementItems { get; set; } = new List<IncrementChild>();
		public virtual IList<IdentityChild> IdentityItems { get; set; } = new List<IdentityChild>();
	}
}
