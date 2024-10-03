using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaSelectPointer : MonoBehaviour
{
    private GachaController gachaController;
    private BoxCollider2D pointerCollider;

    private void Start()
    {
        pointerCollider = GetComponent<BoxCollider2D>();
        gachaController = FindObjectOfType<GachaController>();
    }

    public void SetEnableOfGachaPointer(bool state)
    {
        pointerCollider.enabled = state;
    }
     
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Selected"))
        {
            gachaController.StopWheel();
            SetEnableOfGachaPointer(false);
        }
    }
}
