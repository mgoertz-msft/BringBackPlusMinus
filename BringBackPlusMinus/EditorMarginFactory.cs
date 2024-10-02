using System;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace BringBackPlusMinus
{
    /// <summary>
    /// Export a <see cref="IWpfTextViewMarginProvider"/>, which returns an instance of the margin for the editor to use.
    /// </summary>
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name("OutliningExpander")]
    [Order(After = PredefinedMarginNames.HorizontalScrollBar)]  // Ensure that the margin occurs below the horizontal scrollbar
    [MarginContainer(PredefinedMarginNames.Bottom)]             // Set the container to the bottom of the editor window
    [ContentType("text")]                                       // Show this margin for all text-based types
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class EditorMarginFactory : IWpfTextViewMarginProvider
    {
        private ConditionalWeakTable<IWpfTextViewHost, EditorFormatMapHolder> _editorFormatMapHolders = new ConditionalWeakTable<IWpfTextViewHost, EditorFormatMapHolder>();

        [Import]
        internal IEditorFormatMapService _editorFormatMapService { get; set; } = null;

        #region IWpfTextViewMarginProvider

        /// <summary>
        /// Creates an <see cref="IWpfTextViewMargin"/> for the given <see cref="IWpfTextViewHost"/>.
        /// </summary>
        /// <param name="wpfTextViewHost">The <see cref="IWpfTextViewHost"/> for which to create the <see cref="IWpfTextViewMargin"/>.</param>
        /// <param name="marginContainer">The margin that will contain the newly-created margin.</param>
        /// <returns>The <see cref="IWpfTextViewMargin"/>.
        /// The value may be null if this <see cref="IWpfTextViewMarginProvider"/> does not participate for this context.
        /// </returns>
        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            if (!_editorFormatMapHolders.TryGetValue(wpfTextViewHost, out _))
            {
                var editorFormatMap = _editorFormatMapService.GetEditorFormatMap(wpfTextViewHost.TextView);
                if (editorFormatMap != null)
                {
                    _editorFormatMapHolders.Add(wpfTextViewHost, new EditorFormatMapHolder(this, wpfTextViewHost, editorFormatMap));
                }
            }

            // We don't actually want to create a new margin.
            // We just need a way to update the colors
            return null;
        }

        #endregion

        private class EditorFormatMapHolder
        {
            private readonly EditorMarginFactory _marginFactory;
            private readonly IWpfTextViewHost _wpfTextViewHost;
            private readonly IEditorFormatMap _editorFormatMap;

            public EditorFormatMapHolder(EditorMarginFactory marginFactory, IWpfTextViewHost wpfTextViewHost, IEditorFormatMap editorFormatMap)
            {
                _marginFactory = marginFactory;
                _wpfTextViewHost = wpfTextViewHost;
                _wpfTextViewHost.Closed += OnTextViewHostClosed;
                _editorFormatMap = editorFormatMap;
                _editorFormatMap.FormatMappingChanged += HandleFormatMappingChanged;

                SetColors(_editorFormatMap);
            }

            private void HandleFormatMappingChanged(object sender, FormatItemsEventArgs args)
            {
                if (args.ChangedItems.Contains(OutliningExpanderFormatDefinition.OutliningExpanderIdentifier))
                {
                    SetColors((IEditorFormatMap)sender);
                }
            }

            private void SetColors(IEditorFormatMap editorFormatMap)
            {
                var properties = editorFormatMap.GetProperties(OutliningExpanderFormatDefinition.OutliningExpanderIdentifier);
                _wpfTextViewHost.HostControl.Resources[OutliningExpanderFormatDefinition.OutliningExpanderIdentifier + ".foreground"] = (Brush)properties[EditorFormatDefinition.ForegroundBrushId];
                _wpfTextViewHost.HostControl.Resources[OutliningExpanderFormatDefinition.OutliningExpanderIdentifier + ".background"] = (Brush)properties[EditorFormatDefinition.BackgroundBrushId];
            }

            private void OnTextViewHostClosed(object sender, EventArgs args)
            {
                _wpfTextViewHost.Closed -= OnTextViewHostClosed;
                _editorFormatMap.FormatMappingChanged -= HandleFormatMappingChanged;
                _marginFactory._editorFormatMapHolders.Remove(_wpfTextViewHost);
            }
        }
    }
}
