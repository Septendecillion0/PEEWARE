// using UnityEngine;
// using System.Collections;
// /// <summary>
// /// DEPRECATED - Replaced by VictoryUI and GameOverUI with more modular animation control
// /// Controller for the ending sequence of the game, including both GameOver and Victory sequences
// /// Manages the timing and sequencing of events for both endings
// /// </summary>
// [System.Obsolete("EndingSequenceController is deprecated. Use VictoryUI and GameOverUI for more modular control over ending sequences.")]
// public class EndingSequenceController : MonoBehaviour
// {
//     [SerializeField] private float fadeDuration = 6f;

//     public void PlayGameOver()
//     {
//         StopAllCoroutines();
//         StartCoroutine(GameOverSequence());
//     }

//     public void PlayVictory()
//     {
//         StopAllCoroutines();
//         StartCoroutine(VictorySequence());
//     }

//     /// <summary>
//     /// Coroutine to execute the sequence of events when the game ends in GameOver
//     /// 
//     /// Fades in the YOU_PEED text (fadeDuration)
//     /// shows Quit and Restart buttons
//     /// </summary>
//     /// <returns></returns>
//     private IEnumerator GameOverSequence()
//     {
//         UIManager.Instance.ShowEndingScreen();
//         UIManager.Instance.SetYouPeedAlpha(0f);

//         float t = 0f;
//         while (t < fadeDuration)
//         {
//             t += Time.unscaledDeltaTime;
//             UIManager.Instance.SetYouPeedAlpha(t / fadeDuration);
//             yield return null;
//         }

//         UIManager.Instance.ShowGameOverButtons();
//     }

//     /// <summary>
//     /// Coroutine to execute the sequence of events when the toilet is found
//     /// 
//     /// Fades in the YOU_PEED text (fadeDuration)
//     /// shows 'Congrats' text for 2 seconds (hardcoded)
//     /// shows Credits screen
//     /// waits for 2 seconds (hardcoded) then allows player input to quit
//     /// </summary>
//     /// <returns></returns>
//     private IEnumerator VictorySequence()
//     {
//         UIManager.Instance.ShowEndingScreen();
//         UIManager.Instance.SetYouPeedAlpha(0f);

//         float t = 0f;
//         while (t < fadeDuration)
//         {
//             t += Time.unscaledDeltaTime;
//             UIManager.Instance.SetYouPeedAlpha(t / fadeDuration);
//             yield return null;
//         }

//         UIManager.Instance.ShowCongratsText();

//         yield return new WaitForSecondsRealtime(2f);

//         UIManager.Instance.ShowCredits();

//         yield return new WaitForSecondsRealtime(2f);

//         yield return new WaitUntil(() => Input.anyKeyDown);

//         GameManager.Instance.QuitToMainMenu();
//     }
// }