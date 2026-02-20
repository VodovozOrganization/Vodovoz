using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Заявка на вывод кодов маркировки из оборота
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "заявка на вывод кодов из оборота",
		NominativePlural = "заявки на вывод кодов из оборота"
	)]
	public class WithdrawalEdoRequest : FormalEdoRequest
	{
		/// <summary>
		/// Тип заявки - вывод из оборота
		/// </summary>
		public override EdoRequestType DocumentRequestType => EdoRequestType.Withdrawal;
	}
}
