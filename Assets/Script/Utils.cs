using System.Reflection;
using UnityEngine;


public class Utils
{
    static public Vector2 RotateVector(Vector2 v, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(
            v.x * Mathf.Cos(radians) - v.y * Mathf.Sin(radians),
            v.x * Mathf.Sin(radians) + v.y * Mathf.Cos(radians)
        );
    }
    static public Vector2 GetMousePosInWorld()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    static public float DirectionToAngle(Vector2 dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }
    
    static public T CopyComponent<T>(T source,GameObject dest) where T : Component
    {
        T copy=dest.AddComponent<T>();
        FieldInfo[] fields = source.GetType().GetFields();
        foreach (FieldInfo field in fields)
        {
            field.SetValue(copy,field.GetValue(source));
        }
        PropertyInfo[] props = source.GetType().GetProperties();
        foreach (var prop in props)
        {
            if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") 
                continue;
            prop.SetValue(copy, prop.GetValue(source, null), null);
        }
        return copy;
    }
    static public void SwapInArray<T>(int i1,int i2, T[] array)
    {
        T temp=array[i1];
        array[i1] = array[i2];
        array[i2] = temp;
    }
}
