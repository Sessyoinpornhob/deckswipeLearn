using System;
using System.Collections.Generic;
using DeckSwipe.CardModel;

namespace DeckSwipe.Gamestate {

	// 存储游戏进度信息
	[Serializable]
	public class GameProgress {

		// 记录游戏进行的天数和最长连续游戏天数
		public float daysPassed;
		public float longestRunDays;

		// 存储所有卡片和特殊卡片的进度信息
		public List<CardProgress> cardProgress = new List<CardProgress>();
		public List<SpecialCardProgress> specialCardProgress = new List<SpecialCardProgress>();

		// 用于增加游戏进行的天数
		public void AddDays(float days, float daysPassedPreviously) {
			daysPassed += days;
			float daysPassedThisRun = daysPassed - daysPassedPreviously;
			if (daysPassedThisRun > longestRunDays) {
				longestRunDays = daysPassedThisRun;
			}
		}

		// 用于将卡片对象和卡片进度信息关联起来
		public void AttachReferences(CardStorage cardStorage) {

			foreach (CardProgress entry in cardProgress) {
				Card card = cardStorage.ForId(entry.id);
				if (card != null) {
					card.progress = entry;
				}
			}
			
			foreach (SpecialCardProgress entry in specialCardProgress) {
				SpecialCard specialCard = cardStorage.SpecialCard(entry.id);
				if (specialCard != null) {
					specialCard.progress = entry;
				}
			}

			// Fill in the missing card progress entries
			foreach (KeyValuePair<int, Card> entry in cardStorage.Cards) {
				if (entry.Value.progress == null) {
					CardProgress progress = new CardProgress(
							entry.Key, CardStatus.None);
					cardProgress.Add(progress);
					entry.Value.progress = progress;
				}
			}
			foreach (KeyValuePair<string, SpecialCard> entry in cardStorage.SpecialCards) {
				if (entry.Value.progress == null) {
					SpecialCardProgress progress = new SpecialCardProgress(
							entry.Key, CardStatus.None);
					specialCardProgress.Add(progress);
					entry.Value.progress = progress;
				}
			}
		}

	}

}
