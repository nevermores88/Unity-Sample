using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour {

	public float hitPoint = 100.0f;
	public float stamina = 100.0f;
	private float maxHitPoint;
	private float maxStamina;

	public Text hpText;
	public Text stText;
	public RectTransform hpBar;
	public RectTransform stBar;
	private CharacterController characterController;

	// Use this for initialization
	void Start () {
		characterController = GetComponent<CharacterController>();

		maxHitPoint = hitPoint;
		maxStamina = stamina;

		hpText.text = hitPoint + " / " + maxHitPoint;
		stText.text = ((int)(stamina / maxStamina * 100.0f)).ToString() + "%";
		hpBar.localScale = Vector3.one;
		stBar.localScale = Vector3.one;
	}
	
	// Update is called once per frame
	void Update () {
		if(characterController.velocity.sqrMagnitude > 99 && Input.GetKey(KeyCode.LeftShift) && stamina >0) {
			stamina--;
			UpdateST();
		}
		else if(stamina < maxStamina) {
			stamina += 0.5f;
			UpdateST();
		}

		if (Input.GetKeyDown(KeyCode.K)) {
			ApplyDamage(Random.Range(1, 20));
		}
	}

	public void ApplyDamage(float damage) {
		UpdateHP();
		hitPoint -= damage;

		if(hitPoint <= 0) {
			hitPoint = 0;
		}
	}

	private void UpdateHP() {
		hpText.text = hitPoint + " / " + maxHitPoint;
		hpBar.localScale = new Vector3(hitPoint / maxHitPoint, 1, 1);
	}

	private void UpdateST() {
		stText.text = ((int)(stamina / maxStamina * 100.0f)).ToString() + "%";
		stBar.localScale = new Vector3(stamina / maxStamina, 1, 1);
	}
}
