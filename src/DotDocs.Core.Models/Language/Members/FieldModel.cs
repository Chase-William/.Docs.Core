﻿using System.Reflection;
using LoxSmoke.DocXml;

namespace DotDocs.Core.Models.Language.Members
{
    public class FieldModel : MemberModel<FieldInfo, CommonComments>
    {
        public bool IsReadonly => Info.IsInitOnly;
        public bool IsConstant => Info.IsLiteral;
        public bool IsStatic => Info.IsStatic;

        #region Accessiblity
        public bool IsPublic => Info.IsPublic;
        public bool IsPrivate => Info.IsPrivate;
        public bool IsProtected => Info.IsFamily || Info.IsFamilyOrAssembly;
        public bool IsInternal => Info.IsAssembly || Info.IsFamilyOrAssembly;
        #endregion

        public bool IsLiteral => Info.IsLiteral;
        public object? RawConstantValue => IsLiteral ? Info.GetRawConstantValue() : null;

        public string Type { get; init; }

        public FieldModel(FieldInfo member) : base(member)
        {
            Type = member.FieldType.GetTypeId();
        }

        public FieldModel(FieldInfo member, Type underlyingType) : base(member)
        {
            Type = underlyingType.GetTypeId();
        }
    }
}
