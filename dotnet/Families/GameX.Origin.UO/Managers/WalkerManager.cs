// SPDX-License-Identifier: BSD-2-Clause

// UO Movement System
//
// Turns (direction changes) have no delay. The client sends a 0x02 walk request
// with the new direction and the server confirms it immediately. Both turns and
// tile movements use the same packet (0x02) and confirmation (0x22).
//
// Movement speeds (retail):
//   Walking:       400ms per tile  (2.5 tiles/sec)
//   Running:       200ms per tile  (5.0 tiles/sec)
//   Mounted walk:  200ms per tile  (5.0 tiles/sec)
//   Mounted run:   100ms per tile  (10.0 tiles/sec)
//
// The client may queue up to 5 unconfirmed walk packets (MaxStepCount).
// The Enhanced Client (EC) allows 4 queued moves including the initial turn.
// If the server falls behind on confirmations and the queue fills, the client
// must wait before sending more movement packets.
//
// Each walk request carries a sequence byte (0-255, wrapping). The server echoes
// the sequence in its 0x22 confirmation. If a confirmation arrives for an unknown
// sequence, the client triggers a resync (0x22 "bad step" path).

using GameX.Origin.Formats.UO;

namespace GameX.Origin.UO;

public struct StepInfo {
    public byte Direction;
    public byte OldDirection;
    public byte Sequence;
    public bool Accepted;
    public bool Running;
    public bool NoRotation;
    public long Timer;
    public ushort X, Y;
    public sbyte Z;
}

/// <summary>ClassicUO Mobile.Step. The queued tile, not the confirmed StepInfo.</summary>
public struct Step {
    public int X, Y;
    public sbyte Z;
    public byte Direction;
    public bool Run;
}

public enum CharacterSpeedType {
    Normal,
    FastUnmount,
    CantRun,
    FastUnmountAndCantRun
}

public sealed class FastWalkStack {
    const int MaxFastWalkStackSize = 5;
    readonly uint[] _keys = new uint[MaxFastWalkStackSize];

    public void SetValue(int index, uint value) {
        if (index >= 0 && index < MaxFastWalkStackSize)
            _keys[index] = value;
    }

    public void AddValue(uint value) {
        for (int i = 0; i < MaxFastWalkStackSize; i++) {
            if (_keys[i] == 0) {
                _keys[i] = value;
                break;
            }
        }
    }

    public uint GetValue() {
        for (int i = 0; i < MaxFastWalkStackSize; i++) {
            uint key = _keys[i];
            if (key != 0) {
                _keys[i] = 0;
                return key;
            }
        }
        return 0;
    }
}

public sealed class WalkerManager {
    const int MaxStepCount = 5;
    const int TurnDelayNormal = 80;
    const int TurnDelayFast = 45;
    const int StepDelayWalk = 400;
    const int StepDelayRun = 200;
    const int StepDelayMountWalk = 200;
    const int StepDelayMountRun = 100;
    static readonly sbyte[] DirOffset = { 1, -1 };

    public ushort X, Y;
    public sbyte Z;
    public Direction Direction;
    public Direction ServerDirection;
    public int RangeSizeX, RangeSizeY;
    public readonly Deque<Step> Steps = new Deque<Step>(MaxStepCount);
    public uint LastStepTime;
    public bool IsParalyzed;
    public bool AlwaysRun;
    public bool AlwaysRunUnlessHidden;
    public bool IsDead;
    public bool IsHidden;
    public bool IsMounted;
    public bool IsFlying;
    public bool FastRotation;
    public CharacterSpeedType SpeedMode;
    public ushort Stamina;
    bool _serverDirectionInitialized;

    public FastWalkStack FastWalkStack { get; } = new FastWalkStack();
    public ushort CurrentPlayerZ;
    public byte CurrentWalkSequence;
    public long LastStepRequestTime;
    public ushort NewPlayerZ;
    public bool ResendPacketResync;
    public readonly StepInfo[] StepInfos = new StepInfo[MaxStepCount];
    public int StepsCount;
    public int UnacceptedPacketsCount;
    public bool WalkingFailed;
    public byte WalkSequence;
    public bool WantChangeCoordinates;

    int TurnDelay => FastRotation ? TurnDelayFast : TurnDelayNormal;

    public void DenyWalk(byte sequence, int x, int y, sbyte z) {
        Steps.Clear();
        Reset();
        if (x != -1) {
            RangeSizeX = x;
            RangeSizeY = y;
            X = (ushort)x;
            Y = (ushort)y;
            Z = z;
        }
        ServerDirection = Direction;
    }

    public void ConfirmWalk(byte sequence) {
        if (UnacceptedPacketsCount != 0)
            UnacceptedPacketsCount--;

        int stepIndex = 0;
        for (int i = 0; i < StepsCount; i++) {
            if (StepInfos[i].Sequence == sequence)
                break;
            stepIndex++;
        }

        bool isBadStep = stepIndex == StepsCount;
        if (!isBadStep) {
            if (stepIndex >= CurrentWalkSequence) {
                StepInfos[stepIndex].Accepted = true;
                RangeSizeX = StepInfos[stepIndex].X;
                RangeSizeY = StepInfos[stepIndex].Y;
            }
            else if (stepIndex == 0) {
                RangeSizeX = StepInfos[0].X;
                RangeSizeY = StepInfos[0].Y;
                for (int i = 1; i < StepsCount; i++)
                    StepInfos[i - 1] = StepInfos[i];
                StepsCount--;
                CurrentWalkSequence--;
            }
            else
                isBadStep = true;
        }

        if (isBadStep) {
            if (!ResendPacketResync) {
                // no Send_* in this tree
                ResendPacketResync = true;
            }
            WalkingFailed = true;
            StepsCount = 0;
            CurrentWalkSequence = 0;
        }
    }

    public void Reset() {
        UnacceptedPacketsCount = 0;
        StepsCount = 0;
        WalkSequence = 0;
        CurrentWalkSequence = 0;
        WalkingFailed = false;
        ResendPacketResync = false;
        LastStepRequestTime = 0;
    }

    public bool Walk(Direction direction, bool run) {
        ClientVersion version = UltimaOnline.Current != null ? UltimaOnline.Current.Version : default(ClientVersion);
        if (WalkingFailed || LastStepRequestTime > Time.Ticks || StepsCount >= MaxStepCount || version >= ClientVersion.CV_60142 && IsParalyzed)
            return false;

        run |= AlwaysRun;
        if (SpeedMode >= CharacterSpeedType.CantRun || Stamina <= 1 && !IsDead || IsHidden && AlwaysRunUnlessHidden)
            run = false;

        int x = X;
        int y = Y;
        sbyte z = Z;
        if (!_serverDirectionInitialized) {
            ServerDirection = Direction;
            _serverDirectionInitialized = true;
        }

        Direction oldDirection = ServerDirection;
        bool emptyStack = Steps.Count == 0;
        if (!emptyStack) {
            ref Step walkStep = ref Steps.Back();
            x = walkStep.X;
            y = walkStep.Y;
            z = walkStep.Z;
            oldDirection = (Direction)walkStep.Direction;
        }

        sbyte oldZ = z;
        ushort walkTime = (ushort)TurnDelay;
        if ((oldDirection & Direction.Mask) == (direction & Direction.Mask)) {
            Direction newDir = direction;
            int newX = x;
            int newY = y;
            sbyte newZ = z;
            if (!CanWalk(ref newDir, ref newX, ref newY, ref newZ))
                return false;
            if ((direction & Direction.Mask) != newDir) {
                direction = newDir;
            }
            else {
                direction = newDir;
                x = newX;
                y = newY;
                z = newZ;
                walkTime = (ushort)TimeToCompleteMovement(run, IsMounted || SpeedMode == CharacterSpeedType.FastUnmount || SpeedMode == CharacterSpeedType.FastUnmountAndCantRun || IsFlying);
            }
        }
        else {
            Direction newDir = direction;
            int newX = x;
            int newY = y;
            sbyte newZ = z;
            if (!CanWalk(ref newDir, ref newX, ref newY, ref newZ)) {
                if ((oldDirection & Direction.Mask) == newDir)
                    return false;
            }
            if ((oldDirection & Direction.Mask) == newDir) {
                x = newX;
                y = newY;
                z = newZ;
                walkTime = (ushort)TimeToCompleteMovement(run, IsMounted || SpeedMode == CharacterSpeedType.FastUnmount || SpeedMode == CharacterSpeedType.FastUnmountAndCantRun || IsFlying);
            }
            direction = newDir;
        }

        if (walkTime == (ushort)TurnDelay && UnacceptedPacketsCount >= 3) {
            Direction = direction;
            LastStepRequestTime = Time.Ticks + walkTime;
            return true;
        }

        if (emptyStack)
            LastStepTime = Time.Ticks;

        ref StepInfo step = ref StepInfos[StepsCount];
        step.Sequence = WalkSequence;
        step.Accepted = false;
        step.Running = run;
        step.OldDirection = (byte)(oldDirection & Direction.Mask);
        step.Direction = (byte)direction;
        step.Timer = Time.Ticks;
        step.X = (ushort)x;
        step.Y = (ushort)y;
        step.Z = z;
        step.NoRotation = step.OldDirection == (byte)direction && oldZ - z >= 11;
        StepsCount++;

        Steps.AddToBack(new Step {
            X = x,
            Y = y,
            Z = z,
            Direction = (byte)direction,
            Run = run
        });

        _ = FastWalkStack.GetValue();
        // no Send_* in this tree
        ServerDirection = direction;
        if (WalkSequence == 0xFF)
            WalkSequence = 1;
        else
            WalkSequence++;
        UnacceptedPacketsCount++;
        LastStepRequestTime = Time.Ticks + walkTime;
        return true;
    }

    bool CanWalk(ref Direction direction, ref int x, ref int y, ref sbyte z) {
        int newX = x;
        int newY = y;
        sbyte newZ = z;
        byte newDirection = (byte)direction;
        GetNewXY((byte)direction, ref newX, ref newY);
        bool passed = CalculateNewZ(newX, newY, ref newZ);
        if ((sbyte)direction % 2 != 0) {
            if (passed) {
                for (int i = 0; i < 2 && passed; i++) {
                    int testX = x;
                    int testY = y;
                    sbyte testZ = z;
                    byte testDir = (byte)(((byte)direction + DirOffset[i]) % 8);
                    GetNewXY(testDir, ref testX, ref testY);
                    passed = CalculateNewZ(testX, testY, ref testZ);
                }
            }
            if (!passed) {
                for (int i = 0; i < 2 && !passed; i++) {
                    newX = x;
                    newY = y;
                    newZ = z;
                    newDirection = (byte)(((byte)direction + DirOffset[i]) % 8);
                    GetNewXY(newDirection, ref newX, ref newY);
                    passed = CalculateNewZ(newX, newY, ref newZ);
                }
            }
        }
        if (passed) {
            x = newX;
            y = newY;
            z = newZ;
            direction = (Direction)newDirection;
        }
        return passed;
    }

    static bool CalculateNewZ(int x, int y, ref sbyte z) {
        var assets = UltimaOnline.Current?.Assets;
        if (assets != null)
            z = (sbyte)assets.StepZ(x, y, z);
        return true;
    }

    static void GetNewXY(byte direction, ref int x, ref int y) {
        switch (direction & 7) {
            case 0:
                y--;
                break;
            case 1:
                x++;
                y--;
                break;
            case 2:
                x++;
                break;
            case 3:
                x++;
                y++;
                break;
            case 4:
                y++;
                break;
            case 5:
                x--;
                y++;
                break;
            case 6:
                x--;
                break;
            case 7:
                x--;
                y--;
                break;
        }
    }

    static int TimeToCompleteMovement(bool run, bool mounted) {
        if (mounted)
            return run ? StepDelayMountRun : StepDelayMountWalk;
        return run ? StepDelayRun : StepDelayWalk;
    }
}
