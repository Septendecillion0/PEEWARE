using UnityEngine;

public class FairyScript : Enemy
{
    public Camera cam;
    public float disCam = 5f;

    public Vector2 screenOffset = new Vector2(0.5f, 0.5f);
    public float smoothSpeed = 0.0005f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Look at player
        base.Update();
        
        //Slowly move toward middle of the screen
        Vector3 targetPos = cam.ViewportToWorldPoint(new Vector3(screenOffset.x, screenOffset.y, disCam));
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision){
        //If collided with the tree, they both vanish ... and suck your pee away
        if (collision.gameObject.tag == "Tree"){
            peeMeter.GetComponent<PeeMeterUpdate>().Scare(-20.0f);
            EnemyManager.Instance.EnemyVanish(this.gameObject);
            EnemyManager.Instance.EnemyVanish(collision.gameObject);
        }
        //If not collided with the tree
        else{
            peeMeter.GetComponent<PeeMeterUpdate>().Scare(20.0f);
            EnemyManager.Instance.EnemyVanish(this.gameObject);
        }
    }
}
