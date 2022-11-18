using QS.DomainModel.Entity;

namespace Vodovoz.Old1612ExportTo1c
{
	public class NomenclatureType1c : IDomainObject
	{
		public int Id{ get; set;}
		public string Name{ get; set; }
		public bool IsService{ get; set;}

		public static NomenclatureType1c GoodsType { get; private set;}
		public static NomenclatureType1c ServicesType { get; private set;}

		static NomenclatureType1c()
		{
			GoodsType = new NomenclatureType1c {
				Id = 1,
				Name = "Товары",
				IsService = false
			};

			ServicesType = new NomenclatureType1c {
				Id = 2,
				Name = "Услуги",
				IsService = true
			};
		}
	}
}

