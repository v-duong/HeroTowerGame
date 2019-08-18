using System;
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
    private Bounds currentBounds;

    private bool isDragging;
    private static float dragspeed = 0.35f;
    private bool isZooming;     // Is the camera zooming?
    private static float speed = 3.0f;

    public void SetSummoning(HeroActor actor, Action summonCallback)
    {
        IsSummoningMode = true;
        selectedHero = actor;
        onSummonCallback = summonCallback;
    }

    public void SetCameraBounds(Bounds bounds)
    {
        currentBounds = bounds;
        float ratio = (float)Screen.width / Screen.height;
        maxNegativeX = bounds.center.x - bounds.extents.x + mainCamera.orthographicSize * ratio;
        maxPositiveX = bounds.center.x + bounds.extents.x - mainCamera.orthographicSize * ratio;
        maxNegativeY = bounds.center.y - bounds.extents.y + mainCamera.orthographicSize;
        maxPositiveY = bounds.center.y + bounds.extents.y - mainCamera.orthographicSize;
        if (maxNegativeY > maxPositiveY)
        {
            maxNegativeY = 0;
            maxPositiveY = 0;
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
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!GameManager.Instance.isInBattle)
            return;

        if (IsSummoningMode)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector3 spawnLocation = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                if (hit.collider != null)
                {
                    if (hit.collider.gameObject.layer >= 11)
                        return;
                    else
                    {
                    }
                }
                spawnLocation = Helpers.ReturnTilePosition(StageManager.Instance.HighlightMap.tilemap, spawnLocation, -3);
                selectedHero.transform.position = spawnLocation;
                selectedHero.gameObject.SetActive(true);
                IsSummoningMode = false;
                selectedHero = null;
                onSummonCallback?.Invoke();
            }
            /*
            if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.Translate(new Vector3(speed * Time.deltaTime, 0, 0));
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                transform.Translate(new Vector3(-speed * Time.deltaTime, 0, 0));
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                transform.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                transform.Translate(new Vector3(0, speed * Time.deltaTime, 0));
            }
            */
        }
        else if (IsMovementMode)
        {
            Vector3 moveLocation = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            moveLocation.z = -3;

            if (Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                    selectedHero.StartMovement(moveLocation);
                IsMovementMode = false;
                selectedHero = null;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                LayerMask mask = LayerMask.GetMask("Hero", "Enemy");
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 50, mask);

                if (hit.collider != null)
                {
                    if (hit.collider.gameObject.layer == 13)
                    {
                        HeroActor hero = hit.collider.gameObject.GetComponent<HeroActor>();
                        if (hero != null)
                        {
                            selectedHero = hero;
                            IsMovementMode = true;
                            UIManager.Instance.BattleCharInfoPanel.SetTarget(hero.Data);
                        }
                    }
                    else if (hit.collider.gameObject.layer == 12)
                    {
                        EnemyActor enemy = hit.collider.gameObject.GetComponent<EnemyActor>();
                        if (enemy != null)
                        {
                            UIManager.Instance.BattleCharInfoPanel.SetTarget(enemy.Data);
                        }
                    }
                    return;
                }
                isDragging = true;
            }

            if (Input.mouseScrollDelta.y != 0)
            {
                mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - Input.mouseScrollDelta.y * 0.5f, 4f, 13f);
                zoomRatio = mainCamera.orthographicSize / 7.0f;
                SetCameraBounds(currentBounds);
                ClampCameraPosition();
            }

            if (!Input.GetMouseButton(0)) isDragging = false;

            if (isDragging)
            {
                float speedMultiplier = zoomRatio;
                Vector3 move = new Vector3(Input.GetAxis("Mouse X") * dragspeed * speedMultiplier, Input.GetAxis("Mouse Y") * dragspeed * speedMultiplier, 0);
                mainCamera.transform.Translate(-move, Space.Self);
                ClampCameraPosition();
            }
        }
    }
}