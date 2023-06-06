using System.Collections.Generic;
using DeckSwipe.CardModel;
using DeckSwipe.World;
using UnityEngine;

namespace DeckSwipe.Gamestate {
	
	// 存储游戏统计信息
	public static class Stats {
		
		private const int _maxStatValue = 32;
		private const int _startingCoal = 16;
		private const int _startingFood = 16;
		private const int _startingHealth = 16;
		private const int _startingHope = 16;
		
		// 写的是readonly 但实际上可以随便改。
		private static readonly List<StatsDisplay> _changeListeners = new List<StatsDisplay>();
		
		public static int Coal { get; private set; }
		public static int Food { get; private set; }
		public static int Health { get; private set; }
		public static int Hope { get; private set; }
		
		public static float CoalPercentage => (float) Coal / _maxStatValue;
		public static float FoodPercentage => (float) Food / _maxStatValue;
		public static float HealthPercentage => (float) Health / _maxStatValue;
		public static float HopePercentage => (float) Hope / _maxStatValue;
		
		// 用于应用修改
		// 根据传入的 StatsModification 对象修改煤炭、食物、健康和希望的值，并触发所有的监听器
		public static void ApplyModification(StatsModification mod) {
			Coal = ClampValue(Coal + mod.coal);
			Food = ClampValue(Food + mod.food);
			Health = ClampValue(Health + mod.health);
			Hope = ClampValue(Hope + mod.hope);
			TriggerAllListeners();
		}
		
		// 重置所有统计信息
		public static void ResetStats() {
			ApplyStartingValues();
			TriggerAllListeners();
		}
		
		private static void ApplyStartingValues() {
			Coal = ClampValue(_startingCoal);
			Food = ClampValue(_startingFood);
			Health = ClampValue(_startingHealth);
			Hope = ClampValue(_startingHope);
		}
		
		// 触发所有的监听器
		private static void TriggerAllListeners() {
			for (int i = 0; i < _changeListeners.Count; i++) {
				if (_changeListeners[i] == null) {
					_changeListeners.RemoveAt(i);
				}
				else {
					_changeListeners[i].TriggerUpdate();
				}
			}
		}
		
		public static void AddChangeListener(StatsDisplay listener) {
			_changeListeners.Add(listener);
		}
		
		private static int ClampValue(int value) {
			return Mathf.Clamp(value, 0, _maxStatValue);
		}
		
	}
	
}
