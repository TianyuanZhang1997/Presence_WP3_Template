using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public AudioClip [] aclips;
    public Transform buttonColliders;
    public AudioSource[] aSources;
    // Start is called before the first frame update
    void Start()
    {
        aSources = buttonColliders.GetComponentsInChildren<AudioSource>();

        for (int t = 0; t < aclips.Length; t++)
        {
            AudioClip tmp = aclips[t];
            int r = Random.Range(t, aclips.Length);
            aclips[t] = aclips[r];
            aclips[r] = tmp;
        }

        for (int i =0; i< aclips.Length; i++)
            aSources[i].clip = aclips[i];
    }
   
    // Update is called once per frame

}
