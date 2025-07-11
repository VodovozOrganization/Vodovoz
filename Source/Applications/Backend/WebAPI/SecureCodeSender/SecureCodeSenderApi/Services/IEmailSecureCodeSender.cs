using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.SecureCodes;

namespace SecureCodeSenderApi.Services
{
	public interface IEmailSecureCodeSender
	{
		Task<bool> SendCodeToEmail(IUnitOfWork uow, GeneratedSecureCode secureCode);
	}
}
