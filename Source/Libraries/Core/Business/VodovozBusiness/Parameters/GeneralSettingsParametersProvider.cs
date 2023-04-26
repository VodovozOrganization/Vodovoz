using System;
using System.Collections.Generic;
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
		private const string _subdivisionsForAlternativePricesName = "SubdivisionsForAlternativePricesName";

		public GeneralSettingsParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}
		
		private int[] GetSubdivisionsToInformComplaintHasNoDriver()
		{
			var parameterValue = _parametersProvider.GetParameterValue(_subdivisionsToInformComplaintHasNoDriverParameterName, true);
			var splitedIds = parameterValue.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			return splitedIds
				.Select(x => int.Parse(x))
				.ToArray();
		}

		private int[] GetSubdivisionssForAlternativePrices()
		{
			var parameterValue = _parametersProvider.GetParameterValue(_subdivisionsForAlternativePricesName, true);
			var splitedIds = parameterValue.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			return splitedIds
				.Select(x => int.Parse(x))
				.ToArray();
		}
		
		public string GetRouteListPrintedFormPhones => _parametersProvider.GetStringValue(_routeListPrintedFormPhones);

		public void UpdateRouteListPrintedFormPhones(string text) =>
			_parametersProvider.CreateOrUpdateParameter(_routeListPrintedFormPhones, text);

		public bool GetCanAddForwardersToLargus => _parametersProvider.GetValue<bool>(_canAddForwarderToLargus);

		public string OrderAutoComment => _parametersProvider.GetStringValue(_orderAutoComment);

		public int[] SubdivisionsToInformComplaintHasNoDriver => GetSubdivisionsToInformComplaintHasNoDriver();

		public int[] SubdivisionsForAlternativePrices => GetSubdivisionssForAlternativePrices();

		public void UpdateOrderAutoComment(string value) =>
			_parametersProvider.CreateOrUpdateParameter(_orderAutoComment, value);


		public void UpdateCanAddForwardersToLargus(bool value) =>
			_parametersProvider.CreateOrUpdateParameter(_canAddForwarderToLargus, value.ToString());

		public void UpdateSubdivisionsForParameter(List<int> subdivisionsToAdd, List<int> subdivisionsToRemoves, string parameterName)
		{
			int[] subdivisions;

			switch(parameterName)
			{
				case _subdivisionsToInformComplaintHasNoDriverParameterName:
					subdivisions = SubdivisionsToInformComplaintHasNoDriver;
					break;
				case _subdivisionsForAlternativePricesName:
					subdivisions = SubdivisionsForAlternativePrices;
					break;
				default:
					throw new NotSupportedException("Параметр подразделений не поддерживается.");
			}

			var result = subdivisions
				.Concat(subdivisionsToAdd)
				.Except(subdivisionsToRemoves)
				.Distinct()
				.ToArray();

			_parametersProvider.CreateOrUpdateParameter(parameterName, string.Join(", ", result));
		}

		public string SubdivisionsToInformComplaintHasNoDriverParameterName => _subdivisionsToInformComplaintHasNoDriverParameterName;

		public string SubdivisionsAlternativePricesName => _subdivisionsForAlternativePricesName;
	}
}
