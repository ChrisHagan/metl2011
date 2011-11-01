using System.Windows.Media.Effects;
using System.Windows.Media;
namespace SandRibbon
{/*WPF supports pixel shader effects.  I don't think it's worth doing at this point given that
  it would mean an extra language, but I'd implement grayscale on disabled custom pieces that way.*/
    public static class Effects
    {
        public static DropShadowEffect privacyShadowEffect = new DropShadowEffect { BlurRadius = 50, Color = Colors.Red, ShadowDepth = 0, Opacity = 0.5 };
   }
}