using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightBox : MonoBehaviour {

    Material mat;
    public Color orignalcolor, highlightedcolor;
	// Use this for initialization
	void Start () {
        mat = this.GetComponent<MeshRenderer>().material;

	}

    public void _HighLightBox() {
        mat.color = highlightedcolor;
        Invoke("Resetcolor",5);
    }

    public void Resetcolor() {
        mat.color = orignalcolor;
    }
}
