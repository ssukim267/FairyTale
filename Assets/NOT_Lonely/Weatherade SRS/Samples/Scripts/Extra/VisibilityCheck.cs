using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityCheck : MonoBehaviour
{
    public delegate void VisibilityCheckCallback(bool isVisible);
    public VisibilityCheckCallback onVisibilityChanged;

    private void OnBecameInvisible()
    {
        onVisibilityChanged?.Invoke(false);
    }

    private void OnBecameVisible()
    {
        onVisibilityChanged?.Invoke(true);
    }
}
