
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public enum ShotOutcome { Goal, Miss, NoTap }

// public class BallShooter : MonoBehaviour
// {
//     [Header("Goal Settings")]
//     public float goalMinX = -3f;
//     public float goalMaxX = 3f;
//     public float goalMinY = 0.5f;
//     public float goalMaxY = 3f;
//     public float goalMinZ = -80f;
//     public float goalMaxZ = -60f;

//     [Header("Miss Target")]
//     public Vector3 missTarget = new Vector3(0f, 50f, -50f);

//     [Header("Timing")]
//     public float reactionWindow = 8.0f;
//     public float lateThreshold = 5.0f;
//     public float shootDuration = 1.5f;
//     public float minDelay = 1.0f;
//     public float maxDelay = 3.0f;
//     public float sessionDuration = 180f;

//     [Header("Spawn")]
//     public float spawnX = 0f;
//     public float spawnY = 1.9f;
//     public float spawnZ = 0.5f;

//     [Header("Start Screen")]
//     public GameObject startScreen;

//     private Text scoreText;
//     private Text statusText;
//     private int score = 0;
//     private Vector3 hiddenPosition = new Vector3(0f, 0f, 1000f);

//     private bool waitingForTap = false;
//     private bool shootingInProgress = false;
//     private float ballAppearTime;
//     private float sessionStartTime;
//     private bool sessionActive = false;
//     private Coroutine roundCoroutine;
//     private Vector3 spawnPosition;
//     private Vector3 spawnScale;

//     public List<TrialResult> results = new List<TrialResult>();

//     [System.Serializable]
//     public class TrialResult
//     {
//         public int trialNumber;
//         public float ballAppearedAt;
//         public float playerTappedAt;
//         public float reactionTimeMs;
//         public bool scored;
//     }

//     void CreateUI()
//     {
//         GameObject canvasGO = new GameObject("GameCanvas");
//         Canvas canvas = canvasGO.AddComponent<Canvas>();
//         canvas.renderMode = RenderMode.ScreenSpaceOverlay;
//         canvasGO.AddComponent<CanvasScaler>();
//         canvasGO.AddComponent<GraphicRaycaster>();

//         GameObject scoreGO = new GameObject("ScoreText");
//         scoreGO.transform.SetParent(canvasGO.transform, false);
//         scoreText = scoreGO.AddComponent<Text>();
//         scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
//         scoreText.fontSize = 76;
//         scoreText.color = Color.white;
//         scoreText.alignment = TextAnchor.UpperCenter;
//         scoreText.text = "Score: 0";
//         RectTransform scoreRT = scoreGO.GetComponent<RectTransform>();
//         scoreRT.anchorMin = new Vector2(0, 1);
//         scoreRT.anchorMax = new Vector2(1, 1);
//         scoreRT.pivot = new Vector2(0.5f, 1f);
//         scoreRT.offsetMin = new Vector2(0, -120);
//         scoreRT.offsetMax = new Vector2(0, -20);

//         GameObject statusGO = new GameObject("StatusText");
//         statusGO.transform.SetParent(canvasGO.transform, false);
//         statusText = statusGO.AddComponent<Text>();
//         statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
//         statusText.fontSize = 70;
//         statusText.color = Color.white;
//         statusText.alignment = TextAnchor.UpperCenter;
//         statusText.text = "Wait for the ball!";
//         RectTransform statusRT = statusGO.GetComponent<RectTransform>();
//         statusRT.anchorMin = new Vector2(0, 1);
//         statusRT.anchorMax = new Vector2(1, 1);
//         statusRT.pivot = new Vector2(0.5f, 1f);
//         statusRT.offsetMin = new Vector2(0, -240);
//         statusRT.offsetMax = new Vector2(0, -130);
//     }

//     void UpdateUI(string status)
//     {
//         if (scoreText != null) scoreText.text = $"Score: {score}";
//         if (statusText != null) statusText.text = status;
//     }

//     void Start()
//     {
//         CreateUI();
//         spawnPosition = new Vector3(spawnX, spawnY, spawnZ);
//         spawnScale = transform.localScale;
//         sessionStartTime = Time.time;
//         sessionActive = true;
//         transform.position = hiddenPosition;
//         UpdateUI("");

//         if (startScreen != null) startScreen.SetActive(true);
//         StartCoroutine(WaitForStartTap());
//     }

//     IEnumerator WaitForStartTap()
//     {
//         while (true)
//         {
//             if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
//             {
//                 if (startScreen != null) startScreen.SetActive(false);
//                 UpdateUI("Wait for the ball!");
//                 sessionStartTime = Time.time;
//                 roundCoroutine = StartCoroutine(RunRound());
//                 StartCoroutine(SessionTimer());
//                 yield break;
//             }
//             yield return null;
//         }
//     }

//     void Update()
//     {
//         if (!sessionActive || !waitingForTap || shootingInProgress) return;

//         if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
//         {
//             Debug.Log("TAP DETECTED");
//             OnPlayerTap();
//         }
//     }

//     IEnumerator SessionTimer()
//     {
//         yield return new WaitForSeconds(sessionDuration);
//         EndSession();
//     }

//     IEnumerator RunRound()
//     {
//         transform.position = hiddenPosition;
//         transform.localScale = spawnScale;

//         waitingForTap = false;
//         shootingInProgress = false;

//         UpdateUI("Wait for the ball!");

//         float delay = Random.Range(minDelay, maxDelay);
//         Debug.Log($"Next ball in {delay:F2}s");
//         yield return new WaitForSeconds(delay);

//         if (!sessionActive) yield break;

//         transform.position = spawnPosition;
//         ballAppearTime = Time.time;
//         waitingForTap = true;
//         UpdateUI("Tap!");
//         Debug.Log("Ball appeared");

//         float elapsed = 0f;
//         while (elapsed < reactionWindow)
//         {
//             if (!waitingForTap) yield break;
//             elapsed += Time.deltaTime;
//             yield return null;
//         }

//         waitingForTap = false;
//         UpdateUI("Too slow! Ball missed!");
//         Debug.Log("No tap — rolling wide");

//         TrialResult result = new TrialResult
//         {
//             trialNumber = results.Count + 1,
//             ballAppearedAt = ballAppearTime - sessionStartTime,
//             playerTappedAt = -1f,
//             reactionTimeMs = -1f,
//             scored = false
//         };
//         results.Add(result);

//         StartCoroutine(ShootBall(ShotOutcome.NoTap));
//     }

//     void OnPlayerTap()
//     {
//         if (!waitingForTap || shootingInProgress) return;

//         waitingForTap = false;
//         float tapTime = Time.time;
//         float rt = (tapTime - ballAppearTime) * 1000f;
//         float elapsed = tapTime - ballAppearTime;

//         bool late = elapsed > lateThreshold;
//         bool goal = !late;

//         if (goal)
//         {
//             score++;
//             UpdateUI("Goal!");
//         }
//         else
//         {
//             UpdateUI("Too slow! Missed the goal!");
//         }

//         Debug.Log($"RT={rt:F1}ms, outcome={(goal ? "Goal" : "Miss")}");

//         TrialResult result = new TrialResult
//         {
//             trialNumber = results.Count + 1,
//             ballAppearedAt = ballAppearTime - sessionStartTime,
//             playerTappedAt = tapTime - sessionStartTime,
//             reactionTimeMs = rt,
//             scored = goal
//         };
//         results.Add(result);

//         StartCoroutine(ShootBall(goal ? ShotOutcome.Goal : ShotOutcome.Miss));
//     }

//     IEnumerator ShootBall(ShotOutcome outcome)
//     {
//         shootingInProgress = true;

//         Vector3 startPos = transform.position;
//         Vector3 endPos;

//         float midZ = (goalMinZ + goalMaxZ) / 2f;

//         // if (outcome == ShotOutcome.Goal)
//         // {
//         //     int spot = Random.Range(0, 12);
//         //     switch (spot)
//         //     {
//         //         case 0:
//         //             endPos = new Vector3(goalMinX + Random.Range(0f, 3f), goalMaxY - Random.Range(0f, 2f), Random.Range(goalMinZ, goalMaxZ));
//         //             break;
//         //         case 1:
//         //             endPos = new Vector3(goalMaxX - Random.Range(0f, 3f), goalMaxY - Random.Range(0f, 2f), Random.Range(goalMinZ, goalMaxZ));
//         //             break;
//         //         case 2:
//         //             endPos = new Vector3(goalMinX + Random.Range(0f, 3f), goalMinY + Random.Range(0f, 2f), Random.Range(goalMinZ, goalMaxZ));
//         //             break;
//         //         case 3:
//         //             endPos = new Vector3(goalMaxX - Random.Range(0f, 3f), goalMinY + Random.Range(0f, 2f), Random.Range(goalMinZ, goalMaxZ));
//         //             break;
//         //         case 4:
//         //             endPos = new Vector3(Random.Range(goalMinX * 0.5f, goalMaxX * 0.5f), goalMaxY - Random.Range(0f, 2f), Random.Range(goalMinZ, goalMaxZ));
//         //             break;
//         //         case 5:
//         //             endPos = new Vector3(Random.Range(goalMinX * 0.5f, goalMaxX * 0.5f), goalMinY + Random.Range(0f, 2f), Random.Range(goalMinZ, goalMaxZ));
//         //             break;
//         //         case 6:
//         //             endPos = new Vector3(goalMinX + Random.Range(0f, 4f), Random.Range(goalMinY, goalMaxY), Random.Range(goalMinZ, goalMaxZ));
//         //             break;
//         //         case 7:
//         //             endPos = new Vector3(goalMaxX - Random.Range(0f, 4f), Random.Range(goalMinY, goalMaxY), Random.Range(goalMinZ, goalMaxZ));
//         //             break;
//         //         case 8:
//         //             endPos = new Vector3(Random.Range(goalMinX, goalMaxX), goalMaxY - Random.Range(0f, 3f), Random.Range(goalMinZ, goalMaxZ));
//         //             break;
//         //         case 9:
//         //             endPos = new Vector3(Random.Range(goalMinX, goalMaxX), goalMinY + Random.Range(0f, 3f), Random.Range(goalMinZ, goalMaxZ));
//         //             break;
//         //         case 10:
//         //             endPos = new Vector3(goalMinX + Random.Range(0f, 2f), Random.Range(goalMinY, goalMaxY), Random.Range(goalMinZ, goalMaxZ));
//         //             break;
//         //         case 11:
//         //             endPos = new Vector3(goalMaxX - Random.Range(0f, 2f), Random.Range(goalMinY, goalMaxY), Random.Range(goalMinZ, goalMaxZ));
//         //             break;
//         //         default:
//         //             endPos = new Vector3(Random.Range(goalMinX, goalMaxX), Random.Range(goalMinY, goalMaxY), Random.Range(goalMinZ, goalMaxZ));
//         //             break;
//         //     }
//         // }

//         if (outcome == ShotOutcome.Goal)
// {
//     // Divide the net into zones and pick one randomly each time
//     float zoneRoll = Random.value;
//     float x, y;

//     if (zoneRoll < 0.2f)
//     {
//         // Top left
//         x = Random.Range(goalMinX, goalMinX + (goalMaxX - goalMinX) * 0.35f);
//         y = Random.Range(goalMinY + (goalMaxY - goalMinY) * 0.6f, goalMaxY);
//     }
//     else if (zoneRoll < 0.4f)
//     {
//         // Top right
//         x = Random.Range(goalMinX + (goalMaxX - goalMinX) * 0.65f, goalMaxX);
//         y = Random.Range(goalMinY + (goalMaxY - goalMinY) * 0.6f, goalMaxY);
//     }
//     else if (zoneRoll < 0.55f)
//     {
//         // Bottom left
//         x = Random.Range(goalMinX, goalMinX + (goalMaxX - goalMinX) * 0.35f);
//         y = Random.Range(goalMinY, goalMinY + (goalMaxY - goalMinY) * 0.4f);
//     }
//     else if (zoneRoll < 0.7f)
//     {
//         // Bottom right
//         x = Random.Range(goalMinX + (goalMaxX - goalMinX) * 0.65f, goalMaxX);
//         y = Random.Range(goalMinY, goalMinY + (goalMaxY - goalMinY) * 0.4f);
//     }
//     else if (zoneRoll < 0.82f)
//     {
//         // Top center
//         x = Random.Range(goalMinX + (goalMaxX - goalMinX) * 0.3f, goalMinX + (goalMaxX - goalMinX) * 0.7f);
//         y = Random.Range(goalMinY + (goalMaxY - goalMinY) * 0.55f, goalMaxY);
//     }
//     else if (zoneRoll < 0.92f)
//     {
//         // Bottom center
//         x = Random.Range(goalMinX + (goalMaxX - goalMinX) * 0.3f, goalMinX + (goalMaxX - goalMinX) * 0.7f);
//         y = Random.Range(goalMinY, goalMinY + (goalMaxY - goalMinY) * 0.45f);
//     }
//     else
//     {
//         // True center — least common
//         x = Random.Range(goalMinX + (goalMaxX - goalMinX) * 0.35f, goalMinX + (goalMaxX - goalMinX) * 0.65f);
//         y = Random.Range(goalMinY + (goalMaxY - goalMinY) * 0.35f, goalMinY + (goalMaxY - goalMinY) * 0.65f);
//     }

//     endPos = new Vector3(x, y, Random.Range(goalMinZ, goalMaxZ));
// }
//         else if (outcome == ShotOutcome.Miss)
//         {
//             endPos = missTarget;
//         }
//         else
//         {
//             float side = Random.value > 0.5f ? 1f : -1f;
//             endPos = new Vector3(side * Random.Range(8f, 14f), spawnY, midZ);
//         }

//         float elapsed = 0f;
//         float travelDuration = shootDuration * 0.7f;

//         while (elapsed < travelDuration)
//         {
//             elapsed += Time.deltaTime;
//             float t = Mathf.Clamp01(elapsed / travelDuration);

//             Vector3 pos = Vector3.Lerp(startPos, endPos, t);

//             if (outcome == ShotOutcome.Goal)
//             {
//                 pos.y += Mathf.Sin(t * Mathf.PI) * 2f;
//                 transform.localScale = spawnScale;
//             }
//             else if (outcome == ShotOutcome.Miss)
//             {
//                 transform.localScale = spawnScale;
//             }
//             else
//             {
//                 pos.y = spawnY;
//                 transform.localScale = spawnScale;
//                 transform.Rotate(Vector3.forward, 360f * Time.deltaTime * 2f, Space.World);
//             }

//             if (outcome != ShotOutcome.NoTap)
//                 transform.Rotate(Vector3.right, 360f * Time.deltaTime * 2f, Space.World);

//             transform.position = pos;
//             yield return null;
//         }

//         if (outcome == ShotOutcome.Goal)
//         {
//             yield return StartCoroutine(BounceOffGoalBack(endPos));
//         }
//         else if (outcome == ShotOutcome.Miss)
//         {
//             transform.position = hiddenPosition;
//         }
//         else
//         {
//             yield return StartCoroutine(GroundBounce(endPos, 0.3f));
//             yield return new WaitForSeconds(0.3f);
//             transform.position = hiddenPosition;
//         }

//         shootingInProgress = false;
//         yield return new WaitForSeconds(1.0f);
//         if (sessionActive) roundCoroutine = StartCoroutine(RunRound());
//     }

//     IEnumerator BounceOffGoalBack(Vector3 hitPos)
//     {
//         // Vector3 bounceTarget = new Vector3(
//         //     hitPos.x + Random.Range(-1f, 1f),
//         //     Random.Range(0.3f, 1.5f),
//         //     spawnZ + Random.Range(1f, 3f)
//         // );

//         Vector3 bounceTarget = new Vector3(
//     hitPos.x + Random.Range(-1f, 1f),
//     243f,  // ground level in your scene
//     spawnZ + Random.Range(1f, 3f)
// );

//         float elapsed = 0f;
//         float duration = 0.5f;
//         Vector3 startPos = transform.position;

//         while (elapsed < duration)
//         {
//             elapsed += Time.deltaTime;
//             float t = elapsed / duration;
//             float eased = 1f - (1f - t) * (1f - t);

//             Vector3 pos = Vector3.Lerp(startPos, bounceTarget, eased);
//             pos.y += Mathf.Sin(t * Mathf.PI) * 6f;

//             float scale = Mathf.Lerp(spawnScale.x * 0.35f, spawnScale.x * 0.8f, t);
//             transform.localScale = new Vector3(scale, scale, scale);

//             transform.position = pos;
//             transform.Rotate(Vector3.right, -360f * Time.deltaTime * 2f, Space.World);
//             yield return null;
//         }

//         yield return StartCoroutine(GroundBounce(bounceTarget, 0.4f));
//         yield return new WaitForSeconds(0.5f);
//         transform.position = hiddenPosition;
//     }

//     IEnumerator GroundBounce(Vector3 startPos, float bounceHeight)
//     {
//         Vector3 endPos = startPos + new Vector3(
//             Random.Range(-0.5f, 0.5f),
//             0f,
//             Random.Range(-0.5f, 0.5f)
//         );

//         float elapsed = 0f;
//         float duration = 0.35f;

//         while (elapsed < duration)
//         {
//             elapsed += Time.deltaTime;
//             float t = elapsed / duration;

//             Vector3 pos = Vector3.Lerp(startPos, endPos, t);
//             pos.y += Mathf.Sin(t * Mathf.PI) * bounceHeight;

//             transform.position = pos;
//             yield return null;
//         }
//     }

//     void EndSession()
//     {
//         sessionActive = false;
//         waitingForTap = false;
//         if (roundCoroutine != null) StopCoroutine(roundCoroutine);
//         transform.position = hiddenPosition;
//         UpdateUI($"Session complete! Final score: {score}");

//         SoccerSendDataAPI api = GetComponent<SoccerSendDataAPI>();
//         if (api != null) api.SetupAPIDataToSend(this);

//         SaveResults();
//     }

//     void SaveResults()
//     {
//         Debug.Log($"PVT session complete. {results.Count} trials recorded.");
//         Debug.Log("trial, ball_appeared_at_s, player_tapped_at_s, reaction_time_ms, scored");
//         foreach (TrialResult r in results)
//             Debug.Log($"{r.trialNumber}, {r.ballAppearedAt:F3}, {r.playerTappedAt:F3}, {r.reactionTimeMs:F1}, {r.scored}");
//     }
// }




// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public enum ShotOutcome { Goal, Miss, NoTap }

// public class BallShooter : MonoBehaviour
// {
//     [Header("Goal Settings")]
//     public float goalMinX = 275f;
//     public float goalMaxX = 295f;
//     public float goalMinY = 245f;
//     public float goalMaxY = 258f;
//     public float goalMinZ = -8f;
//     public float goalMaxZ = 8f;

//     [Header("Timing")]
//     public float reactionWindow = 8.0f;
//     public float lateThreshold = 5.0f;
//     public float shootDuration = 1.5f;
//     public float minDelay = 1.0f;
//     public float maxDelay = 3.0f;
//     public float sessionDuration = 180f;

//     [Header("Spawn")]
//     public float spawnX = 320f;
//     public float spawnY = 243f;
//     public float spawnZ = 0.2f;

//     [Header("Ground Level")]
//     public float groundY = 243f;

//     [Header("Start Screen")]
//     public GameObject startScreen;

//     private Text scoreText;
//     private Text statusText;
//     private int score = 0;
//     private Vector3 hiddenPosition = new Vector3(0f, 0f, 1000f);

//     private bool waitingForTap = false;
//     private bool shootingInProgress = false;
//     private float ballAppearTime;
//     private float sessionStartTime;
//     private bool sessionActive = false;
//     private Coroutine roundCoroutine;
//     private Vector3 spawnPosition;
//     private Vector3 spawnScale;

//     public List<TrialResult> results = new List<TrialResult>();

//     [System.Serializable]
//     public class TrialResult
//     {
//         public int trialNumber;
//         public float ballAppearedAt;
//         public float playerTappedAt;
//         public float reactionTimeMs;
//         public bool scored;
//     }

//     void CreateUI()
//     {
//         GameObject canvasGO = new GameObject("GameCanvas");
//         Canvas canvas = canvasGO.AddComponent<Canvas>();
//         canvas.renderMode = RenderMode.ScreenSpaceOverlay;
//         canvasGO.AddComponent<CanvasScaler>();
//         canvasGO.AddComponent<GraphicRaycaster>();

//         GameObject scoreGO = new GameObject("ScoreText");
//         scoreGO.transform.SetParent(canvasGO.transform, false);
//         scoreText = scoreGO.AddComponent<Text>();
//         scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
//         scoreText.fontSize = 76;
//         scoreText.color = Color.white;
//         scoreText.alignment = TextAnchor.UpperCenter;
//         scoreText.text = "Score: 0";
//         RectTransform scoreRT = scoreGO.GetComponent<RectTransform>();
//         scoreRT.anchorMin = new Vector2(0, 1);
//         scoreRT.anchorMax = new Vector2(1, 1);
//         scoreRT.pivot = new Vector2(0.5f, 1f);
//         scoreRT.offsetMin = new Vector2(0, -120);
//         scoreRT.offsetMax = new Vector2(0, -20);

//         GameObject statusGO = new GameObject("StatusText");
//         statusGO.transform.SetParent(canvasGO.transform, false);
//         statusText = statusGO.AddComponent<Text>();
//         statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
//         statusText.fontSize = 70;
//         statusText.color = Color.white;
//         statusText.alignment = TextAnchor.UpperCenter;
//         statusText.text = "Wait for the ball!";
//         RectTransform statusRT = statusGO.GetComponent<RectTransform>();
//         statusRT.anchorMin = new Vector2(0, 1);
//         statusRT.anchorMax = new Vector2(1, 1);
//         statusRT.pivot = new Vector2(0.5f, 1f);
//         statusRT.offsetMin = new Vector2(0, -240);
//         statusRT.offsetMax = new Vector2(0, -130);
//     }

//     void UpdateUI(string status)
//     {
//         if (scoreText != null) scoreText.text = $"Score: {score}";
//         if (statusText != null) statusText.text = status;
//     }

//     void Start()
//     {
//         CreateUI();
//         spawnPosition = new Vector3(spawnX, spawnY, spawnZ);
//         spawnScale = transform.localScale;
//         sessionStartTime = Time.time;
//         sessionActive = true;
//         transform.position = hiddenPosition;
//         UpdateUI("");

//         if (startScreen != null) startScreen.SetActive(true);
//         StartCoroutine(WaitForStartTap());
//     }

//     IEnumerator WaitForStartTap()
//     {
//         while (true)
//         {
//             if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
//             {
//                 if (startScreen != null) startScreen.SetActive(false);
//                 UpdateUI("Wait for the ball!");
//                 sessionStartTime = Time.time;
//                 roundCoroutine = StartCoroutine(RunRound());
//                 StartCoroutine(SessionTimer());
//                 yield break;
//             }
//             yield return null;
//         }
//     }

//     void Update()
//     {
//         if (!sessionActive || !waitingForTap || shootingInProgress) return;

//         if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
//         {
//             Debug.Log("TAP DETECTED");
//             OnPlayerTap();
//         }
//     }

//     IEnumerator SessionTimer()
//     {
//         yield return new WaitForSeconds(sessionDuration);
//         EndSession();
//     }

//     IEnumerator RunRound()
//     {
//         transform.position = hiddenPosition;
//         transform.localScale = spawnScale;

//         waitingForTap = false;
//         shootingInProgress = false;

//         UpdateUI("Wait for the ball!");

//         float delay = Random.Range(minDelay, maxDelay);
//         Debug.Log($"Next ball in {delay:F2}s");
//         yield return new WaitForSeconds(delay);

//         if (!sessionActive) yield break;

//         transform.position = spawnPosition;
//         ballAppearTime = Time.time;
//         waitingForTap = true;
//         UpdateUI("Tap!");
//         Debug.Log("Ball appeared");

//         float elapsed = 0f;
//         while (elapsed < reactionWindow)
//         {
//             if (!waitingForTap) yield break;
//             elapsed += Time.deltaTime;
//             yield return null;
//         }

//         waitingForTap = false;
//         UpdateUI("Too slow! Ball missed!");
//         Debug.Log("No tap — rolling wide");

//         TrialResult result = new TrialResult
//         {
//             trialNumber = results.Count + 1,
//             ballAppearedAt = ballAppearTime - sessionStartTime,
//             playerTappedAt = -1f,
//             reactionTimeMs = -1f,
//             scored = false
//         };
//         results.Add(result);

//         StartCoroutine(ShootBall(ShotOutcome.NoTap));
//     }

//     void OnPlayerTap()
//     {
//         if (!waitingForTap || shootingInProgress) return;

//         waitingForTap = false;
//         float tapTime = Time.time;
//         float rt = (tapTime - ballAppearTime) * 1000f;
//         float elapsed = tapTime - ballAppearTime;

//         bool late = elapsed > lateThreshold;
//         bool goal = !late;

//         if (goal)
//         {
//             score++;
//             UpdateUI("Goal!");
//         }
//         else
//         {
//             UpdateUI("Too slow! Missed the goal!");
//         }

//         Debug.Log($"RT={rt:F1}ms, outcome={(goal ? "Goal" : "Miss")}");

//         TrialResult result = new TrialResult
//         {
//             trialNumber = results.Count + 1,
//             ballAppearedAt = ballAppearTime - sessionStartTime,
//             playerTappedAt = tapTime - sessionStartTime,
//             reactionTimeMs = rt,
//             scored = goal
//         };
//         results.Add(result);

//         StartCoroutine(ShootBall(goal ? ShotOutcome.Goal : ShotOutcome.Miss));
//     }

//     IEnumerator ShootBall(ShotOutcome outcome)
//     {
//         shootingInProgress = true;

//         Vector3 startPos = transform.position;
//         Vector3 endPos;

//         if (outcome == ShotOutcome.Goal)
//         {
//             float zoneRoll = Random.value;
//             float x, y;

//             if (zoneRoll < 0.2f)
//             {
//                 x = Random.Range(goalMinX, goalMinX + (goalMaxX - goalMinX) * 0.35f);
//                 y = Random.Range(goalMinY + (goalMaxY - goalMinY) * 0.6f, goalMaxY);
//             }
//             else if (zoneRoll < 0.4f)
//             {
//                 x = Random.Range(goalMinX + (goalMaxX - goalMinX) * 0.65f, goalMaxX);
//                 y = Random.Range(goalMinY + (goalMaxY - goalMinY) * 0.6f, goalMaxY);
//             }
//             else if (zoneRoll < 0.55f)
//             {
//                 x = Random.Range(goalMinX, goalMinX + (goalMaxX - goalMinX) * 0.35f);
//                 y = Random.Range(goalMinY, goalMinY + (goalMaxY - goalMinY) * 0.4f);
//             }
//             else if (zoneRoll < 0.7f)
//             {
//                 x = Random.Range(goalMinX + (goalMaxX - goalMinX) * 0.65f, goalMaxX);
//                 y = Random.Range(goalMinY, goalMinY + (goalMaxY - goalMinY) * 0.4f);
//             }
//             else if (zoneRoll < 0.82f)
//             {
//                 x = Random.Range(goalMinX + (goalMaxX - goalMinX) * 0.3f, goalMinX + (goalMaxX - goalMinX) * 0.7f);
//                 y = Random.Range(goalMinY + (goalMaxY - goalMinY) * 0.55f, goalMaxY);
//             }
//             else if (zoneRoll < 0.92f)
//             {
//                 x = Random.Range(goalMinX + (goalMaxX - goalMinX) * 0.3f, goalMinX + (goalMaxX - goalMinX) * 0.7f);
//                 y = Random.Range(goalMinY, goalMinY + (goalMaxY - goalMinY) * 0.45f);
//             }
//             else
//             {
//                 x = Random.Range(goalMinX + (goalMaxX - goalMinX) * 0.35f, goalMinX + (goalMaxX - goalMinX) * 0.65f);
//                 y = Random.Range(goalMinY + (goalMaxY - goalMinY) * 0.35f, goalMinY + (goalMaxY - goalMinY) * 0.65f);
//             }

//             endPos = new Vector3(x, y, Random.Range(goalMinZ, goalMaxZ));
//         }
//         else if (outcome == ShotOutcome.Miss)
//         {
//             // Late tap — flies sideways in Z direction (sideways relative to camera)
//             float side = Random.value > 0.5f ? 1f : -1f;
//             endPos = new Vector3(
//                 spawnX + Random.Range(-5f, 5f),
//                 spawnY + Random.Range(5f, 15f),
//                 spawnZ + side * Random.Range(50f, 100f)
//             );
//         }
//         else
//         {
//             // No tap — flies up and over the goal out of frame
//             endPos = new Vector3(
//                 spawnX + Random.Range(-5f, 5f),
//                 spawnY + 500f,
//                 spawnZ
//             );
//         }

//         float elapsed = 0f;
//         float travelDuration = shootDuration * 0.7f;

//         while (elapsed < travelDuration)
//         {
//             elapsed += Time.deltaTime;
//             float t = Mathf.Clamp01(elapsed / travelDuration);

//             Vector3 pos = Vector3.Lerp(startPos, endPos, t);

//             if (outcome == ShotOutcome.Goal)
//             {
//                 pos.y += Mathf.Sin(t * Mathf.PI) * 2f;
//                 transform.localScale = spawnScale;
//             }
//             else if (outcome == ShotOutcome.Miss)
//             {
//                 pos.y += Mathf.Sin(t * Mathf.PI) * 3f;
//                 transform.localScale = spawnScale;
//             }
//             else
//             {
//                 transform.localScale = spawnScale;
//             }

//             if (outcome != ShotOutcome.NoTap)
//                 transform.Rotate(Vector3.right, 360f * Time.deltaTime * 2f, Space.World);

//             transform.position = pos;
//             yield return null;
//         }

//         if (outcome == ShotOutcome.Goal)
//         {
//             yield return StartCoroutine(BounceOffGoalBack(endPos));
//         }
//         else if (outcome == ShotOutcome.Miss)
//         {
//             yield return new WaitForSeconds(0.3f);
//             transform.position = hiddenPosition;
//         }
//         else
//         {
//             transform.position = hiddenPosition;
//         }

//         shootingInProgress = false;
//         yield return new WaitForSeconds(1.0f);
//         if (sessionActive) roundCoroutine = StartCoroutine(RunRound());
//     }

//     IEnumerator BounceOffGoalBack(Vector3 hitPos)
//     {
//         Vector3 bounceTarget = new Vector3(
//             spawnX + Random.Range(-2f, 2f),
//             groundY,
//             spawnZ + Random.Range(-2f, 2f)
//         );

//         float elapsed = 0f;
//         float duration = 0.5f;
//         Vector3 startPos = transform.position;

//         while (elapsed < duration)
//         {
//             elapsed += Time.deltaTime;
//             float t = elapsed / duration;
//             float eased = 1f - (1f - t) * (1f - t);

//             Vector3 pos = Vector3.Lerp(startPos, bounceTarget, eased);
//             pos.y += Mathf.Sin(t * Mathf.PI) * 6f;

//             float scale = Mathf.Lerp(spawnScale.x * 0.35f, spawnScale.x * 0.8f, t);
//             transform.localScale = new Vector3(scale, scale, scale);

//             transform.position = pos;
//             transform.Rotate(Vector3.right, -360f * Time.deltaTime * 2f, Space.World);
//             yield return null;
//         }

//         yield return StartCoroutine(GroundBounce(bounceTarget, 0.4f));
//         yield return new WaitForSeconds(0.5f);
//         transform.position = hiddenPosition;
//     }

//     IEnumerator GroundBounce(Vector3 startPos, float bounceHeight)
//     {
//         Vector3 endPos = startPos + new Vector3(
//             Random.Range(-0.5f, 0.5f),
//             0f,
//             Random.Range(-0.5f, 0.5f)
//         );

//         float elapsed = 0f;
//         float duration = 0.35f;

//         while (elapsed < duration)
//         {
//             elapsed += Time.deltaTime;
//             float t = elapsed / duration;

//             Vector3 pos = Vector3.Lerp(startPos, endPos, t);
//             pos.y += Mathf.Sin(t * Mathf.PI) * bounceHeight;

//             transform.position = pos;
//             yield return null;
//         }
//     }

//     void EndSession()
//     {
//         sessionActive = false;
//         waitingForTap = false;
//         if (roundCoroutine != null) StopCoroutine(roundCoroutine);
//         transform.position = hiddenPosition;
//         UpdateUI($"Session complete! Final score: {score}");

//         SoccerSendDataAPI api = GetComponent<SoccerSendDataAPI>();
//         if (api != null) api.SetupAPIDataToSend(this);

//         SaveResults();
//     }

//     void SaveResults()
//     {
//         Debug.Log($"PVT session complete. {results.Count} trials recorded.");
//         Debug.Log("trial, ball_appeared_at_s, player_tapped_at_s, reaction_time_ms, scored");
//         foreach (TrialResult r in results)
//             Debug.Log($"{r.trialNumber}, {r.ballAppearedAt:F3}, {r.playerTappedAt:F3}, {r.reactionTimeMs:F1}, {r.scored}");
//     }
// }


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ShotOutcome { Goal, Miss, NoTap }

public class BallShooter : MonoBehaviour
{
    [Header("Goal Settings")]
    public float goalMinX = 275f;
    public float goalMaxX = 295f;
    public float goalMinY = 245f;
    public float goalMaxY = 258f;
    public float goalMinZ = -8f;
    public float goalMaxZ = 8f;

    [Header("Timing")]
    public float reactionWindow = 8.0f;
    public float lateThreshold = 5.0f;
    public float shootDuration = 1.5f;
    public float minDelay = 1.0f;
    public float maxDelay = 3.0f;
    public float sessionDuration = 180f;

    [Header("Spawn")]
    public float spawnX = 320f;
    public float spawnY = 243f;
    public float spawnZ = 0.2f;

    [Header("Ground Level")]
    public float groundY = 243f;

    // [Header("Start Screen")]
    // public GameObject startScreen;

    [Header("Start Screen")]
    public GameObject startScreen;
    public UnityEngine.UI.Button startButton;

    private Text scoreText;
    private Text statusText;
    private int score = 0;
    private Vector3 hiddenPosition = new Vector3(0f, 0f, 1000f);

    private bool waitingForTap = false;
    private bool shootingInProgress = false;
    private float ballAppearTime;
    private float sessionStartTime;
    private bool sessionActive = false;
    private Coroutine roundCoroutine;
    private Vector3 spawnPosition;
    private Vector3 spawnScale;

    public List<TrialResult> results = new List<TrialResult>();

    [System.Serializable]
    public class TrialResult
    {
        public int trialNumber;
        public float ballAppearedAt;
        public float playerTappedAt;
        public float reactionTimeMs;
        public bool scored;
    }

    void CreateUI()
    {
        GameObject canvasGO = new GameObject("GameCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject scoreGO = new GameObject("ScoreText");
        scoreGO.transform.SetParent(canvasGO.transform, false);
        scoreText = scoreGO.AddComponent<Text>();
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreText.fontSize = 76;
        scoreText.color = Color.white;
        scoreText.alignment = TextAnchor.UpperCenter;
        scoreText.text = "Score: 0";
        RectTransform scoreRT = scoreGO.GetComponent<RectTransform>();
        scoreRT.anchorMin = new Vector2(0, 1);
        scoreRT.anchorMax = new Vector2(1, 1);
        scoreRT.pivot = new Vector2(0.5f, 1f);
        scoreRT.offsetMin = new Vector2(0, -120);
        scoreRT.offsetMax = new Vector2(0, -20);

        GameObject statusGO = new GameObject("StatusText");
        statusGO.transform.SetParent(canvasGO.transform, false);
        statusText = statusGO.AddComponent<Text>();
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = 70;
        statusText.color = Color.white;
        statusText.alignment = TextAnchor.UpperCenter;
        statusText.text = "Wait for the ball!";
        RectTransform statusRT = statusGO.GetComponent<RectTransform>();
        statusRT.anchorMin = new Vector2(0, 1);
        statusRT.anchorMax = new Vector2(1, 1);
        statusRT.pivot = new Vector2(0.5f, 1f);
        statusRT.offsetMin = new Vector2(0, -240);
        statusRT.offsetMax = new Vector2(0, -130);
    }

    void UpdateUI(string status)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (statusText != null) statusText.text = status;
    }

    void Start()
    {
        CreateUI();
        spawnPosition = new Vector3(spawnX, spawnY, spawnZ);
        spawnScale = transform.localScale;
        sessionStartTime = Time.time;
        sessionActive = true;
        transform.position = hiddenPosition;
        UpdateUI("");

        if (startScreen != null) startScreen.SetActive(true);
        StartCoroutine(WaitForStartTap());
    }

    // IEnumerator WaitForStartTap()
    // {
    //     while (true)
    //     {
    //         if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
    //         {
    //             if (startScreen != null) startScreen.SetActive(false);
    //             UpdateUI("Wait for the ball!");
    //             sessionStartTime = Time.time;
    //             roundCoroutine = StartCoroutine(RunRound());
    //             StartCoroutine(SessionTimer());
    //             yield break;
    //         }
    //         yield return null;
    //     }
    // }

    IEnumerator WaitForStartTap()
{
    if (startButton != null)
    {
        bool pressed = false;
        startButton.onClick.AddListener(() => pressed = true);
        while (!pressed) yield return null;
        startButton.onClick.RemoveAllListeners();
    }
    else
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                break;
            yield return null;
        }
    }

    if (startScreen != null) startScreen.SetActive(false);
    UpdateUI("Wait for the ball!");
    sessionStartTime = Time.time;
    roundCoroutine = StartCoroutine(RunRound());
    StartCoroutine(SessionTimer());
}

    void Update()
    {
        if (!sessionActive || !waitingForTap || shootingInProgress) return;

        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Debug.Log("TAP DETECTED");
            OnPlayerTap();
        }
    }

    IEnumerator SessionTimer()
    {
        yield return new WaitForSeconds(sessionDuration);
        EndSession();
    }

    IEnumerator RunRound()
    {
        transform.position = hiddenPosition;
        transform.localScale = spawnScale;

        waitingForTap = false;
        shootingInProgress = false;

        UpdateUI("Wait for the ball!");

        float delay = Random.Range(minDelay, maxDelay);
        Debug.Log($"Next ball in {delay:F2}s");
        yield return new WaitForSeconds(delay);

        if (!sessionActive) yield break;

        transform.position = spawnPosition;
        ballAppearTime = Time.time;
        waitingForTap = true;
        UpdateUI("Tap!");
        Debug.Log("Ball appeared");

        float elapsed = 0f;
        while (elapsed < reactionWindow)
        {
            if (!waitingForTap) yield break;
            elapsed += Time.deltaTime;
            yield return null;
        }

        waitingForTap = false;
        UpdateUI("Too slow! Ball missed!");
        Debug.Log("No tap — rolling toward camera");

        TrialResult result = new TrialResult
        {
            trialNumber = results.Count + 1,
            ballAppearedAt = ballAppearTime - sessionStartTime,
            playerTappedAt = -1f,
            reactionTimeMs = -1f,
            scored = false
        };
        results.Add(result);

        StartCoroutine(ShootBall(ShotOutcome.NoTap));
    }

    void OnPlayerTap()
    {
        if (!waitingForTap || shootingInProgress) return;

        waitingForTap = false;
        float tapTime = Time.time;
        float rt = (tapTime - ballAppearTime) * 1000f;
        float elapsed = tapTime - ballAppearTime;

        bool late = elapsed > lateThreshold;
        bool goal = !late;

        if (goal)
        {
            score++;
            UpdateUI("Goal!");
        }
        else
        {
            UpdateUI("Too slow! Missed the goal!");
        }

        Debug.Log($"RT={rt:F1}ms, outcome={(goal ? "Goal" : "Miss")}");

        TrialResult result = new TrialResult
        {
            trialNumber = results.Count + 1,
            ballAppearedAt = ballAppearTime - sessionStartTime,
            playerTappedAt = tapTime - sessionStartTime,
            reactionTimeMs = rt,
            scored = goal
        };
        results.Add(result);

        StartCoroutine(ShootBall(goal ? ShotOutcome.Goal : ShotOutcome.Miss));
    }

    IEnumerator ShootBall(ShotOutcome outcome)
    {
        shootingInProgress = true;

        Vector3 startPos = transform.position;
        Vector3 endPos;

        if (outcome == ShotOutcome.Goal)
        {
            float zoneRoll = Random.value;
            float x, y;

            if (zoneRoll < 0.2f)
            {
                x = Random.Range(goalMinX, goalMinX + (goalMaxX - goalMinX) * 0.35f);
                y = Random.Range(goalMinY + (goalMaxY - goalMinY) * 0.6f, goalMaxY);
            }
            else if (zoneRoll < 0.4f)
            {
                x = Random.Range(goalMinX + (goalMaxX - goalMinX) * 0.65f, goalMaxX);
                y = Random.Range(goalMinY + (goalMaxY - goalMinY) * 0.6f, goalMaxY);
            }
            else if (zoneRoll < 0.55f)
            {
                x = Random.Range(goalMinX, goalMinX + (goalMaxX - goalMinX) * 0.35f);
                y = Random.Range(goalMinY, goalMinY + (goalMaxY - goalMinY) * 0.4f);
            }
            else if (zoneRoll < 0.7f)
            {
                x = Random.Range(goalMinX + (goalMaxX - goalMinX) * 0.65f, goalMaxX);
                y = Random.Range(goalMinY, goalMinY + (goalMaxY - goalMinY) * 0.4f);
            }
            else if (zoneRoll < 0.82f)
            {
                x = Random.Range(goalMinX + (goalMaxX - goalMinX) * 0.3f, goalMinX + (goalMaxX - goalMinX) * 0.7f);
                y = Random.Range(goalMinY + (goalMaxY - goalMinY) * 0.55f, goalMaxY);
            }
            else if (zoneRoll < 0.92f)
            {
                x = Random.Range(goalMinX + (goalMaxX - goalMinX) * 0.3f, goalMinX + (goalMaxX - goalMinX) * 0.7f);
                y = Random.Range(goalMinY, goalMinY + (goalMaxY - goalMinY) * 0.45f);
            }
            else
            {
                x = Random.Range(goalMinX + (goalMaxX - goalMinX) * 0.35f, goalMinX + (goalMaxX - goalMinX) * 0.65f);
                y = Random.Range(goalMinY + (goalMaxY - goalMinY) * 0.35f, goalMinY + (goalMaxY - goalMinY) * 0.65f);
            }

            endPos = new Vector3(x, y, Random.Range(goalMinZ, goalMaxZ));
        }
        else if (outcome == ShotOutcome.Miss)
        {
            // Late tap — flies sideways in Z direction
            float side = Random.value > 0.5f ? 1f : -1f;
            endPos = new Vector3(
                spawnX + Random.Range(-5f, 5f),
                spawnY + Random.Range(5f, 15f),
                spawnZ + side * Random.Range(50f, 100f)
            );
        }
        else
        {
            // No tap — rolls toward camera (increasing X away from goal)
            endPos = new Vector3(
                spawnX + Random.Range(50f, 100f),
                groundY,
                spawnZ + Random.Range(-5f, 5f)
            );
        }

        float elapsed = 0f;
        float travelDuration = shootDuration * 0.7f;

        while (elapsed < travelDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / travelDuration);

            Vector3 pos = Vector3.Lerp(startPos, endPos, t);

            if (outcome == ShotOutcome.Goal)
            {
                pos.y += Mathf.Sin(t * Mathf.PI) * 2f;
                transform.localScale = spawnScale;
            }
            else if (outcome == ShotOutcome.Miss)
            {
                pos.y += Mathf.Sin(t * Mathf.PI) * 3f;
                transform.localScale = spawnScale;
            }
            else
            {
                // No tap — rolls flat on ground toward camera
                pos.y = groundY;
                transform.localScale = spawnScale;
                transform.Rotate(Vector3.forward, 360f * Time.deltaTime * 2f, Space.World);
            }

            if (outcome != ShotOutcome.NoTap)
                transform.Rotate(Vector3.right, 360f * Time.deltaTime * 2f, Space.World);

            transform.position = pos;
            yield return null;
        }

        if (outcome == ShotOutcome.Goal)
        {
            yield return StartCoroutine(BounceOffGoalBack(endPos));
        }
        else if (outcome == ShotOutcome.Miss)
        {
            yield return new WaitForSeconds(0.3f);
            transform.position = hiddenPosition;
        }
        else
        {
            // No tap — ground bounce then hide
            yield return StartCoroutine(GroundBounce(endPos, 0.3f));
            yield return new WaitForSeconds(0.3f);
            transform.position = hiddenPosition;
        }

        shootingInProgress = false;
        yield return new WaitForSeconds(1.0f);
        if (sessionActive) roundCoroutine = StartCoroutine(RunRound());
    }

    // IEnumerator BounceOffGoalBack(Vector3 hitPos)
    // {
    //     Vector3 bounceTarget = new Vector3(
    //         spawnX + Random.Range(-2f, 2f),
    //         groundY,
    //         spawnZ + Random.Range(-2f, 2f)
    //     );

    //     float elapsed = 0f;
    //     float duration = 0.5f;
    //     Vector3 startPos = transform.position;

    //     while (elapsed < duration)
    //     {
    //         elapsed += Time.deltaTime;
    //         float t = elapsed / duration;
    //         float eased = 1f - (1f - t) * (1f - t);

    //         Vector3 pos = Vector3.Lerp(startPos, bounceTarget, eased);
    //         pos.y += Mathf.Sin(t * Mathf.PI) * 6f;

    //         float scale = Mathf.Lerp(spawnScale.x * 0.35f, spawnScale.x * 0.8f, t);
    //         transform.localScale = new Vector3(scale, scale, scale);

    //         transform.position = pos;
    //         transform.Rotate(Vector3.right, -360f * Time.deltaTime * 2f, Space.World);
    //         yield return null;
    //     }

    //     yield return StartCoroutine(GroundBounce(bounceTarget, 0.4f));
    //     yield return new WaitForSeconds(0.5f);
    //     transform.position = hiddenPosition;
    // }

IEnumerator BounceOffGoalBack(Vector3 hitPos)
{
    // Bounce back just a short distance inside the net, not all the way to spawn
    Vector3 bounceTarget = new Vector3(
        hitPos.x + Random.Range(3f, 6f),  // small bounce back, not all the way to spawnX
        groundY,
        hitPos.z + Random.Range(-2f, 2f)
    );

    float elapsed = 0f;
    float duration = 0.5f;
    Vector3 startPos = transform.position;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        float eased = 1f - (1f - t) * (1f - t);

        Vector3 pos = Vector3.Lerp(startPos, bounceTarget, eased);
        pos.y += Mathf.Sin(t * Mathf.PI) * 2f;  // reduced from 6f to 2f

        float scale = Mathf.Lerp(spawnScale.x * 0.35f, spawnScale.x * 0.8f, t);
        transform.localScale = new Vector3(scale, scale, scale);

        transform.position = pos;
        transform.Rotate(Vector3.right, -360f * Time.deltaTime * 2f, Space.World);
        yield return null;
    }

    yield return StartCoroutine(GroundBounce(bounceTarget, 0.4f));
    yield return new WaitForSeconds(0.5f);
    transform.position = hiddenPosition;
}


    IEnumerator GroundBounce(Vector3 startPos, float bounceHeight)
    {
        Vector3 endPos = startPos + new Vector3(
            Random.Range(-0.5f, 0.5f),
            0f,
            Random.Range(-0.5f, 0.5f)
        );

        float elapsed = 0f;
        float duration = 0.35f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * bounceHeight;

            transform.position = pos;
            yield return null;
        }
    }

    void EndSession()
    {
        sessionActive = false;
        waitingForTap = false;
        if (roundCoroutine != null) StopCoroutine(roundCoroutine);
        transform.position = hiddenPosition;
        UpdateUI($"Session complete! Final score: {score}");

        SoccerSendDataAPI api = GetComponent<SoccerSendDataAPI>();
        if (api != null) api.SetupAPIDataToSend(this);

        SaveResults();
    }

    void SaveResults()
    {
        Debug.Log($"PVT session complete. {results.Count} trials recorded.");
        Debug.Log("trial, ball_appeared_at_s, player_tapped_at_s, reaction_time_ms, scored");
        foreach (TrialResult r in results)
            Debug.Log($"{r.trialNumber}, {r.ballAppearedAt:F3}, {r.playerTappedAt:F3}, {r.reactionTimeMs:F1}, {r.scored}");
    }
}