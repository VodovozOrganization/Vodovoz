using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Documents
{
	public static class DocumentTemplateErrors
	{
		public static Error NotFound =>
			new Error(
				typeof(DocumentTemplateErrors),
				nameof(NotFound),
				"Не найден шаблон документа");
	}
}
