using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkProgrammingP47.Services
{
    internal class OtpService
    {
        private static readonly Random random = new();

        public static String ConfirmCode(int length=6)
        {
            ArgumentOutOfRangeException
                .ThrowIfLessThan<int>(length, 0);

            return String.Join("", 
                Enumerable.Range(0,length).Select(_ => (char)random.Next(48,58)));
        }

        public static String TempPassword(int length=6)
        {
            ArgumentOutOfRangeException
                .ThrowIfLessThan<int>(length, 0);

            return String.Join("",
                Enumerable.Range(0, length).Select(_ => (char)random.Next(33, 127)));
        }
    }
}
/* OTP (One Time Password) - сервіс генерації кодів підтвердження 
 * та тимчасових паролів (для "забув пароль")
 */
