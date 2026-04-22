using UnityEngine;

public class Object_Waypoint : MonoBehaviour
{
    [SerializeField] private string transferToScene;
    [Space]
    [SerializeField] private RespawnType waypointType;
    [SerializeField] private RespawnType connectedWaypoint;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private bool canBeTriggered = true;

    public void SetCanBeTriggered(bool canBeTriggered)
    {
        this.canBeTriggered = canBeTriggered;
    }

    public RespawnType GetWaypointType()
    {
        return waypointType;
    }

    public Vector3 GetPosition()
    {
        return respawnPoint == null ? transform.position : respawnPoint.position;
    }

    private void OnValidate()
    {
        gameObject.name = "Object_Waypoint - " + waypointType + " - " + transferToScene;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Something entered waypoint: " + collision.name);

        if (!canBeTriggered)
        {
            Debug.Log("Waypoint blocked by canBeTriggered");
            return;
        }
        

        if (!collision.CompareTag("Player"))
        {
            Debug.Log("Not player");
            return;
        }

    

        Debug.Log("Player entered waypoint. Loading scene: " + transferToScene +
              " | respawn type: " + connectedWaypoint);

        SaveManager.instance.SaveGame();
        GameManager.instance.ChangeScene(transferToScene, connectedWaypoint);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        canBeTriggered = true;
    }
}