namespace Library_oAuth
{
    public static class Constants
    {
        public const string Name_regex = "^[A-Z]{1}[A-Za-z]{3,}$";
        public const string Mobile_regex = "^[0]{1}[0-9]{9}$";
        public const string Email_regex = "^[a-zA-Z]+[.+-_]{0,1}[0-9a-zA-Z]+[@][a-zA-Z]+[.]+[a-zA-Z]{2,4}([a-zA-z]{2,4}){0,1}";
        public const string Password_regex = "^[a-zA-Z0-9]{4,}";
        public const string Cnp_regex = "^[1,2,3,4,5,6]{1}[0-9]{12}";
        public const string Ci_regex = "^[A-Z]{2}[0-9]{6}$";
    }
}
