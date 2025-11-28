using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class MagnetVectorRenderer : MonoBehaviour
{
    private SpriteShapeController controller;
    private SpriteShapeRenderer renderer2D;
    private Spline spline;

    private MagnetSpawnerScript spawner;
    private Transform magnetTransform;

    [Header("Anchors")]
    [Tooltip("Where tether starts. If null, uses this transform.")]
    public Transform baseTransform;

    public string magnetCloneName = "MagnetProjectile(Clone)";
    public string magnetLayerName = "Magnet";

    [Header("Point Spacing")]
    [Tooltip("Interval used to decide how many points we WANT.")]
    public float dropInterval = 10f;

    [Tooltip("If magnet gets closer than this to base, reset line.")]
    public float recallResetDistance = 1.0f;

    [Header("Stop Detection (flag only)")]
    [Tooltip("Movement below this per frame counts as 'not moving'.")]
    public float moveThreshold = 0.15f;

    [Header("Stop Detection Timing")]
    [Tooltip("How long the magnet must stay below moveThreshold before we consider it stopped.")]
    public float stoppedMinTime = 0.08f;
    private float stoppedTimer;

    [Header("Electric Arc Parameters")]
    [Tooltip("Overall max perpendicular displacement for the arc.")]
    public float arcAmplitude = 1.5f;

    [Tooltip("Base frequency for the noise along the line.")]
    public float arcFrequency = 1.0f;

    [Tooltip("Number of noise layers (higher = more jagged detail).")]
    [Range(1, 8)]
    public int arcOctaves = 4;

    [Tooltip("Amplitude falloff per octave (0.3–0.7 is typical).")]
    [Range(0.05f, 0.95f)]
    public float arcRoughness = 0.5f;

    [Tooltip("How fast the arc animates over time in SMOOTH mode (time multiplier).")]
    public float arcSpeed = 2.0f;

    [Tooltip("Extra randomization so multiple arcs don't look identical.")]
    public int noiseSeed = 0;

    [Header("Stepped Arc Motion (Optional)")]
    [Tooltip("If ON, arc points jump to new noise positions, then hold for a random duration.")]
    public bool steppedArc = false;

    [Tooltip("Random hold duration range (seconds) per step. Each step picks a new value in this range.")]
    public Vector2 steppedHoldRange = new Vector2(0.05f, 0.15f);

    [Header("Corner Sharpening")]
    [Tooltip("Chance each interior point becomes a sharp 'kink'.")]
    [Range(0f, 1f)]
    public float cornerChance = 0.35f;

    [Tooltip("How often corner pattern refreshes (seconds).")]
    public float cornerReseedInterval = 0.12f;

    [Tooltip("Tangents scale for non-corner points (1 = smooth, lower = tighter).")]
    [Range(0.05f, 1f)]
    public float smoothTangentScale = 0.6f;

    [Tooltip("Tangents scale for corner points (0 = very sharp corner).")]
    [Range(0f, 0.5f)]
    public float cornerTangentScale = 0.0f;

    [Header("Sparks (Optional Endpoints)")]
    [Tooltip("Particles at the player end.")]
    public Transform baseSparks;

    [Tooltip("Particles at the magnet end.")]
    public Transform magnetSparks;

    [Header("Master Array Mode")]
    [Tooltip("If ON, this script does all spline math but stores it in masterArray instead of writing to a SpriteShape.")]
    public bool isMasterArray = false;

    [System.Serializable]
    public struct MasterPoint
    {
        public Vector2 position;
        public Vector2 leftTangent;
        public Vector2 rightTangent;
        public ShapeTangentMode tangentMode;
    }

    [Header("Master Array (runtime)")]
    public MasterPoint[] masterArray;

    [Header("Child of Master Array Mode")]
    [Tooltip("If ON, this instance ignores its own tether math and copies data from a master MagnetVectorRenderer that has isMasterArray = true.")]
    public bool isChildToMasterArray = false;

    [Tooltip("Reference to the master MagnetVectorRenderer (typically on the parent GameObject).")]
    public MagnetVectorRenderer masterArraySource;

    [Header("Child Copy Settings (Master -> Child)")]
    [Tooltip("Only used when isChildToMasterArray is true. Fraction [0,1] of master points to copy as a continuous segment.")]
    public Vector2 childCopyFractionRange = new Vector2(0.4f, 1.0f);

    [Tooltip("If ON, child samples a snapshot of a continuous segment and holds it for a lifetime before resampling.")]
    public bool sharpChild = false;

    [Tooltip("Lifetime range (seconds) for each sampled child snapshot, when sharpChild is ON.")]
    public Vector2 childLifetimeRange = new Vector2(0.05f, 0.15f);

    [Tooltip("Extra random delay after each lifetime before the next resample (seconds).")]
    public Vector2 childDelayRange = new Vector2(0f, 0.05f);

    [Header("Random Sparks Along Spline")]
    [Tooltip("Template particle system used for random sparks along the spline (cloned into a pool on Awake).")]
    public ParticleSystem sparks;

    [Tooltip("Range of time between each 'batch' of random sparks along the spline (seconds).")]
    public Vector2 sparksIntervalRange = new Vector2(0.05f, 0.2f);

    [Tooltip("How long each spark instance plays (seconds).")]
    public Vector2 sparksDurationRange = new Vector2(0.05f, 0.15f);

    [Tooltip("Percent of interior spline points used for sparks each interval (0–1).")]
    [Range(0f, 1f)]
    public float sparksDensity = 0.3f;

    [Tooltip("Maximum number of sparks that may be active at once (size of the pool).")]
    public int maxConcurrentSparks = 5;

    [Header("Visibility")]
    [Tooltip("If ON, this arc (and child arcs) only render while the magnet is stopped (has hit the wall).")]
    public bool onlyShowWhenStopped = false;

    [Header("State Flags")]
    public bool magnetMoving;
    public bool magnetIsStopped;

    [Header("Debug")]
    public float splineLength;
    public int desiredInteriorPoints;

    private bool lastMagnetActive;
    private Vector3 lastMagnetWorldPos;
    private bool hasLastPos;

    // cached particle systems (optional)
    private ParticleSystem basePS;
    private ParticleSystem magnetPS;

    // ----- stepped arc internals -----
    private bool lastSteppedArc;
    private bool steppedInitialized;
    private float steppedSampleTime; // time fed into noise; only changes on step
    private float nextStepTime;      // when to jump next
    private float currentHold;       // current step hold duration

    // ----- child sharp sampling internals -----
    private MasterPoint[] childCachedSegment;
    private bool childHasCachedSegment;
    private float childLifeEndTime;   // when current child segment stops being visible
    private float childDelayEndTime;  // when we are allowed to sample the next segment

    // ----- random sparks pool -----
    private ParticleSystem[] sparksPool;
    private float[] sparkEndTimes;
    private int sparksPoolSize;
    private float nextSparksTime;

    private bool SparksEnabled
        => sparks != null && maxConcurrentSparks > 0;

    void Awake()
    {
        controller = GetComponent<SpriteShapeController>();
        renderer2D = GetComponent<SpriteShapeRenderer>();
        if (controller != null)
            spline = controller.spline;

        // Only master / normal need the spawner; child copies geometry only.
        if (!isChildToMasterArray)
        {
            spawner = FindSpawnerOnSameLevel();
            if (spawner == null)
                Debug.LogWarning("MagnetVectorRenderer: No MagnetSpawnerScript found.");
        }

        if (baseSparks != null)
            basePS = baseSparks.GetComponentInChildren<ParticleSystem>(true);

        if (magnetSparks != null)
            magnetPS = magnetSparks.GetComponentInChildren<ParticleSystem>(true);

        InitRandomSparksPool();

        // INIT depending on mode
        if (!isMasterArray && !isChildToMasterArray)
        {
            if (spline != null && controller != null)
            {
                InitTwoPointLine();
            }

            if (renderer2D != null)
                renderer2D.enabled = false;
        }
        else if (isChildToMasterArray)
        {
            if (controller == null || renderer2D == null)
                Debug.LogWarning($"{name}: isChildToMasterArray is ON but no SpriteShapeController/Renderer found.");
        }
        else if (isMasterArray)
        {
            if (renderer2D != null)
                renderer2D.enabled = false;
        }

        lastSteppedArc = steppedArc;
        steppedInitialized = false;
        childHasCachedSegment = false;
        stoppedTimer = 0f;
    }

    void Update()
    {
        // ----- CHILD MODE (copies from master array) -----
        if (isChildToMasterArray)
        {
            UpdateFromMasterArray();
            return;
        }

        // ----- NORMAL / MASTER-ARRAY MODE -----

        if (spawner == null) return;

        bool magnetActive = spawner.magnetActive;

        if (!magnetActive)
        {
            if (lastMagnetActive)
            {
                if (!isMasterArray && spline != null)
                    InitTwoPointLine();

                magnetTransform = null;
                hasLastPos = false;
                stoppedTimer = 0f;
            }

            if (!isMasterArray && renderer2D != null)
                renderer2D.enabled = false;

            lastMagnetActive = false;
            magnetIsStopped = false;
            magnetMoving = false;

            // reset stepped state so next activation snaps fresh
            steppedInitialized = false;
            lastSteppedArc = steppedArc;

            if (isMasterArray)
                masterArray = null;

            UpdateSparks(null, null);
            StopAllRandomSparks();
            return;
        }

        if (magnetTransform == null)
        {
            TryCacheMagnet();
            lastMagnetActive = true;
            if (magnetTransform == null)
            {
                UpdateSparks(null, null);
                StopAllRandomSparks();
                return;
            }
        }

        DetectMovingFlag();

        Transform baseT = baseTransform != null ? baseTransform : transform;
        Vector3 baseLocal = transform.InverseTransformPoint(baseT.position);
        Vector3 endLocal  = transform.InverseTransformPoint(magnetTransform.position);

        Vector3 delta = endLocal - baseLocal;
        splineLength = delta.magnitude;

        if (splineLength <= recallResetDistance)
        {
            if (!isMasterArray && spline != null)
                InitTwoPointLine();

            hasLastPos = false;
            stoppedTimer = 0f;
            lastMagnetActive = true;
            magnetIsStopped = false;

            steppedInitialized = false;
            lastSteppedArc = steppedArc;

            if (isMasterArray)
                masterArray = null;

            UpdateSparks(null, null);
            StopAllRandomSparks();
            return;
        }

        if (splineLength < 0.0001f)
        {
            if (!isMasterArray && spline != null)
                InitTwoPointLine();

            lastMagnetActive = true;
            magnetIsStopped = true;
            magnetMoving = false;
            stoppedTimer = stoppedMinTime; // treat as fully stopped

            steppedInitialized = false;
            lastSteppedArc = steppedArc;

            bool showNow = !onlyShowWhenStopped || magnetIsStopped;

            if (!isMasterArray && renderer2D != null)
                renderer2D.enabled = showNow;

            if (showNow)
            {
                UpdateSparks(baseLocal, endLocal);
                UpdateRandomSparks();
            }
            else
            {
                UpdateSparks(null, null);
                StopAllRandomSparks();
            }

            return;
        }

        Vector3 dir = delta / splineLength;
        Vector3 perp = new Vector3(-dir.y, dir.x, 0f);

        desiredInteriorPoints = ComputeBufferedInteriorCount(splineLength, dropInterval);
        int targetCount = desiredInteriorPoints + 2;

        float arcTime = GetArcTime(); // smooth or stepped

        if (isMasterArray)
        {
            // Just compute & store into masterArray
            UpdateMasterArray(targetCount, baseLocal, endLocal, dir, perp, arcTime, splineLength);
        }
        else
        {
            if (spline == null || controller == null)
                return; // safety

            AdjustPointCount(targetCount);

            spline.SetPosition(0, baseLocal);

            for (int i = 1; i <= desiredInteriorPoints; i++)
            {
                float tNorm = (float)i / (desiredInteriorPoints + 1);

                Vector3 p = baseLocal + dir * (tNorm * splineLength);

                float off = ComputeArcOffset(tNorm, arcTime);
                p += perp * off;

                spline.SetPosition(i, p);
            }

            spline.SetPosition(targetCount - 1, endLocal);

            ApplyMixedTangents(dir, splineLength, arcTime);

            controller.RefreshSpriteShape();
        }

        lastMagnetActive = true;

        bool show = !onlyShowWhenStopped || magnetIsStopped;

        if (!isMasterArray && renderer2D != null)
            renderer2D.enabled = show;

        if (show)
        {
            UpdateSparks(baseLocal, endLocal);
            UpdateRandomSparks();
        }
        else
        {
            UpdateSparks(null, null);
            StopAllRandomSparks();
        }

        lastSteppedArc = steppedArc;
    }

    // ---------- CHILD: COPY FROM MASTER ARRAY (with partial segments + sharp) ----------

    private void UpdateFromMasterArray()
    {
        if (masterArraySource == null)
        {
            if (renderer2D != null)
                renderer2D.enabled = false;

            UpdateSparks(null, null);
            StopAllRandomSparks();
            return;
        }

        var arr = masterArraySource.masterArray;
        if (arr == null || arr.Length < 2)
        {
            if (renderer2D != null)
                renderer2D.enabled = false;

            UpdateSparks(null, null);
            StopAllRandomSparks();
            return;
        }

        // Mirror state flags from master (for visibility & sparks behavior).
        magnetMoving = masterArraySource.magnetMoving;
        magnetIsStopped = masterArraySource.magnetIsStopped;

        // Make sure we have a spline to draw on.
        if (controller == null || spline == null)
        {
            controller = GetComponent<SpriteShapeController>();
            renderer2D = GetComponent<SpriteShapeRenderer>();
            if (controller != null)
                spline = controller.spline;
        }

        if (controller == null || spline == null)
        {
            Debug.LogWarning($"{name}: isChildToMasterArray is ON but no SpriteShapeController/Spline is available.");
            UpdateSparks(null, null);
            StopAllRandomSparks();
            return;
        }

        float now = Time.time;
        bool allowShow = !onlyShowWhenStopped || magnetIsStopped;

        if (!sharpChild)
        {
            // Continuous copy: pick a fresh random segment each frame.
            if (!allowShow)
            {
                if (renderer2D != null)
                    renderer2D.enabled = false;

                UpdateSparks(null, null);
                StopAllRandomSparks();
                return;
            }

            if (renderer2D != null)
                renderer2D.enabled = true;

            int start, segCount;
            SelectRandomSegmentFromMaster(arr, out start, out segCount);
            CopySegmentToSpline(arr, start, segCount);

            // Random sparks based on the child spline geometry
            UpdateRandomSparks();
        }
        else
        {
            // Sharp: sample once, hold for lifetime, then hide until delay end, then resample.
            if (!childHasCachedSegment || now >= childDelayEndTime)
            {
                int start, segCount;
                SelectRandomSegmentFromMaster(arr, out start, out segCount);
                CacheSegment(arr, start, segCount);

                float minL = Mathf.Max(0.0001f, childLifetimeRange.x);
                float maxL = Mathf.Max(minL, childLifetimeRange.y);
                float life = Random.Range(minL, maxL);

                float minD = Mathf.Max(0f, childDelayRange.x);
                float maxD = Mathf.Max(minD, childDelayRange.y);
                float delayTotal = Random.Range(minD, maxD);

                // Ensure total delay is at least the visible lifetime
                if (delayTotal < life)
                    delayTotal = life;

                childLifeEndTime = now + life;        // visible until here
                childDelayEndTime = now + delayTotal; // next resample at this time
            }

            bool visible = childHasCachedSegment &&
                           now < childLifeEndTime &&
                           allowShow;

            if (visible)
            {
                if (renderer2D != null)
                    renderer2D.enabled = true;

                CopyCachedSegmentToSpline();

                // Random sparks only while visible
                UpdateRandomSparks();
            }
            else
            {
                // Hidden during the remainder of the delay window or while magnet moving.
                if (renderer2D != null)
                    renderer2D.enabled = false;

                UpdateSparks(null, null);
                StopAllRandomSparks();
            }
        }
    }

    private void SelectRandomSegmentFromMaster(MasterPoint[] arr, out int startIndex, out int segmentCount)
    {
        int total = arr.Length;

        float minFrac = Mathf.Clamp01(childCopyFractionRange.x);
        float maxFrac = Mathf.Clamp01(childCopyFractionRange.y);
        if (maxFrac < minFrac)
        {
            float tmp = minFrac;
            minFrac = maxFrac;
            maxFrac = tmp;
        }

        // pick a fraction, then derive a segment length
        float frac = Random.Range(minFrac, maxFrac);
        segmentCount = Mathf.RoundToInt(frac * total);
        segmentCount = Mathf.Clamp(segmentCount, 2, total);

        int maxStart = total - segmentCount;
        startIndex = Random.Range(0, maxStart + 1);
    }

    private void CopySegmentToSpline(MasterPoint[] arr, int startIndex, int segmentCount)
    {
        if (segmentCount < 2) return;

        AdjustPointCount(segmentCount);

        for (int i = 0; i < segmentCount; i++)
        {
            var mp = arr[startIndex + i];
            Vector3 p = new Vector3(mp.position.x, mp.position.y, 0f);

            spline.SetPosition(i, p);
            spline.SetTangentMode(i, mp.tangentMode);
            spline.SetLeftTangent(i, new Vector3(mp.leftTangent.x, mp.leftTangent.y, 0f));
            spline.SetRightTangent(i, new Vector3(mp.rightTangent.x, mp.rightTangent.y, 0f));
        }

        controller.RefreshSpriteShape();

        Vector3 baseLocal = new Vector3(arr[startIndex].position.x, arr[startIndex].position.y, 0f);
        Vector3 endLocal  = new Vector3(arr[startIndex + segmentCount - 1].position.x,
                                        arr[startIndex + segmentCount - 1].position.y, 0f);
        UpdateSparks(baseLocal, endLocal);
    }

    private void CacheSegment(MasterPoint[] arr, int startIndex, int segmentCount)
    {
        if (segmentCount < 2)
        {
            childCachedSegment = null;
            childHasCachedSegment = false;
            return;
        }

        if (childCachedSegment == null || childCachedSegment.Length != segmentCount)
            childCachedSegment = new MasterPoint[segmentCount];

        for (int i = 0; i < segmentCount; i++)
        {
            childCachedSegment[i] = arr[startIndex + i];
        }

        childHasCachedSegment = true;
    }

    private void CopyCachedSegmentToSpline()
    {
        if (!childHasCachedSegment || childCachedSegment == null || childCachedSegment.Length < 2)
        {
            UpdateSparks(null, null);
            return;
        }

        int count = childCachedSegment.Length;
        AdjustPointCount(count);

        for (int i = 0; i < count; i++)
        {
            var mp = childCachedSegment[i];
            Vector3 p = new Vector3(mp.position.x, mp.position.y, 0f);

            spline.SetPosition(i, p);
            spline.SetTangentMode(i, mp.tangentMode);
            spline.SetLeftTangent(i, new Vector3(mp.leftTangent.x, mp.leftTangent.y, 0f));
            spline.SetRightTangent(i, new Vector3(mp.rightTangent.x, mp.rightTangent.y, 0f));
        }

        controller.RefreshSpriteShape();

        Vector3 baseLocal = new Vector3(childCachedSegment[0].position.x, childCachedSegment[0].position.y, 0f);
        Vector3 endLocal  = new Vector3(childCachedSegment[count - 1].position.x,
                                        childCachedSegment[count - 1].position.y, 0f);
        UpdateSparks(baseLocal, endLocal);
    }

    // ---------- stepped timing ----------

    private float GetArcTime()
    {
        if (!steppedArc)
        {
            steppedInitialized = false;
            return Time.time;
        }

        if (!steppedInitialized || !lastSteppedArc)
        {
            steppedInitialized = true;
            SteppedPickNewHoldAndAdvance();
        }

        if (Time.time >= nextStepTime)
        {
            SteppedPickNewHoldAndAdvance();
        }

        return steppedSampleTime;
    }

    private void SteppedPickNewHoldAndAdvance()
    {
        float minH = Mathf.Max(0.0001f, steppedHoldRange.x);
        float maxH = Mathf.Max(minH, steppedHoldRange.y);

        currentHold = Random.Range(minH, maxH);
        nextStepTime = Time.time + currentHold;

        steppedSampleTime = Time.time;
    }

    // ---------- MASTER ARRAY VERSION OF THE SPLINE MATH ----------

    private void EnsureMasterArraySize(int count)
    {
        if (masterArray == null || masterArray.Length != count)
            masterArray = new MasterPoint[count];
    }

    private void UpdateMasterArray(int targetCount,
                                   Vector3 baseLocal,
                                   Vector3 endLocal,
                                   Vector3 dir,
                                   Vector3 perp,
                                   float time,
                                   float length)
    {
        EnsureMasterArraySize(targetCount);

        masterArray[0].position = new Vector2(baseLocal.x, baseLocal.y);

        for (int i = 1; i <= desiredInteriorPoints; i++)
        {
            float tNorm = (float)i / (desiredInteriorPoints + 1);

            Vector3 p = baseLocal + dir * (tNorm * length);

            float off = ComputeArcOffset(tNorm, time);
            p += perp * off;

            masterArray[i].position = new Vector2(p.x, p.y);
        }

        masterArray[targetCount - 1].position = new Vector2(endLocal.x, endLocal.y);

        int count = targetCount;
        if (count < 2) return;

        float segLen = length / (count - 1);
        float baseHandle = Mathf.Min(dropInterval, segLen) * 0.5f;
        int reseedStep = Mathf.FloorToInt(time / Mathf.Max(0.0001f, cornerReseedInterval));

        for (int i = 0; i < count; i++)
        {
            if (i == 0 || i == count - 1)
            {
                Vector3 tan3 = dir * (baseHandle * smoothTangentScale);

                masterArray[i].tangentMode = ShapeTangentMode.Continuous;

                if (i == 0)
                {
                    masterArray[i].leftTangent  = Vector2.zero;
                    masterArray[i].rightTangent = new Vector2(tan3.x, tan3.y);
                }
                else
                {
                    masterArray[i].leftTangent  = new Vector2(-tan3.x, -tan3.y);
                    masterArray[i].rightTangent = Vector2.zero;
                }

                continue;
            }

            float r = Hash01(i, reseedStep, noiseSeed);
            bool isCorner = r < cornerChance;

            if (isCorner)
            {
                masterArray[i].tangentMode = ShapeTangentMode.Linear;
                Vector3 tan3 = dir * (baseHandle * cornerTangentScale);
                masterArray[i].leftTangent  = new Vector2(-tan3.x, -tan3.y);
                masterArray[i].rightTangent = new Vector2(tan3.x, tan3.y);
            }
            else
            {
                masterArray[i].tangentMode = ShapeTangentMode.Continuous;
                Vector3 tan3 = dir * (baseHandle * smoothTangentScale);
                masterArray[i].leftTangent  = new Vector2(-tan3.x, -tan3.y);
                masterArray[i].rightTangent = new Vector2(tan3.x, tan3.y);
            }
        }
    }

    // ---------- Sparks at endpoints ----------

    private void UpdateSparks(Vector3? baseLocalOpt, Vector3? endLocalOpt)
    {
        if (!baseLocalOpt.HasValue || !endLocalOpt.HasValue)
        {
            StopPS(basePS);
            StopPS(magnetPS);
            return;
        }

        Vector3 baseLocal = baseLocalOpt.Value;
        Vector3 endLocal  = endLocalOpt.Value;

        Vector3 baseWorld = transform.TransformPoint(baseLocal);
        Vector3 endWorld  = transform.TransformPoint(endLocal);

        if (baseSparks != null)
            baseSparks.position = baseWorld;

        if (magnetSparks != null)
            magnetSparks.position = endWorld;

        Vector3 dirBaseToMag = (endWorld - baseWorld);
        if (dirBaseToMag.sqrMagnitude > 0.000001f)
        {
            Vector3 n = dirBaseToMag.normalized;
            Quaternion flipX = Quaternion.Euler(180f, 0f, 0f);

            if (baseSparks != null)
                baseSparks.rotation = Quaternion.LookRotation(-n, Vector3.forward) * flipX;

            if (magnetSparks != null)
                magnetSparks.rotation = Quaternion.LookRotation(n, Vector3.forward) * flipX;
        }

        if (magnetIsStopped)
        {
            PlayPS(basePS);
            PlayPS(magnetPS);
        }
        else
        {
            StopPS(basePS);
            StopPS(magnetPS);
        }
    }

    private void PlayPS(ParticleSystem ps)
    {
        if (ps == null) return;
        if (!ps.isPlaying) ps.Play(true);
    }

    private void StopPS(ParticleSystem ps)
    {
        if (ps == null) return;
        if (ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    // ---------- Random Sparks Along Spline ----------

    private void InitRandomSparksPool()
    {
        if (!SparksEnabled)
            return;

        sparksPoolSize = Mathf.Max(1, maxConcurrentSparks);
        sparksPool = new ParticleSystem[sparksPoolSize];
        sparkEndTimes = new float[sparksPoolSize];

        // Use the actual assigned particle system as pool[0]
        sparksPool[0] = sparks;
        sparkEndTimes[0] = 0f;

        // Make sure it's parented to this arc (optional, but usually what we want)
        if (sparks.transform.parent != transform)
        {
            sparks.transform.SetParent(transform, true);
        }

        // Ensure it's initially stopped
        sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Create clones for the rest
        for (int i = 1; i < sparksPoolSize; i++)
        {
            ParticleSystem clone = Instantiate(sparks, transform);
            clone.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            sparksPool[i] = clone;
            sparkEndTimes[i] = 0f;
        }

        nextSparksTime = Time.time + 0.1f;
    }

    private void StopAllRandomSparks()
    {
        if (sparksPool == null) return;

        for (int i = 0; i < sparksPool.Length; i++)
        {
            StopPS(sparksPool[i]);
            sparkEndTimes[i] = 0f;
        }
    }

    private void UpdateRandomSparks()
    {
        if (!SparksEnabled || spline == null || sparksPool == null)
            return;

        int pointCount = spline.GetPointCount();
        // need at least p0 and pLast plus one interior point
        if (pointCount <= 2)
            return;

        float now = Time.time;

        // Clean up expired instances
        for (int i = 0; i < sparksPoolSize; i++)
        {
            var ps = sparksPool[i];
            if (ps == null) continue;

            if (ps.isPlaying && now > sparkEndTimes[i])
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        // Not time to spawn new sparks yet
        if (now < nextSparksTime)
            return;

        // Schedule next interval
        float minInt = Mathf.Max(0.0001f, sparksIntervalRange.x);
        float maxInt = Mathf.Max(minInt, sparksIntervalRange.y);
        float interval = Random.Range(minInt, maxInt);
        nextSparksTime = now + interval;

        int interiorCount = pointCount - 2;
        if (interiorCount <= 0)
            return;

        // How many interior points to use this batch
        int desiredCount = Mathf.RoundToInt(sparksDensity * interiorCount);
        if (desiredCount <= 0)
            return;

        desiredCount = Mathf.Min(desiredCount, sparksPoolSize);

        // Choose unique random indices in [1, pointCount-2]
        List<int> chosenIndices = new List<int>(desiredCount);
        int safety = 0;
        while (chosenIndices.Count < desiredCount && safety < 1000)
        {
            int idx = Random.Range(1, pointCount - 1);
            if (!chosenIndices.Contains(idx))
                chosenIndices.Add(idx);
            safety++;
        }

        float minDur = Mathf.Max(0.0001f, sparksDurationRange.x);
        float maxDur = Mathf.Max(minDur, sparksDurationRange.y);

        foreach (int idx in chosenIndices)
        {
            int poolIndex = GetFreeSparkFromPool();
            if (poolIndex < 0)
                break;

            var ps = sparksPool[poolIndex];
            if (ps == null) continue;

            Vector3 localP = spline.GetPosition(idx);
            Vector3 worldP = transform.TransformPoint(localP);

            ps.transform.position = worldP;

            float dur = Random.Range(minDur, maxDur);
            sparkEndTimes[poolIndex] = now + dur;

            if (!ps.isPlaying)
                ps.Play(true);
        }
    }

    private int GetFreeSparkFromPool()
    {
        if (sparksPool == null) return -1;

        float now = Time.time;

        for (int i = 0; i < sparksPoolSize; i++)
        {
            var ps = sparksPool[i];
            if (ps == null) continue;

            // free if not playing or already past its end time
            if (!ps.isPlaying || now > sparkEndTimes[i])
                return i;
        }

        return -1;
    }

    // ---------- Arc noise ----------

    private float ComputeArcOffset(float t, float time)
    {
        float amp = arcAmplitude;
        float freq = arcFrequency;
        float sum = 0f;

        float seedX = noiseSeed * 17.13f;
        float seedY = noiseSeed * 3.71f;

        float speedMul = steppedArc ? 1f : arcSpeed;

        for (int o = 0; o < arcOctaves; o++)
        {
            float n = Mathf.PerlinNoise(
                seedX + t * freq + time * speedMul,
                seedY + o * 10.0f
            );

            n = (n - 0.5f) * 2f; // -1..1
            sum += n * amp;

            amp *= arcRoughness;
            freq *= 2f;
        }

        return sum;
    }

    // ---------- Tangents on real spline ----------

    private void ApplyMixedTangents(Vector3 dir, float length, float time)
    {
        if (spline == null) return;

        int count = spline.GetPointCount();
        if (count < 2) return;

        float segLen = length / (count - 1);
        float baseHandle = Mathf.Min(dropInterval, segLen) * 0.5f;

        int reseedStep = Mathf.FloorToInt(time / Mathf.Max(0.0001f, cornerReseedInterval));

        for (int i = 0; i < count; i++)
        {
            if (i == 0 || i == count - 1)
            {
                spline.SetTangentMode(i, ShapeTangentMode.Continuous);
                Vector3 tan = dir * (baseHandle * smoothTangentScale);

                if (i == 0)
                {
                    spline.SetLeftTangent(i, Vector3.zero);
                    spline.SetRightTangent(i, tan);
                }
                else
                {
                    spline.SetLeftTangent(i, -tan);
                    spline.SetRightTangent(i, Vector3.zero);
                }
                continue;
            }

            float r = Hash01(i, reseedStep, noiseSeed);
            bool isCorner = r < cornerChance;

            if (isCorner)
            {
                spline.SetTangentMode(i, ShapeTangentMode.Linear);
                Vector3 tan = dir * (baseHandle * cornerTangentScale);
                spline.SetLeftTangent(i, -tan);
                spline.SetRightTangent(i, tan);
            }
            else
            {
                spline.SetTangentMode(i, ShapeTangentMode.Continuous);
                Vector3 tan = dir * (baseHandle * smoothTangentScale);
                spline.SetLeftTangent(i, -tan);
                spline.SetRightTangent(i, tan);
            }
        }
    }

    private float Hash01(int i, int step, int seed)
    {
        uint h = (uint)(i * 374761393 + step * 668265263 + seed * 2246822519u);
        h = (h ^ (h >> 13)) * 1274126177u;
        h ^= (h >> 16);
        return (h & 0xFFFFFF) / (float)0x1000000;
    }

    // ----- core spline count logic -----

    private int ComputeBufferedInteriorCount(float length, float x)
    {
        if (x <= 0.0001f) return 0;

        int baseCount = Mathf.FloorToInt(length / x);
        float remainder = length % x;

        if (remainder > x * 0.5f)
            baseCount += 1;

        return Mathf.Max(0, baseCount);
    }

    // ----- debounced movement / stop detection -----

    private void DetectMovingFlag()
    {
        if (magnetTransform == null)
        {
            magnetMoving = false;
            magnetIsStopped = false;
            hasLastPos = false;
            stoppedTimer = 0f;
            return;
        }

        Vector3 current = magnetTransform.position;

        // First frame after we get a magnet: treat as MOVING, not stopped.
        if (!hasLastPos)
        {
            lastMagnetWorldPos = current;
            hasLastPos = true;

            magnetMoving = true;
            magnetIsStopped = false;
            stoppedTimer = 0f;
            return;
        }

        float movedDist = Vector3.Distance(current, lastMagnetWorldPos);
        lastMagnetWorldPos = current;

        bool aboveThreshold = movedDist > moveThreshold;

        if (aboveThreshold)
        {
            // Definitely moving: reset "stopped" timer
            stoppedTimer = 0f;
            magnetMoving = true;
            magnetIsStopped = false;
        }
        else
        {
            // Below movement threshold – maybe stopped, maybe just a quiet frame
            stoppedTimer += Time.deltaTime;

            if (stoppedTimer >= stoppedMinTime)
            {
                // It's been quiet long enough: call it stopped
                magnetMoving = false;
                magnetIsStopped = true;
            }
            else
            {
                // Not enough quiet time yet – still treat as moving
                magnetMoving = true;
                magnetIsStopped = false;
            }
        }
    }

    private void AdjustPointCount(int targetCount)
    {
        if (spline == null) return;

        int current = spline.GetPointCount();

        while (current < targetCount)
        {
            spline.InsertPointAt(current, spline.GetPosition(Mathf.Max(0, current - 1)));
            current++;
        }

        while (current > targetCount)
        {
            spline.RemovePointAt(current - 1);
            current--;
        }
    }

    private void InitTwoPointLine()
    {
        if (spline == null || controller == null) return;

        for (int i = spline.GetPointCount() - 1; i >= 0; i--)
            spline.RemovePointAt(i);

        spline.InsertPointAt(0, Vector3.zero);
        spline.InsertPointAt(1, Vector3.right);

        spline.SetTangentMode(0, ShapeTangentMode.Continuous);
        spline.SetTangentMode(1, ShapeTangentMode.Continuous);
        spline.SetLeftTangent(0, Vector3.zero);
        spline.SetRightTangent(0, Vector3.right * dropInterval * 0.25f);
        spline.SetLeftTangent(1, Vector3.left * dropInterval * 0.25f);
        spline.SetRightTangent(1, Vector3.zero);

        controller.RefreshSpriteShape();

        desiredInteriorPoints = 0;
        magnetIsStopped = false;
        magnetMoving = false;
        stoppedTimer = 0f;
        hasLastPos = false;
    }

    // ----- magnet / spawner lookup -----

    private void TryCacheMagnet()
    {
        GameObject magnetObj = GameObject.Find(magnetCloneName);
        if (magnetObj == null) return;

        int magnetLayer = LayerMask.NameToLayer(magnetLayerName);
        if (magnetLayer != -1 && magnetObj.layer != magnetLayer) return;

        magnetTransform = magnetObj.transform;
    }

    private MagnetSpawnerScript FindSpawnerOnSameLevel()
    {
        var s = GetComponent<MagnetSpawnerScript>();
        if (s != null) return s;

        Transform parent = transform.parent;
        if (parent != null)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                s = child.GetComponent<MagnetSpawnerScript>();
                if (s != null) return s;
            }
        }

        return Object.FindFirstObjectByType<MagnetSpawnerScript>();
    }
}
