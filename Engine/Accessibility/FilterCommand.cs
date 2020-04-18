using System;
using System.Collections.Generic;
using Engine.Core._2D;
using Engine.Editing;
using Engine.Shaders;
using static Engine.Graphics.GL;

namespace Engine.Accessibility
{
	// TODO: Apply colorblind filters to the canvas as well.
	// This command was originally targeted exclusively at colorblind filters. It was later renamed to open up the
	// possibility of adding additional filters in the future (including multiple filters at once).
	public class FilterCommand : Command
	{
		private Sprite sprite;
		private Shader shader;

		private FilterTypes currentType;

		public FilterCommand(Sprite sprite) : base("filter", "filters")
		{
			const float Gamma = 2.2f;

			this.sprite = sprite;
			
			currentType = FilterTypes.None;
			
			// See https://github.com/nvkelso/color-oracle-java/blob/master/src/ika/colororacle/Simulator.java.
			var gammaToLinear = new int[256];
			var linearToGamma = new int[256];

			for (int i = 0; i < gammaToLinear.Length; i++)
			{
				var f = 0.992052f * (float)Math.Pow(i / 255f, Gamma) + 0.003974f;

				gammaToLinear[i] = (int)(f * 32767);
				linearToGamma[i] = (int)(255 * Math.Pow(i / 255f, 1 / Gamma));
			}

			shader = new Shader();
			shader.Attach(ShaderTypes.Vertex, "Sprite.vert");
			shader.Attach(ShaderTypes.Fragment, "Accessibility/Colorblind.frag");
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.AddAttribute<byte>(4, GL_UNSIGNED_BYTE, true);
			shader.Initialize();
			shader.Use();
			shader.SetUniform("gammaToLinear", gammaToLinear);
			shader.SetUniform("linearToGamma", linearToGamma);
		}

		public override Argument[] Usage => new []
		{
			new Argument("type", ArgumentTypes.Required)
		};

		public override string[] GetOptions(string[] args)
		{
			// Conveniently, all colorblind types already sort alphabetically after "Clear".
			var list = new List<string>();
			list.Add("Clear");
			list.AddRange(Enum.GetNames(typeof(FilterTypes)));

			return list.ToArray();
		}

		public override bool Process(string[] args, out string result)
		{
			var raw = args[0];
			var isValid = Enum.TryParse(raw, true, out FilterTypes type);
			var shouldClear = (isValid && type == FilterTypes.None) ||
				raw.Equals("Clear", StringComparison.CurrentCultureIgnoreCase);

			if (shouldClear)
			{
				if (currentType == FilterTypes.None)
				{
					result = "All filters are already disabled.";
				}
				else
				{
					result = "All filters disabled.";
					sprite.Shader = null;
					currentType = FilterTypes.None;
				}

				return true;
			}

			if (!isValid)
			{
				result = $"'{raw}' is not a valid filter.";

				return false;
			}
			
			var s = type.ToString();

			if (type == currentType)
			{
				result = $"{s} filter is already applied.";
			}
			else
			{
				result = $"{s} filter applied.";

				shader.Use();
				shader.SetUniform("filterType", (int)type);

				sprite.Shader = shader;
				currentType = type;
			}

			return true;
		}
	}
}
