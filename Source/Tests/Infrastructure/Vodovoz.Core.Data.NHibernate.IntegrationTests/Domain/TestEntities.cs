using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System;
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

	public enum BulkChildKind
	{
		First,
		Second
	}

	public class BulkAddress
	{
		public virtual string City { get; set; }
		public virtual string Street { get; set; }
	}

	/// <summary>
	/// Основная дочерняя сущность: все виды колонок, значения от БД, ссылки и собственная коллекция.
	/// </summary>
	public class BulkChild : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual BulkParent Parent { get; set; }
		public virtual string Name { get; set; }
		public virtual string UniqueCode { get; set; }
		public virtual string Notes { get; set; }
		public virtual decimal Amount { get; set; }
		public virtual DateTime OccurredAt { get; set; }
		public virtual int? NullableCount { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual BulkChildKind Kind { get; set; }
		public virtual BulkAddress Address { get; set; }
		public virtual BulkTag Tag { get; set; }
		public virtual DateTime CreatedAt { get; set; }
		public virtual int NameLength { get; set; }
		public virtual IList<BulkSubItem> SubItems { get; set; } = new List<BulkSubItem>();
	}

	public class BulkChildSpecial : BulkChild
	{
		public virtual string Extra { get; set; }
	}

	/// <summary>
	/// Наследник без маппинга (D-06).
	/// </summary>
	public class UnmappedChild : BulkChild
	{
	}

	public class BulkSubItem : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual BulkChild Child { get; set; }
	}

	public class BulkTag : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	public class ChildComponent
	{
		public virtual string Value { get; set; }
	}

	public class SimpleChild : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	public class NonInverseChild : SimpleChild
	{
	}

	public class SetChild : SimpleChild
	{
	}

	public class OrderedChild : SimpleChild
	{
	}

	public class FilteredChild : SimpleChild
	{
		public virtual bool IsArchived { get; set; }
	}

	public class WhereChild : SimpleChild
	{
		public virtual bool IsArchived { get; set; }
	}

	public class ListChild : SimpleChild
	{
	}

	public class ImmutableChild : SimpleChild
	{
	}

	public class JoinedChildBase : SimpleChild
	{
	}

	public class JoinedChild : JoinedChildBase
	{
		public virtual string Extra { get; set; }
	}

	public class SecondaryTableChild : SimpleChild
	{
		public virtual string Extra { get; set; }
	}

	public class VersionedChild : SimpleChild
	{
		public virtual int Version { get; set; }
	}

	public class PropertyRefChild : SimpleChild
	{
	}

	public class IdentityChild : SimpleChild
	{
	}

	public class AssignedChild : SimpleChild
	{
	}

	public class IncrementChild : SimpleChild
	{
	}

	public class HiLoChild : SimpleChild
	{
	}

	public class GuidChild
	{
		public virtual Guid Id { get; set; }
		public virtual string Name { get; set; }
	}

	public class GuidCombChild
	{
		public virtual Guid Id { get; set; }
		public virtual string Name { get; set; }
	}

	/// <summary>
	/// Широкая сущность: параметров в пачке получается больше лимита (F-15).
	/// </summary>
	public class WideChild : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual int C1 { get; set; }
		public virtual int C2 { get; set; }
		public virtual int C3 { get; set; }
		public virtual int C4 { get; set; }
		public virtual int C5 { get; set; }
		public virtual int C6 { get; set; }
		public virtual int C7 { get; set; }
		public virtual int C8 { get; set; }
		public virtual int C9 { get; set; }
		public virtual int C10 { get; set; }
	}

	/// <summary>
	/// Версионный родитель: блокировка увеличивает версию.
	/// </summary>
	public class VersionedParent : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual int Version { get; set; }
		public virtual string Name { get; set; } = "versioned";
		public virtual IList<VersionedParentChild> Items { get; set; } = new List<VersionedParentChild>();
	}

	public class VersionedParentChild : SimpleChild
	{
		public virtual VersionedParent Parent { get; set; }
	}

	/// <summary>
	/// Родитель с составным ключом: свойство дочерней сущности пересекается с ключом коллекции (C-12).
	/// </summary>
	public class CompositeParent
	{
		public virtual int KeyA { get; set; }
		public virtual int KeyB { get; set; }
		public virtual IList<CompositeChild> Items { get; set; } = new List<CompositeChild>();

		public override bool Equals(object obj) =>
			obj is CompositeParent other && other.KeyA == KeyA && other.KeyB == KeyB;

		public override int GetHashCode() => KeyA * 397 ^ KeyB;
	}

	public class CompositeChild : SimpleChild
	{
		public virtual int KeyA { get; set; }
	}
}
