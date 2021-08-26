using System;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Utilities;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModels.Infrastructure.InfoProviders;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashInfoPanelView : Gtk.Bin, IPanelView
	{
		private readonly IUnitOfWork _uow;
		private readonly ICashRepository _cashRepository;

		public CashInfoPanelView(IUnitOfWorkFactory uowFactory, ICashRepository cashRepository)
		{
			this.Build();
			_uow = uowFactory?.CreateWithoutRoot("Боковая панель остатков по кассам") ?? throw new ArgumentNullException(nameof(uowFactory));
			_cashRepository = cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));
		}

		#region IPanelView implementation

		public IInfoProvider InfoProvider { get; set; }

		public bool VisibleOnPanel => true;

		public void OnCurrentObjectChanged(object changedObject) => Refresh();

		public void Refresh()
		{
			if(!(InfoProvider is ICashInfoProvider infoProvider))
			{
				return;
			}

			var filter = infoProvider.CashFilter;
			labelInfo.Text = $"{GetAllCashSummaryInfo(filter)}";
		}

		private string GetAllCashSummaryInfo(CashDocumentsFilter filter)
		{
			if(filter == null)
			{
				return "";
			}

			decimal totalCash = 0;
			var allCashString = "";
			var distinctBalances = _cashRepository
				.CurrentCashForGivenSubdivisions(_uow, filter.SelectedSubdivisions.Select(x => x.Id).ToArray());
			foreach(var node in distinctBalances)
			{
				totalCash += node.Balance;
				allCashString += $"\r\n{node.Name}: {CurrencyWorks.GetShortCurrencyString(node.Balance)}";
			}

			var total = $"Денег в кассе: {CurrencyWorks.GetShortCurrencyString(totalCash)}. ";
			var separatedCash = filter.SelectedSubdivisions.Any() ? $"\r\n\tИз них: {allCashString}" : "";
			return total + separatedCash;
		}

		#endregion
	}
}
