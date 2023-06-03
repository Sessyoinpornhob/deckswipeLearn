using DeckSwipe.CardModel;
using DeckSwipe.CardModel.DrawQueue;
using DeckSwipe.Gamestate;
using DeckSwipe.Gamestate.Persistence;
using DeckSwipe.World;
using Outfrost;
using UnityEngine;

namespace DeckSwipe {

	// 这段代码是一个名为Game的类，它是整个游戏的核心。它包含了许多函数，每个函数都有自己的功能。下面是每个函数的详细解释：
	public class Game : MonoBehaviour {

		private const int _saveInterval = 8;

		public InputDispatcher inputDispatcher;
		public CardBehaviour cardPrefab;
		public Vector3 spawnPosition;
		public Sprite defaultCharacterSprite;
		public bool loadRemoteCollectionFirst;

		public CardStorage CardStorage {
			get { return cardStorage; }
		}

		private CardStorage cardStorage;
		private ProgressStorage progressStorage;
		private float daysPassedPreviously;
		private float daysLastRun;
		private int saveIntervalCounter;
		private CardDrawQueue cardDrawQueue = new CardDrawQueue();

		private void Awake() {
			// Listen for Escape key ('Back' on Android) that suspends the game on Android
			// or ends it on any other platform
			// - Awake(): 这个函数在游戏启动时被调用。
			// 它监听了Escape键的按下事件，如果在Android平台上按下了Escape键，游戏会被挂起。
			// 如果在其他平台上按下了Escape键，游戏会结束。
			// 此外，它还初始化了cardStorage和progressStorage对象，并设置了GameStartOverlay的回调函数。
			#if UNITY_ANDROID
			inputDispatcher.AddKeyUpHandler(KeyCode.Escape,
					keyCode => {
						AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
							.GetStatic<AndroidJavaObject>("currentActivity");
						activity.Call<bool>("moveTaskToBack", true);
					});
			#else
			inputDispatcher.AddKeyDownHandler(KeyCode.Escape,
					keyCode => Application.Quit());
			#endif

			cardStorage = new CardStorage(defaultCharacterSprite, loadRemoteCollectionFirst);
			progressStorage = new ProgressStorage(cardStorage);

			GameStartOverlay.FadeOutCallback = StartGameplayLoop;
		}

		private void Start() {
			CallbackWhenDoneLoading(StartGame);
		}

		// - StartGame(): 这个函数在游戏开始时被调用。
		// 它获取了之前的游戏进度，并调用了GameStartOverlay的StartSequence()函数。
		private void StartGame() {
			daysPassedPreviously = progressStorage.Progress.daysPassed;
			GameStartOverlay.StartSequence(progressStorage.Progress.daysPassed, daysLastRun);
		}

		// 它保存了当前的游戏进度，并重新开始游戏。
		public void RestartGame() {
			progressStorage.Save();
			daysLastRun = progressStorage.Progress.daysPassed - daysPassedPreviously;
			cardDrawQueue.Clear();
			StartGame();
		}

		// 这个函数在游戏开始时被调用。
		// 它重置了所有的统计数据，并调用了DrawNextCard()函数。
		private void StartGameplayLoop() {
			Stats.ResetStats();
			ProgressDisplay.SetDaysSurvived(0);
			DrawNextCard();
		}

		// 这个函数在每次抽卡时被调用。
		// 它根据当前的统计数据抽取了一张卡，并将其实例化。
		// 如果统计数据中的任何一项为0，它会实例化一个特殊的卡片。此外，它还定期保存游戏进度。
		public void DrawNextCard() {
			if (Stats.Coal == 0) {
				SpawnCard(cardStorage.SpecialCard("gameover_coal"));
			}
			else if (Stats.Food == 0) {
				SpawnCard(cardStorage.SpecialCard("gameover_food"));
			}
			else if (Stats.Health == 0) {
				SpawnCard(cardStorage.SpecialCard("gameover_health"));
			}
			else if (Stats.Hope == 0) {
				SpawnCard(cardStorage.SpecialCard("gameover_hope"));
			}
			else {
				IFollowup followup = cardDrawQueue.Next();
				ICard card = followup?.Fetch(cardStorage) ?? cardStorage.Random();
				SpawnCard(card);
			}

			// 这行代码的作用是将saveIntervalCounter减1，然后对_saveInterval取模。
			// 这个变量是用来计算游戏进度保存的间隔的。当saveIntervalCounter等于0时，游戏进度会被保存
			// 考虑一下取模运算。
			saveIntervalCounter = (saveIntervalCounter - 1) % _saveInterval; // 在这用了？
			if (saveIntervalCounter == 0) {
				progressStorage.Save();
			}
		}

		// 这个函数在每次卡片被操作时被调用。
		// 它增加了游戏进度，并调用了DrawNextCard()函数。
		public void CardActionPerformed() {
			progressStorage.Progress.AddDays(Random.Range(0.5f, 1.5f),
					daysPassedPreviously);
			ProgressDisplay.SetDaysSurvived(
					(int)(progressStorage.Progress.daysPassed - daysPassedPreviously));
			DrawNextCard();
		}

		// 这个函数在每次卡片被操作后被调用。
		// 它将一个跟随卡片添加到卡片队列中。
		public void AddFollowupCard(IFollowup followup) {
			cardDrawQueue.Insert(followup);
		}

		// 这个函数在游戏启动时被调用。
		// 它等待游戏进度加载完成后调用回调函数。
		private async void CallbackWhenDoneLoading(Callback callback) {
			await progressStorage.ProgressStorageInit;
			callback();
		}

		// 这个函数在每次抽卡时被调用。
		// 它实例化了一个卡片，并将其设置为当前卡片 
		private void SpawnCard(ICard card) {
			CardBehaviour cardInstance = Instantiate(cardPrefab, spawnPosition,
					Quaternion.Euler(0.0f, -180.0f, 0.0f));
			cardInstance.Card = card;
			cardInstance.snapPosition.y = spawnPosition.y;
			cardInstance.Controller = this;
		}

	}

}
