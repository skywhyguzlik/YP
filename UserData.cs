using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YP
{
    public static class UserData
    {
        /// <summary>
        /// хранение текущего пользователя
        /// </summary>
        public static Users CurrentUser { get; set; } = null;

        public static bool IsAuthenticated => CurrentUser != null;
        public static bool IsAdmin => CurrentUser?.Roles?.RoleName == "Администратор";
        public static bool IsAuthor => CurrentUser?.Roles?.RoleName == "Автор";
        public static bool IsReader => CurrentUser?.Roles?.RoleName == "Читатель";
        public static bool IsFrozen => CurrentUser?.IsFrozen ?? false;
    }
}


