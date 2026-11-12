using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour
{
    [SerializeField] private GameObject cutsceneCam;
    public Camera playerCam;

    [SerializeField] private LevelPreviewCutscene levelPreviewCutscene;

    void Awake()
    {
        cutsceneCam.SetActive(false);
    }

    // TODO: add support for different cutscenes
    public IEnumerator PlayCutscene(bool playerVisible)
    {
        GameManager.Instance.SetState(GameManager.GameState.Cutscene);

        // Switch camera
        if (playerVisible) playerCam.enabled = false;
        else playerCam.gameObject.SetActive(false);
        cutsceneCam.SetActive(true);
        

        yield return StartCoroutine(levelPreviewCutscene.PlayCutscene1(cutsceneCam));
        gameObject.transform.position = cutsceneCam.transform.position; // orient self (cutscene root position)
        cutsceneCam.GetComponent<Camera>().fieldOfView = playerCam.fieldOfView; // construct better "cutscene" settings system in the future
        yield return StartCoroutine(levelPreviewCutscene.PlayCutscene2(cutsceneCam));

        // Show pee text
        UIManager.Instance.ShowWakeUpText();
        yield return new WaitForSecondsRealtime(3f);

        // Restore player camera
        if (playerVisible) playerCam.enabled = true;
        else playerCam.gameObject.SetActive(true);
        cutsceneCam.SetActive(false);

        GameManager.Instance.SetState(GameManager.GameState.Playing);
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance.State == GameManager.GameState.Cutscene)
        {
            // Restore player camera
            if (playerCam.enabled == false)
                playerCam.enabled = true;
            playerCam.gameObject.SetActive(true);
            cutsceneCam.SetActive(false);

            // Stop cutscenes
            StopAllCoroutines();

            // Restore gamestate
            GameManager.Instance.SetState(GameManager.GameState.Playing);
            //Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
