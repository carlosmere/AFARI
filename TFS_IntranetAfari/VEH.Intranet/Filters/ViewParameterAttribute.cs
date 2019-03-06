using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Helpers;
using System.Web.Mvc;
using System.Web.Routing;

namespace VEH.Intranet.Filters
{
    public class ViewParameterAttribute : ActionFilterAttribute
    {
        private readonly String _section;
        private readonly String _pageIcon;
        private readonly String _subSection;

        public ViewParameterAttribute(String Section = null, String PageIcon = null, String SubSection = null)
        {
            _section = Section;
            _pageIcon = PageIcon;
            _subSection = SubSection;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if(!String.IsNullOrEmpty(_section))
                filterContext.Controller.ViewBag.Section = _section;
            if (!String.IsNullOrEmpty(_pageIcon))
                filterContext.Controller.ViewBag.PageIcon = _pageIcon;
            if (!String.IsNullOrEmpty(_subSection))
                filterContext.Controller.ViewBag.SubSection = _subSection;
            
            base.OnActionExecuting(filterContext);
        }
    }
}