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

    OpenGLParticleRenderer.ParticleRenderer ParticleRenderer;

    public ParticleSceneNode(Scene scene, D_ParticleSystem particleSystem) : base(scene) {
        ParticleRenderer = new OpenGLParticleRenderer.ParticleRenderer(Scene.Gfx as OpenGLGfxModel, new ParticleSystemWrapper(particleSystem));
        LocalBoundingBox = ParticleRenderer.BoundingBox;
    }

    public override void Update(Scene.UpdateContext context) {
        ParticleRenderer.Position = Transform.Translation;
        ParticleRenderer.Update(context.Timestep);
        LocalBoundingBox = ParticleRenderer.BoundingBox.Translate(-ParticleRenderer.Position);
    }

    public override void Render(Scene.RenderContext context) => ParticleRenderer.Render(context.Camera, context.Pass);

    public override IEnumerable<string> GetSupportedRenderModes() => ParticleRenderer.GetSupportedRenderModes();
}
