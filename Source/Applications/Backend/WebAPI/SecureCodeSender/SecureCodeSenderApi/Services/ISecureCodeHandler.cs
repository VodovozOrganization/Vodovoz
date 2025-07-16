using System.Threading.Tasks;
using SecureCodeSender.Contracts.Requests;
using Vodovoz.Core.Domain.Results;

namespace SecureCodeSenderApi.Services
{
	public interface ISecureCodeHandler
	{
		Task<Result<(string Code, int TimeForNextCode)>> GenerateAndSendSecureCode(SendSecureCodeDto sendSecureCodeDto);
		(string Code, int TimeForNextCode) GenerateSecureCode(SendSecureCodeDto sendSecureCodeDto);
		(int Response, string Message) CheckSecureCode(CheckSecureCodeDto checkSecureCodeDto);
	}
}
