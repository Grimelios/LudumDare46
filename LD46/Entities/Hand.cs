using System.Collections.Generic;
using Engine;
using Engine.Utility;
using GlmSharp;
using LD46.Entities.Core;

namespace LD46.Entities
{
	public class Hand
	{
		private const float BaseAngle = -1.75f;
		private const float SplitAngle = 0.1f;
		private const float MaxSpread = 0.6f;

		private const int Radius = 700;

		private SimpleScene2D scene;
		private List<Card> cards;

		private vec2 basePosition;

		public Hand(SimpleScene2D scene)
		{
			this.scene = scene;

			var corner = Resolution.WindowDimensions / 2;

			cards = new List<Card>();
			basePosition = new vec2(corner.x - 350, corner.y) - Utilities.Direction(BaseAngle) * (Radius * 0.65f);
		}

		public void Add(Card card)
		{
			cards.Add(card);
			scene.Add(card);

			Refresh();
		}

		public Card Play(int index)
		{
			var card = cards[index];
			cards.RemoveAt(index);

			Refresh();

			return card;
		}

		private void Refresh()
		{
			var split = SplitAngle * (cards.Count - 1) / 2;
			var a = BaseAngle;

			float increment;

			if (split < MaxSpread / 2)
			{
				a -= split;
				increment = SplitAngle;
			}
			else
			{
				a -= MaxSpread / 2;
				increment = MaxSpread / (cards.Count - 1);
			}

			for (int i = 0; i < cards.Count; i++)
			{
				var r = a + increment * i;
				var p = basePosition + Utilities.Direction(r) * Radius;
				var card = cards[i];

				card.Snap(p);
				card.Snap(r + Constants.PiOverTwo);
			}
		}
	}
}
