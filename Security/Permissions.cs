public static class Permissions
{
    public static class Students
    {
        public const string View = "Students.View";
        public const string Create = "Students.Create";
        public const string Edit = "Students.Edit";
        public const string Archive = "Students.Archive";
        public const string Delete = "Students.Delete";
        public const string Enrollment = "Students.Enrollment";

    }

    public static class Teachers
    {
        public const string View = "Teachers.View";
        public const string Create = "Teachers.Create";
        public const string Edit = "Teachers.Edit";
        public const string Delete = "Teachers.Delete";
        public const string Archive = "Teachers.Archive";
    }

    public static class Courses
    {
        public const string View = "Courses.View";
        public const string Create = "Courses.Create";
        public const string Edit = "Courses.Edit";
        public const string Delete = "Courses.Delete";
        public const string Archive = "Courses.Archive";
    }

    public static class Departments
    {
        public const string View = "Departments.View";
        public const string Create = "Departments.Create";
        public const string Edit = "Departments.Edit";
        public const string Delete = "Departments.Delete";
        public const string Archive = "Departments.Archive";
    }

    public static class Enrollments
    {
        public const string View = "Enrollments.View";
        public const string Create = "Enrollments.Create";
        public const string Edit = "Enrollments.Edit";
        public const string Delete = "Enrollments.Delete";
        public const string Archive = "Enrollments.Archive";
    }

    public static class Users
    {
        public const string Manage = "Users.Manage";
        public const string View = "Users.View";

        public const string Create = "Users.Create";

        public const string Edit = "Users.Edit";

        public const string Delete =  "Users.Delete";

        public const string ManageRoles =  "Users.ManageRoles";
    }

    public static class Roles
    {
        public const string Manage = "Roles.Manage";
    }

    public static class Reports
    {
        public const string View = "Reports.View";
    }

    public static class Security
    {
        public const string Manage = "Permissions.Manage";
    }


}