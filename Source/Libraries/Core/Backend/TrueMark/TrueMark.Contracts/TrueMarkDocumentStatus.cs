namespace TrueMark.Contracts
{
	/// <summary>
	/// Статус документа в системе ЧестныйЗнак
	/// </summary>
	public enum TrueMarkDocumentStatus
	{
		/// <summary>
		/// Обработан успешно
		/// </summary>
		Ok,
		/// <summary>
		/// Обработан с ошибками
		/// </summary>
		Error,
		/// <summary>
		/// В системе ЧестныйЗнак не найден документ с данным идентификатором
		/// </summary>
		NotFound
	}
}
