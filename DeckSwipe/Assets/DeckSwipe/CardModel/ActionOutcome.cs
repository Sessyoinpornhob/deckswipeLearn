using DeckSwipe.CardModel.DrawQueue;

namespace DeckSwipe.CardModel {

	// 表示一次操作的结果
	public class ActionOutcome : IActionOutcome {

		private readonly StatsModification statsModification;
		private readonly IFollowup followup;

		// 创建一个默认的操作结果
		public ActionOutcome() {
			statsModification = new StatsModification(0, 0, 0, 0);
		}

		// 创建一个指定修改值的操作结果
		public ActionOutcome(int coalMod, int foodMod, int healthMod, int hopeMod) {
			statsModification = new StatsModification(coalMod, foodMod, healthMod, hopeMod);
		}

		// 没人用，无所谓
		public ActionOutcome(int coalMod, int foodMod, int healthMod, int hopeMod, IFollowup followup) {
			statsModification = new StatsModification(coalMod, foodMod, healthMod, hopeMod);
			this.followup = followup;
		}

		public ActionOutcome(StatsModification statsModification, IFollowup followup) {
			this.statsModification = statsModification;
			this.followup = followup;
		}

		// 执行操作结果，它会根据修改值修改统计信息，并根据后续卡片添加后续卡片
		public void Perform(Game controller) {
			statsModification.Perform();
			if (followup != null) {
				controller.AddFollowupCard(followup);
			}
			controller.CardActionPerformed();
		}

	}

}
