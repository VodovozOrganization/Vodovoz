using System;

namespace Edo.Contracts.Messages.Dto
{
	/// <summary>
	/// Информация о документе
	/// </summary>
	public class DocumentInfo
	{
		/// <summary>
		/// Id документа для идентификации
		/// </summary>
		public Guid DocumentId { get; set; }
	}
}
