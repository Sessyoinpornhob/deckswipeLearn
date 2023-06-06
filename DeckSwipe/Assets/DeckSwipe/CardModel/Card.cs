using System.Collections.Generic;
using DeckSwipe.CardModel.Prerequisite;
using DeckSwipe.Gamestate;
using UnityEngine;

namespace DeckSwipe.CardModel {

	public class Card : ICard {

		public string CardText { get; }
		public string LeftSwipeText { get; }
		public string RightSwipeText { get; }

		public string CharacterName {
			get { return character != null ? character.name : ""; }
		}

		public Sprite CardSprite {
			get { return character?.sprite; }
		}

		public ICardProgress Progress {
			get { return progress; }
		}

		public Character character;
		public CardProgress progress;

		private readonly List<ICardPrerequisite> prerequisites;
		private readonly ActionOutcome leftSwipeOutcome;
		private readonly ActionOutcome rightSwipeOutcome;

		private Dictionary<ICard, ICardPrerequisite> unsatisfiedPrerequisites;
		private List<Card> dependentCards = new List<Card>();

		// 构造函数
		public Card(
				string cardText,
				string leftSwipeText,
				string rightSwipeText,
				Character character,
				ActionOutcome leftOutcome,
				ActionOutcome rightOutcome,
				List<ICardPrerequisite> prerequisites) {
			this.CardText = cardText;
			this.LeftSwipeText = leftSwipeText;
			this.RightSwipeText = rightSwipeText;
			this.character = character;
			leftSwipeOutcome = leftOutcome;
			rightSwipeOutcome = rightOutcome;
			this.prerequisites = prerequisites;
		}
		
		// 下面是一些方法的重载。

		// CardShown 方法用于标记卡片已经显示
		public void CardShown(Game controller) {
			progress.Status |= CardStatus.CardShown;
			foreach (Card card in dependentCards) {
				card.CheckPrerequisite(this, controller.CardStorage);
			}
		}

		// PerformLeftDecision 方法用于执行左滑操作
		public void PerformLeftDecision(Game controller) {
			progress.Status |= CardStatus.LeftActionTaken;
			foreach (Card card in dependentCards) {
				card.CheckPrerequisite(this, controller.CardStorage);
			}
			leftSwipeOutcome.Perform(controller);
		}

		// PerformRightDecision 方法用于执行右滑操作。
		public void PerformRightDecision(Game controller) {
			progress.Status |= CardStatus.RightActionTaken;
			foreach (Card card in dependentCards) {
				card.CheckPrerequisite(this, controller.CardStorage);
			}
			rightSwipeOutcome.Perform(controller);
		}

		// CheckPrerequisite 方法用于检查卡片的前置条件是否满足。
		public void CheckPrerequisite(ICard dependency, CardStorage cardStorage) {
			if (PrerequisitesSatisfied()
					|| !unsatisfiedPrerequisites.ContainsKey(dependency)) {
				dependency.RemoveDependentCard(this);
				return;
			}

			ICardPrerequisite prerequisite = unsatisfiedPrerequisites[dependency];
			if ((dependency.Progress.Status & prerequisite?.Status) == prerequisite?.Status) {
				unsatisfiedPrerequisites.Remove(dependency);
				dependency.RemoveDependentCard(this);
			}

			if (PrerequisitesSatisfied()) {
				// Duplicate-proof because we've verified that this card's
				// prerequisites were not satisfied before
				cardStorage.AddDrawableCard(this);
			}
		}

		// 解决卡片的前置条件
		public void ResolvePrerequisites(CardStorage cardStorage) {
			unsatisfiedPrerequisites = new Dictionary<ICard, ICardPrerequisite>();
			foreach (ICardPrerequisite prerequisite in prerequisites) {
				ICard card = prerequisite.GetCard(cardStorage);
				if (card != null
						&& (card.Progress.Status & prerequisite.Status) != prerequisite.Status
						&& !unsatisfiedPrerequisites.ContainsKey(card)) {
					unsatisfiedPrerequisites.Add(card, prerequisite);
					card.AddDependentCard(this);
				}
			}
		}

		// 添加和删除依赖于该卡片的卡牌。
		public void AddDependentCard(Card card) {
			dependentCards.Add(card);
		}

		public void RemoveDependentCard(Card card) {
			dependentCards.Remove(card);
		}

		public bool PrerequisitesSatisfied() {
			return unsatisfiedPrerequisites.Count == 0;
		}

	}

}
