using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Онлайн виды номенклатур",
		Nominative = "Онлайн вид номенклатуры")]
	[HistoryTrace]
	public class NomenclatureOnlineGroup : PropertyChangedBase, IDomainObject
	{
		private string _name;
		private IList<NomenclatureOnlineCategory> _nomenclatureOnlineCategories = new List<NomenclatureOnlineCategory>();

		public virtual int Id { get; set; }
		
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		public virtual IList<NomenclatureOnlineCategory> NomenclatureOnlineCategories
		{
			get => _nomenclatureOnlineCategories;
			set => SetField(ref _nomenclatureOnlineCategories, value);
		}
	}
}
