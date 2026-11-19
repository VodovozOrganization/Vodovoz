using System.Collections.Generic;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
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
}
