using GlmSharp;

namespace LD46.Entities
{
	public class Board
	{
		private Lane playerLane;
		private Lane opponentLane;

		public Board()
		{
			var p = new vec2(0, 200);

			playerLane = new Lane(p);
			opponentLane = new Lane(-p);
		}

		public Lane PlayerLane => playerLane;
		public Lane OpponentLane => opponentLane;
	}
}
