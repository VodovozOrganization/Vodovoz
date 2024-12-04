using Microsoft.Extensions.Logging;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.OrderChanges
{
	public partial class OrderChangesReportViewModel : DialogTabViewModelBase
	{
		private readonly ILogger<OrderChangesReportViewModel> _logger;
		private readonly IGenericRepository<Organization> _organizationGenericRepository;

		public OrderChangesReportViewModel(
			ILogger<OrderChangesReportViewModel> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IGenericRepository<Organization> organizationGenericRepository)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_logger =
				logger ?? throw new System.ArgumentNullException(nameof(logger));
			_organizationGenericRepository =
				organizationGenericRepository ?? throw new System.ArgumentNullException(nameof(organizationGenericRepository));

			Organizations = GetAllOrganizations();
		}

		public IList<Organization> Organizations { get; }

		public IList<SelectableKeyValueNode> ChangeTypes =
			new List<SelectableKeyValueNode>
			{
				new SelectableKeyValueNode("Фактическое кол-во товара", "ActualCount"),
				new SelectableKeyValueNode("Цена товара", "Price"),
				new SelectableKeyValueNode("Добавление/Удаление товаров", "OrderItemsCount"),
				new SelectableKeyValueNode("Тип оплаты заказа", "PaymentType")
			};

		public IList<SelectableKeyValueNode> IssueTypes =
			new List<SelectableKeyValueNode>
			{
				new SelectableKeyValueNode("Проблемы с смс", "SmsIssues"),
				new SelectableKeyValueNode("Проблемы с qr", "QrIssues"),
				new SelectableKeyValueNode("Проблемы с терминалами", "TerminalIssues"),
				new SelectableKeyValueNode("Проблемы менеджеров", "ManagersIssues")
			};

		private IList<Organization> GetAllOrganizations()
		{
			var organizations = _organizationGenericRepository.Get(UoW).ToList();

			return organizations;
		}
	}
}
