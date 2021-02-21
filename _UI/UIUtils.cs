using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class UIUtils
{
    public static void BalancePrefabs(GameObject prefab, int amount, Transform parent)
    {
        for (int i = parent.childCount; i < amount; ++i)
        {
            GameObject go = GameObject.Instantiate(prefab);
            go.transform.SetParent(parent, false);
        }
        for (int i = parent.childCount-1; i >= amount; --i)
            GameObject.Destroy(parent.GetChild(i).gameObject);
    }
    public static bool AnyInputActive()
    {
        foreach (Selectable sel in Selectable.allSelectables)
            if (sel is InputField && ((InputField)sel).isFocused)
                return true;
        return false;
    }
    public static void DeselectCarefully()
    {
        if (!Input.GetMouseButton(0) &&
            !Input.GetMouseButton(1) &&
            !Input.GetMouseButton(2))
            EventSystem.current.SetSelectedGameObject(null);
    }
}
