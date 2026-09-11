using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using VodovozBusiness.Attributes;

namespace Vodovoz.ViewModels
{
	public class FuelDocumentViewModelConstructorSelector : IConstructorSelector
	{
		private readonly IConstructorSelector _autofacDefault = new MostParametersConstructorSelector();

		/// <summary>
		/// По умолчанию в автофаке используется подбор подходящего конструктора с бОльшим количеством параметров
		/// Если таковых несколько, то вызывается соответствующий IConstructorSelector, либо выбрасывается исключение
		/// У FuelDocumentViewModel несколько конструкторов совпадающих по количеству параметров,
		/// поэтому для определения нужного используем атрибут <see cref="ResolveHelperAttribute"/>
		/// </summary>
		/// <param name="constructorBindings">Информация о подходящих конструкторах</param>
		/// <param name="parameters">Набор параметров для резолва</param>
		/// <returns></returns>
		public BoundConstructor SelectConstructorBinding(BoundConstructor[] constructorBindings, IEnumerable<Parameter> parameters)
		{
			var foundConstructor = constructorBindings
				.FirstOrDefault(x => x.TargetConstructor
					.GetCustomAttributes(typeof(ResolveHelperAttribute), true)
					.Any(y => y != null && ContainsAllTypes((y as ResolveHelperAttribute).Types, parameters)
					)
				);

			return foundConstructor ?? _autofacDefault.SelectConstructorBinding(constructorBindings, parameters);
		}

		private bool ContainsAllTypes(IEnumerable<Type> types, IEnumerable<Parameter> parameters)
		{
			var parameterTypes = parameters.ToLookup(x => x is TypedParameter p ? p.Type : x.GetType());
			return types.All(parameterTypes.Contains);
		}
	}
}
