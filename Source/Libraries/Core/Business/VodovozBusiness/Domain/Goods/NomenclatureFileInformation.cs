using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Common;

namespace VodovozBusiness.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах номенклатур",
		Nominative = "информация о прикрепленном файле номенклатуры")]
	public class NomenclatureFileInformation : FileInformation
	{
		private int _nomenclatureId;

		[Display(Name = "Идентификатор номенклатуры")]
		public virtual int NomenclatureId
		{
			get => _nomenclatureId;
			set => SetField(ref _nomenclatureId, value);
		}
	}
}
