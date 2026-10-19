
public static class PermissionCatalog
{
    public static IReadOnlyList<PermissionDefinition> All =>
    [
        // =====================================================
        // STUDENTS
        // =====================================================

        new PermissionDefinition( Permissions.Students.View, "View students"),

        new PermissionDefinition( Permissions.Students.Create,  "Create students"),

        new PermissionDefinition( Permissions.Students.Edit,  "Edit students"),

        new PermissionDefinition( Permissions.Students.Archive,  "Archive students"),

        new PermissionDefinition( Permissions.Students.Enrollment,  "Enroll students"),


        // =====================================================
        // COURSES
        // =====================================================

        new PermissionDefinition( Permissions.Courses.View, "View courses"),

        new PermissionDefinition(Permissions.Courses.Create, "Create courses"),

        new PermissionDefinition( Permissions.Courses.Edit,  "Edit courses"),
        
        new PermissionDefinition(Permissions.Courses.Archive, "Archive courses"),


        // =====================================================
        // DEPARTMENTS
        // =====================================================

        new PermissionDefinition( Permissions.Departments.View, "View departments"),

        new PermissionDefinition( Permissions.Departments.Create, "Create departments"),

        new PermissionDefinition(Permissions.Departments.Edit, "Edit departments"),

        new PermissionDefinition( Permissions.Departments.Archive,  "Archive departments"),


        // =====================================================
        // TEACHERS
        // =====================================================

        new PermissionDefinition( Permissions.Teachers.View, "View teachers"),

        new PermissionDefinition( Permissions.Teachers.Create, "Create teachers"),

        new PermissionDefinition(Permissions.Teachers.Edit, "Edit teachers"),

        new PermissionDefinition(Permissions.Teachers.Delete, "Delete teachers"),        

        new PermissionDefinition( Permissions.Teachers.Archive,  "Archive teachers"),


        // =====================================================
        // ENROLLMENTS
        // =====================================================

        new PermissionDefinition( Permissions.Enrollments.View,   "View enrollments"),

        new PermissionDefinition(  Permissions.Enrollments.Create,   "Create enrollments"),

        new PermissionDefinition(  Permissions.Enrollments.Edit,   "Edit enrollments"),


        // =====================================================
        // USERS
        // =====================================================

        new PermissionDefinition( Permissions.Users.View,"View application users"),

        new PermissionDefinition( Permissions.Users.Create, "Create application users"),

        new PermissionDefinition( Permissions.Users.Edit,   "Edit application users"),

        new PermissionDefinition( Permissions.Users.Delete, "Delete application users"),

        new PermissionDefinition( Permissions.Users.ManageRoles, "Assign and remove user roles"),


        // =====================================================
        // ROLES
        // =====================================================

        new PermissionDefinition(  Permissions.Roles.Manage,  "Manage application roles"),


        // =====================================================
        // Reports
        // =====================================================

        new PermissionDefinition(  Permissions.Reports.View,  "View reports"),


        // =====================================================
        // SECURITY
        // =====================================================

        new PermissionDefinition( Permissions.Security.Manage,  "Manage application permissions")
    ];
}