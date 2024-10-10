using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Common;

namespace Vodovoz.Core.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах номенклатур",
		Nominative = "информация о прикрепленном файле номенклатуры")]
	[HistoryTrace]
	public class NomenclatureFileInformation : FileInformation
	{
		private int _nomenclatureId;

		[Display(Name = "Идентификатор номенклатуры")]
		[HistoryIdentifier(TargetType = typeof(NomenclatureEntity))]
		public virtual int NomenclatureId
		{
			get => _nomenclatureId;
			set => SetField(ref _nomenclatureId, value);
		}
	}
}
