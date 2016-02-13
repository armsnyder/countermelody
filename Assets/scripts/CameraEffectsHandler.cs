using UnityEngine;
using System.Collections;
using System;
using UnityStandardAssets.ImageEffects;
using Frictionless;

/// <summary>
/// Attach to Main Camera. Handles various visual effects like blurring background upon entering battle.
/// Note that the utilized image effects must also be added as components of Main Camera in the editor.
/// </summary>
public class CameraEffectsHandler : MonoBehaviour {

	struct BlurParams {
		public int blurIterations;
		public float blurSize;
	}

	struct ColorParams {
		public Keyframe[] redChannel;
		public Keyframe[] greenChannel;
		public Keyframe[] blueChannel;
	}

	private BlurOptimized blurComponent;
	private ColorCorrectionCurves colorComponent;
	private MessageRouter messageRouter;
	private BlurParams defaultBlurParams;
	private ColorParams defaultColorParams;

	public float transitionTime = 0.2f;

	void Start () {
		// Get components
		blurComponent = GetComponent<BlurOptimized> ();
		colorComponent = GetComponent<ColorCorrectionCurves> ();

		// Message handlers
		messageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		messageRouter.AddHandler<EnterBattleMessage> (OnEnterBattle);
		messageRouter.AddHandler<ExitBattleMessage> (OnExitBattle);

		// Initialize effects parameters
		if (blurComponent) {
			blurComponent.enabled = false;
			defaultBlurParams.blurSize = blurComponent.blurSize;
			defaultBlurParams.blurIterations = blurComponent.blurIterations;
		}
		if (colorComponent) {
			colorComponent.enabled = false;
			// TODO: Color change doesn't work. Make it work.
			defaultColorParams.redChannel = new Keyframe[colorComponent.redChannel.keys.Length];
			defaultColorParams.greenChannel = new Keyframe[colorComponent.greenChannel.keys.Length];
			defaultColorParams.blueChannel = new Keyframe[colorComponent.blueChannel.keys.Length];
			Array.Copy (colorComponent.redChannel.keys, defaultColorParams.redChannel, 
				colorComponent.redChannel.keys.Length);
			Array.Copy (colorComponent.greenChannel.keys, defaultColorParams.greenChannel, 
				colorComponent.greenChannel.keys.Length);
			Array.Copy (colorComponent.blueChannel.keys, defaultColorParams.blueChannel, 
				colorComponent.blueChannel.keys.Length);
		}
	}

	void OnEnterBattle(EnterBattleMessage m) {
		StopAllCoroutines ();
		if (blurComponent)
			StartCoroutine (FadeInBlur());
		if (colorComponent)
			StartCoroutine (FadeInColor ());
	}

	void OnExitBattle(ExitBattleMessage m) {
		StopAllCoroutines ();
		if (blurComponent)
			StartCoroutine (FadeOutBlur ());
		if (colorComponent)
			StartCoroutine (FadeOutColor ());
	}

	IEnumerator FadeInBlur() {
		blurComponent.enabled = true;
		blurComponent.blurSize = 0;
		blurComponent.blurIterations = defaultBlurParams.blurIterations;
		while (blurComponent.blurSize < defaultBlurParams.blurSize) {
			blurComponent.blurSize += Time.deltaTime / transitionTime * defaultBlurParams.blurSize;
			yield return null;
		}
		blurComponent.blurSize = defaultBlurParams.blurSize;
	}

	IEnumerator FadeOutBlur() {
		blurComponent.enabled = true;
		blurComponent.blurSize = defaultBlurParams.blurSize;
		blurComponent.blurIterations = defaultBlurParams.blurIterations;
		while (blurComponent.blurSize > 0) {
			blurComponent.blurSize -= Time.deltaTime / transitionTime * defaultBlurParams.blurSize;
			yield return null;
		}
		blurComponent.blurSize = 0;
		blurComponent.blurIterations = 0;
		blurComponent.enabled = false;
	}

	IEnumerator FadeInColor() {
		colorComponent.enabled = true;
		SetColorKeyVal (colorComponent.redChannel, 1);
		SetColorKeyVal (colorComponent.greenChannel, 1);
		SetColorKeyVal (colorComponent.blueChannel, 1);
		SetColorKeyTan (colorComponent.redChannel, 1);
		SetColorKeyTan (colorComponent.greenChannel, 1);
		SetColorKeyTan (colorComponent.blueChannel, 1);
		bool allDone = false;
		while (!allDone) {
			allDone = true;
			if (GetColorKeyVal (colorComponent.redChannel) > GetColorKeyVal (defaultColorParams.redChannel)) {
				SetColorKeyVal (colorComponent.redChannel, GetColorKeyVal (colorComponent.redChannel) -
				Time.deltaTime / transitionTime * GetColorKeyVal (defaultColorParams.redChannel));
				allDone = false;
			}
			if (GetColorKeyVal (colorComponent.greenChannel) > GetColorKeyVal (defaultColorParams.greenChannel)) {
				SetColorKeyVal (colorComponent.greenChannel, GetColorKeyVal (colorComponent.greenChannel) -
				Time.deltaTime / transitionTime * GetColorKeyVal (defaultColorParams.greenChannel));
				allDone = false;
			}
			if (GetColorKeyVal (colorComponent.blueChannel) > GetColorKeyVal (defaultColorParams.blueChannel)) {
				SetColorKeyVal (colorComponent.blueChannel, GetColorKeyVal (colorComponent.blueChannel) -
				Time.deltaTime / transitionTime * GetColorKeyVal (defaultColorParams.blueChannel));
				allDone = false;
			}
			if (GetColorKeyTan (colorComponent.redChannel) < GetColorKeyTan (defaultColorParams.redChannel)) {
				SetColorKeyTan (colorComponent.redChannel, GetColorKeyTan (colorComponent.redChannel) +
				Time.deltaTime / transitionTime * GetColorKeyTan (defaultColorParams.redChannel));
				allDone = false;
			}
			if (GetColorKeyTan (colorComponent.greenChannel) < GetColorKeyTan (defaultColorParams.greenChannel)) {
				SetColorKeyTan (colorComponent.greenChannel, GetColorKeyTan (colorComponent.greenChannel) +
				Time.deltaTime / transitionTime * GetColorKeyTan (defaultColorParams.greenChannel));
				allDone = false;
			}
			if (GetColorKeyTan (colorComponent.blueChannel) < GetColorKeyTan (defaultColorParams.blueChannel)) {
				SetColorKeyTan (colorComponent.blueChannel, GetColorKeyTan (colorComponent.blueChannel) +
				Time.deltaTime / transitionTime * GetColorKeyTan (defaultColorParams.blueChannel));
				allDone = false;
			}
			yield return null;
		}
		SetColorKeyVal (colorComponent.redChannel, GetColorKeyVal (defaultColorParams.redChannel));
		SetColorKeyVal (colorComponent.greenChannel, GetColorKeyVal (defaultColorParams.greenChannel));
		SetColorKeyVal (colorComponent.blueChannel, GetColorKeyVal (defaultColorParams.blueChannel));
		SetColorKeyTan (colorComponent.redChannel, GetColorKeyTan (defaultColorParams.redChannel));
		SetColorKeyTan (colorComponent.greenChannel, GetColorKeyTan (defaultColorParams.greenChannel));
		SetColorKeyTan (colorComponent.blueChannel, GetColorKeyTan (defaultColorParams.blueChannel));
	}

	IEnumerator FadeOutColor() {
		colorComponent.enabled = true;
		SetColorKeyVal (colorComponent.redChannel, GetColorKeyVal (defaultColorParams.redChannel));
		SetColorKeyVal (colorComponent.greenChannel, GetColorKeyVal (defaultColorParams.greenChannel));
		SetColorKeyVal (colorComponent.blueChannel, GetColorKeyVal (defaultColorParams.blueChannel));
		SetColorKeyTan (colorComponent.redChannel, GetColorKeyTan (defaultColorParams.redChannel));
		SetColorKeyTan (colorComponent.greenChannel, GetColorKeyTan (defaultColorParams.greenChannel));
		SetColorKeyTan (colorComponent.blueChannel, GetColorKeyTan (defaultColorParams.blueChannel));
		bool allDone = false;
		while (!allDone) {
			allDone = true;
			if (GetColorKeyVal (colorComponent.redChannel) < GetColorKeyVal (defaultColorParams.redChannel)) {
				SetColorKeyVal (colorComponent.redChannel, GetColorKeyVal (colorComponent.redChannel) +
					Time.deltaTime / transitionTime * GetColorKeyVal (defaultColorParams.redChannel));
				allDone = false;
			}
			if (GetColorKeyVal (colorComponent.greenChannel) < GetColorKeyVal (defaultColorParams.greenChannel)) {
				SetColorKeyVal (colorComponent.greenChannel, GetColorKeyVal (colorComponent.greenChannel) +
					Time.deltaTime / transitionTime * GetColorKeyVal (defaultColorParams.greenChannel));
				allDone = false;
			}
			if (GetColorKeyVal (colorComponent.blueChannel) < GetColorKeyVal (defaultColorParams.blueChannel)) {
				SetColorKeyVal (colorComponent.blueChannel, GetColorKeyVal (colorComponent.blueChannel) +
					Time.deltaTime / transitionTime * GetColorKeyVal (defaultColorParams.blueChannel));
				allDone = false;
			}
			if (GetColorKeyTan (colorComponent.redChannel) > GetColorKeyTan (defaultColorParams.redChannel)) {
				SetColorKeyTan (colorComponent.redChannel, GetColorKeyTan (colorComponent.redChannel) -
					Time.deltaTime / transitionTime * GetColorKeyTan (defaultColorParams.redChannel));
				allDone = false;
			}
			if (GetColorKeyTan (colorComponent.greenChannel) > GetColorKeyTan (defaultColorParams.greenChannel)) {
				SetColorKeyTan (colorComponent.greenChannel, GetColorKeyTan (colorComponent.greenChannel) -
					Time.deltaTime / transitionTime * GetColorKeyTan (defaultColorParams.greenChannel));
				allDone = false;
			}
			if (GetColorKeyTan (colorComponent.blueChannel) > GetColorKeyTan (defaultColorParams.blueChannel)) {
				SetColorKeyTan (colorComponent.blueChannel, GetColorKeyTan (colorComponent.blueChannel) -
					Time.deltaTime / transitionTime * GetColorKeyTan (defaultColorParams.blueChannel));
				allDone = false;
			}
			yield return null;
		}
		SetColorKeyVal (colorComponent.redChannel, 1);
		SetColorKeyVal (colorComponent.greenChannel, 1);
		SetColorKeyVal (colorComponent.blueChannel, 1);
		SetColorKeyTan (colorComponent.redChannel, 1);
		SetColorKeyTan (colorComponent.greenChannel, 1);
		SetColorKeyTan (colorComponent.blueChannel, 1);
		colorComponent.enabled = false;
	}

	float GetColorKeyVal(Keyframe[] curve) {
		return curve[curve.Length - 1].value;
	}

	float GetColorKeyTan(Keyframe[] curve) {
		return curve[0].inTangent;
	}

	float GetColorKeyVal(AnimationCurve curve) {
		return curve.keys[curve.keys.Length - 1].value;
	}

	float GetColorKeyTan(AnimationCurve curve) {
		return curve.keys[0].inTangent;
	}

	void SetColorKeyVal(Keyframe[] curve, float value) {
		curve[curve.Length - 1].value = value;
	}

	void SetColorKeyTan(Keyframe[] curve, float inTangent) {
		curve[0].inTangent = inTangent;
	}

	void SetColorKeyVal(AnimationCurve curve, float value) {
		curve.keys[curve.keys.Length - 1].value = value;
	}

	void SetColorKeyTan(AnimationCurve curve, float inTangent) {
		curve.keys[0].inTangent = inTangent;
	}
}
