using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Goods;

namespace VodovozBusiness.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах номенклатур",
		Nominative = "информация о прикрепленном файле номенклатуры")]
	[HistoryTrace]
	public class NomenclatureFileInformation : FileInformation
	{
		private int _nomenclatureId;

		[Display(Name = "Идентификатор номенклатуры")]
		[HistoryIdentifier(TargetType = typeof(Nomenclature))]
		public virtual int NomenclatureId
		{
			get => _nomenclatureId;
			set => SetField(ref _nomenclatureId, value);
		}
	}
}
