using System;

namespace DeckSwipe.Gamestate {
	
	[Serializable]
	// 这个class感觉没做啥，就是单纯getset一些成员变量。
	public class CardProgress : ICardProgress {
		
		public int id;
		public CardStatus status;
		
		public CardStatus Status {
			get { return status; }
			set { status = value; }
		}
		
		public CardProgress(int id, CardStatus status) {
			this.id = id;
			this.status = status;
		}
		
	}
	
}
