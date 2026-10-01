// STUB — compile-check only.
#pragma warning disable
namespace UnityEngine
{
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero => default;
        public static Vector2 one => new Vector2(1, 1);
        public float magnitude => 0f;
        public static Vector2 operator +(Vector2 a, Vector2 b) => a;
        public static Vector2 operator -(Vector2 a, Vector2 b) => a;
        public static Vector2 operator *(Vector2 a, float d) => a;
        public static Vector2 operator *(float d, Vector2 a) => a;
        public static implicit operator Vector2(Vector3 v) => default;
        public static implicit operator Vector3(Vector2 v) => default;
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero => default;
        public static Vector3 one => default;
        public static Vector3 up => default;
        public static Vector3 down => default;
        public static Vector3 forward => default;
        public static Vector3 back => default;
        public static Vector3 right => default;
        public static Vector3 left => default;
        public float magnitude => 0f;
        public float sqrMagnitude => 0f;
        public Vector3 normalized => this;
        public void Normalize() { }
        public static float Distance(Vector3 a, Vector3 b) => 0f;
        public static float Angle(Vector3 from, Vector3 to) => 0f;
        public static float Dot(Vector3 a, Vector3 b) => 0f;
        public static Vector3 Cross(Vector3 a, Vector3 b) => a;
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a;
        public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDelta) => current;
        public static Vector3 RotateTowards(Vector3 current, Vector3 target, float maxRadians, float maxMagnitude) => current;
        public static Vector3 Scale(Vector3 a, Vector3 b) => a;
        public static Vector3 operator +(Vector3 a, Vector3 b) => a;
        public static Vector3 operator -(Vector3 a, Vector3 b) => a;
        public static Vector3 operator -(Vector3 a) => a;
        public static Vector3 operator *(Vector3 a, float d) => a;
        public static Vector3 operator *(float d, Vector3 a) => a;
        public static Vector3 operator /(Vector3 a, float d) => a;
        public static bool operator ==(Vector3 a, Vector3 b) => true;
        public static bool operator !=(Vector3 a, Vector3 b) => false;
        public override bool Equals(object o) => false;
        public override int GetHashCode() => 0;
    }

    public struct Quaternion
    {
        public static Quaternion identity => default;
        public Vector3 eulerAngles { get; set; }
        public static Quaternion Euler(float x, float y, float z) => default;
        public static Quaternion LookRotation(Vector3 forward) => default;
        public static Quaternion RotateTowards(Quaternion from, Quaternion to, float maxDegreesDelta) => from;
        public static Quaternion operator *(Quaternion a, Quaternion b) => a;
        public static Vector3 operator *(Quaternion q, Vector3 v) => v;
    }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b) { this.r = r; this.g = g; this.b = b; a = 1f; }
        public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color white => default;
        public static Color black => default;
        public static Color red => default;
        public static Color green => default;
        public static Color gray => default;
        public static Color magenta => default;
        public static Color Lerp(Color a, Color b, float t) => a;
    }

    public struct Bounds
    {
        public Bounds(Vector3 center, Vector3 size) { }
        public bool Contains(Vector3 point) => false;
    }

    public static class Mathf
    {
        // Real implementations so logic tests can run offline.
        public const float PI = 3.14159265f;
        public const float Deg2Rad = PI / 180f;
        public const float Rad2Deg = 180f / PI;
        public static float Sin(float f) => (float)System.Math.Sin(f);
        public static float Cos(float f) => (float)System.Math.Cos(f);
        public static float Abs(float f) => System.Math.Abs(f);
        public static float Sqrt(float f) => (float)System.Math.Sqrt(f);
        public static float Min(float a, float b) => a < b ? a : b;
        public static int Min(int a, int b) => a < b ? a : b;
        public static float Max(float a, float b) => a > b ? a : b;
        public static int Max(int a, int b) => a > b ? a : b;
        public static float Clamp(float v, float min, float max) => v < min ? min : v > max ? max : v;
        public static int Clamp(int v, int min, int max) => v < min ? min : v > max ? max : v;
        public static float Clamp01(float v) => Clamp(v, 0f, 1f);
        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);
        public static float SmoothStep(float from, float to, float t) { t = Clamp01(t); t = t * t * (3f - 2f * t); return from + (to - from) * t; }
        public static float PingPong(float t, float length) { float r = t % (length * 2f); return length - System.Math.Abs(r - length); }
        public static float MoveTowardsAngle(float current, float target, float maxDelta) => current;
        public static int CeilToInt(float f) => (int)System.Math.Ceiling(f);
        public static int FloorToInt(float f) => (int)System.Math.Floor(f);
    }
}
