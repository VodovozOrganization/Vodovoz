using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class RouteListMileageDistributionNode : PropertyChangedBase
	{
		private string _customForwarderColumn;
		private decimal? _customRecalculatedDistanceColumn;
		public RouteList RouteList { get; set; }
		public string Id => IsRouteList ? RouteList.Id.ToString() : "";
		public string DeliveryShift => IsRouteList ? RouteList.Shift?.Name : "";
		public string Driver => IsRouteList ? RouteList.Driver?.GetPersonNameWithInitials() : "";
		public RouteListDistributionNodeType DistributionNodeType { get; set; }
		public bool IsRouteList => DistributionNodeType == RouteListDistributionNodeType.RouteList;

		public decimal ConfirmedDistance
		{
			get => IsRouteList ? RouteList.ConfirmedDistance : 0;
			set
			{
				if(IsRouteList)
				{
					RouteList.ConfirmedDistance = value;
				}

				OnPropertyChanged(nameof(ConfirmedDistance));
			}
		}

		public string MileageComment
		{
			get => IsRouteList ? RouteList.MileageComment : "";
			set
			{
				if(IsRouteList)
				{
					RouteList.MileageComment = value;
				}
			}
		}

		public string ForwarderColumn
		{
			get => IsRouteList ? RouteList.Forwarder?.GetPersonNameWithInitials() : _customForwarderColumn;
			set
			{
				if(!IsRouteList)
				{
					_customForwarderColumn = value;
				}
				else
				{
					throw new Exception("В этой ячейке нельзя изменить имя экспедитора");
				}
			}
		}

		public decimal? RecalculatedDistanceColumn
		{
			get => IsRouteList ? RouteList.RecalculatedDistance : _customRecalculatedDistanceColumn;
			set
			{
				if(IsRouteList)
				{
					RouteList.RecalculatedDistance = value;
				}
				else
				{
					_customRecalculatedDistanceColumn = value;
				}
			}
		}
	}

	public enum RouteListDistributionNodeType
	{
		RouteList,
		[Display(Name = "Итого за день")]
		Total,
		[Display(Name = "Разница")]
		Substract
	}
}
