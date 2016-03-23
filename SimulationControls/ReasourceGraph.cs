using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ReasourceGraph : MonoBehaviour {
    public Text[] DataFields;
    public Image[] Bars;
    public int[] values;

    void Start() { }

    // Update is called once per frame
    public void MakeGraph()
    {
        float max = 0f;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] > max) { max = values[i]; }
        }
        DataFields[4].text = max.ToString();
        DataFields[5].text = (max/2).ToString();
        for (int i = 0; i < values.Length; i++)
        {
            DataFields[i].text = values[i].ToString();
            Bars[i].fillAmount = values[i] / max;
        }

    }

    public void MakeGraph(int[] Newvalues)
    {
        values = Newvalues;
        float max = 0f;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] > max) { max = values[i]; }
        }
        DataFields[4].text = max.ToString();
        DataFields[5].text = (max / 2).ToString();
        for (int i = 0; i < values.Length; i++)
        {
            DataFields[i].text = values[i].ToString();
            Bars[i].fillAmount = values[i] / max;
        }

    }
}
