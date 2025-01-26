using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour {

    public GameObject target;

    NavMeshAgent agent;
    Drive ds;

    void Start() {

        agent = GetComponent<NavMeshAgent>();
        ds = target.GetComponent<Drive>();
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

    void Update() {

        // Seek(target.transform.position);
        // Flee(target.transform.position);
        // Pursue();
        Evade();
    }
}
