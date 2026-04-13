namespace TaxcomEdoApi.Library;

public static class ExternalApiConstants
{
	private const string _api = "/api";
	private const string _apiVersion = "/v1.3";
	public const string AssistantKeyHeader = "Assistant-Key";
	public const string IntegratorIdHeader = "Integrator-Id";
	
	/// <summary>
	/// Эндпойнт принятия контакта
	/// </summary>
	public const string AcceptContactUri = _apiVersion + _api + "/AcceptContact";
	/// <summary>
	/// Эндпойнт получения контактов
	/// </summary>
	public const string GetContactsUri = _apiVersion + _api + "/GetContacts";
	/// <summary>
	/// Эндпойнт отбраковки контакта
	/// </summary>
	public const string RejectContactUri = _apiVersion + _api + "/RejectContact";
	/// <summary>
	/// Эндпойнт отправки приглашений
	/// </summary>
	public const string SendContactsUri = _apiVersion + _api + "/SendContacts";
	/// <summary>
	/// Эндпойнт проверки клиента на сервере Такском
	/// </summary>
	public const string CheckContragentUri = _apiVersion + _api + "/CheckContragent";
	/// <summary>
	/// Эндпойнт обновлений списка статусов приглашений
	/// </summary>
	public const string GetContactListUpdatesUri = _apiVersion + _api + "/GetContactListUpdates";
	/// <summary>
	/// Эндпойнт отправки контейнера с документами
	/// </summary>
	public const string SendMessageUri = _apiVersion + _api + "/SendMessage";
	/// <summary>
	/// Эндпойнт получения списка документов
	/// </summary>
	public const string GetMessageListUri = _apiVersion + _api + "/GetMessageList";
	/// <summary>
	/// Эндпойнт получения списка ДО со статусами
	/// </summary>
	public const string GetListUri = _apiVersion + _api + "/GetList";
	/// <summary>
	/// Эндпойнт получения документа
	/// </summary>
	public const string GetMessageUri = _apiVersion + _api + "/GetMessage";
	/// <summary>
	/// Эндпойнт авторизации по сертификату
	/// </summary>
	public const string CertificateLoginUri = _apiVersion + _api + "/CertificateLogin";
	/// <summary>
	/// Эндпойнт авторизации по логину/паролю
	/// </summary>
	public const string LoginUri = _apiVersion + _api + "/Login";
}
