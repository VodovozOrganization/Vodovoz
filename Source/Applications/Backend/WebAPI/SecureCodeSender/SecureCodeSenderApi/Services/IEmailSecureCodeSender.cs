using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.SecureCodes;

namespace SecureCodeSenderApi.Services
{
	/// <summary>
	/// Интерфейс для отправки кода авторизации по почте
	/// </summary>
	public interface IEmailSecureCodeSender
	{
		/// <summary>
		/// Отправка кода авторизации по электронной почте
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="secureCode">Данные по коду</param>
		/// <returns><c>true</c> - отправлено, <c>false</c> - не отправлено</returns>
		Task<bool> SendCodeToEmail(IUnitOfWork uow, GeneratedSecureCode secureCode);
	}
}
