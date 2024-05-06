using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Fuel
{
	public class FuelLimit : PropertyChangedBase, IDomainObject
	{
		private string _serviceProductType;
		private string _serviceProductGroup;
		public virtual int Id { get; set; }

		public virtual string ServiceProductType
		{
			get => _serviceProductType;
			set => SetField(ref _serviceProductType, value);
		}

		public virtual string ServiceProductGroup
		{
			get => _serviceProductGroup;
			set => SetField(ref _serviceProductGroup, value);
		}
	}
}
