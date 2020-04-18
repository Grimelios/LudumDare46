using Engine;
using Engine.Components.Interpolation._2D;
using Engine.Content;
using Engine.Core._2D;
using Engine.Graphics;
using GlmSharp;
using LD46.Entities.Core;

namespace LD46.Entities
{
	public class Card : SimpleEntity2D
	{
		private const float EaseTime = 0.3f;

		private static Texture texture = ContentCache.GetTexture("Card.png");

		public static int Width => texture.Width;

		private int attack;
		private int defense;
		private int health;
		private int maxHealth;

		public Card(CardData data)
		{
			const int Offset = 16;

			Name = data.Name;
			Type = data.Type;
			attack = data.Attack;
			defense = data.Defense;
			health = data.Health;
			maxHealth = health;

			var font = ContentCache.GetFont("Debug");
			var w = texture.Width / 2 - 2;
			var h = texture.Height / 2 - 2;

			Attach(new Sprite(texture));
			Attach(new SpriteText(font, data.Name, Alignments.Center, true), new vec2(0, h - 28));
			Attach(new SpriteText(font, attack.ToString(), Alignments.Center, true), new vec2(-w + Offset,
				-h + Offset));
			Attach(new SpriteText(font, defense.ToString(), Alignments.Center, true), new vec2(-w + Offset * 2,
				-h + Offset * 2));
			Attach(new SpriteText(font, health.ToString(), Alignments.Center, true), new vec2(w - Offset,
				-h + Offset));
			Attach(new SpriteText(font, maxHealth.ToString(), Alignments.Center, true), new vec2(w - Offset * 2,
				-h + Offset * 2));
		}

		public string Name { get; }

		public CardTypes Type { get; }

		public void Snap(vec2 p)
		{
			var interpolator = Components.Get<PositionInterpolator2D>();

			if (interpolator != null)
			{
				interpolator.Reset();
				interpolator.Start = Position;
				interpolator.End = p;
			}
			else
			{
				Components.Add(new PositionInterpolator2D(this, Position, p, EaseTypes.CubicOut, EaseTime, false));
			}
		}

		public void Snap(float r)
		{
			var interpolator = Components.Get<RotationInterpolator>();

			if (interpolator != null)
			{
				interpolator.Reset();
				interpolator.Start = Rotation;
				interpolator.End = r;
			}
			else
			{
				Components.Add(new RotationInterpolator(this, Rotation, r, EaseTypes.CubicOut, EaseTime, false));
			}
		}
	}
}
