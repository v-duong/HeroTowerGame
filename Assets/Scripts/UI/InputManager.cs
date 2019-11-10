using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public static Camera mainCamera;
    public static InputManager Instance { get; private set; }
    public bool IsSummoningMode = false;
    public HeroActor selectedHero = null;
    public Action onSummonCallback = null;
    public bool IsMovementMode = false;
    public float zoomRatio = 1.0f;

    private float maxNegativeX, maxPositiveX;
    private float maxNegativeY, maxPositiveY;

    private bool isDragging;
    private static float dragspeed = 0.35f;
    private bool isZooming;     // Is the camera zooming?
    private static float speed = 3.0f;

    private Queue<TargetingCircle> targetingCirclesAvailable = new Queue<TargetingCircle>();
    private List<TargetingCircle> targetingCirclesInUse = new List<TargetingCircle>();

    public void SetSummoning(HeroActor actor, Action summonCallback)
    {
        IsSummoningMode = true;
        selectedHero = actor;
        onSummonCallback = summonCallback;
    }

    public void ResetManager()
    {
        IsMovementMode = false;
        IsSummoningMode = false;
        selectedHero = null;
        onSummonCallback = null;

        targetingCirclesAvailable.Clear();
        targetingCirclesInUse.Clear();

        for (int i = 0; i < 4; i++)
        {
            TargetingCircle circle = Instantiate(ResourceManager.Instance.TargetingCirclePrefab);
            circle.gameObject.SetActive(false);
            targetingCirclesAvailable.Enqueue(circle);
        }
    }

    public void SetCameraBounds()
    {
        Bounds bounds = StageManager.Instance.stageBounds;
        float ratio = (float)Screen.width / Screen.height;
        maxNegativeX = bounds.center.x - bounds.extents.x + mainCamera.orthographicSize * ratio;
        maxPositiveX = bounds.center.x + bounds.extents.x - mainCamera.orthographicSize * ratio;
        maxNegativeY = bounds.center.y - bounds.extents.y + mainCamera.orthographicSize / 2;
        maxPositiveY = bounds.center.y + bounds.extents.y - mainCamera.orthographicSize / 2;
        if (maxNegativeY > maxPositiveY)
        {
            maxNegativeY = -1;
            maxPositiveY = 1;
        }
        if (maxNegativeX > maxPositiveX)
        {
            maxNegativeX = 0;
            maxPositiveX = 0;
        }
        //Debug.Log(maxNegativeX + " " + maxPositiveX + " " + maxNegativeY + " " + maxPositiveY);
    }

    public void ClampCameraPosition()
    {
        Vector3 changedPos = mainCamera.transform.position;
        changedPos.x = Mathf.Clamp(changedPos.x, maxNegativeX, maxPositiveX);
        changedPos.y = Mathf.Clamp(changedPos.y, maxNegativeY, maxPositiveY);
        mainCamera.transform.position = changedPos;
    }

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        mainCamera = Camera.main;
        if (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.WindowsPlayer)
        {
            dragspeed = 0.01f;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (!GameManager.Instance.isInBattle)
            return;

        if (IsSummoningMode)
        {
            if (Input.GetMouseButtonDown(0) && Input.touchCount < 2)
            {
                Vector3 spawnLocation = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, LayerMask.GetMask("Enemy", "Hero", "Obstacles", "Path"));

                if (hit.collider != null)
                {
                    if (hit.collider.gameObject.layer >= 11)
                        return;
                }

                spawnLocation = Helpers.ReturnTilePosition(StageManager.Instance.HighlightMap.tilemap, spawnLocation, -3);
                selectedHero.transform.position = spawnLocation;
                selectedHero.gameObject.SetActive(true);
                selectedHero.EnableHealthBar();
                selectedHero.ClearMovement();
                IsSummoningMode = false;
                onSummonCallback?.Invoke();
            }
        }
        else if (IsMovementMode && !isDragging)
        {
            Vector3 moveLocation = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            moveLocation.z = -3;

            if (Input.GetMouseButtonDown(0) && Input.touchCount < 2)
            {
                if (!EventSystem.current.IsPointerOverGameObject()
                    || EventSystem.current.currentSelectedGameObject == null)
                    selectedHero.StartMovement(moveLocation);
                IsMovementMode = false;
                selectedHero = null;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnMouseDownHandler();
                return;
            }

            if (Input.mouseScrollDelta.y != 0
                || (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved))
            {
                float scrollValue;

                if (Input.touchCount == 2)
                {
                    var curDist = Input.GetTouch(0).position - Input.GetTouch(1).position;
                    var prevDist = Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition - (Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition);
                    scrollValue = (curDist.magnitude - prevDist.magnitude) / 25f;
                }
                else
                {
                    scrollValue = Input.mouseScrollDelta.y;
                }

                mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - scrollValue * 0.5f, 4f, 13f);
                zoomRatio = mainCamera.orthographicSize / 7.0f;
                SetCameraBounds();
                ClampCameraPosition();
            }

            if (!Input.GetMouseButton(0)) isDragging = false;

            if (isDragging && Input.touchCount < 2)
            {
                Vector3 move;
                float speedMultiplier = zoomRatio;

                if (Input.touchCount == 1)
                {
                    move = new Vector3(Input.touches[0].deltaPosition.x * dragspeed * speedMultiplier, Input.touches[0].deltaPosition.y * dragspeed * speedMultiplier, 0);
                }
                else
                {
                    move = new Vector3(Input.GetAxis("Mouse X") * dragspeed * speedMultiplier, Input.GetAxis("Mouse Y") * dragspeed * speedMultiplier, 0);
                }

                mainCamera.transform.Translate(-move, Space.Self);
                ClampCameraPosition();
            }
        }
    }

    private void OnMouseDownHandler()
    {
        LayerMask mask = LayerMask.GetMask("Hero", "Enemy");
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 50, mask);

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (hit.collider != null)
        {
            if (hit.collider.gameObject.layer == 13)
            {
                HeroActor hero = hit.collider.gameObject.GetComponent<HeroActor>();
                if (hero != null)
                {
                    OnTargetSelect(hero);
                }
            }
            else if (hit.collider.gameObject.layer == 12)
            {
                EnemyActor enemy = hit.collider.gameObject.GetComponent<EnemyActor>();
                if (enemy != null)
                {
                    OnTargetSelect(enemy);
                }
            }
            //return;
        }
        else
        {
            OnTargetSelect(null);
        }
        if (!EventSystem.current.IsPointerOverGameObject())
            isDragging = true;
    }

    public void OnTargetSelect(Actor actor)
    {
        UIManager.Instance.BattleCharInfoPanel.SetTarget(actor);

        foreach (TargetingCircle circle in targetingCirclesInUse)
        {
            circle.transform.SetParent(null);
            circle.gameObject.SetActive(false);
            targetingCirclesAvailable.Enqueue(circle);
        }

        targetingCirclesInUse.Clear();

        if (actor != null)
        {
            int i = 0;
            foreach (ActorAbility ability in actor.GetInstancedAbilities())
            {
                TargetingCircle circle = targetingCirclesAvailable.Dequeue();
                targetingCirclesInUse.Add(circle);
                circle.transform.localScale = new Vector3(ability.TargetRange * 2, ability.TargetRange * 2, 1);
                circle.gameObject.SetActive(true);
                circle.transform.SetParent(actor.transform, false);
                circle.transform.localPosition = Vector3.zero;
                circle.transform.eulerAngles = new Vector3(0, 0, i * 12);

                switch (i)
                {
                    case 0:
                        circle.SetColor(new Color(0.7f, 1f, 1f));
                        break;

                    case 1:
                        circle.SetColor(new Color(1f, 0.7f, 1f));
                        break;

                    case 2:
                        circle.SetColor(new Color(1f, 1f, 0.7f));
                        break;

                    case 3:
                        circle.SetColor(new Color(1f, 1f, 1f));
                        break;

                    default:
                        break;
                }

                i++;
            }
        }
    }
}