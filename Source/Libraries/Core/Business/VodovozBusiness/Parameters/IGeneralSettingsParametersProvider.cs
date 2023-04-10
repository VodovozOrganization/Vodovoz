﻿using System.Collections.Generic;

namespace Vodovoz.Parameters
{
	public interface IGeneralSettingsParametersProvider
	{
		string GetRouteListPrintedFormPhones { get; }
		void UpdateRouteListPrintedFormPhones(string text);

		bool GetCanAddForwardersToLargus { get; }
		string OrderAutoComment { get; }
		void UpdateCanAddForwardersToLargus(bool value);
		void UpdateOrderAutoComment(string value);

		int[] SubdivisionsToInformComplaintHasNoDriver { get; }
		int[] SubdivisionsForAlternativePrices { get; }

		string SubdivisionsToInformComplaintHasNoDriverParameterName { get; }
		string SubdivisionsAlternativePricesName { get; }
		void UpdateSubdivisionsForParameter(List<int> subdivisionsToAdd, List<int> subdivisionsToRemoves, string parameterName);
	}
}
