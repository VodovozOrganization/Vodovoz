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
		/// <param name="entity">Документ перемещения</param>
		/// <param name="currentEmployee">Текущий сотрудник</param>
		/// <returns>Модель ТТН</returns>
		TtnReport BuildTtnReport(MovementDocument entity, Employee currentEmployee);

		/// <summary>
		/// Сохранить ТТН в файл по указанному пути.
		/// </summary>
		/// <param name="report">Модель ТТН</param>
		/// <param name="outputPath">Путь сохранения</param>
		void SaveTtnReport(TtnReport report, string outputPath);
	}
}
