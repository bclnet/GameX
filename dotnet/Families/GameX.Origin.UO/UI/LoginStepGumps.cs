// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// Picks the login-step gump. Main and PopUpMessage already exist, so those return null.
/// Character creation is not in this project.
/// </summary>
public static class LoginStepGumps {
    public static Gump Create(LoginSteps step, World world, LoginScene scene) {
        switch (step) {
            case LoginSteps.Connecting:
            case LoginSteps.VerifyingAccount:
            case LoginSteps.LoginInToServer:
            case LoginSteps.EnteringBritania:
                return new LoadingGump(world, scene);
            case LoginSteps.ServerSelection:
                return new ServerSelectionGump(world);
            case LoginSteps.CharacterSelection:
                return new CharacterSelectionGump(world, scene);
            default:
                return null;
        }
    }
}
