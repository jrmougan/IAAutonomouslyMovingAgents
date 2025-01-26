using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour {

    public GameObject target;
    public GameObject sphere;

    GameObject jitter;

    NavMeshAgent agent;
    Drive ds;
    Vector3 wanderTarget = Vector3.zero;

    float q = 0.0f;

    bool coolDown = false;


    void Start() {

        agent = GetComponent<NavMeshAgent>();
        ds = target.GetComponent<Drive>();
        jitter = Instantiate(sphere);
    }

    void Seek(Vector3 location) {

        agent.SetDestination(location);
    }

    void Flee(Vector3 location) {

        Vector3 fleeVector = location - transform.position;
        agent.SetDestination(transform.position - fleeVector);
    }

    void Pursue() {

        Vector3 targetDir = target.transform.position - transform.position;
        float relativeHeading = Vector3.Angle(transform.forward, transform.TransformVector(target.transform.forward));
        float toTarget = Vector3.Angle(transform.forward, transform.TransformVector(targetDir));


        if ((toTarget > 90.0f && relativeHeading < 20.0f) || ds.currentSpeed < 0.01f) {

            // Debug.Log("SEEKING");
            Seek(target.transform.position);
            return;
        }

        // Debug.Log("LOOKING AHEAD");
        float lookAhead = targetDir.magnitude / (agent.speed + ds.currentSpeed);
        Seek(target.transform.position + target.transform.forward * lookAhead);
    }

    void Evade() {

        Vector3 targetDir = target.transform.position - transform.position;
        float lookAhead = targetDir.magnitude / (agent.speed + ds.currentSpeed);
        Flee(target.transform.position + target.transform.forward * lookAhead);
    }

    void Wander() {

        float wanderRadius = 10.0f;
        float wanderDistance = 20.0f;
        float wanderJitter = 1.0f;

        wanderTarget += new Vector3(
            Random.Range(-1.0f, 1.0f) * wanderJitter,
            0.0f,
            Random.Range(-1.0f, 1.0f));
        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget + new Vector3(0.0f, 0.0f, wanderDistance);
        Vector3 targetWorld = gameObject.transform.InverseTransformVector(targetLocal);

        Debug.DrawLine(transform.position, targetWorld, Color.red);
        jitter.transform.position = targetWorld;
        Seek(targetWorld);
    }

    void Hide() {

        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;

        for (int i = 0; i < World.Instance.GetHidingSpots().Length; ++i) {

            Vector3 hideDir = World.Instance.GetHidingSpots()[i].transform.position - target.transform.position;
            Vector3 hidePos = World.Instance.GetHidingSpots()[i].transform.position + hideDir.normalized * 10.0f;

            if (Vector3.Distance(transform.position, hidePos) < dist) {

                chosenSpot = hidePos;
                dist = Vector3.Distance(transform.position, hidePos);
            }
        }

        Seek(chosenSpot);
    }

    void CleverHide() {

        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        Vector3 chosenDir = Vector3.zero;
        GameObject chosenGO = World.Instance.GetHidingSpots()[0];

        for (int i = 0; i < World.Instance.GetHidingSpots().Length; ++i) {

            Vector3 hideDir = World.Instance.GetHidingSpots()[i].transform.position - target.transform.position;
            Vector3 hidePos = World.Instance.GetHidingSpots()[i].transform.position + hideDir.normalized * 10.0f;

            if (Vector3.Distance(transform.position, hidePos) < dist) {

                chosenSpot = hidePos;
                chosenDir = hideDir;
                chosenGO = World.Instance.GetHidingSpots()[i];
                dist = Vector3.Distance(transform.position, hidePos);
            }
        }

        Collider hideCol = chosenGO.GetComponent<Collider>();
        Ray backRay = new Ray(chosenSpot, -chosenDir.normalized);
        RaycastHit info;
        float distance = 100.0f;
        hideCol.Raycast(backRay, out info, distance);

        Seek(info.point + chosenDir.normalized * 2.0f);
    }

    bool CanSeeTarget() {

        RaycastHit raycastInfo;
        Vector3 rayToTarget = target.transform.position - transform.position;
        float lookAngle = Vector3.Angle(transform.forward, rayToTarget);
        if (lookAngle < 60.0f && Physics.Raycast(transform.position, rayToTarget, out raycastInfo)) {

            if (raycastInfo.transform.gameObject.tag == "cop") return true;
        }
        return false;
    }

    bool CanSeeMe() {

        Vector3 rayToTarget = transform.position - target.transform.position;
        float lookAngle = Vector3.Angle(target.transform.forward, rayToTarget);

        if (lookAngle < 60.0f) return true;
        return false;
    }

    void BehaviourCoolDown() {

        coolDown = false;
    }

    void Update() {

        // Seek(target.transform.position);
        // Flee(target.transform.position);
        // Pursue();
        // Evade();
        // Wander();
        // Hide();
        if (!coolDown) {

            if (CanSeeTarget() && CanSeeMe()) {

                CleverHide();
                coolDown = true;
                Invoke("BehaviourCoolDown", 5.0f);
            } else Pursue();
        }
    }
}
