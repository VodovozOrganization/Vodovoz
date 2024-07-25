using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.Payments;

namespace Vodovoz.ViewModels.Orders.Reports
{
	public class OnlinePaymentsReportViewModel : DialogViewModelBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IPaymentsRepository _paymentsRepository;

		public OnlinePaymentsReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IPaymentsRepository paymentsRepository,
			INavigationManager navigation)
			: base(navigation)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			Title = "Отчет по оплатам OnLine заказов";

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot(Title);

			_paymentsRepository = paymentsRepository
				?? throw new ArgumentNullException(nameof(paymentsRepository));

			Shops = _paymentsRepository.GetAllShopsFromTinkoff(_unitOfWork);
		}

		public IEnumerable<string> Shops { get; }
	}
}
