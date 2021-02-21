using UnityEngine;
public class CursorLocking : MonoBehaviour
{
    public GameObject[] windowsThatUnlockCursor;
    bool AnyUnlockWindowActive()
    {
        foreach (GameObject go in windowsThatUnlockCursor)
            if (go.activeSelf)
                return true;
        return false;
    }
    void Update()
    {
        Cursor.lockState = AnyUnlockWindowActive()
                           ? CursorLockMode.None
                           : CursorLockMode.Locked;
        Cursor.visible = Cursor.lockState != CursorLockMode.Locked;
    }
}
