namespace Vodovoz.Errors.Documents
{
	public static class DocumentTemplate
	{
		public static Error NotFound =>
			new Error(
				typeof(DocumentTemplate),
				nameof(NotFound),
				"Не найден шаблон документа");
	}
}
