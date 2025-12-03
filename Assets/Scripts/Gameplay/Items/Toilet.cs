using UnityEngine;

public class Toilet : MonoBehaviour, IInteractable
{
	public KeyCode interactKey = KeyCode.E;
	public PeeMeterUpdate peeMeterUpdate;

	// when the player looks at the object
	public void OnHoverEnter()
	{
		InteractionUI.Instance.Show("[E] PEE!!");
	}

	// when the player looks away from the object
	public void OnHoverExit()
	{
		InteractionUI.Instance.Hide();
	}

	// when the player presses the interact button
	public void Interact()
	{
		peeMeterUpdate.Pee(peeMeterUpdate.maxPee);
		GameManager.Instance.foundToilet = true;
		GameManager.Instance.GameOver();
	}
}
