﻿using UnityEngine;
using System.Collections;

public class AnimatEssence : MonoBehaviour {

    int hydration = 2, satation = 10;

    public void setReaourceAdundance(int Hydration, int Satation) {
        hydration = Hydration;
        satation = Satation;
    }

    public void Start() {
        StartCoroutine("Decay");
    }

    public int[] Consume(int[] RD){
        if (RD[0] > hydration && RD[1] > satation)
        {
            Destroy(this.gameObject);
            return new int[] { hydration, satation };
        }
        else if(RD[0] < hydration && RD[1] > satation)
        {
            satation = Mathf.Clamp(satation - RD[1], 0, 1000);
            RD[0] = hydration;
            hydration = 0;
            return new int[] { RD[0], RD[1] };
        }
        else if (RD[0] > hydration && RD[1] < satation)
        {
            hydration = Mathf.Clamp(hydration - RD[0], 0, 1000);
            RD[0] = satation;
            satation = 0;
            return new int[] { RD[0], RD[1] };
        }
        else
        {
            satation = Mathf.Clamp(satation - RD[1], 0, 1000);
            hydration = Mathf.Clamp(hydration - RD[0], 0, 1000);
            return new int[] { RD[0], RD[1] };
        }
    }

    void DecaySound()
    {
        Collider[] hitColliders;

        int layerMaska = 1 << 29;
        int layerMaskb = 1 << 30;
        int layerMaskc = 1 << 31;
        hitColliders = Physics.OverlapSphere(transform.position, 2, layerMaska);

        foreach (Collider hitcol in hitColliders)
        {
            if (hitcol.gameObject.GetComponent<HearingCotrols>() != null)
            {
                hitcol.gameObject.GetComponent<HearingCotrols>().Alert(transform);
            }
        }

        hitColliders = Physics.OverlapSphere(transform.position, 4, layerMaskb);

        foreach (Collider hitcol in hitColliders)
        {
            if (hitcol.gameObject.GetComponent<HearingCotrols>() != null)
            {
                hitcol.gameObject.GetComponent<HearingCotrols>().Alert(transform);
            }
        }

        hitColliders = Physics.OverlapSphere(transform.position, 6, layerMaskc);

        foreach (Collider hitcol in hitColliders)
        {
            if (hitcol.gameObject.GetComponent<HearingCotrols>() != null)
            {
                hitcol.gameObject.GetComponent<HearingCotrols>().Alert(transform);
            }
        }
    }

    IEnumerable Decay() {
        while (hydration + satation != 0) {
            hydration -= 5;
            satation -= 5;
            DecaySound();
            yield return new WaitForSeconds(1f);
        }
        Destroy(this.gameObject);
    }
}