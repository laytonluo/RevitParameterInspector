using System;
using Autodesk.Revit.DB;
using RevitParameterInspector.Dictionary;
using RevitParameterInspector.Revit.Parameters;
using CoreModels = RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Revit.Builders;

/// <summary>
/// Assembles the full <see cref="CoreModels.ElementContextSnapshot"/>: Document, Identity,
/// Classification, Parameters (instance + type), Geometry, Location, Relationships, and
/// View/Sheet Context (populated only when relevant to the selected element).
/// </summary>
public static class ElementContextSnapshotBuilder
{
    /// <param name="resolver">
    /// Optional Dictionary Engine resolver (HANDOFF Section 20). When null, the snapshot is
    /// still fully built with raw API names only (HANDOFF Section 5.2).
    /// </param>
    /// <param name="activeView">
    /// The active view at inspection time, used by <see cref="ViewSheetContextReader"/> so the
    /// View / Sheet Context page always has at least the active view for any element.
    /// </param>
    public static CoreModels.ElementContextSnapshot Build(Element element, DictionaryResolver? resolver = null, View? activeView = null)
    {
        var document = element.Document;
        var application = document.Application;

        var snapshot = new CoreModels.ElementContextSnapshot
        {
            GeneratedAt = DateTimeOffset.UtcNow,
            RevitVersion = application?.VersionNumber,
            AddinVersion = typeof(ElementContextSnapshotBuilder).Assembly.GetName().Version?.ToString(),
            Document = new CoreModels.DocumentInfo
            {
                Title = document.Title,
                PathName = document.PathName,
                IsWorkshared = document.IsWorkshared,
                IsLinked = document.IsLinked,
                RevitProductName = application?.VersionName,
                RevitBuildNumber = application?.VersionBuild,
            },
            Identity = IdentityInfoBuilder.Build(element, resolver),
            Classification = ClassificationInfoBuilder.Build(element, resolver),
            Geometry = GeometryInfoBuilder.Build(element),
            Location = LocationInfoBuilder.Build(element),
            Relationships = RelationshipInfoBuilder.Build(element),
            ViewContext = ViewContextInfoBuilder.Build(element),
            SheetContext = SheetContextInfoBuilder.Build(element),
            ViewSheetContexts = ViewSheetContextReader.Read(document, activeView, element),
        };

        snapshot.Parameters.AddRange(ParameterReader.ReadInstanceParameters(element, resolver));

        var typeParameters = ParameterReader.ReadTypeParameters(element, resolver);
        if (typeParameters is not null)
        {
            snapshot.Parameters.AddRange(typeParameters);
        }

        if (resolver is not null)
        {
            snapshot.Dictionary.AddRange(resolver.ResolvedTerms);
            snapshot.UnresolvedDictionaryTerms.AddRange(resolver.UnresolvedTerms.GetUnresolvedTerms());
        }

        return snapshot;
    }
}
