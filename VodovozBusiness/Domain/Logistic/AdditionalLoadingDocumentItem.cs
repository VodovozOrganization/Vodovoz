using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "строка документа запаса",
		NominativePlural = "строки документов запаса")]
	[HistoryTrace]
	public class AdditionalLoadingDocumentItem : PropertyChangedBase, IDomainObject
	{
		private Nomenclature _nomenclature;
		private decimal _amount;
		private AdditionalLoadingDocument _additionalLoadingDocument;

		public virtual int Id { get; set; }

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Количество")]
		public virtual decimal Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		[Display(Name = "Документ запаса")]
		public virtual AdditionalLoadingDocument AdditionalLoadingDocument
		{
			get => _additionalLoadingDocument;
			set => SetField(ref _additionalLoadingDocument, value);
		}
	}
}
