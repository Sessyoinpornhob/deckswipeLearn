using System;

namespace DeckSwipe.Gamestate {

	// 定义了卡片的三个状态
	[Serializable]
	[Flags]
	public enum CardStatus {

		// 位运算符 
		None = 0,
		CardShown = 1 << 0,
		RightActionTaken = 1 << 1,
		LeftActionTaken = 1 << 2,

	}

}
