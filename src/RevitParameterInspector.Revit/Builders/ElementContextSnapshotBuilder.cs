using System;
using Autodesk.Revit.DB;
using RevitParameterInspector.Revit.Parameters;
using CoreModels = RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Revit.Builders;

/// <summary>
/// Assembles the partial <see cref="CoreModels.ElementContextSnapshot"/> available at this
/// step: Document, Identity, Classification, and Parameters (instance + type). Geometry,
/// Location, Relationships, and View/Sheet context builders are added in a later step.
/// </summary>
public static class ElementContextSnapshotBuilder
{
    public static CoreModels.ElementContextSnapshot Build(Element element)
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
            Identity = IdentityInfoBuilder.Build(element),
            Classification = ClassificationInfoBuilder.Build(element),
        };

        snapshot.Parameters.AddRange(ParameterReader.ReadInstanceParameters(element));

        var typeParameters = ParameterReader.ReadTypeParameters(element);
        if (typeParameters is not null)
        {
            snapshot.Parameters.AddRange(typeParameters);
        }

        return snapshot;
    }
}
