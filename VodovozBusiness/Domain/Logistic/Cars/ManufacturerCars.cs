using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Logistic.Cars
{
	public class ManufacturerCars : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; }

		private string _name;

		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
	}
}
