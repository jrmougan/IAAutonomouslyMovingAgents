using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class World {

    static readonly World instance = new World();
    static GameObject[] hidingSpots;

    static World() {

        hidingSpots = GameObject.FindGameObjectsWithTag("hide");
    }

    World() { }


    public static World Instance { get { return instance; } }
    public GameObject[] GetHidingSpots() { return hidingSpots; }
}
