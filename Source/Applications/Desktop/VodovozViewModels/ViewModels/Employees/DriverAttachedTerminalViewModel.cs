using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.Utilities.Text;
using QS.ViewModels;
using Vodovoz.Domain.Documents.DriverTerminal;

namespace Vodovoz.ViewModels.ViewModels.Employees
{
	public class DriverAttachedTerminalViewModel : EntityTabViewModelBase<DriverAttachedTerminalDocumentBase>
	{
		public string DriverName => PersonHelper.PersonFullName(Entity?.Driver?.LastName, Entity?.Driver?.Name, Entity?.Driver?.Patronymic);
		public string WarehouseTitle => Entity.GoodsAccountingOperation?.Warehouse?.Name;
		public string AuthorName => PersonHelper.PersonFullName(Entity?.Author?.LastName, Entity?.Author?.Name, Entity?.Author?.Patronymic);
		public string Date => $"{Entity?.CreationDate.ToShortDateString()} {Entity?.CreationDate.ToShortTimeString()}";
		public string DocType => Entity is DriverAttachedTerminalGiveoutDocument ? "Документ выдачи" : "Документ возврата";

		public DriverAttachedTerminalViewModel(
			IEntityUoWBuilder uowBuilder,
            IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices
            ) : base(uowBuilder, uowFactory, commonServices)
		{

		}
	}
}
