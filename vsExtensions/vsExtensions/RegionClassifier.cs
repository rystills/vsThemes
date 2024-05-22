using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace RegionColorizer
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("code")]
    internal class RegionClassifierProvider : IClassifierProvider
    {
        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry = null;

        public IClassifier GetClassifier(ITextBuffer buffer) => buffer.Properties.GetOrCreateSingletonProperty(() => new RegionClassifier(ClassificationRegistry));
    }

    internal class RegionClassifier : IClassifier
    {
        private readonly IClassificationType _regionType;
        internal RegionClassifier(IClassificationTypeRegistryService registry) => _regionType = registry.GetClassificationType("Region");
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            List<ClassificationSpan> classifications = new List<ClassificationSpan>();
            Stack<ITextSnapshotLine> regionStack = new Stack<ITextSnapshotLine>();

            foreach (ITextSnapshotLine line in span.Snapshot.Lines)
            {
                string lineText = line.GetText();
                if (lineText.Contains("#region")) regionStack.Push(line);
                else if (lineText.Contains("#endregion") && regionStack.Count > 0)
                {
                    ITextSnapshotLine regionStartLine = regionStack.Pop();
                    SnapshotSpan regionSpan = new SnapshotSpan(regionStartLine.Start, line.End);
                    classifications.Add(new ClassificationSpan(regionSpan, _regionType));
                }
            }

            return classifications;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "Region")]
    [Name("Region")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class RegionFormatDefinition : ClassificationFormatDefinition
    {
        public RegionFormatDefinition()
        {
            DisplayName = "Region";
            BackgroundColor = Colors.LightBlue;
            ForegroundColor = Colors.DarkBlue;
        }
    }

    internal static class RegionClassificationDefinition
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("Region")]
        internal static ClassificationTypeDefinition RegionClassificationType = null;
    }
}