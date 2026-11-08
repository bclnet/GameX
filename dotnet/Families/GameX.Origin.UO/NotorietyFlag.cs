// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>ClassicUO NotorietyFlag. Hues are Profile.cs defaults. Profile overrides are not ported.</summary>
public enum NotorietyFlag : byte {
    Unknown = 0x00,
    Innocent = 0x01,
    Ally = 0x02,
    Gray = 0x03,
    Criminal = 0x04,
    Enemy = 0x05,
    Murderer = 0x06,
    Invulnerable = 0x07
}

public static class Notoriety {
    public static ushort GetHue(NotorietyFlag flag) {
        switch (flag) {
            case NotorietyFlag.Innocent: return 0x005A;
            case NotorietyFlag.Ally: return 0x0044;
            case NotorietyFlag.Criminal: return 0x03B2;
            case NotorietyFlag.Gray: return 0x03B2;
            case NotorietyFlag.Enemy: return 0x0031;
            case NotorietyFlag.Murderer: return 0x0023;
            case NotorietyFlag.Invulnerable: return 0x0034;
            default: return 0;
        }
    }

    public static string GetHTMLHue(NotorietyFlag flag) {
        switch (flag) {
            case NotorietyFlag.Innocent: return "<basefont color=\"cyan\">";
            case NotorietyFlag.Ally: return "<basefont color=\"lime\">";
            case NotorietyFlag.Criminal:
            case NotorietyFlag.Gray: return "<basefont color=\"gray\">";
            case NotorietyFlag.Enemy: return "<basefont color=\"orange\">";
            case NotorietyFlag.Murderer: return "<basefont color=\"red\">";
            case NotorietyFlag.Invulnerable: return "<basefont color=\"yellow\">";
            default: return string.Empty;
        }
    }
}
