using System;
using System.Collections.Generic;
using System.Text;

namespace ClothesShop.Utility
{
    public static class SD
    {

        public const string Role_User_Indi = "Individual Customer";
        //public const string Role_User_Comp = "Company Customer";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";

        public const string ssShoppingCart = "Shoping Cart Session";

        public const string StatusPending = "Очікується";
        public const string StatusApproved = "НОВЕ";
        public const string StatusInProcess = "В обробці";
        public const string StatusShipped = "Завершено";
        public const string StatusCancelled = "Скасовано";
        public const string StatusRefunded = "Відшкодовано";

        public const string PaymentStatusPending = "Очікується";
        public const string PaymentStatusApproved = "Підтверджено";
        public const string PaymentStatusDelayedPayment = "Підтверджено";
        public const string PaymentStatusRejected = "Відхилено";

        public static string ConvertToRawHtml(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }
    }
}
