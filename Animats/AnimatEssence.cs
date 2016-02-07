using UnityEngine;
using System.Collections;

public class AnimatEssence : MonoBehaviour {

    int hydration = 2, satation = 10;

    public void setReaourceAdundance(int Hydration, int Satation) {
        hydration = Hydration;
        satation = Satation;
    }

    public int[] Consume(){
        Destroy(this.gameObject);
        return new int[] { hydration, satation };
    }
}
