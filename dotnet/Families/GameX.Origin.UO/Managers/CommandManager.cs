// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO CommandManager. The table is info, datetime, hue, and debug.
/// info opens InspectorGump after a client-side target. That gump is not in this tree.
/// </summary>
public sealed class CommandManager {
    readonly Dictionary<string, Action<string[]>> _commands = new();
    readonly World _world;

    /// <summary>ClassicUO CUOEnviroment.Debug. The debug command flips this.</summary>
    public static bool Debug;

    public CommandManager(World world) => _world = world;

    public void Initialize() {
        Register("info", Info);
        Register("datetime", Datetime);
        Register("hue", Hue);
        Register("debug", ToggleDebug);
    }

    public void Register(string name, Action<string[]> callback) {
        name = name.ToLower();
        if (!_commands.ContainsKey(name)) _commands.Add(name, callback);
        else Trace.WriteLine($"Attempted to register command: '{name}' twice.");
    }

    public void UnRegister(string name) {
        name = name.ToLower();
        if (_commands.ContainsKey(name)) _commands.Remove(name);
    }

    public void UnRegisterAll() => _commands.Clear();

    public void Execute(string name, params string[] args) {
        name = name.ToLower();
        if (_commands.TryGetValue(name, out Action<string[]> action)) action.Invoke(args);
        else Trace.WriteLine($"Command: '{name}' not exists");
    }

    public void OnHueTarget(object entity) {
        Mouse.LastLeftButtonClickTime = 0;
        if (entity == null) return;
        // gump is not ported
        if (!GraphicOf(entity, out var graphic)) return;
        // Mobile and Item do not store hue. MessageManager is not ported.
        Trace.WriteLine(string.Format("Item ID: {0}\nHue: {1}", graphic, 0));
    }

    void Info(string[] s) {
        if (UOLoginNet.HasTarget) UOLoginNet.CancelTarget();
        // gump is not ported
    }

    void Datetime(string[] s) {
        if (UOLoginNet.PlayerSerial == 0) return;
        Trace.WriteLine(string.Format("Current DateTime.Now is {0}", DateTime.Now));
    }

    void Hue(string[] s) {
        if (UOLoginNet.HasTarget) UOLoginNet.CancelTarget();
        // gump is not ported
    }

    void ToggleDebug(string[] s) => Debug = !Debug;

    bool GraphicOf(object entity, out ushort graphic) {
        if (entity is Mobile mobile) {
            graphic = mobile.Body;
            return true;
        }
        if (entity is Item item) {
            graphic = item.Graphic;
            return true;
        }
        if (entity is uint serial) {
            if (_world.Mobiles.TryGetValue(serial, out var mob) && mob is Mobile m) {
                graphic = m.Body;
                return true;
            }
            if (_world.Items.TryGetValue(serial, out var it) && it is Item i) {
                graphic = i.Graphic;
                return true;
            }
        }
        graphic = 0;
        return false;
    }
}
