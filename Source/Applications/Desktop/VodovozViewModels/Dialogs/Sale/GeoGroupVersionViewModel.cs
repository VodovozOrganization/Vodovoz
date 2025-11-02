using Gamma.Utilities;
using QS.Osrm;
using QS.ViewModels;
using System;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Sale;
using VodovozInfrastructure.Versions;

namespace Vodovoz.ViewModels.Dialogs.Sales
{
	public class GeoGroupVersionViewModel : ViewModelBase
	{
		public GeoGroupVersionViewModel(GeoGroupVersion geoGroupVersion)
		{
			Entity = geoGroupVersion ?? throw new ArgumentNullException(nameof(geoGroupVersion));
			Entity.PropertyChanged += GeoGroupVersion_PropertyChanged;
		}

		public GeoGroupVersion Entity { get; private set; }

		private void GeoGroupVersion_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(Entity.Id):
					OnPropertyChanged(nameof(Id));
					break;
				case nameof(Entity.CreationDate):
					OnPropertyChanged(nameof(CreationDate));
					break;
				case nameof(Entity.ActivationDate):
					OnPropertyChanged(nameof(ActivationDate));
					break;
				case nameof(Entity.ClosingDate):
					OnPropertyChanged(nameof(ClosingDate));
					break;
				case nameof(Entity.Status):
					OnPropertyChanged(nameof(Status));
					break;
				case nameof(Entity.Author):
					OnPropertyChanged(nameof(Author));
					break;
				default:
					break;
			}
		}

		public int Id => Entity.Id;

		public string CreationDate => Entity.CreationDate.ToString("dd.MM.yyyy HH:mm");
		public string ActivationDate
		{
			get
			{
				if(!Entity.ActivationDate.HasValue)
				{
					return string.Empty;
				}

				return Entity.ActivationDate.Value.ToString("dd.MM.yyyy HH:mm");
			}
		}

		public string ClosingDate
		{
			get
			{
				if(!Entity.ClosingDate.HasValue)
				{
					return string.Empty;
				}

				return Entity.ClosingDate.Value.ToString("dd.MM.yyyy HH:mm");
			}
		}

		public VersionStatus Status => Entity.Status;

		public string StatusTitle => Entity.Status.GetEnumTitle();

		public string Author => Entity.Author.GetPersonNameWithInitials();

		public virtual Subdivision CashSubdivision
		{
			get => Entity?.CashSubdivision;
			set => Entity.CashSubdivision = value;
		}

		public virtual Warehouse Warehouse
		{
			get => Entity?.Warehouse;
			set => Entity.Warehouse = value;
		}

		public virtual PointOnEarth? Coordinates
		{
			get
			{
				if(!Entity.BaseLatitude.HasValue || !Entity.BaseLongitude.HasValue)
				{
					return null;
				}

				return new PointOnEarth(Entity.BaseLatitude.Value, Entity.BaseLongitude.Value);
			}

			set
			{
				if(value == null)
				{
					Entity.BaseLatitude = null;
					Entity.BaseLatitude = null;
					return;
				}

				Entity.BaseLatitude = (decimal)value.Value.Latitude;
				Entity.BaseLongitude = (decimal)value.Value.Longitude;
				OnPropertyChanged(nameof(CoordinatesString));
			}
		}

		public string CoordinatesString
		{
			get
			{
				if(!Coordinates.HasValue)
				{
					return string.Empty;
				}

				return $"ш. {Coordinates.Value.Latitude:F5}, д. {Coordinates.Value.Longitude:F5}";
			}
		}
	}
}
