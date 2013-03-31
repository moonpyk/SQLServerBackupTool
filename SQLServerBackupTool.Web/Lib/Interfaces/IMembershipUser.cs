using System;

namespace SQLServerBackupTool.Web.Lib.Interfaces
{
    public interface IMembershipUser
    {
        string Email
        {
            get;
            set;
        }

        string Comment
        {
            get;
            set;
        }

        bool IsApproved
        {
            get;
            set;
        }

        DateTime LastLoginDate
        {
            get;
            set;
        }

        DateTime LastActivityDate
        {
            get;
            set;
        }

        /*
         * Readonly
         */

        string UserName
        {
            get;
        }

        object ProviderUserKey
        {
            get;
        }

        bool IsLockedOut
        {
            get;
        }

        DateTime LastLockoutDate
        {
            get;
        }

        DateTime CreationDate
        {
            get;
        }

        DateTime LastPasswordChangedDate
        {
            get;
        }

        bool IsOnline
        {
            get;
        }

        string PasswordQuestion
        {
            get;
        }

        string ProviderName
        {
            get;
        }

        void Update();

        string GetPassword();
        string GetPassword(string passwordAnswer);
        string GetPassword(bool throwOnError);
        string GetPassword(string answer, bool throwOnError);

        bool ChangePassword(string oldPassword, string newPassword);
        bool ChangePassword(string oldPassword, string newPassword, bool throwOnError);
        bool ChangePasswordQuestionAndAnswer(string password, string newPasswordQuestion, string newPasswordAnswer);

        string ResetPassword();
        string ResetPassword(string passwordAnswer);
        string ResetPassword(bool throwOnError);
        string ResetPassword(string passwordAnswer, bool throwOnError);

        bool UnlockUser();
    }
}