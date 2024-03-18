using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.Utilities.Extensions;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Orders;
using Vodovoz.Tools.CallTasks;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Cash
{
	public class PaymentByCardViewModel: EntityTabViewModelBase<Order> 
	{
		private readonly Employee _currentEmployee;
		private readonly ICallTaskWorker _callTaskWorker;

		public PaymentByCardViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ICallTaskWorker callTaskWorker,
			IOrderPaymentSettings orderPaymentSettings,
			IOrderSettings orderSettings,
			IDeliveryRulesSettings deliveryRulesSettings,
			Employee currentEmployee) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			if(orderPaymentSettings == null)
			{
				throw new ArgumentNullException(nameof(orderPaymentSettings));
			}
			
			if(orderSettings == null)
			{
				throw new ArgumentNullException(nameof(orderSettings));
			}
			if(deliveryRulesSettings == null)
			{
				throw new ArgumentNullException(nameof(deliveryRulesSettings));
			}

			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_currentEmployee = currentEmployee;

			TabName = "Оплата по карте";

			Entity.PaymentType = PaymentType.Terminal;

			Entity.PropertyChanged += Entity_PropertyChanged;
			
			ValidationContext.ServiceContainer.AddService(orderSettings);
			ValidationContext.ServiceContainer.AddService(deliveryRulesSettings);
		}

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Entity.PaymentType)){
				OnPropertyChanged(nameof(PaymentType));
			}
		}

		public PaymentType PaymentType
		{
			get => Entity.PaymentType;
			set => Entity.PaymentType = value;
		}

		protected override bool BeforeValidation()
        {
			Entity.ChangePaymentTypeToByCardTerminal(_callTaskWorker);

	        if(!Entity.PayAfterShipment)
	        {
		        Entity.SelfDeliveryToLoading(_currentEmployee, CommonServices.CurrentPermissionService, _callTaskWorker);
	        }

	        if(Entity.SelfDelivery)
	        {
		        Entity.IsSelfDeliveryPaid = true;
	        }

	        return true;
        }
    }
}
