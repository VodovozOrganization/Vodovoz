using BitrixApi.Contracts.Dto.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace BitrixApi.Library.Services
{
	/// <summary>
	/// Сервис для отправки документов по электронной почте
	/// </summary>
	public interface IEmalSendService
	{
		/// <summary>
		/// Отправка документа по электронной почте
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task SendDocumentByEmail(SendReportRequest request, CancellationToken cancellationToken);
	}
}
