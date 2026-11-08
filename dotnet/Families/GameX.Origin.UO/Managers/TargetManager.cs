// SPDX-License-Identifier: BSD-2-Clause

using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using System;

namespace GameX.Origin.UO;

public enum CursorTarget {
    Invalid = -1,
    Object = 0,
    Position = 1,
    MultiPlacement = 2,
    SetTargetClientSide = 3,
    Grab,
    SetGrabBag,
    HueCommandTarget,
    IgnorePlayerTarget,
    CallbackTarget
}

public class CursorType {
    public static readonly uint Target = 6983686;
}

public enum TargetType {
    Neutral,
    Harmful,
    Beneficial,
    Cancel
}

public class MultiTargetInfo {
    public MultiTargetInfo(ushort model, ushort x, ushort y, ushort z, ushort hue) {
        Model = model;
        XOff = x;
        YOff = y;
        ZOff = z;
        Hue = hue;
    }

    public readonly ushort XOff, YOff, ZOff, Model, Hue;
}

public class LastTargetInfo {
    public bool IsEntity => SerialHelper.IsValid(Serial);
    public bool IsStatic => !IsEntity && Graphic != 0 && Graphic != 0xFFFF;
    public bool IsLand => !IsStatic;
    public ushort Graphic;
    public uint Serial;
    public ushort X, Y;
    public sbyte Z;

    public void SetEntity(uint serial) {
        Serial = serial;
        Graphic = 0xFFFF;
        X = Y = 0xFFFF;
        Z = sbyte.MinValue;
    }

    public void SetStatic(ushort graphic, ushort x, ushort y, sbyte z) {
        Serial = 0;
        Graphic = graphic;
        X = x;
        Y = y;
        Z = z;
    }

    public void SetLand(ushort x, ushort y, sbyte z) {
        Serial = 0;
        Graphic = 0xFFFF;
        X = x;
        Y = y;
        Z = z;
    }

    public void Clear() {
        Serial = 0;
        Graphic = 0xFFFF;
        X = Y = 0xFFFF;
        Z = sbyte.MinValue;
    }
}

/// <summary>
/// ClassicUO TargetManager. Cursor flags, the last target serial, and SetTargeting, CancelTarget, and Target.
/// A callback takes serial, graphic, x, y, and z. Serial 0 is no object.
/// </summary>
public sealed class TargetManager {
    uint _targetCursorId;
    readonly World _world;
    readonly byte[] _lastDataBuffer = new byte[19];
    Action<uint, ushort, int, int, sbyte> _targetCallback;

    public TargetManager(World world) { _world = world; }

    public uint LastAttack, SelectedTarget, NewTargetSystemSerial;

    public readonly LastTargetInfo LastTargetInfo = new LastTargetInfo();

    public MultiTargetInfo MultiTargetInfo { get; private set; }

    public CursorTarget TargetingState { get; private set; } = CursorTarget.Invalid;

    public bool IsTargeting { get; private set; }

    public TargetType TargetingType { get; private set; }

    void ClearTargetingWithoutTargetCancelPacket() {
        if (TargetingState == CursorTarget.MultiPlacement) {
            MultiTargetInfo = null;
            TargetingState = 0;
            // no HouseManager in this tree
        }
        IsTargeting = false;
    }

    public void Reset() {
        ClearTargetingWithoutTargetCancelPacket();
        _targetCallback = null;
        TargetingState = 0;
        _targetCursorId = 0;
        MultiTargetInfo = null;
        TargetingType = 0;
    }

    public void SetTargeting(Action<uint, ushort, int, int, sbyte> callback, uint cursorID, TargetType cursorType) {
        SetTargeting(CursorTarget.CallbackTarget, cursorID, cursorType);
        _targetCallback = callback;
    }

    public void SetTargeting(CursorTarget targeting, uint cursorID, TargetType cursorType) {
        if (targeting == CursorTarget.Invalid) return;

        var lastTargetting = IsTargeting;
        IsTargeting = cursorType < TargetType.Cancel;
        TargetingState = targeting;
        TargetingType = cursorType;

        if (IsTargeting) {
            //UIManager.RemoveTargetLineGump(LastTarget);
        }
        else if (lastTargetting) CancelTarget();

        // A server cancel still has to answer with the cursor that was active, so the id is stored after that.
        _targetCursorId = cursorID;
    }

    public void CancelTarget() {
        if (TargetingState == CursorTarget.MultiPlacement) {
            // no HouseManager in this tree
            // no CustomHouseManager in this tree
            UIManager.GetGump<HouseCustomizationGump>()?.Update();
        }

        if (TargetingState == CursorTarget.CallbackTarget)
            _targetCallback?.Invoke(0, 0, 0, 0, 0);

        if (IsTargeting || TargetingType == TargetType.Cancel) {
            UOLoginNet.Send(OutgoingPackets.Send_TargetCancel((byte)TargetingState, _targetCursorId, (byte)TargetingType));
            IsTargeting = false;
        }

        Reset();
    }

    public void SetTargetingMulti(uint deedSerial, ushort model, ushort x, ushort y, ushort z, ushort hue) {
        SetTargeting(CursorTarget.MultiPlacement, deedSerial, TargetType.Neutral);
        MultiTargetInfo = new MultiTargetInfo(model, x, y, z, hue);
    }

    public void Target(uint serial, ushort graphic, int x, int y, sbyte z) {
        if (!IsTargeting) return;
        if (serial == 0) return;

        switch (TargetingState) {
            case CursorTarget.Invalid: return;

            case CursorTarget.MultiPlacement:
            case CursorTarget.Position:
            case CursorTarget.Object:
            case CursorTarget.HueCommandTarget:
            case CursorTarget.SetTargetClientSide:
                if (serial != UOLoginNet.PlayerSerial)
                    LastTargetInfo.SetEntity(serial);

                if (SerialHelper.IsMobile(serial) && serial != UOLoginNet.PlayerSerial) {
                    var playerNoto = (NotorietyFlag)UOLoginNet.Notoriety;
                    if (playerNoto == NotorietyFlag.Innocent || playerNoto == NotorietyFlag.Ally) {
                        var mobileNoto = (NotorietyFlag)UOLoginNet.MobileNotoriety(serial);
                        // Profile overrides are not ported. Harmful query defaults on. Beneficial query defaults off.
                        var showCriminalQuery = TargetingType == TargetType.Harmful && mobileNoto == NotorietyFlag.Innocent;

                        if (showCriminalQuery && UIManager.GetGump<QuestionGump>() == null) {
                            var messageBox = new QuestionGump(_world, "This may flag\nyou criminal!", s => {
                                if (!s) return;
                                UOLoginNet.Send(OutgoingPackets.Send_TargetObject(serial, graphic, (ushort)x, (ushort)y, z, _targetCursorId, (byte)TargetingType));
                                ClearTargetingWithoutTargetCancelPacket();
                                // no HitsRequest in this tree
                                if (LastTargetInfo.Serial != serial && SerialHelper.IsValid(serial))
                                    UOLoginNet.Send(OutgoingPackets.Send_StatusRequest(serial));
                            });
                            UIManager.Add(messageBox);
                            return;
                        }
                    }
                }

                if (TargetingState != CursorTarget.SetTargetClientSide) {
                    _lastDataBuffer[0] = 0x6C;
                    _lastDataBuffer[1] = 0x00;
                    _lastDataBuffer[2] = (byte)(_targetCursorId >> 24);
                    _lastDataBuffer[3] = (byte)(_targetCursorId >> 16);
                    _lastDataBuffer[4] = (byte)(_targetCursorId >> 8);
                    _lastDataBuffer[5] = (byte)_targetCursorId;
                    _lastDataBuffer[6] = (byte)TargetingType;
                    _lastDataBuffer[7] = (byte)(serial >> 24);
                    _lastDataBuffer[8] = (byte)(serial >> 16);
                    _lastDataBuffer[9] = (byte)(serial >> 8);
                    _lastDataBuffer[10] = (byte)serial;
                    _lastDataBuffer[11] = (byte)(x >> 8);
                    _lastDataBuffer[12] = (byte)x;
                    _lastDataBuffer[13] = (byte)(y >> 8);
                    _lastDataBuffer[14] = (byte)y;
                    _lastDataBuffer[15] = (byte)(z >> 8);
                    _lastDataBuffer[16] = (byte)z;
                    _lastDataBuffer[17] = (byte)(graphic >> 8);
                    _lastDataBuffer[18] = (byte)graphic;

                    UOLoginNet.Send(OutgoingPackets.Send_TargetObject(serial, graphic, (ushort)x, (ushort)y, z, _targetCursorId, (byte)TargetingType));

                    // no HitsRequest in this tree
                    if (SerialHelper.IsMobile(serial) && LastTargetInfo.Serial != serial && SerialHelper.IsValid(serial))
                        UOLoginNet.Send(OutgoingPackets.Send_StatusRequest(serial));
                }

                ClearTargetingWithoutTargetCancelPacket();
                Mouse.CancelDoubleClick = true;
                break;

            case CursorTarget.Grab:
                if (SerialHelper.IsItem(serial)) {
                    var bag = UOLoginNet.PlayerBackpack;
                    // no GrabBagSerial in this tree
                    if (bag != 0) {
                        var amount = UOLoginNet.AmountOf(serial);
                        // a ground stack amount is not stored
                        if (amount == 0) amount = 1;
                        UOLoginNet.Send(OutgoingPackets.Send_PickUpRequest(serial, amount));
                        var version = UltimaOnline.Current?.Version ?? 0;
                        if (version >= ClientVersion.CV_6017)
                            UOLoginNet.Send(OutgoingPackets.Send_DropRequest(serial, 0xFFFF, 0xFFFF, 0, 0, bag));
                        else
                            UOLoginNet.Send(OutgoingPackets.Send_DropRequest_Old(serial, 0xFFFF, 0xFFFF, 0, bag));
                    }
                }
                ClearTargetingWithoutTargetCancelPacket();
                return;

            case CursorTarget.SetGrabBag:
                if (SerialHelper.IsItem(serial)) {
                    // no GrabBagSerial in this tree
                    // no Print in this tree
                }
                ClearTargetingWithoutTargetCancelPacket();
                return;

            case CursorTarget.IgnorePlayerTarget:
                // no SelectedObject in this tree
                CancelTarget();
                return;

            case CursorTarget.CallbackTarget:
                _targetCallback?.Invoke(serial, graphic, x, y, z);
                ClearTargetingWithoutTargetCancelPacket();
                return;
        }
    }

    public void Target(ushort graphic, ushort x, ushort y, short z, bool wet = false) {
        if (!IsTargeting) return;

        switch (TargetingState) {
            case CursorTarget.CallbackTarget:
                // no Map tiles in this tree
                ClearTargetingWithoutTargetCancelPacket();
                return;
        }

        if (graphic == 0) {
            if (TargetingState == CursorTarget.Object) return;
        }
        else {
            // no StaticData in this tree
        }

        LastTargetInfo.SetStatic(graphic, x, y, (sbyte)z);
        TargetPacket(graphic, x, y, (sbyte)z);
    }

    public void SendMultiTarget(ushort x, ushort y, sbyte z) {
        TargetPacket(0, x, y, z);
        MultiTargetInfo = null;
    }

    public void TargetLast() {
        if (!IsTargeting) return;

        _lastDataBuffer[0] = 0x6C;
        _lastDataBuffer[1] = (byte)TargetingState;
        _lastDataBuffer[2] = (byte)(_targetCursorId >> 24);
        _lastDataBuffer[3] = (byte)(_targetCursorId >> 16);
        _lastDataBuffer[4] = (byte)(_targetCursorId >> 8);
        _lastDataBuffer[5] = (byte)_targetCursorId;
        _lastDataBuffer[6] = (byte)TargetingType;

        var kind = _lastDataBuffer[1];
        var serial = ((uint)_lastDataBuffer[7] << 24) | ((uint)_lastDataBuffer[8] << 16) | ((uint)_lastDataBuffer[9] << 8) | _lastDataBuffer[10];
        var x = (ushort)((_lastDataBuffer[11] << 8) | _lastDataBuffer[12]);
        var y = (ushort)((_lastDataBuffer[13] << 8) | _lastDataBuffer[14]);
        var z = (sbyte)_lastDataBuffer[16];
        var graphic = (ushort)((_lastDataBuffer[17] << 8) | _lastDataBuffer[18]);
        if (kind == 0x00)
            UOLoginNet.Send(OutgoingPackets.Send_TargetObject(serial, graphic, x, y, z, _targetCursorId, (byte)TargetingType));
        else if (kind == 0x01 && serial == 0)
            UOLoginNet.Send(OutgoingPackets.Send_TargetXYZ(graphic, x, y, z, _targetCursorId, (byte)TargetingType));
        else {
            // no Send_* in this tree
        }

        Mouse.CancelDoubleClick = true;
        ClearTargetingWithoutTargetCancelPacket();
    }

    void TargetPacket(ushort graphic, ushort x, ushort y, sbyte z) {
        if (!IsTargeting) return;

        _lastDataBuffer[0] = 0x6C;
        _lastDataBuffer[1] = 0x01;
        _lastDataBuffer[2] = (byte)(_targetCursorId >> 24);
        _lastDataBuffer[3] = (byte)(_targetCursorId >> 16);
        _lastDataBuffer[4] = (byte)(_targetCursorId >> 8);
        _lastDataBuffer[5] = (byte)_targetCursorId;
        _lastDataBuffer[6] = (byte)TargetingType;
        _lastDataBuffer[7] = 0;
        _lastDataBuffer[8] = 0;
        _lastDataBuffer[9] = 0;
        _lastDataBuffer[10] = 0;
        _lastDataBuffer[11] = (byte)(x >> 8);
        _lastDataBuffer[12] = (byte)x;
        _lastDataBuffer[13] = (byte)(y >> 8);
        _lastDataBuffer[14] = (byte)y;
        _lastDataBuffer[15] = (byte)(z >> 8);
        _lastDataBuffer[16] = (byte)z;
        _lastDataBuffer[17] = (byte)(graphic >> 8);
        _lastDataBuffer[18] = (byte)graphic;

        UOLoginNet.Send(OutgoingPackets.Send_TargetXYZ(graphic, x, y, z, _targetCursorId, (byte)TargetingType));
        Mouse.CancelDoubleClick = true;
        ClearTargetingWithoutTargetCancelPacket();
    }
}
