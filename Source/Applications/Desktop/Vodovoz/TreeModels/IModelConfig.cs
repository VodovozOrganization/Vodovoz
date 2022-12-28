using System;
using System.Collections;
using System.Reflection;

namespace Vodovoz.TreeModels
{
	public interface IModelConfig
	{
		Type Type { get; }
		PropertyInfo ParentProperty { get; }
		IList GetChilds(object node);
	}
}
