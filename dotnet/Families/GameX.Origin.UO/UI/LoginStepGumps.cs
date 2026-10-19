// SPDX-License-Identifier: BSD-2-Clause

namespace GameX.Origin.UO;

/// <summary>
/// Picks the login-step gump. Main stays LoginGump in LoginScene.GumpForStep.
/// </summary>
public static class LoginStepGumps {
    public static Gump Create(LoginSteps step, World world, LoginScene scene) {
        switch (step) {
            case LoginSteps.Connecting:
            case LoginSteps.VerifyingAccount:
            case LoginSteps.LoginInToServer:
            case LoginSteps.EnteringBritania:
            case LoginSteps.CharacterCreationDone:
                return new LoadingGump(world, scene);
            case LoginSteps.ServerSelection:
                return new ServerSelectionGump(world, scene);
            case LoginSteps.CharacterSelection:
                return new CharacterSelectionGump(world, scene);
            case LoginSteps.CharacterCreation:
                return new CharacterCreationGump(world, scene);
            case LoginSteps.PopUpMessage:
                return new PopupMessageGump(world, scene);
            default:
                return null;
        }
    }
}
