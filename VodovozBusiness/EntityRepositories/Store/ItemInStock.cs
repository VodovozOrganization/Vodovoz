namespace Vodovoz.EntityRepositories.Store
{
	public class ItemInStock
	{
		public int Id { get; set; }
		public decimal Amount => Added - Removed;
		public decimal Added { get; set; }
		public decimal Removed { get; set; }
	}
}