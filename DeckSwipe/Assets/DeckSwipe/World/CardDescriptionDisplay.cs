using System.Collections.Generic;
using Outfrost;
using TMPro;
using UnityEngine;

namespace DeckSwipe.World {
	
	// 显示卡片的描述和角色名称
	public class CardDescriptionDisplay : MonoBehaviour {
		
		private static readonly List<CardDescriptionDisplay> _changeListeners = new List<CardDescriptionDisplay>();
		
		public TextMeshProUGUI cardText;
		public TextMeshProUGUI characterNameText;
		
		// 当对象被创建时调用，如果该对象不是预制体，则将该对象添加到更改监听器列表中，并重置描述。
		private void Awake() {
			if (!Util.IsPrefab(gameObject)) {
				_changeListeners.Add(this);
				ResetDescription();
			}
		}
		
		// 设置所有显示器的描述和角色名称。
		public static void SetDescription(string cardCaption, string characterName) {
			SetAllDisplays(cardCaption, characterName);
		}
		
		// 将所有显示器的描述和角色名称重置为空字符串。
		public static void ResetDescription() {
			SetDescription("", "");
		}

		// 设置所有显示器的描述和角色名称。
		private static void SetAllDisplays(string cardCaption, string characterName) {
			for (int i = 0; i < _changeListeners.Count; i++) {
				if (_changeListeners[i] == null) {
					_changeListeners.RemoveAt(i);
				}
				else {
					_changeListeners[i].SetDisplay(cardCaption, characterName);
				}
			}
		}
		
		// 设置单个显示器的描述和角色名称。
		private void SetDisplay(string cardCaption, string characterName) {
			cardText.text = cardCaption;
			characterNameText.text = characterName;
		}
		
	}
	
}
