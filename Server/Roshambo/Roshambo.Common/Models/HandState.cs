using Chip.Net;
using Chip.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambo.Common.Models
{
	public class HandState : ISerializable
	{
		public int ScissorsCount { get; set; } = 0;
		public int PaperCount { get; set; } = 0;
		public int RockCount { get; set; } = 0;

		public bool HasCards => Total != 0;
		public int Total => ScissorsCount + PaperCount + RockCount;
		public List<CardType> AsCardTypes => (new List<CardType>[] {
			Enumerable.Repeat(CardType.Scissors, ScissorsCount).ToList(),
			Enumerable.Repeat(CardType.Paper, PaperCount).ToList(),
			Enumerable.Repeat(CardType.Rock, RockCount).ToList(),
		}).SelectMany(i => i).ToList();

		public int Take(CardType type, int count) {
			switch (type) {
				case CardType.Paper:
					this.PaperCount -= count;
					if (this.PaperCount < 0) {
						count += this.PaperCount;
						this.PaperCount = 0;
					}
					break;

				case CardType.Rock:
					this.RockCount -= count;
					if (this.RockCount < 0) {
						count += this.RockCount;
						this.RockCount = 0;
					}
					break;

				case CardType.Scissors:
					this.ScissorsCount -= count;
					if (this.ScissorsCount < 0) {
						count += this.ScissorsCount;
						this.ScissorsCount = 0;
					}
					break;
			}

			return count;
		}

		public void Put(CardType type, int count) {
			switch(type) {
				case CardType.Paper: this.PaperCount += count; break;
				case CardType.Rock: this.RockCount += count; break;
				case CardType.Scissors: this.ScissorsCount += count; break;
			}
		}

		public CardType PickRandom(Random random) {
			if (HasCards == false)
				throw new Exception("Cannot pick from empty hand");

			var cards = AsCardTypes;
			var card = cards.Skip(random.Next(cards.Count - 1)).First();
			switch (card) {
				case CardType.Paper: PaperCount--; break;
				case CardType.Rock: RockCount--; break;
				case CardType.Scissors: ScissorsCount--; break;
			}

			return card;
		}

		public void WriteTo(DataBuffer buffer) {
			buffer.Write((int)this.RockCount);
			buffer.Write((int)this.PaperCount);
			buffer.Write((int)this.ScissorsCount);
		}

		public void ReadFrom(DataBuffer buffer) {
			this.RockCount = buffer.ReadInt32();
			this.PaperCount = buffer.ReadInt32();
			this.ScissorsCount = buffer.ReadInt32();
		}
	}
}