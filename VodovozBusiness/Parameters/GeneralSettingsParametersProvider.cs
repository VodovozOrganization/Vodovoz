using System;

namespace Vodovoz.Parameters
{
	public class GeneralSettingsParametersProvider : IGeneralSettingsParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;
		private const string _routeListPrintedFormPhones = "route_list_printed_form_phones";
		private const string _canAddForwarderToLargus = "can_add_forwarders_to_largus";

		public GeneralSettingsParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public string GetRouteListPrintedFormPhones => _parametersProvider.GetStringValue(_routeListPrintedFormPhones);

		public void UpdateRouteListPrintedFormPhones(string text) =>
			_parametersProvider.CreateOrUpdateParameter(_routeListPrintedFormPhones, text);

		public bool GetCanAddForwardersToLargus => _parametersProvider.GetValue<bool>(_canAddForwarderToLargus);

		public void UpdateCanAddForwardersToLargus(bool value) =>
			_parametersProvider.CreateOrUpdateParameter(_canAddForwarderToLargus, value.ToString());
	}
}
