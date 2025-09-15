using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Enum representing 8 cardinal and diagonal directions.
/// Used for 8-directional movement, sprites, or UI elements.
/// Values correspond to 45-degree increments starting from Up (0°).
/// </summary>
public enum Direction8
{
    Up = 0,         // 0°
    UpRight = 1,    // 45°
    Right = 2,      // 90°
    DownRight = 3,  // 135°
    Down = 4,       // 180°
    DownLeft = 5,   // 225°
    Left = 6,       // 270°
    UpLeft = 7      // 315°
}

/// <summary>
/// Enum representing 9 directions including stationary.
/// Used for movement systems that need to detect when an object is not moving.
/// Includes all 8 directional values plus Stationary for no movement.
/// </summary>
public enum Direction9
{
    Up = 0,         // 0°
    UpRight = 1,    // 45°
    Right = 2,      // 90°
    DownRight = 3,  // 135°
    Down = 4,       // 180°
    DownLeft = 5,   // 225°
    Left = 6,       // 270°
    UpLeft = 7,     // 315°
    Stationary = 8  // No movement
}

/// <summary>
/// Utility class for direction calculations, angle conversions, and point transformations.
/// Provides methods for converting between different coordinate systems, calculating angles,
/// rotating points, and determining directional enums from vectors.
/// 
/// Features:
/// - Angle calculations between points
/// - Direction enum conversion from vectors
/// - Point rotation around origin or arbitrary center
/// - Quaternion to Euler angle conversion
/// - Angle comparison utilities
/// 
/// Usage Examples:
/// // Get direction from movement vector
/// Vector2 movement = new Vector2(1, 1);
/// Direction8 dir = Direction.GetDirection8(movement); // Returns UpRight
/// 
/// // Rotate a point around origin
/// float2 rotated = Direction.RotatePoint(new float2(1, 0), 90f); // Rotates 90 degrees
/// 
/// // Calculate angle between two points
/// float angle = Direction.GetAngle(pointA, pointB); // Returns angle in degrees
/// </summary>
public static class Direction 
{
    /// <summary>
    /// Converts a Vector3 to float2 by discarding the Z component.
    /// </summary>
    /// <param name="input">The Vector3 to convert</param>
    /// <returns>float2 with X and Y components</returns>
    public static float2 ToFloat2(this Vector3 input)
    {
        return new float2(input.x, input.y);
    }

    /// <summary>
    /// Calculates the angle from center point to target point in 3D space.
    /// Converts to 2D by discarding Z component.
    /// </summary>
    /// <param name="point">The target point</param>
    /// <param name="center">The center/reference point</param>
    /// <returns>Angle in degrees (0-360)</returns>
    public static float GetAngle(float3 point, float3 center)
    {
       return GetAngle(ToFloat2(point), ToFloat2(center));
    }

    /// <summary>
    /// Calculates the angle from center point to target point in 2D space.
    /// Returns angle in degrees with 0° pointing right, 90° pointing up.
    /// </summary>
    /// <param name="point">The target point</param>
    /// <param name="center">The center/reference point</param>
    /// <returns>Angle in degrees (0-360)</returns>
    public static float GetAngle(float2 point, float2 center)
    {
        float2 relPoint = point - center;
        return (ToDegrees(math.atan2(-relPoint.y, relPoint.x)) + 450f) % 360f;
    }

    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    /// <param name="radians">Angle in radians</param>
    /// <returns>Angle in degrees</returns>
    public static float ToDegrees(float radians) => radians * 180f / math.PI;

    /// <summary>
    /// Determines the 8-directional enum value from a 2D direction vector.
    /// Divides 360° into 8 sectors of 45° each, starting from Up (0°).
    /// </summary>
    /// <param name="direction">The direction vector (normalized or not)</param>
    /// <returns>Direction8 enum value representing the closest cardinal/diagonal direction</returns>
    public static Direction8 GetDirection8(float2 direction)
    {
        float dirDegree = GetAngle(direction, float2.zero);
        return (Direction8)((int)math.floor((dirDegree + 22.5f) / 45f) % 8);
    }

    /// <summary>
    /// Determines the 9-directional enum value from a 2D direction vector.
    /// Returns Stationary if the vector magnitude is below the threshold.
    /// Otherwise returns the corresponding Direction8 value.
    /// </summary>
    /// <param name="direction">The direction vector</param>
    /// <param name="stationary">Magnitude threshold below which movement is considered stationary</param>
    /// <returns>Direction9 enum value (Stationary or one of the 8 directions)</returns>
    public static Direction9 GetDirection9(float2 direction , float stationary = 50f)
    {
        if ( math.distance( direction , float2.zero) < stationary )
        {
            return Direction9.Stationary;
        }
        else
        {
            return (Direction9)(int)GetDirection8(direction);
        }
    }

    /// <summary>
    /// Converts a quaternion to Euler angles (roll, pitch, yaw).
    /// Uses the standard aerospace convention for rotation order.
    /// </summary>
    /// <param name="q">The quaternion to convert</param>
    /// <returns>Euler angles in radians as float3 (x=roll, y=pitch, z=yaw)</returns>
    public static float3 ToEulerAngles(quaternion q)
    {
        float3 angles;

        // roll (x-axis rotation)
        double sinr_cosp = 2 * (q.value.w * q.value.x + q.value.y * q.value.z);
        double cosr_cosp = 1 - 2 * (q.value.x * q.value.x + q.value.y * q.value.y);
        angles.x = (float)math.atan2(sinr_cosp, cosr_cosp);

        // pitch (y-axis rotation)
        double sinp = 2 * (q.value.w * q.value.y - q.value.z * q.value.x);
        if (math.abs(sinp) >= 1)
            angles.y = (float)CopySign(math.PI / 2, sinp); // use 90 degrees if out of range
        else
            angles.y = (float)math.asin(sinp);

        // yaw (z-axis rotation)
        double siny_cosp = 2 * (q.value.w * q.value.z + q.value.x * q.value.y);
        double cosy_cosp = 1 - 2 * (q.value.y * q.value.y + q.value.z * q.value.z);
        angles.z = (float)math.atan2(siny_cosp, cosy_cosp);

        return angles;
    }

    /// <summary>
    /// Helper method to copy the sign of one number to another.
    /// </summary>
    /// <param name="a">The magnitude value</param>
    /// <param name="b">The sign source</param>
    /// <returns>Value with magnitude of 'a' and sign of 'b'</returns>
    private static double CopySign(double a, double b)
    {
        return math.abs(a) * math.sign(b);
    }

    /// <summary>
    /// Converts a float3 to float2 by discarding the Z component.
    /// </summary>
    /// <param name="float3">The float3 to convert</param>
    /// <returns>float2 with X and Y components</returns>
    public static float2 ToFloat2(float3 float3)
    {
        return new float2(float3.x, float3.y);
    }

    /// <summary>
    /// Rotates a 3D point around the origin by the specified angle.
    /// Converts to 2D by discarding Z component.
    /// </summary>
    /// <param name="pointToRotate">The point to rotate</param>
    /// <param name="angleInDegrees">Rotation angle in degrees (positive = counterclockwise)</param>
    /// <returns>Rotated point as float2</returns>
    public static float2 RotatePoint(float3 pointToRotate, float angleInDegrees)
    {
        return RotatePoint(ToFloat2(pointToRotate), angleInDegrees);
    }

    /// <summary>
    /// Rotates a 2D point around the origin by the specified angle.
    /// Uses standard 2D rotation matrix.
    /// </summary>
    /// <param name="pointToRotate">The point to rotate</param>
    /// <param name="angleInDegrees">Rotation angle in degrees (positive = counterclockwise)</param>
    /// <returns>Rotated point as float2</returns>
    public static float2 RotatePoint(float2 pointToRotate, float angleInDegrees)
    {
        float angleInRadians = angleInDegrees * (math.PI / 180);
        float cosTheta = math.cos(angleInRadians);
        float sinTheta = math.sin(angleInRadians);
        return new float2
        {
            x = cosTheta * pointToRotate.x -
                sinTheta * pointToRotate.y  ,
            y = sinTheta * pointToRotate.x +
                cosTheta * pointToRotate.y  
        };
    }

    /// <summary>
    /// Rotates a 3D point around a center point by the specified angle.
    /// Converts to 2D by discarding Z components.
    /// </summary>
    /// <param name="pointToRotate">The point to rotate</param>
    /// <param name="centerPoint">The center of rotation</param>
    /// <param name="angleInDegrees">Rotation angle in degrees (positive = counterclockwise)</param>
    /// <returns>Rotated point as float2</returns>
    public static float2 RotatePoint(float3 pointToRotate, float3 centerPoint, float angleInDegrees)
    {
        return RotatePoint( ToFloat2(pointToRotate), ToFloat2(centerPoint) , angleInDegrees);
    }

    /// <summary>
    /// Rotates a 2D point around a center point by the specified angle.
    /// Uses standard 2D rotation matrix with translation.
    /// </summary>
    /// <param name="pointToRotate">The point to rotate</param>
    /// <param name="centerPoint">The center of rotation</param>
    /// <param name="angleInDegrees">Rotation angle in degrees (positive = counterclockwise)</param>
    /// <returns>Rotated point as float2</returns>
    public static float2 RotatePoint(float2 pointToRotate, float2 centerPoint, float angleInDegrees)
    {
        float angleInRadians = angleInDegrees * (math.PI / 180);
        float cosTheta = math.cos(angleInRadians);
        float sinTheta = math.sin(angleInRadians);
        return new float2
        {
            x =
                (cosTheta * (pointToRotate.x - centerPoint.x) -
                sinTheta * (pointToRotate.y - centerPoint.y) + centerPoint.x),
            y =
                (sinTheta * (pointToRotate.x - centerPoint.x) +
                cosTheta * (pointToRotate.y - centerPoint.y) + centerPoint.y)
        };
    }

    /// <summary>
    /// Calculates the shortest angular difference between two angles.
    /// Returns the signed difference (-180 to +180 degrees).
    /// Positive values indicate counterclockwise rotation needed.
    /// </summary>
    /// <param name="current">Current angle in degrees</param>
    /// <param name="target">Target angle in degrees</param>
    /// <returns>Shortest angular difference in degrees (-180 to +180)</returns>
    public static float CompareAngle (this float current , float target)
    {
        current = ((current + 180f) % 360f);
        target = ((target + 180f) % 360f);

        return (target - current) < 180 ? (target - current) : 360 - (target - current);
    }
}
