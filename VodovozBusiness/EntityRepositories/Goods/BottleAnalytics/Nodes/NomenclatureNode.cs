using System;
using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.Goods.BottleAnalytics
{
	public class NomenclatureNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string VeryShortName { get; set; }
		public bool IsShabbyBottle { get; set; }
		public bool IsDefectiveBottle { get; set; }
		public string CategoryAsString { get; set; }

		private NomenclatureCategory? _category;

		public NomenclatureCategory Category
		{
			get
			{
				if(_category == null)
				{
					_category = (NomenclatureCategory)Enum.Parse(typeof(NomenclatureCategory), CategoryAsString);
				}
				return _category.Value;
			}
		}
	}
}
