
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
public class Utils
{
    public static double Clamp(double value, double min, double max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
    public static bool AnyKeyUp(KeyCode[] keys)
    {
        foreach (KeyCode key in keys)
            if (Input.GetKeyUp(key))
                return true;
        return false;
    }
    public static bool AnyKeyDown(KeyCode[] keys)
    {
        foreach (KeyCode key in keys)
            if (Input.GetKeyDown(key))
                return true;
        return false;
    }
    public static bool AnyKeyPressed(KeyCode[] keys)
    {
        foreach (KeyCode key in keys)
            if (Input.GetKey(key))
                return true;
        return false;
    }
    public static bool IsPointInScreen(Vector2 point)
    {
        return 0 <= point.x && point.x <= Screen.width &&
               0 <= point.y && point.y <= Screen.height;
    }
    
    
    public static float ClosestDistance(Collider a, Collider b)
    {
        if (a.bounds.Intersects(b.bounds))
            return 0;
        return Vector3.Distance(a.ClosestPoint(b.transform.position),
                                b.ClosestPoint(a.transform.position));
    }
    static Dictionary<Transform, int> castBackups = new Dictionary<Transform, int>();
    public static bool RaycastWithout(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance, GameObject ignore, int layerMask=Physics.DefaultRaycastLayers)
    {
        castBackups.Clear();
        foreach (Transform tf in ignore.GetComponentsInChildren<Transform>(true))
        {
            castBackups[tf] = tf.gameObject.layer;
            tf.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
        bool result = Physics.Raycast(origin, direction, out hit, maxDistance, layerMask);
        foreach (KeyValuePair<Transform, int> kvp in castBackups)
            kvp.Key.gameObject.layer = kvp.Value;
        return result;
    }
    public static bool SphereCastWithout(Vector3 origin, float sphereRadius, Vector3 direction, out RaycastHit hit, float maxDistance, GameObject ignore, int layerMask=Physics.DefaultRaycastLayers)
    {
        castBackups.Clear();
        foreach (Transform tf in ignore.GetComponentsInChildren<Transform>(true))
        {
            castBackups[tf] = tf.gameObject.layer;
            tf.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
        bool result = Physics.SphereCast(origin, sphereRadius, direction, out hit, maxDistance, layerMask);
        foreach (KeyValuePair<Transform, int> kvp in castBackups)
            kvp.Key.gameObject.layer = kvp.Value;
        return result;
    }
    public static float GetAxisRawScrollUniversal()
    {
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll < 0) return -1;
        if (scroll > 0) return  1;
        return 0;
    }
    public static float GetPinch()
    {
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
            return touchDeltaMag - prevTouchDeltaMag;
        }
        return 0;
    }
    public static float GetZoomUniversal()
    {
        if (Input.mousePresent)
            return GetAxisRawScrollUniversal();
        else if (Input.touchSupported)
            return GetPinch();
        return 0;
    }
    static Regex lastNountRegEx = new Regex(@"([A-Z][a-z]*)"); 
    public static string ParseLastNoun(string text)
    {
        MatchCollection matches = lastNountRegEx.Matches(text);
        return matches.Count > 0 ? matches[matches.Count-1].Value : "";
    }
    public static bool IsCursorOverUserInterface()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return true;
        for (int i = 0; i < Input.touchCount; ++i)
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                return true;
        return GUIUtility.hotControl != 0;
    }
    public static string PBKDF2Hash(string text, string salt)
    {
        byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
        Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(text, saltBytes, 10000);
        byte[] hash = pbkdf2.GetBytes(20);
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }
    public static Vector3 RandomUnitCircleOnNavMesh(Vector3 position, float radiusMultiplier)
    {
        Vector2 r = UnityEngine.Random.insideUnitCircle * radiusMultiplier;
        Vector3 randomPosition = new Vector3(position.x + r.x, position.y, position.z + r.y);
        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, radiusMultiplier * 2, NavMesh.AllAreas))
            return hit.position;
        return position;
    }
    public static Vector3 ReachableRandomUnitCircleOnNavMesh(Vector3 position, float radiusMultiplier, int solverAttempts)
    {
        for (int i = 0; i < solverAttempts; ++i)
        {
            Vector3 candidate = RandomUnitCircleOnNavMesh(position, radiusMultiplier);
            if (!NavMesh.Raycast(position, candidate, out NavMeshHit hit, NavMesh.AllAreas))
                return candidate;
        }
        return position;
    }
    public static bool IsReachableVertically(Collider origin, Collider other, float maxDistance)
    {
        Vector3 originClosest = origin.ClosestPoint(other.transform.position);
        Vector3 otherClosest = other.ClosestPoint(origin.transform.position);
        Vector3 otherCenter = new Vector3(otherClosest.x, other.bounds.center.y, otherClosest.z); 
        Vector3 otherTop    = otherCenter + Vector3.up * other.bounds.extents.y;
        Vector3 otherBottom = otherCenter + Vector3.down * other.bounds.extents.y;
        Vector3 originCenter = new Vector3(originClosest.x, origin.bounds.center.y, originClosest.z); 
        Vector3 originTop    = originCenter + Vector3.up * origin.bounds.extents.y;
        float originHalf = origin.bounds.size.y / 2;
        if (Vector3.Distance(originCenter, otherCenter) <= maxDistance &&
            !Physics.Linecast(originCenter, otherCenter, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(originCenter, otherCenter, Color.white);
            return true;
        }
        else Debug.DrawLine(originCenter, otherCenter, Color.gray);
        if (Vector3.Distance(originCenter, otherTop) <= maxDistance &&
            !Physics.Linecast(originCenter, otherTop, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(originCenter, otherTop, Color.white);
            return true;
        }
        else Debug.DrawLine(originCenter, otherTop, Color.gray);
        if (Vector3.Distance(originCenter, otherBottom) <= maxDistance &&
            !Physics.Linecast(originCenter, otherBottom, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(originCenter, otherBottom, Color.white);
            return true;
        }
        else Debug.DrawLine(originCenter, otherBottom, Color.gray);
        if (Vector3.Distance(originTop, otherCenter) <= maxDistance - originHalf &&
            !Physics.Linecast(originTop, otherCenter, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(originTop, otherCenter, Color.white);
            return true;
        }
        else Debug.DrawLine(originTop, otherCenter, Color.gray);
        if (Vector3.Distance(originTop, otherTop) <= maxDistance - originHalf &&
            !Physics.Linecast(originTop, otherTop, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(originTop, otherTop, Color.white);
            return true;
        }
        else Debug.DrawLine(originTop, otherTop, Color.gray);
        if (Vector3.Distance(originTop, otherBottom) <= maxDistance - originHalf &&
            !Physics.Linecast(originTop, otherBottom, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(originTop, otherBottom, Color.white);
            return true;
        }
        else Debug.DrawLine(originTop, otherBottom, Color.gray);
        return false;
    }
    public static Quaternion ClampRotationAroundXAxis(Quaternion q, float min, float max)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;
        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);
        angleX = Mathf.Clamp (angleX, min, max);
        q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);
        return q;
    }
}
