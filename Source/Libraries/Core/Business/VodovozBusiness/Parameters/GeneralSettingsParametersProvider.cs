using System;
using System.Linq;

namespace Vodovoz.Parameters
{
	public class GeneralSettingsParametersProvider : IGeneralSettingsParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;
		private const string _routeListPrintedFormPhones = "route_list_printed_form_phones";
		private const string _canAddForwarderToLargus = "can_add_forwarders_to_largus";
		private const string _orderAutoComment = "OrderAutoComment";
		private const string _subdivisionsToInformComplaintHasNoDriverParameterName = "SubdivisionsToInformComplaintHasNoDriver";

		public GeneralSettingsParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public string GetRouteListPrintedFormPhones => _parametersProvider.GetStringValue(_routeListPrintedFormPhones);

		public void UpdateRouteListPrintedFormPhones(string text) =>
			_parametersProvider.CreateOrUpdateParameter(_routeListPrintedFormPhones, text);

		public bool GetCanAddForwardersToLargus => _parametersProvider.GetValue<bool>(_canAddForwarderToLargus);

		public string OrderAutoComment => _parametersProvider.GetStringValue(_orderAutoComment);

		public int[] SubdivisionsToInformComplaintHasNoDriver => GetSubdivisionsToInformComplaintHasNoDriver();

		public void UpdateOrderAutoComment(string value) =>
			_parametersProvider.CreateOrUpdateParameter(_orderAutoComment, value);


		public void UpdateCanAddForwardersToLargus(bool value) =>
			_parametersProvider.CreateOrUpdateParameter(_canAddForwarderToLargus, value.ToString());

		public void UpdateSubdivisionsToInformComplaintHasNoDriver(int[] subdivisionIds)
		{
			_parametersProvider.CreateOrUpdateParameter(_subdivisionsToInformComplaintHasNoDriverParameterName, string.Join(", ", subdivisionIds));
		}

		private int[] GetSubdivisionsToInformComplaintHasNoDriver()
		{
			var parameterValue = _parametersProvider.GetParameterValue(_subdivisionsToInformComplaintHasNoDriverParameterName, true);
			var splitedIds = parameterValue.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			return splitedIds
				.Select(x => int.Parse(x))
				.ToArray();
		}
	}
}
