using GameX.Valve.Formats.Vpk;
using OpenStack;
using OpenStack.Gfx.Egin;
using OpenStack.Gfx.OpenGL;
using System.Collections.Generic;

namespace GameX.Valve.OpenGL.Scenes;

//was:Renderer/ParticleSceneNode
public class ParticleSceneNode : SceneNode {
    class ParticleSystemWrapper(D_ParticleSystem source) : IParticleSystem {
        readonly D_ParticleSystem _source = source;
        IDictionary<string, object> IParticleSystem.Data => _source.Data;
        IEnumerable<IDictionary<string, object>> IParticleSystem.Renderers => _source.Renderers;
        IEnumerable<IDictionary<string, object>> IParticleSystem.Operators => _source.Operators;
        IEnumerable<IDictionary<string, object>> IParticleSystem.Initializers => _source.Initializers;
        IEnumerable<IDictionary<string, object>> IParticleSystem.Emitters => _source.Emitters;
        IEnumerable<string> IParticleSystem.GetChildParticleNames(bool enabledOnly) => _source.GetChildParticleNames(enabledOnly);
    }

    ParticleRenderer.ChildRenderer ChildRenderer;

    public ParticleSceneNode(Scene scene, D_ParticleSystem particleSystem) : base(scene) {
        ChildRenderer = new ParticleRenderer.ChildRenderer(Scene.GfxModel as OpenGLGfxModel, new ParticleSystemWrapper(particleSystem));
        LocalBoundingBox = ChildRenderer.BoundingBox;
    }

    public override void Update(Scene.UpdateContext context) {
        ChildRenderer.Position = Transform.Translation;
        ChildRenderer.Update(context.Timestep);
        LocalBoundingBox = ChildRenderer.BoundingBox.Translate(-ChildRenderer.Position);
    }

    public override void Render(Scene.RenderContext context) => ChildRenderer.Render(context.Camera, context.Pass);

    public override IEnumerable<string> GetSupportedRenderModes() => ChildRenderer.GetSupportedRenderModes();
}
