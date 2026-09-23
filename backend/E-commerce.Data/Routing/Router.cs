using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Data.Routing
{
    public static class Router
    {
        const string root = "api";

        public static class Version1
        {
            const string version = "v1";
            const string v1Rule = "/" + root + "/" + version;

            public static class Product
            {
                const string controller = "product";
                const string productRule = "/" + v1Rule + "/" + controller;
                #region Endpoints
                public const string GetPagedProducts = productRule + "/" + "getPagedProducts";
                public const string GetProductById = productRule + "/" + "getProductById/{id}";
                public const string CreateProduct = productRule + "/" + "create";
                public const string UpdateProduct = productRule + "/" + "{id}";
                public const string DeleteProduct = productRule + "/" + "{id}";
                #endregion
            }

            public static class Category
            {
                const string controller = "category";
                const string categoryRule = "/" + v1Rule + "/" + controller;
                public const string GetAll = categoryRule;
                public const string GetById = categoryRule + "/{id}";
                public const string Create = categoryRule;
                public const string Update = categoryRule + "/{id}";
                public const string Delete = categoryRule + "/{id}";
            }

            public static class Order
            {
                const string controller = "order";
                const string orderRule = "/" + v1Rule + "/" + controller;
                public const string Create = orderRule;
                public const string GetMyOrders = orderRule;
                public const string GetDetails = orderRule + "/{orderId}";
                public const string Delete = orderRule + "/{orderId}";
                public const string UpdateStatus = orderRule + "/{orderId}/status";
            }

            public static class User
            {
                const string controller = "user";
                const string userRule = "/" + v1Rule + "/" + controller;
                #region Endpoints
                public const string AddUser = userRule + "/" + "addUser";
                public const string GetPagedUsers = userRule + "/" + "getPagedUsers";
                public const string GetById = userRule + "/" + "getById/{id}";
                #endregion
            }
            public static class Authentication
            {
                const string controller = "auth";
                const string authRule = "/" + v1Rule + "/" + controller;
                #region Endpoints
                public const string SignIn = authRule + "/" + "signIn";
                public const string SignUp = authRule + "/" + "signUp";
                public const string RefreshToken = authRule + "/" + "refreshToken";
                public const string ConfirmEmail = authRule + "/" + "confirmEmail";
                public const string ResetPassword = authRule + "/" + "resetPassword";
                public const string ConfirmCode = authRule + "/" + "confirmCode";
                public const string AddUserToRole = authRule + "/" + "addUserToRole";
                public const string DeleteUserFromRole = authRule + "/" + "deleteUserFromRole";
                #endregion
            }
            public static class Authorization
            {
                const string controller = "authorization";
                const string authRule = "/" + v1Rule + "/" + controller;
                #region Endpoints
                public const string AddRole = authRule + "/" + "addRole";
                public const string EditRole = authRule + "/" + "editRole";
                public const string DeleteRole = authRule + "/" + "deleteRole/{roleId}";
                public const string GetAllRoles = authRule + "/" + "getAllRoles";
                public const string GetRoleById = authRule + "/" + "getRoleById/{roleId}";
                public const string GetManageUserRoleList = authRule + "/" + "getUserRoles/{userId}";
                public const string GetManageUserClaimsList = authRule + "/" + "getUserClaims/{userId}";
                public const string UpdateUserClaims = authRule + "/" + "updateUserClaims";
                #endregion
            }
        }

        }
}
