using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



public class background : MonoBehaviour
{
    public GameObject tilemapGameObject;
    public GameObject totem;
    public GameObject enemy;

    public GameObject light2DGameObject;

    public GameObject player;

    private PlayerMovement playerMovement;

    private UnityEngine.Rendering.Universal.Light2D light2DComponent;

    public float duration = 5.0f; // Duration over which to reduce the radius
    public float startRadius = 5.0f;

    private Coroutine reduceCoroutine;

    void Start()
    {
        GameObject myLight = Instantiate(light2DGameObject, new Vector3(0, 0, 0), Quaternion.identity);
        light2DComponent = myLight.GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        GameObject playerInstance =  Instantiate(player, new Vector3(0, 0, 0), Quaternion.identity);
        playerMovement = playerInstance.GetComponent<PlayerMovement>();
        playerMovement.Ilumination = myLight;
        totem.GetComponent<Totem>().playerLight = myLight;
        totem.GetComponent<Totem>().Map = gameObject;
        totem.GetComponent<Totem>().mapScript = this;
        reduceCoroutine = StartCoroutine(ReduceOuterRadiusOverTime());

    }

    void Update()
    {
        PaintTilesUntilCameraEdges();
        if (reduceCoroutine == null)
            light2DComponent.pointLightOuterRadius = startRadius;
    }

    static int count;
    void PaintTilesUntilCameraEdges()
    {
        Camera mainCamera = Camera.main;
        Vector3 cameraPosition = mainCamera.transform.position;
        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        Vector3 bottomLeft = cameraPosition - new Vector3(cameraWidth, cameraHeight);
        Vector3 topRight = cameraPosition + new Vector3(cameraWidth, cameraHeight);

        
        for (int x = (int)bottomLeft.x - 2; x < (int)topRight.x + 2; x++)
        {
            for (int y = (int)bottomLeft.y - 2; y < (int)topRight.y + 2; y++)
            {
                Vector3 position = new Vector3(x, y, 0);
                Collider2D collider = Physics2D.OverlapCircle(position, 0.1f, LayerMask.GetMask("Background_tile"));
                if (collider == null)
                {
                    if (count % 7 == 0)
                        Instantiate(enemy, position, Quaternion.identity);
                    if (count % 720 == 0)
                        Instantiate(totem, position, Quaternion.identity);
                    GameObject newTilemap = Instantiate(tilemapGameObject, position, Quaternion.identity);
                    newTilemap.tag = "Background_tilemap";
                    count++;
                }
            }
        }
        EraseObjectsOutsideSquare();
    }
    void SpawnChance(float chance, Vector3 position)
    {
        chance *= 0.001f;

        if (Random.value < chance)
            Instantiate(totem, position, Quaternion.identity);
    }

    void EraseObjectsOutsideSquare()
    {
        Camera mainCamera = Camera.main;
        Vector3 cameraPosition = mainCamera.transform.position;
        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        Vector3 bottomLeft = (cameraPosition - new Vector3(cameraWidth + 30, cameraHeight + 30));
        Vector3 topRight = (cameraPosition + new Vector3(cameraWidth + 30, cameraHeight + 30));

        Collider2D[] colliders = Physics2D.OverlapAreaAll(bottomLeft, topRight);
        foreach (Collider2D collider in colliders)
        {
            if (!IsInsideSquare(collider.transform.position, bottomLeft, topRight))
            {
                Destroy(collider.gameObject);
            }
        }
    }

    bool IsInsideSquare(Vector3 position, Vector3 bottomLeft, Vector3 topRight)
    {
        return position.x >= bottomLeft.x && position.x <= topRight.x &&
               position.y >= bottomLeft.y && position.y <= topRight.y;
    }

    private IEnumerator ReduceOuterRadiusOverTime()
    {
        float timer = 0.0f;
        float startRadius = light2DComponent.pointLightOuterRadius > 30 ? 30 : light2DComponent.pointLightOuterRadius;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration; // Calculate the progress as a fraction of the duration
            light2DComponent.pointLightOuterRadius = Mathf.Lerp(startRadius, 0f, t); // Interpolate between startRadius and 0 over time
            yield return null; // Wait for the next frame
        }

        // Ensure the radius is exactly 0 at the end
        light2DComponent.pointLightOuterRadius = 0f;
    }

    public void RestartCoroutine(float newValue)
    {
        if (reduceCoroutine != null)
            StopCoroutine(reduceCoroutine);
        light2DComponent.pointLightOuterRadius = newValue;
        reduceCoroutine = StartCoroutine(ReduceOuterRadiusOverTime());
    }
}
