using Dapper.GraphQL;
using HotChocolate.Language;

namespace Davivienda.GraphQL.Extensions
{
    public class MappedField
    {
        public NameNode? Name { get; set; }
        public NameNode? Alias { get; set; }
        public MappedSelectionSet? SelectionSet { get; set; }
        public MappedField(NameNode? name, NameNode? alias) { Name = name; Alias = alias; }
    }

    public class MappedSelectionSet
    {
        private List<MappedField> _fields = new List<MappedField>();
        public void Add(MappedField field) => _fields.Add(field);
        public IEnumerable<MappedField> Fields => _fields;
    }

    public static class SelectionSetExtensions
    {
        public static MappedField MapSelectionSetToField(this SelectionSetNode selectionSetNode)
        {
            var field = new MappedField(null, null);
            var selectionSet = new MappedSelectionSet();
            foreach (var selection in selectionSetNode.Selections)
            {
                if (selection is FieldNode fieldNode)
                {
                    var mappedField = new MappedField(new NameNode(fieldNode.Name.Value), new NameNode(fieldNode.Name.Value));
                    if (fieldNode.SelectionSet != null)
                        mappedField.SelectionSet = MapSelectionSetNodeToSelectionSet(fieldNode.SelectionSet);
                    selectionSet.Add(mappedField);
                }
            }
            field.SelectionSet = selectionSet;
            return field;
        }

        private static MappedSelectionSet MapSelectionSetNodeToSelectionSet(SelectionSetNode? selectionSetNode)
        {
            var selectionSet = new MappedSelectionSet();
            if (selectionSetNode == null) return selectionSet;
            foreach (var selection in selectionSetNode.Selections)
            {
                if (selection is FieldNode fieldNode)
                {
                    var field = new MappedField(new NameNode(fieldNode.Name.Value), new NameNode(fieldNode.Name.Value));
                    if (fieldNode.SelectionSet != null)
                        field.SelectionSet = MapSelectionSetNodeToSelectionSet(fieldNode.SelectionSet);
                    selectionSet.Add(field);
                }
            }
            return selectionSet;
        }
    }
}