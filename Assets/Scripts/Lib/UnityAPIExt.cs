
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLua;

[LuaCallCSharp]
public static class UnityAPIExt
{
    /// <summary>
    ///     Gets the missing component.
    /// </summary>
    public static T GetMissingComponent<T>(this GameObject go) where T : Component
    {
        var t = go.GetComponent<T>();
        if (t == null)
        {
            t = go.AddComponent<T>();
        }
        return t;
    }

    public static Component GetMissingComponent(this GameObject go, System.Type type)
    {
        var t = go.GetComponent(type);
        if (t == null)
        {
            t = go.AddComponent(type);
        }
        return t;
    }

    #region find by Name

    public static Transform[] FindChildsByName(this Transform root, string childName, bool includeInactive = false)
    {
        if (string.IsNullOrEmpty(childName)) return null;

        var allChilds = root.GetComponentsInChildren<Transform>(includeInactive);
        var result = new List<Transform>(allChilds.Length);
        foreach (var child in allChilds)
        {
            if (child.name == childName)
            {
                result.Add(child);
            }
        }

        return result.ToArray();
    }

    public static Transform FindChildByName(this Transform root, string childName)
    {
        if (string.IsNullOrEmpty(childName)) return null;

        Transform result = null;
        for (int i = 0, max = root.childCount; i < max; i++)
        {
            var child = root.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }
            result = FindChildByName(child, childName);
            if (result)
                return result;
        }

        return result;
    }

    public static Component[] GetComponentsInChildrenByName(this Transform root, System.Type type, string childName,
        bool includeInactive = false)
    {
        if (string.IsNullOrEmpty(childName)) return null;

        var allChilds = root.GetComponentsInChildren(type, includeInactive);
        var result = new List<Component>(allChilds.Length);
        foreach (var child in allChilds)
        {
            if (child.name == childName)
            {
                result.Add(child);
            }
        }

        return result.ToArray();
    }

    public static T[] GetComponentsInChildrenByName<T>(this Transform root, string childName, bool includeInactive = false) where T : Component
    {
        if (string.IsNullOrEmpty(childName)) return null;

        var allChilds = root.GetComponentsInChildren<T>(includeInactive);
        var result = new List<T>(allChilds.Length);
        foreach (var child in allChilds)
        {
            if (child.name == childName)
            {
                result.Add(child);
            }
        }

        return result.ToArray();
    }
    #endregion

    /// <summary>
    /// UnityEngine.Object重载的==操作符，当一个对象被Destroy，未初始化等情况，obj == null返回true，但这C#对象并不为null，可以通过System.Object.ReferenceEquals(null, obj)来验证下。
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static bool is_null(this UnityEngine.Object o)
    {
        return o == null;
    }

    public static Transform FindChildWithTag(this GameObject go, string tag, bool includeInactive)
    {
        return FindChildWithTag(go.transform, tag, includeInactive);
    }

    public static Transform FindChildWithTag(this Transform parent, string tag, bool includeInactive = false)
    {
        Transform[] childs = parent.GetComponentsInChildren<Transform>(includeInactive);
        foreach (Transform t in childs)
        {
            if (t.CompareTag(tag))
            {
                return t;
            }
        }
        return null;
    }

    public static GameObject active(this GameObject go, bool active)
    {
        go.SetActive(active);
        return go;
    }

    public static Component active(this Component com, bool active)
    {
        GameObject go = com.gameObject;
        if (go != null)
        {
            go.SetActive(active);
        }
        return com;
    }

    public static Transform mount(this Transform trans, string path, GameObject go)
    {
        return mount(trans, path, go.transform);
    }

    public static Transform mount(this Transform trans, string path, Transform child)
    {
        if (child == null) return trans;

        if (string.IsNullOrEmpty(path))
        {
            trans.add_child(child);
        }
        else
        {
            Transform anchor = trans.Find(path);
            if (anchor != null)
            {
                anchor.add_child(child);
            }
            else
            {
                Debug.LogError("Can't not find path node:" + path);
            }
        }

        return trans;
    }

    public static Transform add_child(this GameObject parent, GameObject child)
    {
        if (child != null)
        {
            return parent.add_child(child.transform);
        }

        return null;
    }

    public static Transform add_child(this GameObject parent, Transform child)
    {
        return parent.transform.add_child(child);
    }

    public static Transform add_child(this Transform parent, GameObject child)
    {
        return parent.add_child(child.transform);
    }

    public static Transform add_child(this Transform parent, Transform child)
    {
        if (child != null)
        {
            child.SetParent(parent, false);
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;
        }

        return child;
    }

    /// <summary>
    /// 替换原节点，若原节点存在返回对应Transform
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="path"></param>
    /// <param name="child"></param>
    /// <returns></returns>
    public static Transform replace(this Transform trans, string path, Transform child)
    {
        string parentPath = Path.GetDirectoryName(path);
        string childName = Path.GetFileName(path);
        Transform parent = trans.Find(parentPath);
        Transform oldChild = null;
        if (parent != null)
        {
            oldChild = parent.Find(childName);
            child.name = childName;
            parent.add_child(child);
        }
        return oldChild;
    }

    public static float world_pos(this Transform trans, out float y, out float z)
    {
        var worldPos = trans.position;
        y = worldPos.y;
        z = worldPos.z;
        return worldPos.x;
    }

    public static Transform add_pos(this Transform trans, float x, float y, float z = 0f, bool worldPos = false)
    {
        return trans.add_pos(new Vector3(x, y, z), worldPos);
    }

    public static Transform add_pos(this Transform trans, Vector3 pos, bool worldPos = false)
    {
        if (worldPos)
        {
            trans.position += pos;
        }
        else
        {
            trans.localPosition += pos;
        }

        return trans;
    }

    public static Component ps(this Component com, float x = 0f, float y = 0f, float z = 0, float scale = 1f)
    {
        Transform trans = com as Transform;
        if (trans == null)
        {
            trans = com.transform;
        }
        trans.ps(x, y, z, scale);
        return com;
    }

    public static Transform ps(this Transform trans, float x = 0f, float y = 0f, float z = 0, float scale = 1f)
    {
        return ps(trans, new Vector3(x, y, z), scale);
    }

    public static Transform ps(this Transform trans, Vector3 pos, float scale = 1f)
    {
        trans.localPosition = pos;
        trans.localScale = new Vector3(scale, scale, scale);
        return trans;
    }

    public static Component world_ps(this Component com, float x = 0f, float y = 0f, float z = 0, float scale = 1f)
    {
        Transform trans = com as Transform;
        if (trans == null)
        {
            trans = com.transform;
        }
        trans.world_ps(x, y, z, scale);
        return com;
    }

    public static Transform world_ps(this Transform trans, float x = 0f, float y = 0f, float z = 0, float scale = 1f)
    {
        return world_ps(trans, new Vector3(x, y, z), scale);
    }

    public static Transform world_ps(this Transform trans, Vector3 pos, float scale = 1f)
    {
        trans.position = pos;
        trans.localScale = new Vector3(scale, scale, scale);
        return trans;
    }

    public static Transform sr(this Transform trans, float sx = 1f, float sy = 1f, float sz = 1f)
    {
        trans.localScale = new Vector3(sx, sy, sz);
        return trans;
    }

    public static Transform rot(this Transform trans, float rx = 0f, float ry = 0f, float rz = 0f)
    {
        trans.localEulerAngles = new Vector3(rx, ry, rz);
        return trans;
    }

    public static RectTransform set_size(this RectTransform trans, float w, float h)
    {
        trans.sizeDelta = new Vector2(w, h);
        return trans;
    }

    public static RectTransform set_corner_offset(this RectTransform trans, float top, float bottom, float left, float right)
    {
        trans.offsetMax = new Vector2(-right, -top);
        trans.offsetMin = new Vector2(left, bottom);
        return trans;
    }

    private static Vector3[] s_corners = new Vector3[4];

    public static Vector3 get_local_corner(this RectTransform trans, int index)
    {
        trans.GetLocalCorners(s_corners);
        return s_corners[index];
    }

    public static Vector3[] get_local_corners(this RectTransform trans)
    {
        Vector3[] ret = new Vector3[4];
        trans.GetLocalCorners(ret);
        return ret;
    }

    public static Vector3 get_world_corner(this RectTransform trans, int index)
    {
        trans.GetWorldCorners(s_corners);
        return s_corners[index];
    }

    public static Vector3[] get_world_corners(this RectTransform trans)
    {
        Vector3[] ret = new Vector3[4];
        trans.GetWorldCorners(ret);
        return ret;
    }

    public static void reset_pivot(this RectTransform rectTransform, float px, float py, bool keepPos = false)
    {
        reset_pivot(rectTransform, new Vector2(px, py), keepPos);
    }

    public static void reset_pivot(this RectTransform rectTransform, Vector2 pivot, bool keepPos = false)
    {
        if (rectTransform == null) return;

        if (keepPos)
        {
            Vector2 size = rectTransform.rect.size;
            Vector2 deltaPivot = rectTransform.pivot - pivot;
            Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
            rectTransform.pivot = pivot;
            rectTransform.localPosition -= deltaPosition;
        }
        else
        {
            rectTransform.pivot = pivot;
        }
    }

    public static int to_hex(this Color32 color)
    {
        int hex = color.a << 24 | color.r << 16 | color.g << 8 | color.b;
        return hex;
    }

    //hex为ARGB格式，默认alpha值为1，如：0x00FFFFFF 这种值没啥意义不做特殊处理
    public static Color itoc(uint hex)
    {
        byte b = (byte)(hex & 0xFF);
        hex = hex >> 8;

        byte g = (byte)(hex & 0xFF);
        hex = hex >> 8;

        byte r = (byte)(hex & 0xFF);
        hex = hex >> 8;

        byte a = byte.MaxValue;
        if (hex > 0)
        {
            a = (byte)(hex & 0xFF);
        }
        return new Color32(r, g, b, a);
    }

    public static Color stoc(string colorStr)
    {
        Color c = Color.white;
        ColorUtility.TryParseHtmlString(colorStr, out c);
        return c;
    }

    /// <summary>
    /// #RGBA,#RGB 示例红色#FF0000,半透红色#FF00007D
    /// </summary>
    /// <param name="com"></param>
    /// <param name="colorStr"></param>
    /// <returns></returns>
    public static Graphic SetColor(this Graphic com, string colorStr)
    {
        com.color = stoc(colorStr);
        return com;
    }

    /// <summary>
    /// 0xARGB 0xRGB
    /// </summary>
    /// <param name="com"></param>
    /// <param name="colorHex"></param>
    /// <returns></returns>
    public static Graphic SetColor(this Graphic com, uint colorHex)
    {
        com.color = itoc(colorHex);
        return com;
    }

    public static ParticleSystem SetColor(this ParticleSystem ps, string colorStr)
    {
        Color color = stoc(colorStr);
        var main = ps.main;
        main.startColor = color;
        return ps;
    }

    public static ParticleSystem SetColor(this ParticleSystem ps, uint colorHex)
    {
        Color color = itoc(colorHex);
        var main = ps.main;
        main.startColor = color;
        return ps;
    }

    public static Graphic SetAlpha(this Graphic com, float alpha)
    {
        var rawColor = com.color;
        rawColor.a = alpha;
        com.color = rawColor;
        return com;
    }

    public static CanvasGroup SetAlpha(this CanvasGroup group, float alpha)
    {
        group.alpha = alpha;
        return group;
    }

    public static CanvasGroup active(this CanvasGroup group, bool active)
    {
        group.alpha = active ? 1 : 0;
        return group;
    }

    public static SpriteRenderer SetColor(this SpriteRenderer com, string colorStr)
    {
        com.color = stoc(colorStr);
        return com;
    }

    public static SpriteRenderer SetColor(this SpriteRenderer com, uint colorHex)
    {
        com.color = itoc(colorHex);
        return com;
    }

    public static SpriteRenderer SetAlpha(this SpriteRenderer com, float alpha)
    {
        var rawColor = com.color;
        rawColor.a = alpha;
        com.color = rawColor;
        return com;
    }

    public static TextMesh SetColor(this TextMesh com, string colorStr)
    {
        com.color = stoc(colorStr);
        return com;
    }

    public static TextMesh SetColor(this TextMesh com, uint colorHex)
    {
        com.color = itoc(colorHex);
        return com;
    }

    public static TextMesh SetAlpha(this TextMesh com, float alpha)
    {
        var rawColor = com.color;
        rawColor.a = alpha;
        com.color = rawColor;
        return com;
    }

    public static Vector2 to_v2(this Vector3 v3)
    {
        return v3;
    }

    public static Vector3 to_v3(this Vector2 v2)
    {
        return v2;
    }

    public static void AddListener(this EventTrigger trigger, EventTriggerType type, UnityAction<BaseEventData> callback)
    {
        EventTrigger.Entry entry = null;
        foreach (var e in trigger.triggers)
        {
            if (e.eventID == type)
            {
                entry = e;
                break;
            }
        }

        if (entry == null)
        {
            entry = new EventTrigger.Entry { eventID = type };
            trigger.triggers.Add(entry);
        }
        entry.callback.AddListener(callback);
    }
}
