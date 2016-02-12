using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Frictionless;

public class CMCellGrid : CellGrid {

	private MessageRouter MessageRouter;
	private Boolean isFirstFrame = true;

	void Update() {
		if (isFirstFrame) {
			MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
			MessageRouter.AddHandler<SwitchPlayerMessage>(OnSwitchPlayer);
			isFirstFrame = false;
		}
	}

	void OnSwitchPlayer(SwitchPlayerMessage m) {
		Units.FindAll(u => u.PlayerNumber.Equals(m.PlayerNumber)).ForEach(u => { u.OnTurnStart(); });
	}
}
