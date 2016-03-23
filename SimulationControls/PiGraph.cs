using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PiGraph : MonoBehaviour {

    public int[] values;
    public Color[] wedgeColour;
    public Image wedgePrefab;
    public Text[] DataFields;

    // Use this for initialization
    void Start () {
	}

    public void MakePiGraph(int[] Newvalues)
    {
        foreach(Image wedge in gameObject.GetComponentsInChildren<Image>()) {
            GameObject.DestroyImmediate(wedge);
        }
        values = Newvalues;
        float total = 0f;
        float zRotation = 0f;
        for (int i = 0; i < values.Length; i++)
        {
            DataFields[(i + 1)].text = values[i].ToString();
            total += values[i];
        }
        if (values.Length == 4)
        {
            DataFields[0].text = (total - values[3]).ToString();
        }
        else {
            DataFields[0].text = total.ToString();
        }
        for (int i = 0; i < values.Length; i++)
        {
            Image newWedge = Instantiate(wedgePrefab) as Image;
            newWedge.transform.SetParent(transform, false);
            newWedge.color = wedgeColour[i];
            newWedge.fillAmount = values[i] / total;
            newWedge.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, zRotation));
            zRotation -= newWedge.fillAmount * 360f;
        }

    }
}
