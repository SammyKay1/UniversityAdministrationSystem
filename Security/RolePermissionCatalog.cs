public static class RolePermissionCatalog
{
    public static IReadOnlyList<RolePermissionDefinition> All =>
    [
        new RolePermissionDefinition("Administrator",  PermissionCatalog.All
                .Select(x => x.Name).ToList()
        ),

        new RolePermissionDefinition( "Registrar",   new[]
            {
                Permissions.Students.View,
                Permissions.Students.Create,
                Permissions.Students.Edit,
                Permissions.Students.Archive,

                Permissions.Courses.View,
                Permissions.Courses.Create,
                Permissions.Courses.Edit,

                Permissions.Departments.View,
                Permissions.Departments.Create,
                Permissions.Departments.Edit,
                Permissions.Departments.Archive,
                Permissions.Departments.Delete,

                Permissions.Enrollments.View,
                Permissions.Enrollments.Create,
                Permissions.Enrollments.Edit,

                Permissions.Teachers.View,
                Permissions.Teachers.Create,
                Permissions.Teachers.Edit,
                Permissions.Teachers.Archive,
                            }
        ),

        new RolePermissionDefinition("Lecturer",   new[]
            {
                Permissions.Students.View,

                Permissions.Courses.View,
                Permissions.Courses.Edit,

                Permissions.Enrollments.View,

                Permissions.Departments.View,
                
                Permissions.Teachers.View

            }
        ),

        new RolePermissionDefinition("Student",   new[]
            {
                Permissions.Courses.View,
                Permissions.Enrollments.View
            }
        )
    ];
}