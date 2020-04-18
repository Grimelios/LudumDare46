using Engine.Graphics._2D;
using Engine.Utility;
using LD46.Entities;
using LD46.Entities.Core;

namespace LD46.Loops
{
	public class GameplayLoop : LDLoop
	{
		private SimpleScene2D scene;

		public override void Initialize()
		{
			var data = JsonUtilities.Deserialize<CardData[]>("Cards.json");

			scene = new SimpleScene2D();
			scene.Camera = Camera;
			scene.Canvas = Canvas;
			scene.Add(new Card(data[0]));
		}

		public override void Update()
		{
			scene.Update();

			base.Update();
		}

		public override void Draw(SpriteBatch sb, float t)
		{
			scene.Draw(sb, t);
		}
	}
}
