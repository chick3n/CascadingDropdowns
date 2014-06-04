using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using FlyingHippo.CascadingDropdowns.Code;

namespace FlyingHippo.CascadingDropdowns.Fields
{
    public class CascadeDropdownFieldEditor : UserControl, IFieldEditor
    {
        private CascadeDropdownFieldType parentField;
        private string CascadeType;
        private string CascadeList;
        private string CascadeParent;
        private string CascadeDisplayName;
        private string CascadeCompareField;

        private string internal_parentListGuid;

        protected DropDownList ddlParentList;
        protected DropDownList ddlParentListDisplayName;
        
        protected DropDownList ddlChildList;
        protected DropDownList ddlChildLookup;
        protected DropDownList ddlChildLookupDisplayName;

        protected RadioButtonList rblCascadeType;

        public bool DisplayAsNewSection
        {
            get { return false; }
        }

        public void InitializeWithField(Microsoft.SharePoint.SPField field)
        {
            parentField = field as CascadeDropdownFieldType;

            if (parentField != null)
            {
                CascadeType = parentField.CascadeType;
                CascadeList = parentField.CascadeList;
                CascadeParent = parentField.CascadeParent;
                CascadeDisplayName = parentField.CascadeDisplayField;
                CascadeCompareField = parentField.CascadeCompareField;
            }  
          
            rblCascadeType.SelectedIndexChanged += rblCascadeType_SelectedIndexChanged;
            ddlParentList.SelectedIndexChanged += ddlParentList_SelectedIndexChanged;
            ddlChildList.SelectedIndexChanged += ddlChildList_SelectedIndexChanged;
            ddlChildLookup.SelectedIndexChanged += ddlChildLookup_SelectedIndexChanged;

            if (!Page.IsPostBack)
            {
                if (string.IsNullOrEmpty(CascadeType))
                {
                    rblCascadeType.SelectedIndex = 0;
                }
                else if (CascadeType == "Child")
                {
                    rblCascadeType.SelectedIndex = 1;
                }

                rblCascadeType_SelectedIndexChanged(null, EventArgs.Empty);
            }
        }

        void ddlChildLookup_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateCascadeLookupListDisplayName();
        }

        void ddlChildList_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateCascadeLookupList();
            PopulateCascadeLookupListDisplayName();
        }

        void ddlParentList_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateParentListDisplayNames();
        }

        void rblCascadeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rblCascadeType.SelectedIndex == 1) //child
                DisplayCascadeOption();
            else DisplayListOption();
        }

        private void DisplayListOption()
        {
            //enable
            ddlParentList.Enabled = true;
            ddlParentListDisplayName.Enabled = true;

            //disable
            ddlChildList.Enabled = false;
            ddlChildLookup.Enabled = false;
            ddlChildLookupDisplayName.Enabled = false;

            PopulateParentList();
            PopulateParentListDisplayNames();
        }


        private void DisplayCascadeOption()
        {
            //enable
            ddlChildList.Enabled = true;
            ddlChildLookup.Enabled = true;
            ddlChildLookupDisplayName.Enabled = true;

            //disable 
            ddlParentList.Enabled = false;
            ddlParentListDisplayName.Enabled = false;

            PopulateCascadeList();
            PopulateCascadeLookupList();
            PopulateCascadeLookupListDisplayName();
        }

        private void PopulateCascadeLookupListDisplayName()
        {
            ddlChildLookupDisplayName.Items.Clear();

            if (String.IsNullOrEmpty(ddlChildLookup.SelectedValue))
                return;

            using (SPWeb web = SPContext.Current.Site.OpenWeb())
            {
                SPList list = web.TryGetList(ddlChildLookup.SelectedValue);
                if (list == null)
                    return;

                foreach (SPField field in list.Fields)
                {
                    ListItem item = new ListItem();
                    item.Text = field.InternalName;
                    item.Value = field.StaticName;

                    if (!String.IsNullOrEmpty(CascadeDisplayName) && CascadeDisplayName == item.Value)
                        item.Selected = true;

                    ddlChildLookupDisplayName.Items.Add(item);
                }
            }
        }

        private void PopulateCascadeLookupList()
        {
            ddlChildLookup.Items.Clear();

            using (SPWeb web = SPContext.Current.Site.OpenWeb())
            {
                SPField parentField = SPContext.Current.List.TryGetFieldByString(ddlChildList.SelectedValue);
                if (parentField == null)
                    return;

                if (parentField is SPFieldLookup)
                {
                    SPFieldLookup parentFieldLookup = parentField as SPFieldLookup;

                    string parentFieldLookupListGuid = internal_parentListGuid = parentFieldLookup.LookupList;
                    SPList pflList = web.TryGetList(parentFieldLookupListGuid);

                    if (pflList == null)
                        return;

                    foreach (SPList list in web.Lists)
                    {
                        foreach (SPField field in list.Fields)
                        {
                            if (field.Type != SPFieldType.Lookup)
                                continue;

                            if (parentFieldLookupListGuid == ((SPFieldLookup)field).LookupList)
                            {
                                ListItem item = new ListItem();
                                item.Text = list.Title;
                                item.Value = list.ID.ToString("B");
                                if (!String.IsNullOrEmpty(CascadeList) && CascadeList == item.Value)
                                    item.Selected = true;

                                ddlChildLookup.Items.Add(item);

                                break;
                            }
                        }
                    }
                }
            }
        }

        private void PopulateParentList()
        {
            ddlParentList.Items.Clear();

            using (SPWeb web = SPContext.Current.Site.OpenWeb())
            {
                SPListCollection lists = web.Lists;

                foreach (SPList list in lists)
                {
                    if (list.BaseType != SPBaseType.GenericList)
                        continue;

                    ListItem item = new ListItem();
                    item.Text = list.Title;
                    item.Value = list.ID.ToString("B");
                    if (!String.IsNullOrEmpty(CascadeList) && item.Value == CascadeList)
                        item.Selected = true;

                    ddlParentList.Items.Add(item);
                }
            }
        }
        
        private void PopulateParentListDisplayNames()
        {
            ddlParentListDisplayName.Items.Clear();

            using (SPWeb web = SPContext.Current.Site.OpenWeb())
            {
                SPList parentList = web.TryGetList(ddlParentList.SelectedValue);

                if (parentList == null)
                    return;

                SPFieldCollection columns = parentList.Fields;

                foreach (SPField column in columns)
                {
                    ListItem item = new ListItem();
                    item.Text = column.InternalName;
                    item.Value = column.StaticName;
                    if (!String.IsNullOrEmpty(CascadeDisplayName) && item.Value == CascadeDisplayName)
                        item.Selected = true;

                    ddlParentListDisplayName.Items.Add(item);
                }
            }
        }

        private void PopulateCascadeList()
        {
            ddlChildList.Items.Clear();

            using (SPWeb web = SPContext.Current.Site.OpenWeb())
            {
                SPFieldCollection columns = SPContext.Current.List.Fields;

                foreach (SPField column in columns)
                {
                    if (column.TypeAsString == "CascadingDropdownsFieldType")
                    {
                        ListItem item = new ListItem();
                        item.Text = column.Title;
                        item.Value = column.StaticName;
                        if (!String.IsNullOrEmpty(CascadeParent) && item.Value == CascadeParent)
                            item.Selected = true;

                        ddlChildList.Items.Add(item);
                    }
                }
            }
        }

        public void OnSaveChange(Microsoft.SharePoint.SPField field, bool isNewField)
        {
            CascadeDropdownFieldType customFieldType = field as CascadeDropdownFieldType;

            if (customFieldType != null)
            {
                customFieldType.IsNew = isNewField;
                if (rblCascadeType.SelectedIndex == 0) //Parent ROOT cascade list choice
                {
                    customFieldType.CascadeList = ddlParentList.SelectedValue;
                    customFieldType.CascadeDisplayField = ddlParentListDisplayName.SelectedValue;
                    customFieldType.CascadeType = "Parent";
                }
                else if (rblCascadeType.SelectedIndex == 1)
                {
                    customFieldType.CascadeType = "Child";
                    customFieldType.CascadeParent = ddlChildList.SelectedValue;
                    customFieldType.CascadeList = ddlChildLookup.SelectedValue;
                    customFieldType.CascadeDisplayField = ddlChildLookupDisplayName.SelectedValue;

                    customFieldType.CascadeCompareField = GetCascadeParentCompareField();
                }
            }
        }

        private string GetCascadeParentCompareField()
        {
            if (String.IsNullOrEmpty(internal_parentListGuid))
                return string.Empty;

            using (SPWeb web = SPContext.Current.Web)
            {
                SPList childList = web.TryGetList(ddlChildLookup.SelectedValue);

                if (childList == null)
                    return string.Empty;

                foreach (SPField field in childList.Fields)
                {
                    if (field.Type == SPFieldType.Lookup)
                    {
                        if (((SPFieldLookup)field).LookupList == internal_parentListGuid)
                            return field.StaticName;
                    }
                }
            }

            return string.Empty;
        }
    }
}
