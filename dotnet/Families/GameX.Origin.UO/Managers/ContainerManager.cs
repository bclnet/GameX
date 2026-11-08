// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Netcode.UO;
using System.Collections.Generic;

namespace GameX.Origin.UO;

/// <summary>
/// ClassicUO ContainerManager. containers.txt is not read.
/// The table is the gump id, the item-slot bounds, the sounds, and the minimizer.
/// </summary>
public sealed class ContainerManager {
    const int ContainerRectStep = 20;
    const int ContainerRectDefaultPosition = 40;
    const int ContainerRectLineStep = 800;
    const int BankLayer = 0x1D;

    readonly Dictionary<ushort, ContainerData> _data = new Dictionary<ushort, ContainerData>();
    readonly List<UOEquipState> _equip = new List<UOEquipState>();
    readonly List<UOContainedState> _contents = new List<UOContainedState>();
    readonly World _world;

    public ContainerManager(World world) {
        _world = world;
        BuildContainerFile(false);
    }

    public int DefaultX { get; } = 40;
    public int DefaultY { get; } = 40;

    public int X { get; private set; } = 40;
    public int Y { get; private set; } = 40;

    public ContainerData Get(ushort graphic) {
        // A gump missing from the table still opens. It has no sound of its own.
        if (!_data.TryGetValue(graphic, out var value))
            _data[graphic] = value = new ContainerData(graphic, 0, 0, 44, 65, 186, 159);
        return value;
    }

    public void CalculateContainerPosition(uint serial, ushort g) {
        if (!GumpMetrics.TrySize(g, out var gw, out var gh) || gw <= 0 || gh <= 0) return;
        var scale = 1f;
        var width = (int)(gw * scale);
        var height = (int)(gh * scale);
        ClientSize(out var boundsW, out var boundsH);
        // Profile path is not ported.
        if (ProfileOverride(out var setting, out var ox, out var oy)) {
            switch (setting) {
                case 0:
                    SetPositionNearGameObject(g, serial, width, height);
                    break;
                case 1:
                    X = boundsW - width;
                    Y = 0;
                    break;
                case 2:
                case 3:
                    X = ox - (width >> 1);
                    Y = oy - (height >> 1);
                    break;
            }
            if (X + width > boundsW) X -= width;
            if (Y + height > boundsH) Y -= height;
        }
        else {
            var passed = 0;
            for (var i = 0; i < 4 && passed == 0; i++) {
                if (X + width + ContainerRectStep > boundsW) {
                    X = ContainerRectDefaultPosition;
                    if (Y + height + ContainerRectLineStep > boundsH) Y = ContainerRectDefaultPosition;
                    else Y += ContainerRectLineStep;
                }
                else if (Y + height + ContainerRectStep > boundsH) {
                    if (X + width + ContainerRectLineStep > boundsW) X = ContainerRectDefaultPosition;
                    else X += ContainerRectLineStep;
                    Y = ContainerRectDefaultPosition;
                }
                else passed = i + 1;
            }
            if (passed == 0) {
                X = DefaultX;
                Y = DefaultY;
            }
            else if (passed == 1) {
                X += ContainerRectStep;
                Y += ContainerRectStep;
            }
        }
    }

    void SetPositionNearGameObject(ushort g, uint serial, int width, int height) {
        if (!Locate(serial, out var onGround, out var ix, out var iy, out var iz, out var parent)) return;
        ClientSize(out var camW, out var camH);
        var camX = 0;
        var camY = 0;
        var player = UOLoginNet.PlayerSerial;
        var bank = LayerOf(player, BankLayer);
        if (bank != 0 && serial == bank)
            PlaceNear(UOLoginNet.PlayerX, UOLoginNet.PlayerY, UOLoginNet.PlayerZ, camX, camY, camW, camH, height);
        else if (onGround)
            PlaceNear(ix, iy, iz, camX, camY, camW, camH, height);
        else if (SerialHelper.IsMobile(parent)) {
            if (parent == player)
                PlaceNear(UOLoginNet.PlayerX, UOLoginNet.PlayerY, UOLoginNet.PlayerZ, camX, camY, camW, camH, height);
            else if (_world.Mobiles.TryGetValue(parent, out var obj) && obj is Mobile mobile)
                PlaceNear(mobile.X, mobile.Y, mobile.Z, camX, camY, camW, camH, height);
        }
        else {
            var parentContainer = UIManager.GetGump<ContainerGump>(parent);
            if (parentContainer != null) {
                X = parentContainer.X + (width >> 1);
                Y = parentContainer.Y;
            }
        }
    }

    public void BuildContainerFile(bool force) {
        // Profile path is not ported.
        MakeDefault();
    }

    /// <summary>ClassicUO MakeDefault. GRAPHIC OPEN CLOSE LEFT TOP RIGHT BOTTOM, then icon and minimizer when set.</summary>
    void MakeDefault() {
        _data.Clear();
        Add(0x0007, 0x0000, 0x0000, 30, 30, 270, 170);
        Add(0x0009, 0x0000, 0x0000, 20, 85, 124, 196);
        Add(0x003C, 0x0048, 0x0058, 44, 65, 186, 159, 0x0050, 105, 162);
        Add(0x003D, 0x0048, 0x0058, 29, 34, 137, 128);
        Add(0x003E, 0x002F, 0x002E, 33, 36, 142, 148);
        Add(0x003F, 0x004F, 0x0058, 19, 47, 182, 123);
        Add(0x0040, 0x002D, 0x002C, 16, 38, 152, 125);
        Add(0x0041, 0x004F, 0x0058, 40, 30, 139, 123);
        Add(0x0042, 0x002D, 0x002C, 18, 105, 162, 178);
        Add(0x0043, 0x002D, 0x002C, 16, 51, 184, 124);
        Add(0x0044, 0x002D, 0x002C, 20, 10, 170, 100);
        Add(0x0047, 0x0000, 0x0000, 16, 10, 148, 138);
        Add(0x0048, 0x002F, 0x002E, 16, 10, 154, 94);
        Add(0x0049, 0x002D, 0x002C, 18, 105, 162, 178);
        Add(0x004A, 0x002D, 0x002C, 18, 105, 162, 178);
        Add(0x004B, 0x002D, 0x002C, 16, 51, 184, 124);
        Add(0x004C, 0x002D, 0x002C, 46, 74, 196, 184);
        Add(0x004D, 0x002F, 0x002E, 76, 12, 140, 68);
        Add(0x004E, 0x002D, 0x002C, 24, 18, 100, 152);
        Add(0x004F, 0x002D, 0x002C, 24, 18, 100, 152);
        Add(0x0051, 0x002F, 0x002E, 16, 10, 154, 94);
        Add(0x0052, 0x0000, 0x0000, 0, 0, 110, 62);
        Add(0x0102, 0x004F, 0x0058, 35, 10, 190, 95);
        Add(0x0103, 0x0048, 0x0058, 41, 21, 173, 104);
        Add(0x0104, 0x002F, 0x002E, 10, 10, 160, 105);
        Add(0x0105, 0x002F, 0x002E, 10, 10, 160, 105);
        Add(0x0106, 0x002F, 0x002E, 10, 10, 160, 105);
        Add(0x0107, 0x002F, 0x002E, 10, 10, 160, 105);
        Add(0x0108, 0x004F, 0x0058, 10, 10, 160, 105);
        Add(0x0109, 0x002D, 0x002C, 10, 10, 160, 105);
        Add(0x010A, 0x002D, 0x002C, 10, 10, 160, 105);
        Add(0x010B, 0x002D, 0x002C, 10, 10, 160, 105);
        Add(0x010C, 0x002F, 0x002E, 10, 10, 160, 105);
        Add(0x010D, 0x002F, 0x002E, 10, 10, 160, 105);
        Add(0x010E, 0x002F, 0x002E, 10, 10, 160, 105);
        Add(0x0116, 0x0000, 0x0000, 40, 25, 140, 110);
        Add(0x011A, 0x0000, 0x0000, 10, 65, 125, 160);
        Add(0x011B, 0x0000, 0x0000, 45, 10, 175, 95);
        Add(0x011C, 0x0000, 0x0000, 37, 10, 175, 105);
        Add(0x011D, 0x0000, 0x0000, 43, 10, 165, 110);
        Add(0x011E, 0x0000, 0x0000, 30, 22, 263, 106);
        Add(0x011F, 0x0000, 0x0000, 45, 10, 175, 95);
        Add(0x0120, 0x0000, 0x0000, 56, 30, 160, 107);
        Add(0x0121, 0x0000, 0x0000, 77, 32, 162, 107);
        Add(0x0123, 0x0000, 0x0000, 36, 19, 111, 157);
        Add(0x0484, 0x0000, 0x0000, 0, 45, 175, 125);
        Add(0x058E, 0x0000, 0x0000, 50, 150, 348, 250);
        Add(0x06D3, 0x0000, 0x0000, 10, 65, 125, 160);
        Add(0x06D4, 0x0000, 0x0000, 10, 65, 125, 160);
        Add(0x06D5, 0x0000, 0x0000, 10, 65, 125, 160);
        Add(0x06D6, 0x0000, 0x0000, 10, 65, 125, 160);
        Add(0x06E5, 0x0000, 0x0000, 66, 74, 306, 520);
        Add(0x06E6, 0x0000, 0x0000, 66, 74, 306, 520);
        Add(0x06E7, 0x0000, 0x0000, 50, 60, 548, 308);
        Add(0x06E8, 0x0000, 0x0000, 50, 60, 548, 308);
        Add(0x06E9, 0x0000, 0x0000, 60, 80, 318, 324);
        Add(0x06EA, 0x0000, 0x0000, 50, 60, 548, 308);
        Add(0x091A, 0x0000, 0x0000, 0, 0, 282, 230);
        Add(0x092E, 0x0000, 0x0000, 0, 0, 282, 210);
        Add(0x266A, 0x0000, 0x0000, 16, 51, 184, 124);
        Add(0x266B, 0x0000, 0x0000, 16, 51, 184, 124);
        Add(0x2A63, 0x0187, 0x01C9, 60, 33, 460, 348);
        // 0x2A63 was also tried as 29, 34, 137, 128. That area is bugged in the original client.
        Add(0x4D0C, 0x0000, 0x0000, 25, 65, 220, 155);
        Add(0x775E, 0x0048, 0x0058, 44, 65, 186, 159, 0x775F, 105, 178);
        Add(0x7760, 0x0048, 0x0058, 44, 65, 186, 159, 0x7761, 105, 178);
        Add(0x7762, 0x0048, 0x0058, 44, 65, 186, 159, 0x7763, 105, 178);
        Add(0x777A, 0x0000, 0x0000, 32, 40, 184, 116);
        Add(0x9CD9, 0x0000, 0x0000, 10, 10, 160, 105);
        Add(0x9CDB, 0x0000, 0x0000, 50, 60, 548, 308);
        Add(0x9CDD, 0x0000, 0x0000, 50, 60, 548, 308);
        Add(0x9CDF, 0x0000, 0x0000, 50, 60, 548, 308);
        Add(0x9CE3, 0x0000, 0x0000, 50, 60, 548, 308);
        Add(0x9CE4, 0x0000, 0x0000, 44, 65, 186, 159);
        Add(0x9CE5, 0x0000, 0x0000, 44, 65, 186, 159);
        Add(0x9CE7, 0x0000, 0x0000, 44, 65, 186, 159);
    }

    void Add(ushort graphic, ushort openSound, ushort closedSound, int x, int y, int width, int height, ushort iconizedGraphic = 0, int minimizerX = 0, int minimizerY = 0) {
        _data[graphic] = new ContainerData(graphic, openSound, closedSound, x, y, width, height, iconizedGraphic, minimizerX, minimizerY);
    }

    bool Locate(uint serial, out bool onGround, out int x, out int y, out int z, out uint parent) {
        onGround = false;
        x = y = z = 0;
        parent = 0;
        if (_world.Items.TryGetValue(serial, out var obj) && obj is Item item) {
            onGround = true;
            x = item.X;
            y = item.Y;
            z = item.Z;
            return true;
        }
        var player = UOLoginNet.PlayerSerial;
        if (Held(player, serial)) {
            parent = player;
            return true;
        }
        foreach (var kv in _world.Mobiles) {
            if (kv.Key == player || !Held(kv.Key, serial)) continue;
            parent = kv.Key;
            return true;
        }
        foreach (var gump in UIManager.Gumps) {
            if (gump.IsDisposed || gump is not ContainerGump) continue;
            UOLoginNet.CopyContents(gump.ServerSerial, _contents);
            for (var i = 0; i < _contents.Count; i++) {
                if (_contents[i].Serial != serial) continue;
                parent = _contents[i].Container;
                return true;
            }
        }
        return false;
    }

    bool Held(uint mobile, uint serial) {
        if (mobile == 0 || serial == 0) return false;
        UOLoginNet.CopyEquip(mobile, _equip);
        for (var i = 0; i < _equip.Count; i++)
            if (_equip[i].Serial == serial) return true;
        return false;
    }

    uint LayerOf(uint mobile, byte layer) {
        if (mobile == 0) return 0;
        UOLoginNet.CopyEquip(mobile, _equip);
        for (var i = 0; i < _equip.Count; i++)
            if (_equip[i].Layer == layer) return _equip[i].Serial;
        return 0;
    }

    void PlaceNear(int ex, int ey, int ez, int camX, int camY, int camW, int camH, int height) {
        Screen(ex, ey, ez, camX, camY, camW, camH, out var sx, out var sy);
        X = sx + 40;
        Y = sy - (height >> 1);
    }

    /// <summary>
    /// ClassicUO RealScreenPosition plus the camera origin.
    /// The player sub-tile offset is not stored, so that term is 0.
    /// </summary>
    static void Screen(int ex, int ey, int ez, int camX, int camY, int camW, int camH, out int sx, out int sy) {
        var px = UOLoginNet.PlayerX;
        var py = UOLoginNet.PlayerY;
        var pz = UOLoginNet.PlayerZ;
        var centerX = camW >> 1;
        var centerY = (camH >> 1) + (pz << 2);
        var offsetX = ((px - py) * 22) - centerX;
        var offsetY = ((px + py) * 22) - centerY;
        sx = (((ex - ey) * 22) - offsetX - 22) + camX;
        sy = (((ex + ey) * 22 - (ez << 2)) - offsetY - 22) + camY;
    }

    static void ClientSize(out int w, out int h) {
        var s = Mouse.Current;
        w = s.BufW > 0 ? s.BufW : 640;
        h = s.BufH > 0 ? s.BufH : 480;
    }

    /// <summary>Profile OverrideContainerLocation. Defaults are off, setting 0, position 200,200. Returns false.</summary>
    static bool ProfileOverride(out int setting, out int x, out int y) {
        setting = 0;
        x = 200;
        y = 200;
        return false;
    }

    /// <summary>
    /// ClassicUO ContainerData. X and Y are the left and top.
    /// Width and Height are the right and bottom edges, not a size.
    /// A set minimizer is a 16 by 16 rectangle. An unset one is 0,0,0,0.
    /// </summary>
    public readonly struct ContainerData {
        public readonly ushort Graphic;
        public readonly ushort OpenSound;
        public readonly ushort ClosedSound;
        public readonly int X, Y, Width, Height;
        public readonly ushort IconizedGraphic;
        public readonly int MinimizerX, MinimizerY, MinimizerWidth, MinimizerHeight;

        public ContainerData(ushort graphic, ushort openSound, ushort closedSound, int x, int y, int width, int height, ushort iconizedGraphic = 0, int minimizerX = 0, int minimizerY = 0) {
            Graphic = graphic;
            OpenSound = openSound;
            ClosedSound = closedSound;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            IconizedGraphic = iconizedGraphic;
            if (minimizerX == 0 && minimizerY == 0)
                MinimizerX = MinimizerY = MinimizerWidth = MinimizerHeight = 0;
            else {
                MinimizerX = minimizerX;
                MinimizerY = minimizerY;
                MinimizerWidth = 16;
                MinimizerHeight = 16;
            }
        }
    }
}
