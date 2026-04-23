using UnityEngine;
/// <summary>
/// Interactable toilet object
/// Single-use trigger
/// Activates victory state and ending sequence
public class Toilet : MonoBehaviour, IInteractable
{
	private bool isUsed = false;

	// when the player looks at the object
	public void OnHoverEnter()
	{
		if (!isUsed)
		{
			InteractionUI.Instance.Show("[E] PEE!!");
		}
	}

	// when the player looks away from the object
	public void OnHoverExit()
	{
		InteractionUI.Instance.Hide();
	}

	// when the player presses the interact button
	public void Interact()
	{
		if (isUsed)
			return;

		PeeMeterManager.Instance.Pee(PeeMeterManager.Instance.maxPee);
		GameManager.Instance.foundToilet = true;
		GameManager.Instance.SetState(GameManager.GameState.Victory);

		isUsed = true;
		InteractionUI.Instance.Hide();
	}
}
