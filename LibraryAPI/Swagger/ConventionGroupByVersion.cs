using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace LibraryAPI.Swagger
{
    public class ConventionGroupByVersion : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            // Example: Controllers.V1
            var namespaceDelController = controller.ControllerType.Namespace;
            var version = namespaceDelController!.Split(".").Last().ToLower();
            controller.ApiExplorer.GroupName = version;
        }
    }
}
