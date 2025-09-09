namespace TaxcomEdoApi.Library.Config
{
	/// <summary>
	/// Эндпойнты для работы с документами
	/// </summary>
	public sealed class DocflowUri
	{
		/// <summary>
		/// Эндпойнт отправки контейнера с документами
		/// </summary>
		public string SendMessageUri { get; set; }
		/// <summary>
		/// Эндпойнт получения списка документов
		/// </summary>
		public string GetMessageListUri { get; set; }
		/// <summary>
		/// Эндпойнт получения документа
		/// </summary>
		public string GetMessageUri { get; set; }
	}
}
