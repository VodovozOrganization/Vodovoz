using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Tdi;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.Factories
{
	public class DialogsFactory : IDialogsFactory
	{
		public ITdiTab CreateReadOnlyOrderDlg(int orderId)
		{
			var dlg = new OrderDlg(orderId);
			dlg.HasChanges = false;
			dlg.SetDlgToReadOnly();

			return dlg;
		}

		public ITdiDialog CreateCounterpartyDlg(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory uowFactory) =>
			new CounterpartyDlg(uowBuilder, uowFactory);
	}
}
