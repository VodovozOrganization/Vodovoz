using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Онлайн типы номенклатур",
		Nominative = "Онлайн тип номенклатуры")]
	[HistoryTrace]
	public class NomenclatureOnlineCategory : PropertyChangedBase, IDomainObject
	{
		private string _name;
		private NomenclatureOnlineGroup _nomenclatureOnlineGroup;
		
		public virtual int Id { get; set; }
		
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		public virtual NomenclatureOnlineGroup NomenclatureOnlineGroup
		{
			get => _nomenclatureOnlineGroup;
			set => SetField(ref _nomenclatureOnlineGroup, value);
		}
	}
}
