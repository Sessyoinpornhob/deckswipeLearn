using System.Collections;
using System.Collections.Generic;
using Outfrost;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeckSwipe.World {
	
	// 它用于管理游戏开始时的遮罩层。
	public class GameStartOverlay : MonoBehaviour {
		
		private enum OverlayState {
			
			Hidden,
			FadingHiddenToBlack,
			Black,
			FadingBlackToVisible,
			Visible,
			FadingVisibleToHidden
			
		}
		
		private const float _fadeDuration = 0.5f;
		
		// 这个作者好喜欢用这种方式...
		private static readonly List<GameStartOverlay> _controlListeners = new List<GameStartOverlay>();
		
		public static Callback FadeOutCallback { private get; set; }
		
		public float overlayTimeout = 2.0f;
		public float dayCounterRewindDuration = 1.0f;
		public Image backgroundImage;
		public Image blackSlate;
		public TextMeshProUGUI currentTimeText;
		public TextMeshProUGUI daysSurvivedLabel;
		public TextMeshProUGUI daysSurvivedText;
		
		private static float rewindStartDays;
		
		private float fadeStartTime;
		private OverlayState overlayState = OverlayState.Hidden;
		
		private float rewindStartTime;
		private bool rewindingDaysCounter;
		
		// 当对象被创建时调用，如果该对象不是预制体，则将该对象添加到控制监听器列表中。
		private void Awake() {
			if (!Util.IsPrefab(gameObject)) {
				_controlListeners.Add(this);
			}
		}

		// 在对象被激活时调用，将遮罩层设置为黑色。
		private void Start() {
			SetOverlayVisible(false);
			SetBlackSlateVisible(true);
			overlayState = OverlayState.Black;
		}
		
		// 每帧调用，用于动画遮罩层的出现和消失，并管理遮罩层的状态。
		private void Update() {
			// Animate overlay by interpolating alpha values, manage fade states
			float fadeProgress;
			switch (overlayState) {

				case OverlayState.FadingHiddenToBlack:

					// 从fadeStartTime开始到现在的时间除以淡入/淡出持续时间
					fadeProgress = (Time.time - fadeStartTime) / _fadeDuration;
					if (fadeProgress > 1.0f) {
						SetBlackSlateAlpha(1.0f);
						overlayState = OverlayState.Black;
						FadeToVisible();
					}
					else {
						SetBlackSlateAlpha(Mathf.Clamp01(fadeProgress));
					}
					// 函数和动画状态机写的很好，抄了。
					break;

				case OverlayState.FadingBlackToVisible:
					fadeProgress = (Time.time - fadeStartTime) / _fadeDuration;
					if (fadeProgress > 1.0f) {
						SetBlackSlateVisible(false);
						overlayState = OverlayState.Visible;
						DelayForSeconds(FadeOut, overlayTimeout);
						rewindStartTime = Time.time;
						rewindingDaysCounter = true;
					}
					else {
						SetBlackSlateAlpha(Mathf.Clamp01(1.0f - fadeProgress));
					}
					break;

				case OverlayState.Visible:
					// 开始界面的Visible状态确实没必要等很久。
					if (rewindingDaysCounter) {
						float rewindProgress = (Time.time - rewindStartTime) / dayCounterRewindDuration;
						if (rewindProgress > 1.0f) {
							rewindingDaysCounter = false;
							ProgressDisplay.SetDaysSurvived(0);
						}
						else {
							ProgressDisplay.SetDaysSurvived((int) Mathf.Lerp(rewindStartDays, 0.0f, rewindProgress));
						}
					}
					break;

				case OverlayState.FadingVisibleToHidden:
					fadeProgress = (Time.time - fadeStartTime) / _fadeDuration;
					if (fadeProgress > 1.0f) {
						SetOverlayVisible(false);
						overlayState = OverlayState.Hidden;
					}
					else {
						SetOverlayAlpha(Mathf.Clamp01(1.0f - fadeProgress));
					}
					break;
			}
		}

		// 用于开始遮罩层的出现和消失动画，并将天数计数器重置为0。
		public static void StartSequence(float daysPassed, float daysLastRun) {
			rewindStartDays = daysLastRun;
			for (int i = 0; i < _controlListeners.Count; i++) {
				if (_controlListeners[i] == null) {
					_controlListeners.RemoveAt(i);
				}
				else {
					_controlListeners[i].SetCurrentTimeText(daysPassed);
					_controlListeners[i].FadeIn();
				}
			}
		}

		// 用于开始遮罩层的出现动画。
		private void FadeIn() {
			switch (overlayState) {
				case OverlayState.Hidden:
					FadeToBlack();
					break;
				case OverlayState.Black:
					FadeToVisible();
					break;
				case OverlayState.FadingVisibleToHidden:
					FadeToBlack();
					break;
			}
		}

		// 用于开始遮罩层从隐藏到黑色的动画。
		private void FadeToBlack() {
			fadeStartTime = Time.time;
			SetBlackSlateEnabled(true);
			overlayState = OverlayState.FadingHiddenToBlack;
		}
		
		// 从黑到可见。
		private void FadeToVisible() {
			fadeStartTime = Time.time;
			SetOverlayVisible(true);
			overlayState = OverlayState.FadingBlackToVisible;
		}
		
		// 用于开始遮罩层从可见到隐藏的动画，并在动画结束时调用FadeOutCallback。
		private void FadeOut() {
			fadeStartTime = Time.time;
			overlayState = OverlayState.FadingVisibleToHidden;
			FadeOutCallback?.Invoke();
		}

		// 用于设置当前时间的文本。
		private void SetCurrentTimeText(float daysPassed) {
			currentTimeText.text = ApproximateDate(daysPassed);
		}
		
		// 用于将天数转换为大致日期。
		private static string ApproximateDate(float daysPassed) {
			int year = 1887 + (int)(daysPassed / 365.25f);
			int month = (int)((daysPassed % 365.25f) / 30.4375f);
			return MonthName(month) + " " + year;
		}
		
		// 用于将月份索引转换为月份名称。
		private static string MonthName(int monthIndex) {
			switch (monthIndex) {
				case 0:
					return "January";
				case 1:
					return "February";
				case 2:
					return "March";
				case 3:
					return "April";
				case 4:
					return "May";
				case 5:
					return "June";
				case 6:
					return "July";
				case 7:
					return "August";
				case 8:
					return "September";
				case 9:
					return "October";
				case 10:
					return "November";
				case 11:
					return "December";
				default:
					return "";
			}
		}
		
		// 用于设置图像的alpha值。
		private static void SetColorAlpha(Graphic image, float alpha) {
			Color color = image.color;
			color.a = alpha;
			image.color = color;
		}
		
		// 用于启用或禁用所有显示器。
		private void SetOverlayEnabled(bool enabled) {
			backgroundImage.enabled = enabled;
			currentTimeText.enabled = enabled;
			daysSurvivedLabel.enabled = enabled;
			daysSurvivedText.enabled = enabled;
		}
		
		// 用于设置所有显示器的alpha值。
		private void SetOverlayAlpha(float alpha) {
			SetColorAlpha(backgroundImage, alpha);
			SetColorAlpha(currentTimeText, alpha);
			SetColorAlpha(daysSurvivedLabel, alpha);
			SetColorAlpha(daysSurvivedText, alpha);
		}
		
		// 用于设置所有显示器的可见性。
		private void SetOverlayVisible(bool visible) {
			SetOverlayEnabled(visible);
			SetOverlayAlpha(visible ? 1.0f : 0.0f);
		}
		
		// 用于启用或禁用黑色石板。
		private void SetBlackSlateEnabled(bool enabled) {
			blackSlate.enabled = enabled;
		}
		
		// 用于设置黑色石板的alpha值。
		private void SetBlackSlateAlpha(float alpha) {
			SetColorAlpha(blackSlate, alpha);
		}
		
		// 用于设置黑色石板的可见性。
		private void SetBlackSlateVisible(bool visible) {
			SetBlackSlateEnabled(visible);
			SetBlackSlateAlpha(visible ? 1.0f : 0.0f);
		}
		
		// 用于在指定时间后调用回调函数。
		private void DelayForSeconds(Callback callback, float seconds) {
			StartCoroutine(Util.DelayCoroutine(callback, seconds));
		}
		
	}
	
}
