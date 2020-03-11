using QSOrmProject.RepresentationModel;

namespace Vodovoz.JournalFilters
{
	public partial class WarehouseFilter : RepresentationFilterBase<WarehouseFilter>
	{
		public WarehouseFilter()
		{
			this.Build();
			ycheckRestrictArchive.Toggled += (sender, e) => {
				RestrictArchive = ycheckRestrictArchive.Active;
				OnRefiltered();
			};
		}

		bool restrictArchive;
		public virtual bool RestrictArchive {
			get => restrictArchive;
			set { restrictArchive = value;
				ycheckRestrictArchive.Active = value;
			}
		}
	}
}
