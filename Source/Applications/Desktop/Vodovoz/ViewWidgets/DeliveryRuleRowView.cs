using Gamma.ColumnConfig;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.ViewWidgets
{
	[ToolboxItem(true)]
	public partial class DeliveryRuleRowView : Gtk.Bin
	{
		public DeliveryRuleRowView()
		{
			Build();
			Title = "";
			Schedule = "";
			ylabelTitle.UseMarkup = true;
			ylabelShifts.UseMarkup = true;
		}

		public string Title
		{
			get => ylabelTitle.LabelProp;
			set => ylabelTitle.LabelProp = $"<b>{value}</b>";
		}

		public string Schedule
		{
			get => ylabelShifts.LabelProp;
			set => ylabelShifts.LabelProp = value;
		}

		public void ConfigureDeliveryRulesTreeView(IList<DeliveryRuleRow> deliveryRules, bool isForService)
		{
			ytreeviewTodayDeliveryRules.Visible = deliveryRules.Any();

			if(!deliveryRules.Any())
			{
				return;
			}

			var deliveryRulesConfig = new FluentColumnsConfig<DeliveryRuleRow>();

			var dynamicColumnsCount = deliveryRules.First().DynamicColumns.Count;

			if(isForService)
			{
				for(int i = 0; i < dynamicColumnsCount; i++)
				{
					var currentIndex = i;

					deliveryRulesConfig
						.AddColumn(deliveryRules.First().DynamicColumns[currentIndex])
						.AddTextRenderer(n => n.DynamicColumns[currentIndex]);
				}
			}
			else
			{
				deliveryRulesConfig
					.AddColumn("Цена\nдоставки")
					.AddTextRenderer(n => n.Volune);

				for(int i = 0; i < dynamicColumnsCount; i++)
				{
					var currentIndex = i;

					deliveryRulesConfig
						.AddColumn(deliveryRules.First().DynamicColumns[currentIndex])
						.AddTextRenderer(n => $"до {n.DynamicColumns[currentIndex]}");
				}

				deliveryRulesConfig
					.AddColumn("Бесплатно")
					.AddTextRenderer(n => $"от {n.FreeDeliveryBottlesCount}");
			}

			ytreeviewTodayDeliveryRules.EnableGridLines = Gtk.TreeViewGridLines.Both;
			ytreeviewTodayDeliveryRules.ColumnsConfig = deliveryRulesConfig.Finish();
			ytreeviewTodayDeliveryRules.ItemsDataSource = deliveryRules.Skip(1).ToList();
		}
	}
}
