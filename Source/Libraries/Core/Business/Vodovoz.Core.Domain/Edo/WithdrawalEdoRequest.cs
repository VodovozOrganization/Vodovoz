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
		private DocumentEdoTask _baseDocumentEdoTask;

		/// <summary>
		/// Тип заявки - вывод из оборота
		/// </summary>
		public override EdoRequestType DocumentRequestType => EdoRequestType.Withdrawal;

		/// <summary>
		/// Базовая задача ЭДО, которая послужила основанием для создания заявки на вывод из оборота
		/// </summary>
		public virtual DocumentEdoTask BaseDocumentEdoTask
		{
			get => _baseDocumentEdoTask;
			set => SetField(ref _baseDocumentEdoTask, value);
		}
	}
}
