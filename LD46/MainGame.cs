using System;
using System.Collections.Generic;
using Engine;
using LD46.Loops;

namespace LD46
{
	public class MainGame : Game2D<LDLoop, LoopTypes>
	{
		public MainGame() : base("Ludum Dare 46", new Dictionary<LoopTypes, Type>
		{
			{ LoopTypes.Gameplay, typeof(GameplayLoop) }
		}, LoopTypes.Gameplay)
		{
		}
	}
}
