using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class DayReport : MonoBehaviour {

    public ReasourceGraph[] graphs;
    public PiGraph[] charts;
    public Text daycount;

    /*void Start() {
        int[] placeHolderReportValues = { 4, 20, 30, 41, 44, 10, 18, 20, 25, 10, 5, 16, 9, 21, 14 };
        GenerateDayReport(placeHolderReportValues);

    }*/

    public void GenerateDayReport(int[] DayData) {
        int DeathCount = 0;
        daycount.text = DayData[0].ToString();
        List<int> graphA = new List<int>();
        List<int> graphB = new List<int>();
        List<int> chartA = new List<int>();
        List<int> chartB = new List<int>();

        for (int i = 1; i < 5; i++) {
            graphA.Add(DayData[i]);
            graphB.Add(DayData[(i+4)]);
        }
        for (int i = 9; i < DayData.Length; i=i+2) {
            chartA.Add(DayData[i]);
            chartB.Add(DayData[(i+1)]);
            DeathCount += (DayData[i] - DayData[(i + 1)]);
        }
        chartB.Add(DeathCount);
        graphs[0].MakeGraph(graphA.ToArray());
        graphs[1].MakeGraph(graphB.ToArray());
        charts[0].MakePiGraph(chartA.ToArray());
        charts[1].MakePiGraph(chartB.ToArray());
    }

    public void BackToSim() {
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }
}
