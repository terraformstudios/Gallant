using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragObject : MonoBehaviour, IBeginDragHandler,IDragHandler, IEndDragHandler {
    public static GameObject itemBeingDragged;
    Vector3 startPosition;
    Transform StartParent;

    public void OnBeginDrag(PointerEventData eventData)
    {
        itemBeingDragged = this.gameObject;
        startPosition = this.transform.position;
        StartParent = transform.parent;
        this.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.GetComponent<CanvasGroup>().blocksRaycasts = true;
        itemBeingDragged = null;
        if (transform.parent == StartParent)
        {
            this.transform.position = startPosition;
        }
        else {
            // enable the next child
            for (int i = 0; i < StartParent.childCount; i++) {
                if (i == 0)
                {
                    StartParent.transform.GetChild(i).gameObject.SetActive(true);// make the next child active 
                }
                else {
                    StartParent.transform.GetChild(i).gameObject.SetActive(false); // make the other childern de-active
                }
            }
        }
        
    }
}
