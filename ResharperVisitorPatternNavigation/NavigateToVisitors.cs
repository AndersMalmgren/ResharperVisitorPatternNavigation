using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.DataContext;
using JetBrains.Application.UI.DataContext;
using JetBrains.Application.UI.PopupLayout;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.Navigation.ContextNavigation;
using JetBrains.ReSharper.Feature.Services.Navigation.ExecutionHosting;
using JetBrains.ReSharper.Feature.Services.Occurrences;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace ResharperVisitorPatternNavigation
{
    [ContextNavigationProvider]
    public class NavigateToVisitors : INavigateFromHereProvider
    {
        public IEnumerable<ContextNavigation> CreateWorkflow(IDataContext dataContext)
        {

            var referenceName = dataContext.GetSelectedTreeNode<IReferenceName>();
            var declaration = (referenceName?.Reference.Resolve().DeclaredElement ?? dataContext.GetSelectedTreeNode<IDeclaration>()?.DeclaredElement) as ITypeElement;
            if (declaration != null && !(declaration is ICompiledElement))
            {
                yield return new ContextNavigation("Goto &Visitor", null, NavigationActionGroup.Blessed, () =>
                    {
                        var solution = dataContext.GetData(ProjectModelDataConstants.SOLUTION).NotNull();

                        var foundMethods = declaration
                            .GetPsiServices()
                            .Finder
                            .FindAllReferences(declaration)
                            .Select(r => ((r.GetTreeNode().Parent as IUserTypeUsage)?
                                .Parent as IRegularParameterDeclaration)?
                                .Parent as IFormalParameterList)
                            .Where(list => list != null && list.ParameterDeclarations.Count == 1)
                            .Select(m => m.Parent as IMethodDeclaration)
                            .Where(m => m != null)
                            .ToList();

                        if (!foundMethods.Any())
                        {
                            solution.GetComponent<DefaultNavigationExecutionHost>().ShowToolip(dataContext, "No visitors found");
                            return;
                        }

                        var occurrences = foundMethods.Select(x => new LinkedTypesOccurrence(x.DeclaredElement.NotNull(), OccurrenceType.Occurrence)).ToList<IOccurrence>();
                        ShowOccurrencePopupMenu(new[] { declaration }, occurrences, solution, dataContext.GetData(UIDataConstants.PopupWindowContextSource));
                    });
            }
        }

        private void ShowOccurrencePopupMenu(ICollection<ITypeElement> typesInContext, ICollection<IOccurrence> occurrences, ISolution solution, PopupWindowContextSource window)
        {
            var navigationExecutionHost = solution.GetComponent<DefaultNavigationExecutionHost>();
            navigationExecutionHost.ShowGlobalPopupMenu(
                solution,
                occurrences,
                activate: true,
                windowContext: window,
                descriptorBuilder: () => new LinkedTypesOccurrenceBrowserDescriptor(solution, typesInContext, occurrences),
                options: new OccurrencePresentationOptions(),
                skipMenuIfSingleEnabled: true,
                title: "Go to visitor handlers ");
        }
    }
}
