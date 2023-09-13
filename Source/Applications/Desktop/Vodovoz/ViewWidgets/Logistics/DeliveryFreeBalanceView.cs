using Gamma.Binding.Core;
using Gtk;
using QS.Views.GtkUI;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Pango;
using Vodovoz.Domain.Operations;
using Vodovoz.ViewModels.Widgets;
using Vodovoz.Infrastructure;

namespace Vodovoz.ViewWidgets.Logistics
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryFreeBalanceView : WidgetViewBase<DeliveryFreeBalanceViewModel>
	{
		private TextTag _defaultTag;
		private TextTag _boldTag;
		private TextTag _redBoldTag;

		public DeliveryFreeBalanceView(DeliveryFreeBalanceViewModel viewModel) : base(viewModel)
		{
			Build();

			Binding = new BindingControler<DeliveryFreeBalanceView>(this, new Expression<Func<DeliveryFreeBalanceView, object>>[]
			{
				w => w.ViewModel.ObservableDeliveryFreeBalanceOperations
			});

			Configure();
		}

		public GenericObservableList<DeliveryFreeBalanceOperation> ObservableDeliveryFreeBalanceOperations
		{
			get => ViewModel.ObservableDeliveryFreeBalanceOperations;
			set => ViewModel.ObservableDeliveryFreeBalanceOperations = value;
		}

		private void Configure()
		{
			InitializeBuffer();
			ViewModel.UpdateAction = Refresh;
		}

		public BindingControler<DeliveryFreeBalanceView> Binding { get; }

		private void Refresh()
		{
			var operations = ViewModel.ObservableDeliveryFreeBalanceOperations;

			if(operations == null)
			{
				return;
			}

			var buffer = ytextview.Buffer;
			buffer.Clear();
			var iter = buffer.EndIter;
			buffer.InsertWithTags(ref iter, "Свободные остатки в МЛ: ", _boldTag);

			var groupedOperations = operations.GroupBy(o => o.Nomenclature.Id).ToArray();

			int lastIndex = groupedOperations.Length - 1;
			for(var i = 0; i < groupedOperations.Length; i++)
			{
				var item = groupedOperations[i];
				var sum = item.Sum(o => o.Amount);

				if(sum == 0)
				{
					continue;
				}

				iter = buffer.EndIter;
				buffer.InsertWithTags(ref iter, item.First().Nomenclature.Name + ": ", _defaultTag);
				iter = buffer.EndIter;
				buffer.InsertWithTags(ref iter, sum.ToString("N0"), sum < 0 ? _redBoldTag : _boldTag);
				iter = buffer.EndIter;
				buffer.InsertWithTags(ref iter, " " + item.First().Nomenclature.Unit?.Name + (i == lastIndex ? "" : ", "), _defaultTag);
			}
		}

		private void InitializeBuffer()
		{
			var textTags = new TextTagTable();
			_boldTag = new TextTag("Bold");
			_boldTag.Weight = Pango.Weight.Bold;
			textTags.Add(_boldTag);

			_defaultTag = new TextTag("Default");
			textTags.Add(_defaultTag);
			
			_redBoldTag = new TextTag("Red");
			_redBoldTag.ForegroundGdk = GdkColors.Red;
			_redBoldTag.Weight = Weight.Bold;
			textTags.Add(_redBoldTag);

			ytextview.Buffer = new TextBuffer(textTags);
			ytextview.Editable = false;
		}
	}
}
