
using UnityEngine;

public static class PhysicsExt
{
	//xlua函数重载识别不了，需要单独封装一下来调用
	public static bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		return Physics.Raycast(ray, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
	}

	public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		return Physics.Raycast(origin, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
	}
}
