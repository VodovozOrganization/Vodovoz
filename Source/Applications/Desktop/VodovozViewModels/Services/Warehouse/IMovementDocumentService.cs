using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Employees;
using VodovozBusiness.Nodes.TTN;

namespace Vodovoz.ViewModels.Services.Warehouse
{
	public interface IMovementDocumentService
	{
		/// <summary>
		/// Собрать отчёт ТТН по документу перемещения.
		/// </summary>
		TtnReport BuildTtnReport(MovementDocument entity, Employee currentEmployee);

		/// <summary>
		/// Сохранить ТТН в файл по указанному пути.
		/// </summary>
		void SaveTtnReport(TtnReport report, string outputPath);
	}
}
