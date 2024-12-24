using System;

namespace Edo.Transport.Messages.Dto
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
