﻿using Docsharp.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Docsharp.Core
{
    /// <summary>
    /// A tree of namespaces and types created lazily that represents the structure of one's source code.
    /// </summary>
    public class MetadataTree
    {
        public NamespaceNode Root { get; private set; }

        public MetadataTree(string rootNamespace)            
            => Root = new NamespaceNode(null, rootNamespace);

        /// <summary>
        /// Adds a new type under the provided namespace.
        /// </summary>
        /// <param name="fullNamespaceToType"></param>
        public void AddType(string fullNamespaceToType, TypeMember<TypeInfo> member)
        {
            // We know the last item in this string[] is our type
            // Everything before it, a namespace            
            var segments = fullNamespaceToType.Split(".");

            ArraySegment<string> a = segments;

            Root.AddType(a, member);           
        }

        /// <summary>
        /// Inserts a new type within the provided type using the namespace.
        /// </summary>
        /// <param name="newType">To be inserted.</param>
        /// <param name="containingTypeWithNamespace">To contain the insertion.</param>
        //public void InsertTypeWithinType(string newType, string containingTypeWithNamespace)
        //{

        //}

        /// <summary>
        /// Adds a new namespace to the tree.
        /// </summary>
        /// <param name="fullNamespace">Entire namespace including the new.</param>
        //public void AddNamespace(string fullNamespace)
        //{
        //    var namespaces = fullNamespace.Split();

        //    Node current = RootNamespace;
        //    for (int i = 0; i < namespaces.Length; i++)
        //    {
        //        current.
        //    }
        //}
    }

    public class NamespaceNode : Node, ITypeContainable
    {
        private readonly string _namespace;

        public NamespaceNode Parent { get; set; }
        public Dictionary<string, NamespaceNode> Namespaces { get; set; } = new();
        public Dictionary<string, Node> Types { get; set; } = new();

        public NamespaceNode(NamespaceNode parent, string _namespace)
        {
            Parent = parent;
            this._namespace = _namespace;
        }

        public override string GetName()
            => _namespace;

        public void AddType(ArraySegment<string> segments, TypeMember<TypeInfo> member)
        {
            string name = segments.First();

            // End of namespace chain reached, now currently at a Type
            if (segments.Count == 1)
            {
                /**
                 * Handle case where only one type is present (no types nested within types)
                 */

                segments = name.Split("+");
                name = segments.First();
                // Our last iteration will be some kind of Type               
                if (segments.Count == 1)
                {
                    // Case for a type that contain internally defined types
                    if (member.CanHaveInternalTypes)
                    {
                        Types.Add(name, new TypeContainable(this, member));
                        return;
                    }
                    // Add a type that cannot contain other internally defined types                  
                    Types.Add(name, new TypeNode(this, member));
                    return;

                    // Should return to caller in this case without further execution
                }

                /**
                 * Handle case where there are types nested within types
                 */

                // Key for namespace already exist, therefore get a ref and add type
                if (Types.ContainsKey(name))
                {
                    ((ITypeContainable)Types[name]).AddType(segments[1..segments.Count], member);
                    return;
                }
                var typeNode = new TypeContainable(this, member);
                Types.Add(name, typeNode);
                // Now iterate through nested type chain
                typeNode.AddType(segments[1..segments.Count], member);
                return;
            }
            
            if (!Namespaces.ContainsKey(name))
            {
                Namespaces.Add(name, new NamespaceNode(this, name));
            }
            segments = segments[1..segments.Count];
            Namespaces[name].AddType(segments, member);

            // Only iterate over namespaces, not types
            // Removes the need to check for last iteration during each iteration
            //int namespaceCount = segments.Length - 1;
            //string key;
            //for (int i = 0; i < namespaceCount; i++)
            //{
            //    key = segments[i];
            //    if (current.Namespaces.ContainsKey(key))
            //    {

            //    }
            //}
        }        
    }

    public class TypeNode : Node
    {        
        public Node Parent { get; private set; }

        public TypeMember<TypeInfo> Member { get; private set; }

        public TypeNode(Node parent, TypeMember<TypeInfo> member)
        {
            Parent = parent;
            Member = member;
        }

        public override string GetName()
            => Member.Name;


        //public override void AddType(ArraySegment<string> types, Member<TypeInfo> member)
        //{
        //    string typeName = types.First();
        //    // End of nested type chain reached
        //    if (types.Count == 1)
        //    {
        //        Types.Add(typeName, new TypeNode(this, member));
        //        return;
        //    }

        //    if (!Types.ContainsKey(typeName))
        //    {
        //        Types.Add(typeName, new TypeNode(this, null));
        //    }
        //    types = types[1..types.Count];
        //    Types[typeName].AddType(types, member);
        //}
    }

    public abstract class Node
    {
        public abstract string GetName();
        // public Dictionary<string, Node> Types { get; set; } = new();
        // public abstract void AddType(ArraySegment<string> segments, Member<TypeInfo> member);
    }

    public class TypeContainable : TypeNode, ITypeContainable
    {
        public Dictionary<string, Node> Types { get; set; } = new();

        public TypeContainable(Node parent, TypeMember<TypeInfo> member) : base(parent, member)
        { }

        public void AddType(ArraySegment<string> types, TypeMember<TypeInfo> member)
        {
            string typeName = types.First();
            // End of nested type chain reached
            if (types.Count == 1)
            {
                if (member.CanHaveInternalTypes)
                {
                    Types.Add(typeName, new TypeContainable(this, member));
                    return;
                }
                Types.Add(typeName, new TypeNode(this, member));
                return;
            }

            TypeContainable type;
            if (!Types.ContainsKey(typeName))
            {
                type = new TypeContainable(this, null);
                Types.Add(typeName, type);
            }
            else
                type = (TypeContainable)Types[typeName];
            types = types[1..types.Count];
            type.AddType(types, member);
        }
    }

    /// <summary>
    /// Represents a type that can contain other type definitions internally.
    /// For example, this can represent a class or struct as both can contain other class/struct type definitions.
    /// </summary>
    public interface ITypeContainable
    {
        public Dictionary<string, Node> Types { get; set; }

        public void AddType(ArraySegment<string> types, TypeMember<TypeInfo> member);
    }
}