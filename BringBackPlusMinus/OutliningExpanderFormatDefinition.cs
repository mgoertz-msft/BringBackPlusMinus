using System.ComponentModel.Composition;
using System.Windows.Media;
using BringBackPlusMinus.Properties;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace BringBackPlusMinus
{
    [Export(typeof(EditorFormatDefinition))]
    [Name(OutliningExpanderIdentifier)]
    [UserVisible(true)]
    internal sealed class OutliningExpanderFormatDefinition : EditorFormatDefinition
    {
        public const string OutliningExpanderIdentifier = "outlining.plusminus";

        public OutliningExpanderFormatDefinition()
        {
            this.ForegroundColor = Color.FromRgb(0x55, 0x55, 0x55);
            this.BackgroundColor = Color.FromRgb(0xE2, 0xE2, 0xE2);
            this.DisplayName = Strings.OutliningMarginPlusMinus;
        }
    }
}
