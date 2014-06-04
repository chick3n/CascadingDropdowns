using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingHippo.CascadingDropdowns.Code
{
    public class CascadingList
    {
        public string FieldName { get; set; }

        public CascadingList Prev { get; set; }
        public CascadingList Next { get; set; }

    }

    public static class CascadingDropdownHelper
    {
        public static void AddCascadingList(CascadingList root, string parent, string fieldName)
        {
            if (root == null)
                return;

            if (root.FieldName == parent)
            {
                CascadingList child = new CascadingList();
                child.Prev = root;
                child.Next = root.Next;
                child.FieldName = fieldName;

                return;
            }

            if (root.Next != null)
                AddCascadingList(root.Next, parent, fieldName);
            else
            {
                CascadingList child = new CascadingList();
                child.Prev = root;
                child.Next = null;
                child.FieldName = fieldName;
            }
        }
    }

    public static class Extensions
    {
        public static string TryGetFieldName(this SPList list, string guid)
        {
            try
            {
                SPField field = list.Fields[new Guid(guid)];
                return field.StaticName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static SPField TryGetFieldByString(this SPList list, string name)
        {
            try
            {
                SPField field = list.Fields[name];
                return field;
            }
            catch(Exception)
            {
                return null;
            }
        }

        public static string TryGetItemValue(this SPItem item, string staticName)
        {
            try
            {
                var data = item[staticName];
                return data.ToString();
            }
            catch (Exception)
            {
            }

            return null;
        }

        public static SPList TryGetList(this SPWeb web, string guid)
        {
            try
            {
                return web.Lists[new Guid(guid)];
            }
            catch (Exception)
            {
            }

            return null;
        }
    }
}
