using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NetworkProgrammingP47.Models
{
    /// <summary>
    /// Entity/DTO/DataModel - клас для відображення таблиці БД на об'єкт
    /// </summary>
    internal class UserEntity
    {
        public UserEntity()
        {
            
        }
        public UserEntity(SqlDataReader dataReader)
        {
            Id = dataReader.GetGuid("Id");
            Email = dataReader.GetString("Email");
            Name = dataReader.GetString("Name");
            Dk = dataReader.GetString("Dk");
            RegisteredAt = dataReader.GetDateTime("RegAt");
            ConfirmCode = dataReader["Code"] == DBNull.Value ? null :
                dataReader.GetString("Code");
            ConfirmCodeSentAt = dataReader["CodeAt"] == DBNull.Value ? null :
                dataReader.GetDateTime("CodeAt");
        }

        public Guid Id { get; set; }
        public String Email { get; set; } = null!;
        public String Name { get; set; } = null!;
        public String Dk { get; set; } = null!;
        public String? ConfirmCode { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? ConfirmCodeSentAt { get; set; }


    }
}
