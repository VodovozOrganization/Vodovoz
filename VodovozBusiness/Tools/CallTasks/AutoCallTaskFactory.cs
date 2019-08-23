using System;
using System.ComponentModel;
using System.Data.Bindings.Utilities;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Tools.CallTasks
{
	public class AutoCallTaskFactory
	{
		private Order order;
		public virtual Order Order {
			get { return order; }
			set {
				order = value; 
				ConfigureOrderChangeHeandlers(); 
			}
		}

		public AutoCallTaskFactory(Order order)
		{
			this.order = order ?? throw new ArgumentNullException(nameof(order));
		}

		private void ConfigureOrderChangeHeandlers()
		{
			if(order == null)
				return;
			order.PropertyChanged += OrderPropertyChanged;
		}

		private void OrderPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			string statusPropertyName = order.GetPropertyName(x => x.OrderStatus);
			if(String.Equals(e.PropertyName, statusPropertyName))
				TryCreateTask();
		}

		private bool TryCreateTask()
		{
			switch(order.OrderStatus) 
			{
				case OrderStatus.Accepted: return TryCreateCallTask();
				case OrderStatus.Shipped: return TryCreateDepositReturnTask();
				case OrderStatus.DeliveryCanceled: return false;	
				default: return false;
			}
		}

		private bool TryCreateCallTask()
		{
			return false;
		}

		private bool TryCreateDepositReturnTask()
		{
			return false;
		}

		private bool TryDeleteTask()
		{
			return false;
		}

		private bool SaveTask(CallTask callTask)
		{
			return false;
		}
	}
}
