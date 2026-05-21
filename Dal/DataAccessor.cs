using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;
using NetworkProgrammingP47.Models;
using NetworkProgrammingP47.Services;

namespace NetworkProgrammingP47.Dal
{
    internal class DataAccessor
    {
        private const String DatabaseName = "NetworkProgrammingP47";
        private readonly String connectionString = BuildConnectionString(DatabaseName);
        private SqlConnection connection = null!;

        public DataAccessor()
        {
            // Підключення до БД
            try
            {
                EnsureDatabase();
                connection = new SqlConnection(connectionString);
                connection.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private static String BuildConnectionString(String databaseName)
        {
            SqlConnectionStringBuilder builder = new()
            {
                DataSource = @"(LocalDB)\MSSQLLocalDB",
                InitialCatalog = databaseName,
                IntegratedSecurity = true,
                TrustServerCertificate = true
            };

            return builder.ConnectionString;
        }

        private static void EnsureDatabase()
        {
            using SqlConnection masterConnection = new(BuildConnectionString("master"));
            masterConnection.Open();

            String sql = @"
                IF DB_ID(@DatabaseName) IS NULL
                BEGIN
                    DECLARE @Sql NVARCHAR(MAX) = N'CREATE DATABASE ' + QUOTENAME(@DatabaseName)
                    EXEC(@Sql)
                END";
            using SqlCommand cmd = new(sql, masterConnection);
            cmd.Parameters.AddWithValue("@DatabaseName", DatabaseName);
            cmd.ExecuteNonQuery();
        }

        public void ConfirmEmail(UserEntity userEntity)
        {
            // метод, що викликається при успішному введені коду підтвердження пошти
            String sql = $"UPDATE Users SET Code = NULL, CodeAt = NULL WHERE Id = '{userEntity.Id}'";
            using SqlCommand cmd = new(sql, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public bool IsEmailUsed(String email)
        {
            String sql = "SELECT COUNT(*) FROM Users u WHERE u.Email = @Email";
            using SqlCommand cmd = new(sql, connection);
            cmd.Parameters.AddWithValue("@Email", email);

            try
            {
                return (int)cmd.ExecuteScalar()! > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public UserEntity? Authenticate(String email, String password)
        {
            /* Оскільки пароль не зберігається у БД, тільки DK, ми не можемо
             * сформувати єдиний запит на перевірку як логіна (email), так і 
             * паролю.
             * В такому разі до запиту включається тільки логін (email), а 
             * правильність паролю перевіряється шляхом розрахунку DK за 
             * переданим паролем та порівнянням його зі збереженим DK
             */
            // email = user' or '1'='1
            // SELECT * FROM Users u WHERE u.Email = '{email}'
            // SELECT * FROM Users u WHERE u.Email = 'user' or '1'='1'

            String sql = $"SELECT * FROM Users u WHERE u.Email = @Email";
            using SqlCommand command = new(sql, connection);
            command.Parameters.AddWithValue("@Email", email);
            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                UserEntity userEntity = new(reader);   // знайшли користувача за email
                // перевіряємо пароль:
                // сіль (у даному разі) - це Id, 
                // виконуємо розрахунок DK з переданого паролю та солі з БД
                String dk = KdfService.Dk(password, userEntity.Id.ToString());
                // порінюємо результат зі збереженим у БД
                if (dk == userEntity.Dk) return userEntity;
            }            
            return null;
        }

        public void AddUser(UserSignupModel model)
        {
            String sql = "INSERT INTO Users(Id, Name, Email, Code, CodeAt, Dk) " +
                "VALUES(@Id, @Name, @Email, @Code, @CodeAt, @Dk)";
            String id = Guid.NewGuid().ToString();
            String dk = KdfService.Dk(model.Password, id);

            using SqlCommand cmd = new(sql, connection);
            cmd.Parameters.AddWithValue("@Id",     id               );
            cmd.Parameters.AddWithValue("@Name",   model.Name       );
            cmd.Parameters.AddWithValue("@Email",  model.Email      );
            cmd.Parameters.AddWithValue("@Code",   model.ConfirmCode);
            cmd.Parameters.AddWithValue("@CodeAt", DateTime.Now     );
            cmd.Parameters.AddWithValue("@Dk",     dk               );

            try { cmd.ExecuteNonQuery(); }
            catch(Exception ex) { Console.WriteLine( ex.Message ); throw; }
        }

        public String? ResetPassword(UserEntity userEntity, String name)
        {
            String sql = "SELECT u.Id FROM Users u WHERE u.Name = @Name and u.Email = @Email";
            using SqlCommand cmd = new(sql, connection);
            cmd.Parameters.AddWithValue("@Name", userEntity.Name);
            cmd.Parameters.AddWithValue("@Email", userEntity.Email);

            String id;
            try { id = (String)cmd.ExecuteScalar(); }
            catch (Exception ex) { Console.WriteLine(ex.Message); throw; }
            if (id == DBNull.Value.ToString())
            {
                return null;
            }

            String newPassword = OtpService.TempPassword();
            String dk = KdfService.Dk(newPassword, id);
            sql = "UPDATE Users SET Code = @Code, CodeAt = @CodeAt, Dk = @Dk WHERE Id = @Id";

            try {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); throw; }

            return newPassword;
        }

        public void ForgotPassword()
        {
            // метод, що викликається при успішному введені коду підтвердження пошти

            Console.WriteLine("Введiть ваш E-mail: ");
        }

        public void InstallTables()
        {
            String sql = @"
                IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Users(
                        [Id]     UNIQUEIDENTIFIER  PRIMARY KEY,
                        [Name]   NVARCHAR(128)     NOT NULL,
                        [Email]  NVARCHAR(256)     NOT NULL  UNIQUE,
                        [Code]   VARCHAR(10)       NULL,
                        [CodeAt] DATETIME2         NULL,
                        [RegAt]  DATETIME2         DEFAULT   CURRENT_TIMESTAMP,
                        [Dk]     CHAR(32)          NOT NULL  --  COMMENT 'Derived Key by RFC 2898'
                    )
                END";
            using SqlCommand cmd = new(sql, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
/* DAL - Data Access Layer - архітектурний шар, що бере на себе
 * формалізм роботи з джерелом даних та перетворення збережених
 * даних у форму об'єктів мови програмування
 * 
 */
