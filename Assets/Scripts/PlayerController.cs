using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float pullForce = 100f;
    [SerializeField] private float rotateSpeed = 360f;

    private GameObject closestTower;
    private GameObject hookedTower;
    private bool isPulled = false;

    private Rigidbody2D rb2D;
    private UIControllerScript uiControl;
    [SerializeField] private AudioSource myAudio;
    [SerializeField] private AudioSource audioPullForce;
    [SerializeField] private AudioSource audioLevelCleared;
    [SerializeField] private GameObject[] trails;

    [SerializeField] private LayerMask towerMask;
    private Vector2 startPosition;
    private bool isPulling;
    private bool isCrashed = false;
    private bool isLevelCleared = false;

    private Color32 colorClosestTower;
    private Color32 colorHookedTower;

    private void Start()
    {
        rb2D = gameObject.GetComponent<Rigidbody2D>();
        myAudio = gameObject.GetComponent<AudioSource>();
        uiControl = GameObject.Find("Canvas").GetComponent<UIControllerScript>();

        startPosition = transform.position;

        colorHookedTower = new Color32(240,150,40,255);
        colorClosestTower = new Color32(255,220,60,255);

        isCrashed = false;
        isLevelCleared = false;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (isCrashed)
        {
            if (!myAudio.isPlaying && !isLevelCleared)
            {
                restartPosition();
            }
        }
        else
        {
            if (!isLevelCleared)
                rb2D.velocity = -transform.up * moveSpeed * 50f *  Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(0) && !isLevelCleared) // Manually check OnMouseDown on tower with this Raycast
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero, 100f, towerMask);
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Tower")) 
            {
                if (hit.collider.gameObject == closestTower)
                {
                    Magnetize();

                    hit.collider.gameObject.GetComponent<SpriteRenderer>().color = colorHookedTower;
                
                    if (!isPulling)
                    {
                        audioPullForce.Play();

                        isPulling = true;
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))  // Remove closest tower and stop magnetize
        {
            StopMagnetize();

            audioPullForce.Stop();

            isPulling = false;

            if (closestTower != null)
                closestTower.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Wall"))
        {
            if (!isCrashed)
            {
                myAudio.Play();
                rb2D.velocity = new Vector3(0f, 0f, 0f);
                rb2D.angularVelocity = 0f;
                isCrashed = true;
                SetActiveTrails(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Goal"))
        {
            Debug.Log("Levelclear!");
            isLevelCleared = true;
            uiControl.endGame();
            audioLevelCleared.Play();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Tower"))
        {
            closestTower = collision.gameObject;
        
            //Change tower color back to green as indicator
            if (!isPulled)
                collision.gameObject.GetComponent<SpriteRenderer>().color = colorClosestTower;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isPulled) return;
    
        if(collision.gameObject.CompareTag("Tower"))
        {
            closestTower = null;
            hookedTower = null;

            collision.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        }

        if (collision.gameObject.CompareTag("Goal"))
        {
            rb2D.velocity = Vector2.zero;
            rb2D.angularVelocity = 0;
            isLevelCleared = true;
        }
    }

    public float AngleDir(Vector2 A, Vector2 B)
    {
        return -A.x * B.y + A.y * B.x;
    }

    private void Magnetize()//GameObject tower)
    {
        if(!isPulled && !isLevelCleared)//&& tower == closestTower)
        {
            if(closestTower != null && hookedTower == null)
            {
                hookedTower = closestTower;
            }
            if(hookedTower)
            {
                float distance = Vector2.Distance(transform.position, hookedTower.transform.position);

                Vector3 pullDirection = (hookedTower.transform.position - transform.position).normalized;

                float newPullForce = Mathf.Clamp(pullForce / distance, 20, 50);

                rb2D.AddForce(pullDirection * newPullForce);

                float dotResult = AngleDir(-transform.up, pullDirection);   

                if (dotResult < 0) // check if the tower in on the left 
                    rb2D.angularVelocity = rotateSpeed / distance; 
                else   // right side
                    rb2D.angularVelocity = -rotateSpeed / distance; // negative = right, positive = left
                    
                isPulled = true;
            }
        }
    }

    private void StopMagnetize()
    {
        isPulled = false;
        hookedTower = null;
        rb2D.angularVelocity = 0;
    }

    private void SetActiveTrails(bool state)
    {
        foreach (GameObject trail in trails)
        {
            trail.SetActive(state);
        }
    }

    private void restartPosition()
    {
        transform.position = startPosition;

        transform.rotation = Quaternion.Euler(0f, 0f, 90f);

        isCrashed = false;
        isLevelCleared = false;

        if (closestTower)
        {
            closestTower.GetComponent<SpriteRenderer>().color = Color.white;
            closestTower = null;
        }    

        rb2D.angularVelocity = 0;

        SetActiveTrails(true);

        audioPullForce.Stop();
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.yellow;

        if (closestTower != null)
            Gizmos.DrawLine(transform.position, closestTower.transform.position);
        Gizmos.DrawLine(transform.position, -transform.up * 5f);
    }
}
