public interface IInteractable
{
	void OnHoverEnter(); // The player looks at the object
	void OnHoverExit();  // The player looks away from the object
	void Interact();     // Press the interact button
}
