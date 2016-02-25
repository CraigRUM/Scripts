using UnityEngine;
using System.Collections;

public class AnimatEssence : MonoBehaviour, IInspectable {

    int hydration = 2, satation = 10;

    public string BeInspected()
    {
        string OutputString = string.Format(
            @"Animat Essence
Hydration Amount: {0}
Satation Amount: {0}
", hydration.ToString(), satation.ToString());
        return OutputString;
    }

    public void setReaourceAdundance(int Hydration, int Satation) {
        hydration = Hydration;
        satation = Satation;
    }

    public void Start() {
        StartCoroutine(Decay());
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

    void Decomposition() {
        Collider[] hitColliders;

        int layerMaska = 1 << 25;

        hitColliders = Physics.OverlapSphere(transform.position, 4, layerMaska);

        foreach (Collider hitcol in hitColliders)
        {
            if (hitcol.gameObject.GetComponentInParent<Terrain>() != null && Random.Range(1,5) == 3)
            {
                hitcol.gameObject.GetComponentInParent<Terrain>().Fertilize();
            }
        }
    }

    IEnumerator Decay() {
        int decayCount = 6;
        while (hydration + satation != 0 && decayCount > 0) {
            hydration -= (int)0.25f*hydration;
            satation -= (int)0.25f * satation;
            transform.localScale -= transform.localScale * 0.25f;
            DecaySound();
            Decomposition();
            decayCount--;
            yield return new WaitForSeconds(10f);
        }
        Destroy(this.gameObject);
    }
}
