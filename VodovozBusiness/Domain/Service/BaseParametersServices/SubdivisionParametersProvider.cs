using System;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.Domain.Service.BaseParametersServices
{
	public class SubdivisionParametersProvider : ISubdivisionService
	{
		public static SubdivisionParametersProvider Instance { get; private set; }

		static SubdivisionParametersProvider()
		{
			Instance = new SubdivisionParametersProvider();
		}

		private SubdivisionParametersProvider()
		{
		}

		public int GetOkkId()
		{
			if(!ParametersProvider.Instance.ContainsParameter("номер_отдела_ОКК")) {
				throw new InvalidProgramException("В параметрах базы не настроен параметр : номер_отдела_ОКК");
			}
			return int.Parse(ParametersProvider.Instance.GetParameterValue("номер_отдела_ОКК"));
		}

		public int GetSubdivisionIdForRLAccept()
		{
			if(!ParametersProvider.Instance.ContainsParameter("accept_route_list_subdivision_restrict")) {
				throw new InvalidOperationException(String.Format("В базе не настроен параметр: accept_route_list_subdivision_restrict"));
			}
			return int.Parse(ParametersProvider.Instance.GetParameterValue("accept_route_list_subdivision_restrict"));
		}

		public int GetParentVodovozSubdivisionId()
		{
			if(!ParametersProvider.Instance.ContainsParameter("Id_Главного_подразделения_Веселый_Водовоз")) {
				throw new InvalidProgramException("В параметрах базы не настроен параметр : Id_Главного_подразделения_Веселый_Водовоз");
			}
			string value = ParametersProvider.Instance.GetParameterValue("Id_Главного_подразделения_Веселый_Водовоз");

			if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result)) {
				throw new InvalidProgramException("В параметрах базы неверно заполнено значение " +
					"Id подразделения Веселый Водовоз (Id_Главного_подразделения_Веселый_Водовоз)");
			}

			return result;
		}
	}
}
