using QS.DomainModel.Entity;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
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
}
