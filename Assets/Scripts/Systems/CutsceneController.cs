using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CutsceneController : MonoBehaviour
{
    public void PlayCutscene()
    {
        StartCoroutine(PlayCutsceneRoutine());
    }

    private IEnumerator PlayCutsceneRoutine()
    {
        GameManager.Instance.SetState(GameManager.GameState.Cutscene);

        // Switch camera

        yield return null;
        //yield return StartCoroutine();

        // Restore player camera

        GameManager.Instance.SetState(GameManager.GameState.Playing);
    }
}
