using System.Collections.Generic;
using GlmSharp;

namespace LD46.Entities
{
	public class Lane
	{
		private List<Card> cards;

		private vec2 position;

		public Lane(vec2 position)
		{
			this.position = position;

			cards = new List<Card>();
		}

		public void Add(Card card)
		{
			cards.Add(card);
			Refresh();
		}

		private void Refresh()
		{
			const int Spacing = 8;

			var p = position - new vec2(((Card.Width + Spacing) * (cards.Count - 1)) / 2, 0);

			foreach (var card in cards)
			{
				card.SetPosition(p, false);
				p.x += Spacing + Card.Width;
			}
		}
	}
}
