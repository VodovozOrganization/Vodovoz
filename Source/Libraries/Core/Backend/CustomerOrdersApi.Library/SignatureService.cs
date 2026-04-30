using CustomerOrders.Contracts;
using CustomerOrdersApi.Library.Config;

namespace CustomerOrdersApi.Library
{
	/// <summary>
	/// Сервис для работы с подписью
	/// </summary>
	public class SignatureService
	{
		/// <summary>
		/// Получение подписи источника, для генерации подписи запроса
		/// </summary>
		/// <param name="source">Источник</param>
		/// <param name="signatureOptions">Настройки с данными о подписях</param>
		/// <returns>Подпись источника</returns>
		protected string GetSourceSign(ExternalSource source, SignatureOptions signatureOptions)
		{
			var signature =
				(string)typeof(SignatureOptions).GetProperty(source.ToString())?
					.GetValue(signatureOptions);
			
			return signature;
		}
	}
}
