using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesForceOAuth.Models
{
    // string token, string ObjectRef, int GroupId
    public class DYOrganizationDetail
    {
        public string Token { get; set; }

        public string ObjectRef { get; set; }

        public int GroupId { get; set; }

        public string SiteRef { get; set; }
    }

    public class FieldsModel : DYOrganizationDetail
    {
        public int ID { get; set; }

        public string FieldName { get; set; }

        public string FieldLabel { get; set; }

        public string FieldType { get; set; }

        public string EntityType { get; set; }

        public string ValueType { get; set; }

        public string ValueDetail { get; set; }

        public string RelatedEntity { get; set; }

        public string RelatedField { get; set; }

        public int BusinessRequired { get; set; }

        public int MaxLength { get; set; }
    }

    public class CustomFields
    {
        public string Entity { get; set; }

        public List<CustomFieldModel> CustomFieldsList { get; set; }
    }

    public class CustomFieldModel
    {
        public int Sr { get; set; }

        public int Id { get; set; }

        public string FieldName { get; set; }

        public string FieldLabel { get; set; }

        public string Value { get; set; }

        public string FieldType { get; set; }

        public int BusinessRequired { get; set; }

        public int MaxLength { get; set; }

        public string RelatedEntity { get; set; }

        public string RelatedEntityFieldName { get; set; }

        public string Table { get; set; }

        //public int Sr { get; set; }

        //public int Id { get; set; }

        //public string FieldName { get; set; }

        //public string FiledLabel { get; set; }

        //public string RelatedEntity { get; set; }

        //public int BusinessRequired { get; set; }

        //public int MaxLength { get; set; }

        //public string FieldType { get; set; }
    }

    public class MessageResponce
    {
        public bool Success { get; set; }

        public string Error { get; set; }
    }

    public class MessageDataCopy
    {
        public string siteRef { get; set; }
        public string token { get; set; }
        public string ObjectRef { get; set; }
        public int GroupId { get; set; }
        public int SessionId { get; set; }
        public Guid ChatId { get; set; }
        public string EntitytId { get; set; }
        public string EntitytType { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; }
        public string OwnerPhone { get; set; }
        public string App { get; set; }
    }

    //public class EntityModel
    //{
    //    public string EntityPrimaryKey { get; set; }
    //    public string EntityPrimaryName { get; set; }
    //}

    //public class InputFields
    //{
    //    public string FieldName { get; set; }
    //    public string FieldLabel { get; set; }
    //    public string Value { get; set; }
    //    public string FieldType { get; set; }
    //    public string RelatedEntity { get; set; }
    //    public string RelatedEntityFieldName { get; set; }
    //}

    public class EntitySettings : DYOrganizationDetail
    {
        public int ID { get; set; }
        public int IsAccountRequired { get; set; }
        public int IsContactRequired { get; set; }
        public int IsLeadRequired { get; set; }
        public int AllowAccountCreation { get; set; }
        public int AllowContactCreation { get; set; }
        public int AllowLeadCreation { get; set; }
        public int UseAliveChat { get; set; }
    }

    public class DefaultFieldSettings : DYOrganizationDetail
    {
        public int ID { get; set; }
        public int IsAccountPhoneRequired { get; set; }
        public int IsAccountDescriptionRequired { get; set; }
        public int IsContactPhoneRequired { get; set; }
        public int IsContactEmailRequired { get; set; }
        public int IsLeadPhoneRequired { get; set; }
        public int IsLeadEmailRequired { get; set; }
        public int IsLeadCompanyRequired { get; set; }
    }

    public enum Crm
    {
        SalesForce,
        Dynamic
    }

    public enum EntityName
    {
        Account,
        Contact,
        Lead,
        Opportunity,
        Case
    }
}