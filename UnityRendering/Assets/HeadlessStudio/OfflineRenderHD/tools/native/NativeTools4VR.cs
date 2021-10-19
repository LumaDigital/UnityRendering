using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

public static class NativeTools4VR
{
    [DllImport("tools4vr", CallingConvention = CallingConvention.Cdecl, EntryPoint = "build_equirectangular_lut")]
    private static extern unsafe void BuildNative(void* limits_ptr, void* data_ptr, uint width, uint height, char mode);

    /// <summary>
    /// Generates a equirectangular image from a limit spherical projection on cube maps.
    /// WIP, still has a few issues. Horizontal limit needs to be half of what is wanted.
    /// </summary>
    /// <param name="limits">Horinztal limit</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="faceSize"></param>
    /// <returns></returns>
    public static unsafe ComputeBuffer Build(NativeArray<double> limits, int width, int height, uint mode)
    {
        var limits_ptr = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(limits);
        var data = new NativeArray<byte>(width * height * 3 * sizeof(float), Allocator.Temp);
        var data_ptr = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(data);
        BuildNative(limits_ptr, data_ptr, Convert.ToUInt32(width), Convert.ToUInt32(height), (char)mode);
        var buffer = new ComputeBuffer(data.Length, 4);
        buffer.SetData(data);
        return buffer;
    }
}
