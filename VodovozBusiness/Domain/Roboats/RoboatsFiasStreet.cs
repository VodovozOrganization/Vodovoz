using QS.DomainModel.Entity;
using System;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Roboats
{
	public class RoboatsFiasStreet : PropertyChangedBase, IDomainObject
	{
		private RoboatsStreet _roboatsAddress;
		private Guid _fiasStreetGuid;

		public virtual int Id { get; set; }

		public virtual RoboatsStreet RoboatsAddress
		{
			get => _roboatsAddress;
			set => SetField(ref _roboatsAddress, value);
		}

		public virtual Guid FiasStreetGuid
		{
			get => _fiasStreetGuid;
			set => SetField(ref _fiasStreetGuid, value);
		}
	}
}
