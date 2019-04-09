using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropObject : MonoBehaviour , IDropHandler{
    public GameObject item
    {
        get
        {
            if (transform.childCount > 0)
            {
                return transform.GetChild(0).gameObject;
            }
            else
            {
                return null;
            }
            
        }
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (!item)// this is the case, if the drop item is empty, doesnt have any child at all
        {// drop the object only in case if selected action slot is empty or have the same tag as the current object being dragged
            if (transform.tag == "SelectedActions" || DragObject.itemBeingDragged.transform.name == transform.tag) {
                DragObject.itemBeingDragged.transform.SetParent(transform);
            }  
        } else if (item) {// if the drop item already has childern, you can do two things, swipe the positions in case of 
            // selected actions, and if not the selected actions slots then add only if the tag is same 
            if(transform.tag == DragObject.itemBeingDragged.transform.parent.tag)// if the current item tag, and the objects parent tags are same, SWIPE
            {
                item.transform.SetParent(DragObject.itemBeingDragged.transform.parent);
                DragObject.itemBeingDragged.transform.SetParent(transform);
            }else if(transform.tag == DragObject.itemBeingDragged.transform.name)
            {
                DragObject.itemBeingDragged.transform.SetParent(transform);
                DragObject.itemBeingDragged.GetComponent<CanvasGroup>().blocksRaycasts = true;
                for (int i = 0; i < transform.childCount; i++) {
                    if (i == 0)
                    {
                        transform.GetChild(i).gameObject.SetActive(true);
                    }
                    else {
                        transform.GetChild(i).gameObject.SetActive(false);
                    }
                }
            }
            
        }
    }
}
