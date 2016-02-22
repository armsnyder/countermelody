using UnityEngine;
using System.Collections;
using Frictionless;

public class Target : MonoBehaviour {

	public Sprite fretUpSprite;
	public Sprite fretDownSprite;
	public int player;
	public InputButton color {
		set { 
			_color = value;
			StartCoroutine (setRenderColor (_color));
		}
		get { return _color; }
	}
	
	[SerializeField]
	private InputButton _color;

	private MessageRouter messageRouter;
	private SpriteRenderer spriteRenderer;

	void Start () {
		messageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		messageRouter.AddHandler<ButtonDownMessage> (OnButtonDown);
		messageRouter.AddHandler<ButtonUpMessage> (OnButtonUp);
		messageRouter.AddHandler<EnterBattleMessage> (OnEnterBattle);
		messageRouter.AddHandler<ExitBattleMessage> (OnExitBattle);
		spriteRenderer = GetComponent<SpriteRenderer> ();
		StartCoroutine (setRenderColor (_color));
		spriteRenderer.enabled = false;
	}

	void OnButtonDown(ButtonDownMessage m) {
		if (m.PlayerNumber == player && m.Button == _color) {
			spriteRenderer.sprite = fretDownSprite;
		}
	}

	void OnButtonUp(ButtonUpMessage m) {
		if (m.PlayerNumber == player && m.Button == _color) {
			spriteRenderer.sprite = fretUpSprite;
		}
	}

	IEnumerator setRenderColor(InputButton button) {
		while (spriteRenderer == null)
			yield return null;
		switch (button) {
		case InputButton.GREEN:
			spriteRenderer.color = Color.green;
			break;
		case InputButton.RED:
			spriteRenderer.color = Color.red;
			break;
		case InputButton.YELLOW:
			spriteRenderer.color = Color.yellow;
			break;
		case InputButton.BLUE:
			spriteRenderer.color = Color.blue;
			break;
		case InputButton.ORANGE:
			spriteRenderer.color = new Color (1, 0.5f, 0);
			break;
		default:
			break;
		}
	}

	void OnEnterBattle(EnterBattleMessage m) {
		spriteRenderer.enabled = true;
	}

	void OnExitBattle(ExitBattleMessage m) {
		spriteRenderer.enabled = false;
	}
}
