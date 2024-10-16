using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes
{

	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "коды ЧЗ товаров",
			Nominative = "код ЧЗ товара")]
	[HistoryTrace]
	public abstract class TrueMarkProductCode : PropertyChangedBase, IDomainObject
	{
		private TrueMarkWaterIdentificationCode _trueMarkCode;

		public virtual int Id { get; set; }

		[Display(Name = "Код честного знака")]
		public virtual TrueMarkWaterIdentificationCode TrueMarkCode
		{
			get => _trueMarkCode;
			set => SetField(ref _trueMarkCode, value);
		}
	}
}
