using DeckSwipe.CardModel;
using Outfrost;
using TMPro;
using UnityEngine;

namespace DeckSwipe.World {

	public class CardBehaviour : MonoBehaviour {

		// 动画状态机定义。
		private enum AnimationState {

			Idle,
			Converging,
			FlyingAway,
			Revealing

		}

		private const float _animationDuration = 0.4f;

		public float swipeThreshold = 1.0f;
		public Vector3 snapPosition;
		public Vector3 snapRotationAngles;
		public Vector2 cardImageSpriteTargetSize;
		public TextMeshPro leftActionText;
		public TextMeshPro rightActionText;
		public SpriteRenderer cardBackSpriteRenderer;
		public SpriteRenderer cardFrontSpriteRenderer;
		public SpriteRenderer cardImageSpriteRenderer;

		private ICard card;
		private Vector3 dragStartPosition;
		private Vector3 dragStartPointerPosition;
		private Vector3 animationStartPosition;
		private Vector3 animationStartRotationAngles;
		private float animationStartTime;
		private AnimationState animationState = AnimationState.Idle;
		private bool animationSuspended;

		// 构造函数，设置card的各种东西。
		public ICard Card {
			set {
				card = value;
				leftActionText.text = card.LeftSwipeText;
				rightActionText.text = card.RightSwipeText;
				if (card.CardSprite != null) {
					Vector2 targetSizeRatio = cardImageSpriteTargetSize / card.CardSprite.bounds.size;
					float scaleFactor = Mathf.Min(targetSizeRatio.x, targetSizeRatio.y);

					Vector3 scale = cardImageSpriteRenderer.transform.localScale;
					scale.x = scaleFactor;
					scale.y = scaleFactor;
					cardImageSpriteRenderer.transform.localScale = scale;

					cardImageSpriteRenderer.sprite = card.CardSprite;
				}
			}
		}

		public Game Controller { private get; set; }

		// 初始化了一些变量，并设置了左右滑动的文本透明度为0。
		private void Awake() {
			ShowVisibleSide();

			Util.SetTextAlpha(leftActionText, 0.0f);
			Util.SetTextAlpha(rightActionText, 0.0f);
		}

		// 它设置了卡片的初始位置和旋转角度，并调用了CardShown()函数。
		private void Start() {
			// Rotate clockwise on reveal instead of anticlockwise
			snapRotationAngles.y += 360.0f;

			animationStartPosition = transform.position;
			animationStartRotationAngles = transform.eulerAngles;
			animationStartTime = Time.time;
			animationState = AnimationState.Revealing;

			card.CardShown(Controller);
		}

		// 它根据卡片的状态进行动画，并根据卡片的位置设置左右滑动的文本透明度。
		private void Update() {
			// Animate card by interpolating translation and rotation, destroy swiped cards
			if (animationState != AnimationState.Idle && !animationSuspended) {
				float animationProgress = (Time.time - animationStartTime) / _animationDuration;
				float scaledProgress = ScaleProgress(animationProgress);

				if (scaledProgress > 1.0f || animationProgress > 1.0f) {
					transform.position = snapPosition;
					transform.eulerAngles = snapRotationAngles;

					if (animationState == AnimationState.Revealing) {
						CardDescriptionDisplay.SetDescription(card.CardText, card.CharacterName);
						snapRotationAngles.y -= 360.0f;
					}

					if (animationState == AnimationState.FlyingAway) {
						Destroy(gameObject);
					}
					else {
						animationState = AnimationState.Idle;
					}
				}
				else {
					transform.position = Vector3.Lerp(animationStartPosition, snapPosition, scaledProgress);
					transform.eulerAngles = Vector3.Lerp(animationStartRotationAngles, snapRotationAngles, scaledProgress);

					ShowVisibleSide();
				}
				if (animationState != AnimationState.Revealing) {
					float alphaCoord = (transform.position.x - snapPosition.x) / (swipeThreshold / 2);
					Util.SetTextAlpha(leftActionText, Mathf.Clamp01(-alphaCoord));
					Util.SetTextAlpha(rightActionText, Mathf.Clamp01(alphaCoord));
				}
			}
		}

		// 这个函数在拖动卡片时被调用。它暂停了卡片的动画，并记录了拖动的起始位置
		public void BeginDrag() {
			animationSuspended = true;
			dragStartPosition = transform.position;
			dragStartPointerPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		}

		// 这个函数在拖动卡片时被调用。它根据鼠标的位置移动卡片，并根据卡片的位置设置左右滑动的文本透明度。
		public void Drag() {
			Vector3 displacement = Camera.main.ScreenToWorldPoint(Input.mousePosition) - dragStartPointerPosition;
			displacement.z = 0.0f;
			transform.position = dragStartPosition + displacement;

			float alphaCoord = (transform.position.x - snapPosition.x) / (swipeThreshold / 2);
			Util.SetTextAlpha(leftActionText, -alphaCoord);
			Util.SetTextAlpha(rightActionText, alphaCoord);
		}

		// 这个函数在拖动卡片结束时被调用。它记录了卡片的位置和旋转角度，并根据卡片的位置执行相应的操作。
		public void EndDrag() {
			animationStartPosition = transform.position;
			animationStartRotationAngles = transform.eulerAngles;
			animationStartTime = Time.time;
			if (animationState != AnimationState.FlyingAway) {
				if (transform.position.x < snapPosition.x - swipeThreshold) {
					card.PerformLeftDecision(Controller);
					Vector3 displacement = animationStartPosition - snapPosition;
					snapPosition += displacement.normalized
					                * Util.OrthoCameraWorldDiagonalSize(Camera.main)
					                * 2.0f;
					snapRotationAngles = transform.eulerAngles;
					animationState = AnimationState.FlyingAway;
					CardDescriptionDisplay.ResetDescription();
				}
				else if (transform.position.x > snapPosition.x + swipeThreshold) {
					card.PerformRightDecision(Controller);
					Vector3 displacement = animationStartPosition - snapPosition;
					snapPosition += displacement.normalized
					                * Util.OrthoCameraWorldDiagonalSize(Camera.main)
					                * 2.0f;
					snapRotationAngles = transform.eulerAngles;
					animationState = AnimationState.FlyingAway;
					CardDescriptionDisplay.ResetDescription();
				}
				else if (animationState == AnimationState.Idle) {
					animationState = AnimationState.Converging;
				}
			}
			animationSuspended = false;
		}

		// ShowVisibleSide()函数是用来根据卡片是否面向主摄像机来显示正确的卡片元素的。
		// 如果卡片面向主摄像机，则显示卡片的正面元素，否则显示卡片的背面元素。
		// 具体来说，它会根据卡片的状态来设置cardBackSpriteRenderer、cardFrontSpriteRenderer、cardImageSpriteRenderer、leftActionText和rightActionText的可见性。
		private void ShowVisibleSide() {
			// Display correct card elements based on whether it's facing the main camera
			bool isFacingCamera = Util.IsFacingCamera(gameObject);
			cardBackSpriteRenderer.enabled = !isFacingCamera;
			cardFrontSpriteRenderer.enabled = isFacingCamera;
			cardImageSpriteRenderer.enabled = isFacingCamera;
			leftActionText.enabled = isFacingCamera;
			rightActionText.enabled = isFacingCamera;
		}

		// ScaleProgress()函数是用来计算动画进度的。
		// 它接受一个动画进度参数，并返回一个经过缩放的动画进度。
		// 具体来说，它会根据动画的状态来计算缩放后的动画进度，并返回结果。
		private float ScaleProgress(float animationProgress) {
			switch (animationState) {
				case AnimationState.Converging:
					return 0.15f * Mathf.Pow(animationProgress, 3.0f)
					       - 1.5f * Mathf.Pow(animationProgress, 2.0f)
					       + 2.38f * animationProgress;
				case AnimationState.FlyingAway:
					return 1.5f * Mathf.Pow(animationProgress, 3.0f)
					       + 0.55f * animationProgress;
				default:
					return animationProgress;
			}
		}

	}

}
