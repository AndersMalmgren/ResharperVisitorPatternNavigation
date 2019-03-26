using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Psi;

namespace ResharperVisitorPatternNavigation
{
    public class LinkedTypesOccurrence : DeclaredElementOccurrence
    {
        public LinkedTypesOccurrence([NotNull] IDeclaredElement element, OccurrenceType occurrenceKind)
            : base(element, occurrenceKind)
        {
        }
    }
}