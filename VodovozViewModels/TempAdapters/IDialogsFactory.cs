using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Tdi;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface IDialogsFactory
	{
		ITdiTab CreateReadOnlyOrderDlg(int orderId);
		ITdiDialog CreateCounterpartyDlg(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory uowFactory);
	}
}
