using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Geocoder
{
	public abstract class AddressCacheBase : PropertyChangedBase
	{
		private string _address;
		private decimal _latitude;
		private decimal _longitude;

		public virtual string Address
		{
			get => _address;
			set => SetField(ref _address, value);
		}

		public virtual decimal Latitude
		{
			get => _latitude;
			set => SetField(ref _latitude, value);
		}

		public virtual decimal Longitude
		{
			get => _longitude;
			set => SetField(ref _longitude, value);
		}
	}
}
