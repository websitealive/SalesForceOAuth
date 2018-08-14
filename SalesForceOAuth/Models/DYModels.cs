﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesForceOAuth.Models
{
    // string token, string ObjectRef, int GroupId
    public class DYOrganizationDetail
    {
        public string token { get; set; }

        public string ObjectRef { get; set; }

        public int GroupId { get; set; }
    }

    public class ExportFieldsModel
    {
        public string token { get; set; }

        public string ObjectRef { get; set; }

        public int GroupId { get; set; }

        public string FieldName { get; set; }

        public string FiledLabel { get; set; }

        public string EntityType { get; set; }

        public string ValueType { get; set; }

        public string ValueDetail { get; set; }

        public int BusinessRequired { get; set; }

        public int MaxLength { get; set; }
    }

    public class FieldModel
    {
        public int ID { get; set; }

        public string FieldName { get; set; }

        public string FiledLabel { get; set; }

        public string EntityType { get; set; }

        public string ValueType { get; set; }

        public string ValueDetail { get; set; }
    }
}