using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Edo
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
	public class Tag1260CodeCheckResult : PropertyChangedBase, IDomainObject
	{
		private long _reqTimestamp;
		private Guid _reqId;

		public virtual int Id { get; set; }

		[Display(Name = "Уникальный идентификатор запроса")]
		public virtual Guid ReqId
		{
			get => _reqId;
			set => _reqId = value;
		}

		[Display(Name = "Дата и время регистрации запроса (в UTC)")]
		public virtual long ReqTimestamp
		{
			get => _reqTimestamp;
			set => SetField(ref _reqTimestamp, value);
		}
	}
}
