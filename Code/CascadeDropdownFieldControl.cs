using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Threading;

using System.Collections;
using Microsoft.SharePoint;

using FlyingHippo.CascadingDropdowns.Code;
using System.Web.UI;

namespace FlyingHippo.CascadingDropdowns.Fields
{
    public class CascadeDropdownFieldControl : BaseFieldControl
    {
        private CascadeDropdownFieldType parentField;

        protected DropDownList cascader;
        protected string THREAD_SELECTEDVALUE = "SelectedValue";
        protected bool HasChanged;

        public CascadeDropdownFieldControl(CascadeDropdownFieldType parent)
        {
            this.parentField = parent;
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (this.ControlMode != SPControlMode.Display)
            {
                cascader = new DropDownList();
                cascader.Attributes.Add("cascader", this.FieldName);
                cascader.AutoPostBack = true;
                cascader.SelectedIndexChanged += cascader_SelectedIndexChanged;
                HasChanged = false;

                PopulateCascadingDropdown(); 

                base.Controls.Add(cascader);
            }
        }

        private CascadeDropdownFieldControl GetParentCausedPostback()
        {
            if (!Page.IsPostBack)
                return null;

            var controlPostback = Page.Request["__EVENTTARGET"];
            CascadeDropdownFieldControl postbackControl = null;
            List<CascadeDropdownFieldControl> childControls = new List<CascadeDropdownFieldControl>();

            //find post back caused by control
            foreach (var control in SPContext.Current.FormContext.FieldControlCollection)
            {
                if (control is CascadeDropdownFieldControl)
                {
                    if (((CascadeDropdownFieldControl)control).cascader.UniqueID == controlPostback)
                    {
                        postbackControl = control as CascadeDropdownFieldControl;
                        continue;
                    }

                    childControls.Add(control as CascadeDropdownFieldControl);
                }
            }

            if (postbackControl == null) return null;
            if (this == postbackControl) return null;

            return postbackControl;
        }

        private bool DidParentChange()
        {
            if (!Page.IsPostBack)
                return false;

            var controlPostback = Page.Request["__EVENTTARGET"];
            CascadeDropdownFieldControl postbackControl = null;
            List<CascadeDropdownFieldControl> childControls = new List<CascadeDropdownFieldControl>();

            //find post back caused by control
            foreach (var control in SPContext.Current.FormContext.FieldControlCollection)
            {
                if (control is CascadeDropdownFieldControl)
                {
                    if (((CascadeDropdownFieldControl)control).cascader.UniqueID == controlPostback)
                    {
                        postbackControl = control as CascadeDropdownFieldControl;
                        continue;
                    }

                    childControls.Add(control as CascadeDropdownFieldControl);
                }
            }
            
            if (postbackControl == null) return false;
            if (this == postbackControl) return false;

            if (childControls.Count > 0)
            {
                var control = childControls[0];

                while(control != null)
                {
                    if (control.parentField.CascadeParent == postbackControl.FieldName)
                        return true;

                    //find controls parent control and check
                    CascadeDropdownFieldControl tempControl = null;
                    for (int x = 0; x < childControls.Count; x++)
                    {
                        if (control.parentField.CascadeParent == childControls[x].FieldName)
                        {
                            tempControl = childControls[x];
                            break;
                        }
                    }

                    if (tempControl != control)
                        control = tempControl;
                }
            }

            return false;

        }

        private void PopulateCascadingDropdown()
        {
            cascader.Items.Clear();

            using (SPWeb web = SPContext.Current.Web)
            {
                SPList list = web.TryGetList(parentField.CascadeList);

                if (list == null)
                    return;

                string selectedParentValue = GetParentValue();

                string queryStr = string.Empty;
                if (!String.IsNullOrEmpty(selectedParentValue))
                    queryStr = String.Format(@"<Where><Eq><FieldRef Name=""{1}"" LookupId=""TRUE"" /><Value Type=""Lookup"">{0}</Value></Eq></Where>",
                        selectedParentValue, 
                        (String.IsNullOrEmpty(parentField.CascadeCompareField)) ? parentField.CascadeParent : parentField.CascadeCompareField);

                SPQuery query = new SPQuery();
                query.Query = queryStr;
                query.ViewFields = String.Format(@"<FieldRef Name=""ID"" /><FieldRef Name=""{0}"" />", parentField.CascadeDisplayField);
                query.ViewFieldsOnly = true;

                foreach (SPItem item in list.GetItems(query))
                {
                    string value = item.TryGetItemValue("ID");
                    string text = item.TryGetItemValue(parentField.CascadeDisplayField);

                    if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(text))
                    {
                        ListItem entry = new ListItem();
                        entry.Text = text;
                        entry.Value = value;
                        if (IsLookupValueSelected(value))
                            entry.Selected = true;

                        cascader.Items.Add(entry);
                    }
                }
            }
        }

        private string GetParentValue()
        {
            if (parentField.CascadeType == "Parent")
            {
                
                return null;
            }

            foreach (var field in SPContext.Current.FormContext.FieldControlCollection)
            {
                if (field is CascadeDropdownFieldControl)
                {
                    CascadeDropdownFieldControl cdfField = field as CascadeDropdownFieldControl;

                    if (cdfField.FieldName == parentField.CascadeParent)
                    {
                        if (cdfField.cascader != null)
                        {
                            //bool parentChanged = DidParentChange();
                            CascadeDropdownFieldControl parentPostbackControl = GetParentCausedPostback();

                            if (Page.IsPostBack && parentPostbackControl != null && parentPostbackControl.FieldName == parentField.CascadeParent)
                            {
                                var headerData = Page.Request[cdfField.cascader.UniqueID];
                                if (!String.IsNullOrEmpty(headerData))
                                    return headerData;
                            }
                            
                            return cdfField.cascader.SelectedValue;
                        }
                    }
                }
            }

            return null;
        }

        void cascader_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private bool IsLookupValueSelected(string value)
        {
            SPFieldLookupValue savedEntry = Value as SPFieldLookupValue;

            if (savedEntry != null)
            {
                if (value == savedEntry.LookupId.ToString())
                    return true;
            }

            return false;
        }

        public override void Validate()
        {
            base.Validate();

            if (ControlMode == SPControlMode.Display || !IsValid)
                return;


            IsValid = true;
        }

        public override void UpdateFieldValueInItem()
        {
            ItemFieldValue = Value;
        }

        public override object Value
        {
            get
            {
                this.EnsureChildControls();

                if(cascader == null || string.IsNullOrEmpty(cascader.SelectedValue))
                    return null;

                return new SPFieldLookupValue(int.Parse(cascader.SelectedValue), cascader.SelectedItem.Text);
            }
            set
            {
                this.EnsureChildControls();

                if (value == null)
                    return;

                base.Value = value;
            }
        }

        #region Threading

        private string GetThreadDataValue(string propertyName)
        {
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot(propertyName);
            object dataSlot = Thread.GetData(slot);

            if (dataSlot != null)
            {
                return dataSlot.ToString();
            }

            return string.Empty;
        }

        private void SetThreadDataValue(string propertyName, object value)
        {
            Thread.SetData(Thread.GetNamedDataSlot(propertyName), value);
        }

        private void FreeThreadData()
        {
            Thread.FreeNamedDataSlot(THREAD_SELECTEDVALUE);
        }

        #endregion Threading

    }
}
