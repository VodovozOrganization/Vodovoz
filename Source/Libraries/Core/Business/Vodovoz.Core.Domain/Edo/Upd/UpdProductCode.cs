using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Domain.Edo.Upd
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "Коды маркировки",
		Nominative = "Код маркировки")]
	public class UpdProductCode : PropertyChangedBase, IDomainObject
	{
		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		[Display(Name = "Количество")]
		public virtual int Quantity { get; set; }

		/// <summary>
		/// Значение транспортного кода на момент формирования УПД трансфера.
		/// </summary>
		[Display(Name = "Транспортный код")]
		public virtual string TransportCode { get; set; }

		/// <summary>
		/// Значение индивидуального или группового кода на момент формирования УПД трансфера.
		/// </summary>
		[Display(Name = "Индивидуальный или групповой код")]
		public virtual string IdentificationCode { get; set; }

		/// <summary>
		/// Товар в УПД
		/// </summary>
		[Display(Name = "Товар в УПД")]
		public virtual UpdInventPosition InventPosition { get; set; }

		/// <summary>
		/// Индивидуальный код
		/// </summary>
		[Display(Name = "Индивидуальный код")]
		public virtual TrueMarkWaterIdentificationCode IndividualCode { get; set; }

		/// <summary>
		/// Групповой код
		/// </summary>
		[Display(Name = "Групповой код")]
		public virtual TrueMarkWaterGroupCode GroupCode { get; set; }
	}
}
