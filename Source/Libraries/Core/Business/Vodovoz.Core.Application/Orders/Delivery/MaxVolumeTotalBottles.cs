namespace Vodovoz.Core.Application.Orders.Delivery
{
	/// <inheritdoc/>
	public class MaxVolumeTotalBottles : IMaxVolumeTotalBottles
	{
		private MaxVolumeTotalBottles(decimal max, decimal current)
		{
			Max = max;
			Current = current;
		}
		
		/// <inheritdoc/>
		public decimal Max { get; }
		/// <inheritdoc/>
		public decimal Current { get; }

		public static IMaxVolumeTotalBottles Create(decimal max, decimal current) =>
			new MaxVolumeTotalBottles(max, current);
	}
}
