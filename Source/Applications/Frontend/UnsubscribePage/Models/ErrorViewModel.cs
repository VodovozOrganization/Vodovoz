/// <summary>
/// ViewModel страницы ошибки.
/// </summary>
public class ErrorViewModel
{
	/// <summary>
	/// Идентификатор HTTP-запроса.
	/// Используется для поиска ошибки в логах.
	/// </summary>
	public string RequestId { get; set; }

	/// <summary>
	/// Нужно ли отображать RequestId.
	/// </summary>
	public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
