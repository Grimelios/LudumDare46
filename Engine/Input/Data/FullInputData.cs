using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Utility;

namespace Engine.Input.Data
{
	public class FullInputData
	{
		private InputData[] dataArray;

		public FullInputData()
		{
			dataArray = new InputData[Utilities.EnumCount<InputTypes>()];
		}

		public InputStates this[InputBind bind] => dataArray[(int)bind.InputType][bind.Data];

		internal void Set(InputTypes inputType, InputData data)
		{
			dataArray[(int)inputType] = data;
		}

		// This accounts for situations in which a particular device isn't connected (for example, if the keyboard is
		// unplugged).
		public bool TryGetData<T>(out T data) where T : InputData
		{
			data = (T)dataArray.FirstOrDefault(d => d is T);

			return data != null;
		}

		// This direct access function should be used sparingly (likely only for internal features like the freecam).
		// All other input queries should use binds.
		public bool Query(InputTypes inputType, int data, InputStates state)
		{
			var inputData = dataArray[(int)inputType];

			Debug.Assert(inputData != null, $"Direct input access for type {inputType} is not connected.");

			return inputData.Query(data, state);
		}

		public bool Query(InputBind bind, InputStates state)
		{
			return dataArray[(int)bind.InputType].Query(bind.Data, state);
		}

		public bool Query(List<InputBind> binds, InputStates state)
		{
			return binds.Any(b => Query(b, state));
		}

		public bool Query(List<InputBind> binds, InputStates state, out InputBind bindUsed)
		{
			foreach (var bind in binds)
			{
				if (dataArray[(int)bind.InputType].Query(bind.Data, state))
				{
					bindUsed = bind;

					return true;
				}
			}

			bindUsed = null;

			return false;
		}
	}
}
