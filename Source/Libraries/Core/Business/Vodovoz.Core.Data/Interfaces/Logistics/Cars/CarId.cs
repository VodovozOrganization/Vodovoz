namespace Vodovoz.Core.Data.Interfaces.Logistics.Cars
{
	public class CarId
	{
		protected CarId() { }

		private CarId(int carId)
		{
			Id = carId;
		}

		public int? Id { get; }

		public static CarId Create(int carId)
			=> new CarId(carId);
	}
}
