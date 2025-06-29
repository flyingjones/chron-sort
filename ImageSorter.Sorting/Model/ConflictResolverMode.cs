namespace ImageSorter.Sorting.Model;

public enum ConflictResolverMode
{
    Throw,
    ChooseOne,
    SemanticRename,
    RandomRename,
    HashRename
}