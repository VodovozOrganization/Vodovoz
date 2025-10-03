namespace ResourceLocker.Library
{
	/// <summary>
	/// Результат блокировки ресурса
	/// </summary>
	public class ResourceLockResult
	{
		/// <summary>
		/// Блокировка завершилась успехом
		/// </summary>
		public bool IsSuccess { get; set; }
		
		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Владелец блокировки
		/// </summary>
		public string OwnerLockValue { get; set; }
	}
}
