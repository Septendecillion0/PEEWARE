using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
/// <summary>
/// Controls the camera zoom preview that executes at the start of each level
/// Receives map from MapGenerator
/// </summary>

// TODO: currently holds the cutscene audio - should be placed in dedicated script
public class LevelPreviewCutscene : MonoBehaviour
{
    [SerializeField] private PlayableAsset cutscene;
    [SerializeField] private PlayableDirector director;

    [HideInInspector] public MapData map;

    [SerializeField] private float cameraSpeed = 3f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float accelFactor = 1.1f;
    [SerializeField] private float cameraHeightOffset = 0.9f;
    [SerializeField] private float cameraVentOffset = 0.2f;
    [SerializeField] private float cameraDoorOffset = 0.5f;

    [SerializeField] private AudioSource flushSFX;
    [SerializeField] private AudioSource whooshSFX;

    // Phase 1: Level Preview
    public IEnumerator PlayCutscene1(GameObject camera)
    {   
        List<Room> roomPath = map.FindPath(map.endRoom, map.startRoom);
        List<Vector3> cameraPath = BuildCameraPath(roomPath);

        camera.transform.position = cameraPath[0];
        camera.transform.rotation = map.endRoom.waypoint.rotation;

        // wait for the level to load and screen to fade in
        // TODO: move UImanager fadein logic here
        flushSFX.Play();

        yield return new WaitForSecondsRealtime(4f);
        yield return FollowRoomPath(cameraPath, camera);

        // Orient camera starting position
        camera.transform.position = map.startRoom.waypoint.position;
        camera.transform.rotation = map.startRoom.waypoint.rotation;
    }

    // Phase 2: Getting out of bed
    public IEnumerator PlayCutscene2(GameObject camera)
    {
        UIManager.Instance.PlayFadeIn(1f);

        director.time = 0;
        director.playableAsset = cutscene;

        director.Play();

        yield return new WaitUntil(() => director.state != PlayState.Playing);
    }


    private IEnumerator FollowRoomPath(List<Vector3> path, GameObject camera)
    {
        float currentAccel = 1f;

        for (int i = 1; i < path.Count; i++)
        {
            Vector3 target = path[i];

            while (Vector3.Distance(camera.transform.position, target) > 0.01f)
            {
                // Apply exponential acceleration
                currentAccel *= Mathf.Pow(accelFactor, Time.deltaTime);

                // Calculate direction opposite to movement
                Vector3 direction = camera.transform.position - target;
                direction.y = 0f;

                if (direction.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);

                    camera.transform.rotation = Quaternion.Slerp(
                        camera.transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime * currentAccel
                    );
                }

                // Move toward next position
                camera.transform.position = Vector3.MoveTowards(
                    camera.transform.position,
                    target,
                    cameraSpeed * Time.deltaTime * currentAccel
                );

                yield return null;
            }

            camera.transform.position = target;

            // TODO: implement better solution to decide when the whoosh should be played (when passing through actual doorways)
            if (i % 3 == 2) {
                whooshSFX.pitch = 0.6f + ((0.6f * i) / path.Count); // scale from 60% pitch to 120%
                whooshSFX.PlayOneShot(whooshSFX.clip, 0.3f);
            }
        }
    }

    /// <summary>
    /// Builds a Vec3 List as a path for the camera to follow
    /// Uses the roomPath to build the list, going from exit to exit of connected rooms,
    /// optionally visiting waypoints in between exits
    /// </summary>
    public List<Vector3> BuildCameraPath(List<Room> roomPath)
    {
        List<Vector3> path = new List<Vector3>();

        if (roomPath == null || roomPath.Count == 0)
            return path;

        for (int i = 0; i < roomPath.Count - 1; i++)
        {
            Room currentRoom = roomPath[i];

            // Visit waypoint if one exists
            if (currentRoom.waypoint != null)
            {
                path.Add(GetCameraPosition(currentRoom.waypoint.transform));
            }

            // No exit to navigate to after the final room
            if (i == roomPath.Count - 1)
            {   
                continue;
            }

            Room nextRoom = roomPath[i + 1];

            Room.Exit exit = currentRoom.GetExitTo(nextRoom);

            if (exit == null)
            {
                Debug.LogError(
                    $"No connection found between {currentRoom.roomName} " +
                    $"and {nextRoom.roomName}.");
                return new List<Vector3>();
            }
            
            bool isVent = exit.doorType == Room.DoorType.Vent;

            // Position just inside the current room, in front of the doorway.
            path.Add(GetCameraPosition(exit.transform, isVent, true));

            // Position directly inside the doorway
            path.Add(GetCameraPosition(exit.transform, isVent));

            // Position just inside the next room, past the doorway.
            path.Add(GetCameraPosition(exit.connectedExit.transform, isVent, true));
        }

        return path;
    }
    
    private Vector3 GetCameraPosition(Transform exitTransform, bool isVent = false, bool isDoor = false)
    {
        float heightOffset = isVent ? cameraVentOffset : cameraHeightOffset;
        float doorOffset = isDoor ? cameraDoorOffset : 0f;

        return exitTransform.position
            + Vector3.up * heightOffset
            - exitTransform.forward * doorOffset;
    }
}
