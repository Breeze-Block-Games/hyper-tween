using BreezeBlockGames.DependencyInjection.UnityShared;
using BreezeBlockGames.HyperTween.UnityShared.API;
using VContainer;

namespace BreezeBlockGames.HyperTween.UnityShared.Installers
{
    public class BootstrapInstaller : PackageInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(TweenCollection.Create());
        }
    }
}