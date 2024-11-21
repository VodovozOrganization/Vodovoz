using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;

namespace Vodovoz.ViewModels.ViewModels.Suppliers
{
	public class WarehousesBalanceSummaryReportParameters
	{
		private SelectableParameterSet _nomenclaturesSet;
		private SelectableParameterSet _instancesSet;
		private int[] _nomenclaturesIds;
		private int[] _instancesIds;
		private int[] _warehousesIds;
		private int[] _employeesIds;
		private int[] _carsIds;
		
		public SelectableParameterSet NomenclaturesSet
		{
			get => _nomenclaturesSet;
			private set
			{
				_nomenclaturesSet = value;
				Nomenclatures = _nomenclaturesSet?.GetIncludedParameters()?.ToList();
			}
		}

		public List<SelectableParameter> Nomenclatures { get; private set; }

		public SelectableParameterSet InstancesSet
		{
			get => _instancesSet;
			private set
			{
				_instancesSet = value;
				Instances = _instancesSet?.GetIncludedParameters()?.ToList();
			}
		}
		public List<SelectableParameter> Instances { get; private set; }

		public SelectableParameterSet NomenclatureTypesSet { get; private set; }
		public IList<SelectableParameter> NomenclatureTypes => NomenclatureTypesSet?.GetIncludedParameters()?.ToList();

		public SelectableParameterSet NomenclatureGroupsSet { get; private set; }
		public IList<SelectableParameter> NomenclatureGroups => NomenclatureGroupsSet?.GetIncludedParameters()?.ToList();

		public IList<SelectableParameter> WarehouseStorages { get; private set; }
		public IList<SelectableParameter> EmployeeStorages { get; private set; }
		public IList<SelectableParameter> CarStorages { get; private set; }
		
		public int[] NomenclaturesIds =>
			_nomenclaturesIds ?? (_nomenclaturesIds = Nomenclatures?.Select(x => (int)x.Value).ToArray() ?? Array.Empty<int>() );
		public int[] InstancesIds =>
			_instancesIds ?? (_instancesIds = Instances?.Select(x => (int)x.Value).ToArray() ?? Array.Empty<int>());
		public int[] WarehousesIds =>
			_warehousesIds ?? (_warehousesIds = WarehouseStorages?.Select(x => (int)x.Value).ToArray() ?? Array.Empty<int>());
		public int[] EmployeesIds =>
			_employeesIds ?? (_employeesIds = EmployeeStorages?.Select(x => (int)x.Value).ToArray() ?? Array.Empty<int>());
		public int[] CarsIds =>
			_carsIds ?? (_carsIds = CarStorages?.Select(x => (int)x.Value).ToArray() ?? Array.Empty<int>());
		public int[] NomenclatureGroupsIds =>
			NomenclatureGroups?.Select(x => (int)x.Value).ToArray() ?? Array.Empty<int>();
		public bool NomenclatureGroupsSelected => NomenclatureGroups?.Any() ?? false;
		public bool NomenclatureTypesSelected => NomenclatureTypes?.Any() ?? false;
		public bool NomenclaturesSelected => Nomenclatures?.Any() ?? false;
		public bool InstancesSelected => Instances?.Any() ?? false;
		public bool AllNomenclaturesSelected => Nomenclatures?.Count == NomenclaturesSet?.Parameters.Count;
		public bool AllInstancesSelected => Instances?.Count == InstancesSet?.Parameters.Count;

		public WarehousesBalanceSummaryReportParameters AddNomenclaturesSet(SelectableParameterSet set)
		{
			NomenclaturesSet = set;
			return this;
		}
		
		public WarehousesBalanceSummaryReportParameters AddInstancesSet(SelectableParameterSet set)
		{
			InstancesSet = set;
			return this;
		}
		
		public WarehousesBalanceSummaryReportParameters AddNomenclatureTypesSet(SelectableParameterSet set)
		{
			NomenclatureTypesSet = set;
			return this;
		}
		
		public WarehousesBalanceSummaryReportParameters AddNomenclatureGroupsSet(SelectableParameterSet set)
		{
			NomenclatureGroupsSet = set;
			return this;
		}
		
		public WarehousesBalanceSummaryReportParameters AddWarehouseStorages(
			SelectableParametersReportFilter filter,
			string parameterName)
		{
			WarehouseStorages = filter.ParameterSets
				.FirstOrDefault(ps => ps.ParameterName == parameterName)?.GetIncludedParameters()?.ToList();
			return this;
		}
		
		public WarehousesBalanceSummaryReportParameters AddEmployeeStorages(
			SelectableParametersReportFilter filter,
			string parameterName)
		{
			EmployeeStorages = filter.ParameterSets
				.FirstOrDefault(ps => ps.ParameterName == parameterName)?.GetIncludedParameters()?.ToList();
			return this;
		}
		
		public WarehousesBalanceSummaryReportParameters AddCarStorages(
			SelectableParametersReportFilter filter,
			string parameterName)
		{
			CarStorages = filter.ParameterSets
				.FirstOrDefault(ps => ps.ParameterName == parameterName)?.GetIncludedParameters()?.ToList();
			return this;
		}
	}
}
