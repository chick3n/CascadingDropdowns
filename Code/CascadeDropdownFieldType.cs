using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Security.Permissions;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.WebControls;
using System.Xml;
using System.Threading;
using System.Reflection;


namespace FlyingHippo.CascadingDropdowns.Fields
{
    public class CascadeDropdownFieldType : SPFieldLookup
    {
        #region Threading IDs
        private const string CASCADETYPE = "CascadeType";
        private const string CASCADELIST = "CascadeList";
        private const string CASCADEPARENT = "CascadeParent";
        private const string CASCADEDISPLAYFIELD = "CascadeDisplayField";
        private const string CASCADECOMPAREFIELD = "CascadeCompareField";
        #endregion

        public bool IsNew { get; set; }

        /// <summary>
        /// Cascade Type: Parent or Child
        /// </summary>
        public string CascadeType
        {
            get
            {
                //string value = (string)GetThreadDataValue(THREAD_SEARCHLISTGUID);
                string value = (string)GetFieldAttribute(CASCADETYPE);
                if (value == null)
                    return string.Empty;
                return value;
            }
            set
            {
                SetFieldAttribute(CASCADETYPE, value);
            }
        }

        /// <summary>
        /// Guid string of the list being display
        /// </summary>
        public string CascadeList
        {
            get
            {
                string value = (string)GetFieldAttribute(CASCADELIST);
                if (value == null)
                    return string.Empty;
                return value;
            }
            set
            {
                SetFieldAttribute(CASCADELIST, value);
            }
        }

        /// <summary>
        /// Column name of the parent dropdown, null if CascadeType is Parent
        /// </summary>
        public string CascadeParent
        {
            get
            {
                string value = (string)GetFieldAttribute(CASCADEPARENT);
                if (value == null)
                    return string.Empty;
                return value;
            }
            set
            {
                SetFieldAttribute(CASCADEPARENT, value);
            }
        }

        /// <summary>
        /// Static name of list field being displayed.
        /// </summary>
        public string CascadeDisplayField
        {
            get
            {
                string value = (string)GetFieldAttribute(CASCADEDISPLAYFIELD);
                if (value == null)
                    return string.Empty;
                return value;
            }
            set
            {
                SetFieldAttribute(CASCADEDISPLAYFIELD, value);
            }
        }

        public string CascadeCompareField
        {
            get
            {
                string value = (string)GetFieldAttribute(CASCADECOMPAREFIELD);
                if (value == null)
                    return string.Empty;
                return value;
            }
            set
            {
                SetFieldAttribute(CASCADECOMPAREFIELD, value);
            }
        }

        public CascadeDropdownFieldType(SPFieldCollection fields, string fieldName)
            : base(fields, fieldName)
        {
            
        }

        public CascadeDropdownFieldType(SPFieldCollection fields, string typeName, string displayName)
            : base(fields, typeName, displayName)
        {
            
        }

        public override BaseFieldControl FieldRenderingControl
        {
            [SharePointPermission(SecurityAction.LinkDemand, ObjectModel = true)]
            get
            {
                BaseFieldControl fieldControl = new CascadeDropdownFieldControl(this);
                fieldControl.FieldName = InternalName;

                return fieldControl;
            }
        }

        public override void OnAdded(SPAddFieldOptions op)
        {
            base.OnAdded(op);
            Update();
        }

        public override void Update()
        {
            SetCustomProperty(CASCADETYPE, CascadeType);

            if (CascadeType == "Child")
            {
                SetCustomProperty(CASCADEPARENT, CascadeParent);
                SetCustomProperty(CASCADECOMPAREFIELD, CascadeCompareField);
            }

            SetCustomProperty(CASCADETYPE, CascadeType);
            SetCustomProperty(CASCADELIST, CascadeList);
            SetCustomProperty(CASCADEDISPLAYFIELD, CascadeDisplayField);
            

            this.LookupList = CascadeList;
            this.LookupField = CascadeDisplayField;
            this.LookupWebId = SPContext.Current.Web.ID;
            
            base.Update();
        }

        #region Reflection

        private void SetFieldAttribute(string attribute, string value)
        {
            Type baseType;
            BindingFlags flags;
            MethodInfo mi;

            baseType = typeof(SPFieldText);
            flags = BindingFlags.Instance | BindingFlags.NonPublic;
            mi = baseType.GetMethod("SetFieldAttributeValue", flags);
            mi.Invoke(this, new object[] { attribute, value });
        }

        private string GetFieldAttribute(string attribute)
        {
            Type baseType;
            BindingFlags flags;
            MethodInfo mi;

            baseType = typeof(SPFieldText);
            flags = BindingFlags.Instance | BindingFlags.NonPublic;
            mi = baseType.GetMethod("GetFieldAttributeValue", flags, null, new Type[] { typeof(String) }, null);
            object obj = mi.Invoke(this, new object[] { attribute });

            if (obj == null)
                return string.Empty;
            return obj.ToString();
        }

        private bool GetFieldAttributeAsBool(string attribute)
        {
            Type baseType;
            BindingFlags flags;
            MethodInfo mi;

            baseType = typeof(SPFieldText);
            flags = BindingFlags.Instance | BindingFlags.NonPublic;
            mi = baseType.GetMethod("GetFieldAttributeValue", flags, null, new Type[] { typeof(String) }, null);
            object obj = mi.Invoke(this, new object[] { attribute });

            if (obj == null)
                return false;

            bool result = false;
            Boolean.TryParse(obj.ToString(), out result);
            return result;
        }

        #endregion

    }
}
