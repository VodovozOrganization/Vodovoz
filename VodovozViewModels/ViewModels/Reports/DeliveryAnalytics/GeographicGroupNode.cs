using System;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModels.Logistic
{
	public class GeographicGroupNode : PropertyChangedBase
	{
		private bool selected;
		public virtual bool Selected {
			get => selected;
			set => SetField(ref selected, value);
		}

		private GeoGroup geographicGroup;
		public virtual GeoGroup GeographicGroup {
			get => geographicGroup;
			set => SetField(ref geographicGroup, value);
		}

		public GeographicGroupNode(GeoGroup geographicGroup)
		{
			GeographicGroup = geographicGroup ?? throw new ArgumentNullException(nameof(geographicGroup));
		}

		public override string ToString()
		{
			return geographicGroup.Name;
		}
	}
}
