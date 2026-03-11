Some refactor advice (from Claude and reviewed by Chloe):

**`GameManager`**

- [ ] `SetState` is doing too much work, consider use C# event:
    ```C#
    public event Action<GameState> OnStateChanged;

    public void SetState(GameState newState) {
        if (State == newState) return;
        State = newState;
        OnStateChanged?.Invoke(newState);
    }
    ```
    And let `PlayerController`, `UIManager` handle their own side effects separately.
- [ ] Player control flags (`Jump`, `Crouch`, `FirstPersonLook`) are hard coded. Should be solved by using C# event as stated above.
- [ ] Some functions of `ResetState` is duplicated with `SetState`
- [ ] `foundToilet` might be a bit strange to be placed in `GameManager`. May consider using a separate `GameProgressManager` to store such in game process states.


**`PauseManager`**

- [ ] Replace per-frame panel `SetActive` sync in `Update()` with an event-driven approach: subscribe to GameManager.OnStateChanged, update panels only when state actually changes
- [ ] Remove `IsGameOver` guard once `GameManager.OnStateChanged` is adopted. Ending state simply won't trigger a pause-relevant transition, guard becomes unnecessary


**`SettingsManager` & `SettingsMenuUI`**

- [ ] Move `PlayerPrefs` loading and applying settings from `SettingsMenuUI` into `SettingsManager.Awake()`
- [ ] The four slider init blocks in SettingsMenuUI.Start() are basically identical, pull them into a single InitSlider() helper method
- [ ] Move the Mathf.Clamp logic and the 0.001f magic number into SettingsManager — the UI shouldn't need to know about these implementation details


**`EndingManager`**

- [ ] Rename to EndingScreenUI — the name "Manager" is misleading. This class only controls UI, it doesn't manage any game logic
- [ ] The ending branch logic (foundToilet check) shouldn't live here — GameManager should decide which ending to trigger and pass it in, e.g. Show(EndingType type) instead of reading GameManager internals directly
- [ ] Replace the Update() fade loop with a Coroutine — ShowCredits() already uses one, keep the style consistent and get rid of the fadingIn flag and fadeTimer fields
- [ ] Field name YOU_PEED breaks C# naming conventions, rename to youPeedImage to match the other serialized fields (?)

**`EnemyManager`**

- [ ] Blinded() and Hurt() don't belong here — these are player feedback effects,
    not enemy logic; move them to a PlayerEffectsManager or the player script itself
- [ ] The polling loop in FindPlayerAndStartSpawning() is inefficient and fragile. The script spawned the player can directly tell `EnemyManager`:
    ``` C#
    // Player spawn script
    GameObject playerObj = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    EnemyManager.Instance.RegisterPlayer(playerObj);
    ```

    ``` C#
    // EnemyManager.cs
    public void RegisterPlayer(GameObject p)
    {
        player = p;
        playerCam = p.GetComponentInChildren<Camera>();
        StartCoroutine(SpawnEnemiesRoutine());
    }
    ```
    Or use C# event to broadcast player spawn:

    ``` C#
    public static class GameEvents
    {
        public static event Action<GameObject> OnPlayerSpawned;
        public static void PlayerSpawned(GameObject p) => OnPlayerSpawned?.Invoke(p);
    }

    // Player spawn script
    GameEvents.PlayerSpawned(playerObj);

    // EnemyManager.cs
    private void OnEnable() =>  GameEvents.OnPlayerSpawned += RegisterPlayer;
    private void OnDisable() => GameEvents.OnPlayerSpawned -= RegisterPlayer;
    ```
- [ ] Switch to Singleton<EnemyManager> to stay consistent with the rest of the codebase,
    the manual Instance = this doesn't have the duplicate-destruction safety the base class provides
- [ ] peeMeter is declared but never used anywhere in this file
- [ ] SpawnInterval and spawnRadius are public but should probably be [SerializeField] private
    — nothing outside this class should be changing spawn settings directly
