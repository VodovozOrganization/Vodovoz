using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class ReturnsNode
	{
		public int Id { get; set; }
		public Nomenclature Nomenclature { get; set; }
		public NomenclatureCategory NomenclatureCategory { get; set; }
		public int NomenclatureId { get; set; }
		public string Name { get; set; }
		public decimal Amount { get; set; }
		public bool Trackable { get; set; }
		public EquipmentKind EquipmentKind { get; set; }
		public DefectSource DefectSource { get; set; }

		public string Serial
		{
			get
			{
				if(Trackable)
					return Id > 0 ? Id.ToString() : "(не определен)";
				return string.Empty;
			}
		}

		public bool Returned
		{
			get => Amount > 0;
			set => Amount = value ? 1 : 0;
		}
	}
}
