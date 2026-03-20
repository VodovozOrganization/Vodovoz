using System;

namespace TrueMark.Contracts
{
	/// <summary>
	/// Данные о документе ЧЗ
	/// </summary>
	public class TrueMarkDocumentInfo
	{
		/// <summary>
		/// Идентификатор документа в системе ЧестныйЗнак
		/// </summary>
		public Guid DocumentId { get; set; }

		/// <summary>
		/// Статус документа
		/// </summary>
		public TrueMarkDocumentStatus Status { get; set; }

		/// <summary>
		/// Описание ошибки, если статус документа Error
		/// </summary>
		public string ErrorMessage { get; set; }
	}
}
