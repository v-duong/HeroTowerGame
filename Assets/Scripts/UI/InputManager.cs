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
    public int pointerId = 0;

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
        SetTileHighlight(true);
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
        maxNegativeY = bounds.center.y - bounds.extents.y + mainCamera.orthographicSize / 1.4f;
        maxPositiveY = bounds.center.y + bounds.extents.y - mainCamera.orthographicSize / 1.4f;
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
            pointerId = 0;
        }
        else
        {
            pointerId = -1;
        }
    }

    public void SetTileHighlight(bool active)
    {
        StageManager.Instance.HighlightMap.gameObject.SetActive(active);
    }

    public bool IsLocationTargetable(Vector3 position)
    {
        UnityEngine.Tilemaps.Tilemap highlightTilemap = StageManager.Instance.HighlightMap.tilemap;
        if (highlightTilemap.GetTile(Helpers.ReturnTilePosition_Int(highlightTilemap, position)) == null)
            return false;

        var correctedPosition = Helpers.ReturnTilePosition(highlightTilemap, position, -3);

        if (StageManager.Instance.BattleManager.activeHeroes.FindAll(x => Vector2.Distance(x.transform.position, correctedPosition) < 0.2f).Count > 0)
            return false;

        return true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!GameManager.Instance.isInBattle && GameManager.Instance.isInMainMenu)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                UIManager.Instance.CloseCurrentWindow();
            }

            if (UIManager.Instance.ArchetypeCanvas != null && (UIManager.Instance.currentWindow == UIManager.Instance.ArchetypeUITreeWindow.gameObject))
            {
                if (Input.mouseScrollDelta.y != 0
                || (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved))
                {
                    UIManager.Instance.ArchetypeUITreeWindow.ScrollView.enabled = false;

                    float scrollValue;

                    if (Input.touchCount == 2)
                    {
                        var curDist = Input.GetTouch(0).position - Input.GetTouch(1).position;
                        var prevDist = Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition - (Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition);
                        scrollValue = (curDist.magnitude - prevDist.magnitude) / 500f;
                    }
                    else
                    {
                        scrollValue = Input.mouseScrollDelta.y / 9;
                    }

                    RectTransform contentRect = UIManager.Instance.ArchetypeUITreeWindow.ScrollView.content;
                    float scaleValue = Mathf.Clamp(contentRect.transform.localScale.x + scrollValue, 0.4f, 3f);
                    float scaleMultiplier = scaleValue / contentRect.transform.localScale.x;
                    contentRect.transform.localScale = new Vector2(scaleValue, scaleValue);
                    contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x * scaleMultiplier, contentRect.anchoredPosition.y * scaleMultiplier);
                }
                else
                {
                    UIManager.Instance.ArchetypeUITreeWindow.ScrollView.enabled = true;
                }
            } 

            return;
        }
        else if (GameManager.Instance.isInBattle)
        {
            BattleInputUpdate();
        }
    }

    private void BattleInputUpdate()
    {
        if (IsSummoningMode)
        {
            IsMovementMode = false;
            if (Input.GetMouseButtonDown(0) && Input.touchCount < 2)
            {
                if (EventSystem.current.IsPointerOverGameObject(pointerId))
                    return;

                Vector3 spawnLocation = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                spawnLocation.z = -3;
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, LayerMask.GetMask("Enemy", "Hero", "Obstacles", "Paths"));

                if (hit.collider != null)
                {
                    if (hit.collider.gameObject.layer >= 11)
                        return;
                }

                if (!IsLocationTargetable(spawnLocation))
                    return;

                spawnLocation = Helpers.ReturnTilePosition(StageManager.Instance.HighlightMap.tilemap, spawnLocation, -3);

                selectedHero.transform.position = spawnLocation;
                selectedHero.SummonHero();
                StageManager.Instance.BattleManager.activeHeroes.Add(selectedHero);
                IsSummoningMode = false;
                onSummonCallback?.Invoke();
            }
        }
        else if (IsMovementMode && !isDragging)
        {
            IsSummoningMode = false;
            Vector3 moveLocation = Helpers.ReturnTilePosition(StageManager.Instance.HighlightMap.tilemap, mainCamera.ScreenToWorldPoint(Input.mousePosition), -3);

            if (Input.GetMouseButtonDown(0) && Input.touchCount < 2)
            {
                if (!IsLocationTargetable(moveLocation))
                    return;

                if (!EventSystem.current.IsPointerOverGameObject(pointerId)
                    || EventSystem.current.currentSelectedGameObject == null)
                {
                    selectedHero.StartMovement(moveLocation);
                }
                SetTileHighlight(false);
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

        if (EventSystem.current.IsPointerOverGameObject(pointerId))
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
        if (!EventSystem.current.IsPointerOverGameObject(pointerId))
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
                if (ability.abilityBase.abilityType == AbilityType.SELF_BUFF)
                    continue;
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