using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class Util
{
    public static IEnumerator DelayedCall(float time, Action func)
    {
        yield return new WaitForSeconds(time);
        func?.Invoke();
    }

    public static IEnumerator FDelayedCall(int frames, Action func)
    {
        for (int i = 0; i < frames; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        func?.Invoke();
    }

    // onUpdate param: t (0 to 1)
    public static IEnumerator ContinuousCall(float time, Action<float> onUpdate, Action onEnd, bool fixedUpdate = false)
    {
        float startTime = Time.time;
        while (Time.time < startTime + time)
        {
            onUpdate?.Invoke((Time.time - startTime) / time);
            if (!fixedUpdate)
            {
                yield return new WaitForEndOfFrame();
            }
            else
            {
                yield return new WaitForFixedUpdate();
            }
        }
        onEnd?.Invoke();
    }

    /// <summary>
    /// Set the LocalScale.x and keep the y and z the same
    /// </summary>
    /// <param name="anObject"></param>
    /// <param name="x"></param>
    public static void SetLocalScaleX(this GameObject anObject, float x)
    {
        float scaleY = anObject.transform.localScale.y;
        float scaleZ = anObject.transform.localScale.z;
        anObject.transform.localScale = new Vector3(x, scaleY, scaleZ);
    }

    /// <summary>
    /// Set the anObject.transform.localScale.y and keep the x and z the same
    /// </summary>
    /// <param name="anObject"></param>
    /// <param name="y"></param>
    public static void SetLocalScaleY(this GameObject anObject, float y)
    {
        float scaleX = anObject.transform.localScale.x;
        float scaleZ = anObject.transform.localScale.z;
        anObject.transform.localScale = new Vector3(scaleX, y, scaleZ);
    }

    /// <summary>
    /// Set the anObject.transform.localScale.z and keep the x and y the same
    /// </summary>
    /// <param name="anObject"></param>
    /// <param name="z"></param>
    public static void SetLocalScaleZ(this GameObject anObject, float z)
    {
        float scaleX = anObject.transform.localScale.x;
        float scaleY = anObject.transform.localScale.y;
        anObject.transform.localScale = new Vector3(scaleX, scaleY, z);
    }

    /// <summary>
    /// Computes the minimum separation between a point and a rect the point, where the separation is a vector 
    /// with a magnitude equal to the minimum distance between the point and an edge of the rect
    /// and a direction towards the rect
    /// </summary>
    /// <param name="point"></param>
    /// <param name="rect"></param>
    /// <returns>The minimum separation vector, or the zero vector if the point is in the rect</returns>
    public static Vector2 MinPointRectSeparation(Vector2 point, Rect rect)
    {
        if (rect.Contains(point)) return Vector2.zero;

        Vector2 separation = Vector2.zero;

        if (point.x < rect.xMin) separation.x = rect.xMin - point.x;
        else if (point.x > rect.xMax) separation.x = rect.xMax - point.x;
        if (point.y < rect.yMin) separation.y = rect.yMin - point.y;
        else if (point.y > rect.yMax) separation.y = rect.yMax - point.y;

        return separation;
    }


    /// <summary>
    /// Draws a rectangle for debugging purposes
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="color"></param>
    /// <param name="time">the time the draw rectangle with last</param>
    public static void DebugDrawRect(Rect rect, Color color, float time)
    {
        Debug.DrawLine(new Vector2(rect.xMin, rect.yMax), rect.min, color, time);
        Debug.DrawLine(new Vector2(rect.xMin, rect.yMax), rect.max, color, time);
        Debug.DrawLine(new Vector2(rect.xMax, rect.yMin), rect.min, color, time);
        Debug.DrawLine(new Vector2(rect.xMax, rect.yMin), rect.max, color, time);
    }

    /// <summary>
    /// Projects a Vector2 a onto another Vector2 b
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>a proj b</returns>
    public static Vector2 Vec2Proj(Vector2 a, Vector2 b)
    {
        return Vector2.Dot(a, b) / b.magnitude * b;
    }

    public static float SignOr0(float num)
    {
        if (Mathf.Approximately(num, 0)) return 0;
        return Mathf.Sign(num);
    }

    public static Vector2 NormalizePerAxis(this Vector2 vec)
    {
        return new Vector2(SignOr0(vec.x), SignOr0(vec.y));
    }

    public static string SecondsToTime(float time)
    {
        // private String secondsToTime(float time)
        {
            string toReturn = "";
            int seconds = (int)(time % 60);
            int minutes = (int)(time / 60);

            if (minutes < 10)
            {
                toReturn += "0";
                toReturn += minutes;
            }
            else
            {
                toReturn += minutes;
            }

            toReturn += ":";

            if (seconds < 10)
            {
                toReturn += "0";
                toReturn += seconds;
            }
            else
            {
                toReturn += seconds;
            }

            return toReturn;
        }
    }

    public static Vector2 PDir2Vec(PDir pDir)
    {
        switch (pDir)
        {
            case PDir.Up:
                return Vector2.up;
            case PDir.Down:
                return Vector2.down;
            case PDir.Left:
                return Vector2.left;
            case PDir.Right:
                return Vector2.right;
        }
        return Vector2.zero;
    }

    public static void StretchCapsuleBetween(
        CapsuleCollider2D capsule,
        Vector2 p0,
        Vector2 p1,
        float radius)
    {
        capsule.direction = CapsuleDirection2D.Vertical;

        Vector2 delta = p1 - p0;
        float distance = delta.magnitude;

        // Position
        capsule.transform.position = (p0 + p1) * 0.5f;

        // Rotation (capsule points "up" by default)
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90f;
        capsule.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Size
        capsule.size = new Vector2(
            radius * 2f,
            distance + radius * 2f
        );
    }

    public static Vector2 ProjectPointOntoSegment(
        Vector2 p,
        Vector2 a,
        Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(p - a, ab) / Vector2.Dot(ab, ab);
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }

    public static int ProjectionRegion(
    Vector2 p,
    Vector2 a,
    Vector2 b)
    {
        Vector2 ab = b - a;
        float abLenSq = Vector2.Dot(ab, ab);

        if (abLenSq < Mathf.Epsilon)
            return 0; // degenerate

        float t = Vector2.Dot(p - a, ab) / abLenSq;

        if (t < 0f) return -1; // before A
        if (t > 1f) return 1; // past B
        return 0;              // on segment
    }

    public static float RepeatSigned(float value, float length)
    {
        float range = 2f * length;
        return Mathf.Repeat(value + length, range) - length;
    }

    /// <summary>
    /// Some characters, such as Enter and Space, show up as empty when returned by GetBindingDisplayString()
    /// since these are "control characters"
    /// </summary>
    public static string FixControlString(string s, InputAction action, int index = 0)
    {
        if ((int)s.ToCharArray()[0] <= 32)
        {
            string fallbackText = action.bindings[index].path;
            fallbackText = fallbackText.Substring(fallbackText.LastIndexOf('/') + 1);
            fallbackText = char.ToUpperInvariant(fallbackText[0]) + fallbackText.Substring(1);
            return fallbackText;
        }
        return s;
    }
}
