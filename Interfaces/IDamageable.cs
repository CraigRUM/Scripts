using UnityEngine;
using System.Collections;

/// <summary>
/// Extention of living entity use for testing and interations
/// </summary>

public interface IDamageable {
    void TakeHit(float damage, RaycastHit hit);
}
