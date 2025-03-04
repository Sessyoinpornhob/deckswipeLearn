﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeckSwipe.CardModel;
using DeckSwipe.CardModel.Import;
using DeckSwipe.CardModel.Import.Resource;
using DeckSwipe.CardModel.Prerequisite;
using UnityEngine;

namespace DeckSwipe.Gamestate {

	public class CardStorage {

		private static readonly Character _defaultGameOverCharacter = new Character("", null);

		private readonly Sprite defaultSprite;
		private readonly bool loadRemoteCollectionFirst;

		// 存储所有的卡片对象
		public Dictionary<int, Card> Cards { get; private set; }
		// 存储所有的特殊卡片对象
		public Dictionary<string, SpecialCard> SpecialCards { get; private set; }

		// 异步加载卡片集合
		public Task CardCollectionImport { get; }

		private List<Card> drawableCards = new List<Card>();

		// 构造函数
		public CardStorage(Sprite defaultSprite, bool loadRemoteCollectionFirst) {
			this.defaultSprite = defaultSprite;
			this.loadRemoteCollectionFirst = loadRemoteCollectionFirst;
			CardCollectionImport = PopulateCollection();
		}

		public Card Random() {
			return drawableCards[UnityEngine.Random.Range(0, drawableCards.Count)];
		}

		// 通过ID找卡片
		public Card ForId(int id) {
			Card card;
			Cards.TryGetValue(id, out card);
			return card;
		}

		public SpecialCard SpecialCard(string id) {
			SpecialCard card;
			SpecialCards.TryGetValue(id, out card);
			return card;
		}

		// 解析卡片的前置条件
		public void ResolvePrerequisites() {
			foreach (Card card in Cards.Values) {
				card.ResolvePrerequisites(this);
				if (card.PrerequisitesSatisfied()) {
					AddDrawableCard(card);
				}
			}
		}

		// 将卡片添加到可绘制卡片列表中
		public void AddDrawableCard(Card card) {
			drawableCards.Add(card);
		}


		private async Task PopulateCollection() {
			// 由于 Import 函数是一个异步函数，因此在调用它时需要使用 await 关键字
			// 在 CollectionImporter.Import() 完成后返回ImportedCards值
			ImportedCards importedCards =
					await new CollectionImporter(defaultSprite, loadRemoteCollectionFirst).Import();
			Cards = importedCards.cards;
			SpecialCards = importedCards.specialCards;
			if (Cards == null || Cards.Count == 0) {
				PopulateFallback();
			}
			VerifySpecialCards();
		}

		// 在卡片集合加载失败时添加一些占位卡片
		private void PopulateFallback() {
			Cards = new Dictionary<int, Card>();
			Character placeholderPerson = new Character("Placeholder Person", defaultSprite);
			Cards.Add(0, new Card("Placeholder card 1",
					"A",
					"B",
					placeholderPerson,
					new ActionOutcome(-2, 4, -2, 2),
					new ActionOutcome(2, 0, 2, -2),
					new List<ICardPrerequisite>()));
			Cards.Add(1, new Card("Placeholder card 2",
					"A",
					"B",
					placeholderPerson,
					new ActionOutcome(-1, -1, -1, -1),
					new ActionOutcome(2, 2, 2, 2),
					new List<ICardPrerequisite>()));
			Cards.Add(2, new Card("Placeholder card 3",
					"A",
					"B",
					placeholderPerson,
					new ActionOutcome(1, 1, 0, -2),
					new ActionOutcome(2, 2, -2, -4),
					new List<ICardPrerequisite>()));
		}

		// 用于验证特殊卡片是否存在：验证死亡卡片是否存在。
		private void VerifySpecialCards() {
			if (SpecialCards == null) {
				SpecialCards = new Dictionary<string, SpecialCard>();
			}

			if (!SpecialCards.ContainsKey("gameover_coal")) {
				SpecialCards.Add("gameover_coal", new SpecialCard("The city runs out of coal to run the generator, and freezes over.", "", "",
						_defaultGameOverCharacter,
						new GameOverOutcome(),
						new GameOverOutcome()));
			}
			if (!SpecialCards.ContainsKey("gameover_food")) {
				SpecialCards.Add("gameover_food", new SpecialCard("Hunger consumes the city, as food reserves deplete.", "", "",
						_defaultGameOverCharacter,
						new GameOverOutcome(),
						new GameOverOutcome()));
			}
			if (!SpecialCards.ContainsKey("gameover_health")) {
				SpecialCards.Add("gameover_health", new SpecialCard("The city's population succumbs to wounds and spreading diseases.", "", "",
						_defaultGameOverCharacter,
						new GameOverOutcome(),
						new GameOverOutcome()));
			}
			if (!SpecialCards.ContainsKey("gameover_hope")) {
				SpecialCards.Add("gameover_hope", new SpecialCard("All hope among the people is lost.", "", "",
						_defaultGameOverCharacter,
						new GameOverOutcome(),
						new GameOverOutcome()));
			}
		}

	}

}
