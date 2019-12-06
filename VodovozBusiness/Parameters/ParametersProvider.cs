using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
namespace Vodovoz.Parameters
{
	public class ParametersProvider
	{
		public static ParametersProvider Instance { get; private set; }

		static ParametersProvider()
		{
			Instance = new ParametersProvider();
		}

		private ParametersProvider()
		{
		}

		Dictionary<string, string> parameters = new Dictionary<string, string>();

		public bool ContainsParameter(string parameterName)
		{
			RefreshParameter(parameterName);
			return parameters.ContainsKey(parameterName);
		}

		public string GetParameterValue(string parameterName)
		{
			if(string.IsNullOrWhiteSpace(parameterName)) {
				throw new ArgumentNullException(nameof(parameterName));
			}

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				string freshParameterValue = uow.Session.QueryOver<BaseParameter>()
					.Where(x => x.Name == parameterName)
					.Select(x => x.StrValue)
					.SingleOrDefault<string>();
				if(parameters.ContainsKey(parameterName)) {
					parameters[parameterName] = freshParameterValue;
					return freshParameterValue;
				}
				if(!parameters.ContainsKey(parameterName) && !string.IsNullOrWhiteSpace(freshParameterValue)) {
					parameters.Add(parameterName, freshParameterValue);
					return freshParameterValue;
				}
				throw new InvalidProgramException($"В параметрах базы не найден параметр ({parameterName})");
			}
		}

		private void RefreshParameter(string parameterName)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				BaseParameter parameter = uow.Session.QueryOver<BaseParameter>()
					.Where(x => x.Name == parameterName)
					.SingleOrDefault<BaseParameter>();
				if(parameter == null) {
					return;
				}

				if(parameters.ContainsKey(parameterName)) {
					parameters[parameterName] = parameter.StrValue;
					return;
				}
				if(!parameters.ContainsKey(parameterName)) {
					parameters.Add(parameter.Name, parameter.StrValue);
					return;
				}
			}
		}

		public void RefreshParameters()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var allParameters = uow.Session.QueryOver<BaseParameter>().List();
				parameters.Clear();
				foreach(var parameter in allParameters) {
					if(parameters.ContainsKey(parameter.Name)) {
						continue;
					}
					parameters.Add(parameter.Name, parameter.StrValue);
				}
			}
		}


	}
}
