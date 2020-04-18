using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD46.Entities
{
	public class CardData
	{
		public string Name { get; set; }

		public int Attack { get; set; }
		public int Defense { get; set; }
		public int Health { get; set; }

		public CardTypes Type { get; set; }
	}
}
