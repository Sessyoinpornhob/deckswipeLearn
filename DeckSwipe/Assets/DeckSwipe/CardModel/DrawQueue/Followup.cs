using System;
using DeckSwipe.Gamestate;

namespace DeckSwipe.CardModel.DrawQueue {

	[Serializable]
	public class Followup : IFollowup {

		public int id;
		public int delay;

		public int Delay {
			get { return delay; }
			set { delay = value; }
		}

		public Followup(int id, int delay) {
			this.id = id;
			this.delay = delay;
		}

		// 创建一个新的 Followup
		public IFollowup Clone() {
			return new Followup(id, delay);
		}

		// 获取后续卡片
		public ICard Fetch(CardStorage cardStorage) {
			return cardStorage.ForId(id);
		}

	}

}
