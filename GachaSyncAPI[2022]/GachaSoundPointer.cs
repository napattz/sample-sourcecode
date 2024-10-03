using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaSoundPointer : MonoBehaviour
{
    private BoxCollider2D pointerCollider;
    private bool isDelay;
    private void Start()
    {
        isDelay = true;
        StartCoroutine(DelayStartSound());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isDelay)
        {
            AudioManager.instance.LoadResPlay(AudioNames.gachaHit);
        }
    }

    IEnumerator DelayStartSound()
    {
        yield return new WaitForSeconds(0.1f);
        isDelay = false;
        pointerCollider = GetComponent<BoxCollider2D>();
    }
}
