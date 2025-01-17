using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.TrueMark
{
	/// <summary>
	/// Результат проверки разрешительного режима для тэга 1260
	/// </summary>
	[
	Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "результаты проверки разрешительного режима для тэга 1260",
		Nominative = "результат проверки разрешительного режима для тэга 1260"
		)
	]
	public class Tag1260CodeCheckResult : IDomainObject
	{
		public virtual int Id { get; set; }

		[Display(Name = "Уникальный идентификатор запроса")]
		public virtual Guid ReqId { get; set; }

		[Display(Name = "Дата и время регистрации запроса (в UTC)")]
		public virtual long ReqTimestamp { get; set; }
	}
}
