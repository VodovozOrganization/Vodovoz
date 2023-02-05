using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Operations;

namespace Vodovoz.ViewModels.Widgets
{
	public class DeliveryFreeBalanceViewModel : WidgetViewModelBase
	{
		private GenericObservableList<DeliveryFreeBalanceOperation> _observableDeliveryFreeBalanceOperations;
		public GenericObservableList<DeliveryFreeBalanceOperation> ObservableDeliveryFreeBalanceOperations
		{
			get => _observableDeliveryFreeBalanceOperations;
			set
			{
				if(_observableDeliveryFreeBalanceOperations == value)
				{
					return;
				}
				UnsubscribeFromChanges();
				_observableDeliveryFreeBalanceOperations = value;
				SubscribeToChanges();
				UpdateAction?.Invoke();
			}
		}

		public Action UpdateAction { get; set; }

		private void SubscribeToChanges()
		{
			if(_observableDeliveryFreeBalanceOperations == null)
			{
				return;
			}
			_observableDeliveryFreeBalanceOperations.ElementAdded += OnElementAdded;
			_observableDeliveryFreeBalanceOperations.ElementRemoved += OnElementRemoved;
			_observableDeliveryFreeBalanceOperations.ElementChanged += OnElementChanged;
		}

		private void UnsubscribeFromChanges()
		{
			if(_observableDeliveryFreeBalanceOperations == null)
			{
				return;
			}
			_observableDeliveryFreeBalanceOperations.ElementAdded -= OnElementAdded;
			_observableDeliveryFreeBalanceOperations.ElementRemoved -= OnElementRemoved;
			_observableDeliveryFreeBalanceOperations.ElementChanged -= OnElementChanged;
		}

		private void OnElementAdded(object alist, int[] aidx)
		{
			UpdateAction?.Invoke();
		}

		private void OnElementRemoved(object alist, int[] aidx, object aobject)
		{
			UpdateAction?.Invoke();
		}

		private void OnElementChanged(object alist, int[] aidx)
		{
			UpdateAction?.Invoke();
		}
	}
}
