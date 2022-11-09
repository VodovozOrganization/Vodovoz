using System;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements.Base
{

	[Serializable]
	public abstract class BaseElement
	{
		protected Dictionary<string, int?> InitializedPropertyIndexes = new Dictionary<string, int?>();

		protected virtual bool IsInitialized(string propertyName)
		{
			return InitializedPropertyIndexes.ContainsKey(propertyName);
		}

		protected virtual void Initialize(string propertyName, int? index)
		{
			if(IsInitialized(propertyName))
			{
				return;
			}
			InitializedPropertyIndexes.Add(propertyName, index);
		}

		protected virtual void UpdateIndex(string propertyName, int? index)
		{
			if(!IsInitialized(propertyName))
			{
				Initialize(propertyName, index);
				return;
			}
			InitializedPropertyIndexes[propertyName] = index;
		}
	}
}
