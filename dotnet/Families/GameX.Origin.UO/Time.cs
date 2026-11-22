// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>ClassicUO Time. Ticks is milliseconds. LoginScene.Update copies GlobalTime.Ticks and GlobalTime.Delta each frame.</summary>
public static class Time {
    public static uint Ticks;
    public static float Delta;
}
