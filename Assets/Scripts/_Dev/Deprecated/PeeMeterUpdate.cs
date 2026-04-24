// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.Audio;

// // TODO: remove manager references, replace with Instance
// //       remove UI image and add to UIManager

// public class PeeMeterUpdate : MonoBehaviour
// {
//     public Slider peeMeter;
//     public float deltaPee;

//     private Image fillImage;
//     public AudioClip deathSound;

//     public int maxPee { get; private set; } = 100;

//     // Colors for the pee meter
//     Color32 startPee = new Color32(255, 255, 165, 255);
//     Color32 endPee = new Color32(255, 208, 20, 255);

//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         peeMeter.value = 0;
//         fillImage = peeMeter.fillRect.GetComponent<Image>();
//     }

//     // TODO: fix GameManager calls with current implementation
//     // TODO: make AudioManager call clearer and separate responsibilities in update
//     void Update()
//     {
//         peeMeter.value += deltaPee * Time.deltaTime;
//         fillImage.color = Color.Lerp(startPee, endPee, peeMeter.value / 100);
//         float dB = Mathf.Lerp(-20f, 0f, peeMeter.value / maxPee); // starting volume of -20dB, increases to 0dB as pee meter fills
//         AudioManager.Instance.ChangeMusicVolume(dB);

//         if (!GameManager.Instance.IsGameOver && Mathf.Approximately(peeMeter.value, 100f))
//         {
//             // trigger global game over
//             AudioManager.Instance.FadeOutMusic(0.2f);
//             AudioManager.Instance.PlaySFX(deathSound);
//             GameManager.Instance.SetState(GameManager.GameState.GameOver);
//         }
//     }

//     //A sudden PeeMeter increase due to scare
//     public void Scare(float drink)
//     {
//         peeMeter.value += drink;
//     }

//     public void Pee(float amount)
//     {
//         peeMeter.value -= amount;
//         if (peeMeter.value < 0)
//         {
//             peeMeter.value = 0;
//         }
//     }
// }
