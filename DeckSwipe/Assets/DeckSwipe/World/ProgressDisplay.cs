using System.Collections.Generic;
using Outfrost;
using TMPro;
using UnityEngine;

namespace DeckSwipe.World {
	
	public class ProgressDisplay : MonoBehaviour {
		
		private static readonly List<ProgressDisplay> _changeListeners = new List<ProgressDisplay>();
		
		public TextMeshProUGUI daysSurvivedText;
		
		private void Awake() {
			// 如果该对象不是预制体，则将该对象添加到更改监听器列表中，并重置天数计数器。
			/*if (!Util.IsPrefab(gameObject)) {
				//Debug.Log(gameObject.name);
				_changeListeners.Add(this);
				SetDisplay(0);
			}*/
			
			// m_脚本测试
			_changeListeners.Add(this);
			SetDisplay(0);
		}
		
		public static void SetDaysSurvived(int days) {
			SetAllDisplays(days);
		}
		
		private static void SetAllDisplays(int days) {
			// 遍历更改监听器列表中的所有显示器，并调用SetDisplay函数设置它们的天数计数器。
			for (int i = 0; i < _changeListeners.Count; i++) {
				if (_changeListeners[i] == null) {
					_changeListeners.RemoveAt(i);
				}
				else {
					_changeListeners[i].SetDisplay(days);
				}
			}
		}
		
		private void SetDisplay(int days) {
			// 设置单个显示器的天数计数器。
			daysSurvivedText.text = days.ToString();
		}
		
	}
	
}
