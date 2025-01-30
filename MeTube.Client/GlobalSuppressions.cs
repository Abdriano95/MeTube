// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
using System.Diagnostics.CodeAnalysis;
[assembly: SuppressMessage(
    "CommunityToolkit.Mvvm.SourceGenerators.ObservableObjectGenerator",
    "MVVMTK0033:Inherit from ObservableObject instead of using [ObservableObject]",
    Justification = "Can use .NET attribute instead of inheritance",
    Scope = "namespaceanddescendants",
    Target = "MeTube.Client")]
