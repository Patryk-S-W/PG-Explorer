using UnityEngine;
namespace Controller2k
{
    public static class Helpers
    {
        public static Vector3 GetTopSphereWorldPosition(Vector3 position, Vector3 transformedCenter, float scaledRadius, float scaledHeight)
        {
            Vector3 sphereOffsetY = Vector3.up * (scaledHeight / 2.0f - scaledRadius);
            return position + transformedCenter + sphereOffsetY;
        }
        public static Vector3 GetTopSphereWorldPositionSimulated(Transform transform, Vector3 center, float scaledRadius, float height)
        {
            float scaledHeight = height * transform.lossyScale.y;
            Vector3 transformedCenter = transform.TransformVector(center);
            return GetTopSphereWorldPosition(transform.position, transformedCenter, scaledRadius, scaledHeight);
        }
        public static Vector3 GetBottomSphereWorldPosition(Vector3 position, Vector3 transformedCenter, float scaledRadius, float scaledHeight)
        {
            Vector3 sphereOffsetY = Vector3.up * (scaledHeight / 2.0f - scaledRadius);
            return position + transformedCenter - sphereOffsetY;
        }
        public static Vector3 GetBottomSphereWorldPositionSimulated(Transform transform, Vector3 center, float scaledRadius, float height)
        {
            float scaledHeight = height * transform.lossyScale.y;
            Vector3 transformedCenter = transform.TransformVector(center);
            return GetBottomSphereWorldPosition(transform.position, transformedCenter, scaledRadius, scaledHeight);
        }
        public static Vector3 CalculateCenterWithSameFootPosition(Vector3 center, float height, float newHeight, float skinWidth)
        {
            float localFootY = center.y - (height / 2.0f + skinWidth);
            float newCenterY = localFootY + (newHeight / 2.0f + skinWidth);
            return new Vector3(center.x, newCenterY, center.z);
        }
        public static bool IsMoveVectorAlmostZero(Vector3 moveVector, float smallThreshold)
        {
            return Mathf.Abs(moveVector.x) <= smallThreshold &&
                   Mathf.Abs(moveVector.y) <= smallThreshold &&
                   Mathf.Abs(moveVector.z) <= smallThreshold;
        }
    }
}