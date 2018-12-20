using System;
using QS.Dialog.Gtk;
using Vodovoz.Additions.Logistic.RouteOptimization;

namespace Vodovoz.Dialogs.Logistic
{
	[System.ComponentModel.ToolboxItem(false)]
	public partial class OptimizingParametersDlg : TdiTabBase
	{
		public OptimizingParametersDlg()
		{
			this.Build();
			this.TabName = "Настройки оптимизации";
			btnAccept.Clicked += BtnAccept_Clicked;
			btnCancel.Clicked += BtnCancel_Clicked;
			btnDefault.Clicked += BtnDefault_Clicked;
			ConfigureDlg();
			ConfigureDescription();
		}

		private void ConfigureDescription()
		{
			lblUnlikeDistrictPenaltyDesc.LabelProp = yspinUnlikeDistrictPenalty.TooltipText =
				"Штраф за поездку в отсутствующий в списке водителя район.";
			lblRemoveOrderFromExistRLPenaltyDesc.LabelProp = yspinRemoveOrderFromExistRLPenalty.TooltipText =
				"Штраф за передачу заказа другому водителю, если заказ уже находится в маршрутном листе сформированным до начала оптимизации.";
			lblDistrictPriorityPenaltyDesc.LabelProp = yspinDistrictPriorityPenalty.TooltipText =
				"Штраф за каждый шаг приоритета к каждому адресу, в менее приоритеном районе.";
			lblDriverPriorityPenaltyDesc.LabelProp = yspinDriverPriorityPenalty.TooltipText =
				"Штраф каждому менее приоритетному водителю, за единицу приоритета, при выходе на маршрут.";
			lblDriverPriorityAddressPenaltyDesc.LabelProp = yspinDriverPriorityAddressPenalty.TooltipText =
				"Штраф каждому менее приоритетному водителю на единицу приоритета, на каждом адресе.";
			lblMaxDistanceAddressPenaltyDesc.LabelProp = yspinMaxDistanceAddressPenalty.TooltipText =
				"Штраф за неотвезенный заказ. Или максимальное расстояние на которое имеет смысл ехать.";
			lblMaxBottlesInOrderForLargusDesc.LabelProp = yspinMaxBottlesInOrderForLargus.TooltipText =
				"Максимальное количество бутелей в заказе для ларгусов.";
			lblLargusMaxBottlePenaltyDesc.LabelProp = yspinLargusMaxBottlePenalty.TooltipText =
				"Штраф за добавление в лагрус большего количества бутелей. Сейчас установлено больше чем стоимость недоставки заказа. То есть такого проиходить не может.";
			lblSmallOrderNotLargusPenaltyDesc.LabelProp = yspinSmallOrderNotLargusPenalty.TooltipText =
				"Штраф обычному водителю если он взял себе адрес ларгуса.";
			lblMinAddressesInRoutePenaltyDesc.LabelProp = yspinMinAddressesInRoutePenalty.TooltipText =
				"Штраф за каждый адрес в маршруте меньше минимального позволенного в настройках машины";
			lblMinBottlesInRoutePenaltyDesc.LabelProp = yspinMinBottlesInRoutePenalty.TooltipText =
				"Штраф за каждую бутыль в маршруте меньше минимального позволенного в настройках машины";
		}

		void ConfigureDlg(){
			yspinUnlikeDistrictPenalty.Value = RouteOptimizer.UnlikeDistrictPenalty;
			yspinRemoveOrderFromExistRLPenalty.Value = RouteOptimizer.RemoveOrderFromExistRLPenalty;
			yspinDistrictPriorityPenalty.Value = RouteOptimizer.DistrictPriorityPenalty;
			yspinDriverPriorityPenalty.Value = RouteOptimizer.DriverPriorityPenalty;
			yspinDriverPriorityAddressPenalty.Value = RouteOptimizer.DriverPriorityAddressPenalty;
			yspinMaxDistanceAddressPenalty.Value = RouteOptimizer.MaxDistanceAddressPenalty;
			yspinMaxBottlesInOrderForLargus.Value = RouteOptimizer.MaxBottlesInOrderForLargus;
			yspinLargusMaxBottlePenalty.Value = RouteOptimizer.LargusMaxBottlePenalty;
			yspinSmallOrderNotLargusPenalty.Value = RouteOptimizer.SmallOrderNotLargusPenalty;
			yspinMinAddressesInRoutePenalty.Value = RouteOptimizer.MinAddressesInRoutePenalty;
			yspinMinBottlesInRoutePenalty.Value = RouteOptimizer.MinBottlesInRoutePenalty;
		}

		void AcceptNewValues(){
			RouteOptimizer.UnlikeDistrictPenalty = (long)yspinUnlikeDistrictPenalty.Value;
			RouteOptimizer.RemoveOrderFromExistRLPenalty = (long)yspinRemoveOrderFromExistRLPenalty.Value;
			RouteOptimizer.DistrictPriorityPenalty = (long)yspinDistrictPriorityPenalty.Value;
			RouteOptimizer.DriverPriorityPenalty = (long)yspinDriverPriorityPenalty.Value;
			RouteOptimizer.DriverPriorityAddressPenalty = (long)yspinDriverPriorityAddressPenalty.Value;
			RouteOptimizer.MaxDistanceAddressPenalty = (long)yspinMaxDistanceAddressPenalty.Value;
			RouteOptimizer.MaxBottlesInOrderForLargus = yspinMaxBottlesInOrderForLargus.ValueAsInt;
			RouteOptimizer.LargusMaxBottlePenalty = (long)yspinLargusMaxBottlePenalty.Value;
			RouteOptimizer.SmallOrderNotLargusPenalty = (long)yspinSmallOrderNotLargusPenalty.Value;
			RouteOptimizer.MinAddressesInRoutePenalty = (long)yspinMinAddressesInRoutePenalty.Value;
			RouteOptimizer.MinBottlesInRoutePenalty = (long)yspinMinBottlesInRoutePenalty.Value;
		}

		void SetSefaults()
		{
			RouteOptimizer.UnlikeDistrictPenalty = 20000;
			RouteOptimizer.RemoveOrderFromExistRLPenalty = 100000;
			RouteOptimizer.DistrictPriorityPenalty = 250;
			RouteOptimizer.DriverPriorityPenalty = 1000;
			RouteOptimizer.DriverPriorityAddressPenalty = 500;
			RouteOptimizer.MaxDistanceAddressPenalty = 300000;
			RouteOptimizer.MaxBottlesInOrderForLargus = 42;
			RouteOptimizer.LargusMaxBottlePenalty = 500000;
			RouteOptimizer.SmallOrderNotLargusPenalty = 2500;
			RouteOptimizer.MinAddressesInRoutePenalty = 25000;
			RouteOptimizer.MinBottlesInRoutePenalty = 10000;
			ConfigureDlg();
		}

		void BtnAccept_Clicked(object sender, EventArgs e)
		{
			AcceptNewValues();
			this.OnCloseTab(false);
		}

		void BtnDefault_Clicked(object sender, EventArgs e)
		{
			SetSefaults();
		}

		void BtnCancel_Clicked(object sender, EventArgs e)
		{
			this.OnCloseTab(false);
		}
	}
}
