﻿using System;
using System.Collections.Generic;
using System.Linq;
using Fb2.Document.Constants;
using Fb2.Documents.TestsV2.Common;

namespace Fb2.Documents.TestsV2.DataCollections
{
    public abstract class Fb2ModelsDataCollectionBase
    {
        protected ElementNames names = new ElementNames();

        public List<string> AllElementsNames;
        public List<Type> AllModelTypes;

        public Fb2ModelsDataCollectionBase()
        {
            var namesResult = Utils.GetAllFieldsOfType<ElementNames, string>(names);
            AllElementsNames = new List<string>(namesResult);

            var assembly = names.GetType().Assembly;
            AllModelTypes = assembly.GetExportedTypes()
                .Where(type => type.FullName.StartsWith("Fb2.Document.Models.") && !type.IsAbstract && type.IsClass)
                .ToList();
        }
    }
}
