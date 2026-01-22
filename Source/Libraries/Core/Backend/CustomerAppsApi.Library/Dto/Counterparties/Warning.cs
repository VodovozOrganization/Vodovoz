namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Класс для размещения спец виджета предупреждения
	/// </summary>
	public class Warning
	{
		/// <summary>
		/// Заголовок
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// Описание
		/// </summary>
		public string Description { get; set; }
		/// <summary>
		/// Название кнопки
		/// </summary>
		public string Button { get; set; }

		public static Warning CreateAnotherAccountExists() => new Warning
		{
			Title = "У этой компании уже есть учетная запись",
			Description = "Зайдите в профиль компании через другую почту или обратитесь в поддержку",
			Button = "support"
		};
		
		public static Warning CreateCounterpartyArchived() => new Warning
		{
			Title = "Невозможно продолжить регистрацию",
			Description = "Создание личного кабинета для этой компании невозможно, пожалуйста, обратитесь в поддержку",
			Button = "support"
		};
	}
}
