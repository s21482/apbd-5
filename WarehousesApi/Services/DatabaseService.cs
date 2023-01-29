using WarehousesApi.Models;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace WarehousesApi.Services
{
    public interface IDatabaseService
    {
        Task<Response> AddProductToWarehouse(ProductWarehouse productWarehouse);
        Task<int> AddProductToWarehouseByStoredProcedure(ProductWarehouse productWarehouse);
    }

    public class DatabaseService : IDatabaseService
    {
        string conString = "Data Source=db-mssql16.pjwstk.edu.pl;Initial Catalog=s21482;Integrated Security=True";

        public async Task<Response> AddProductToWarehouse(ProductWarehouse productWarehouse)
        {

            using var con = new SqlConnection(conString);
            using var com = new SqlCommand("", con);

            await con.OpenAsync();
            DbTransaction tran = await con.BeginTransactionAsync();
            com.Transaction = (SqlTransaction)tran;

            try
            {
                if (productWarehouse.Amount <= 0)
                {
                    tran.Rollback();
                    return new Response(400, "Amount must be greater than 0");
                }

                com.CommandText = "SELECT COUNT(*) FROM Product WHERE IdProduct=@IdProduct";
                com.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                int productCount = (int)await com.ExecuteScalarAsync();
                com.Parameters.Clear();

                if (productCount == 0)
                {
                    tran.Rollback();
                    return new Response(404, "Product with this id does not exist");
                }

                com.CommandText = "SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse=@IdWarehouse";
                com.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
                int warehouseCount = (int)await com.ExecuteScalarAsync();
                com.Parameters.Clear();

                if (warehouseCount == 0)
                {
                    tran.Rollback();
                    return new Response(404, "Warehouse with this id does not exist");
                }

                com.CommandText = "SELECT IdOrder FROM [Order] WHERE IdProduct=@IdProduct AND Amount=@Amount AND CreatedAt < @CreatedAt";
                com.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                com.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
                com.Parameters.AddWithValue("@CreatedAt", productWarehouse.CreatedAt);
                int? idOrder = (int?)await com.ExecuteScalarAsync();
                com.Parameters.Clear();

                if (idOrder == 0)
                {
                    tran.Rollback();
                    return new Response(404, "Order was not found");
                }

                com.CommandText = "SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder=@IdOrder";
                com.Parameters.AddWithValue("@IdOrder", idOrder);
                int orderCompletedCount = (int)await com.ExecuteScalarAsync();
                com.Parameters.Clear();

                if (orderCompletedCount > 0)
                {
                    tran.Rollback();
                    return new Response(300, "Order has already been completed");
                }

                com.CommandText = "UPDATE [Order] SET FulfilledAt=@FulfilledAt WHERE IdOrder=@IdOrder";
                com.Parameters.AddWithValue("@FulfilledAt", productWarehouse.CreatedAt);
                com.Parameters.AddWithValue("@IdOrder", idOrder);
                await com.ExecuteNonQueryAsync();
                com.Parameters.Clear();

                com.CommandText = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
                com.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                decimal price = Convert.ToDecimal(await com.ExecuteScalarAsync());

                com.Parameters.Clear();
                com.CommandText = "INSERT INTO Product_Warehouse (IdProduct, IdWarehouse, Amount, CreatedAt, Price, IdOrder) OUTPUT Inserted.IdProductWarehouse VALUES (@IdProduct, @IdWarehouse, @Amount, @CreatedAt, @Price, @IdOrder)";
                com.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                com.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
                com.Parameters.AddWithValue("@IdOrder", idOrder);
                com.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
                com.Parameters.AddWithValue("@CreatedAt", productWarehouse.CreatedAt);
                com.Parameters.AddWithValue("@Price", price * productWarehouse.Amount);
                int id = (int)await com.ExecuteScalarAsync();

                await tran.CommitAsync();

                return new Response(200, id.ToString());
            }
            catch (SqlException exc)
            {
                await tran.RollbackAsync();
            }
            catch (Exception exc)
            {
                await tran.RollbackAsync();
            }

            return new Response(500, "Server error");
        }

        public async Task<int> AddProductToWarehouseByStoredProcedure(ProductWarehouse productWarehouse)
        {
            using var con = new SqlConnection(conString);
            using var com = new SqlCommand("AddProductToWarehouse", con);
            com.CommandType = CommandType.StoredProcedure;

            com.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
            com.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
            com.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
            com.Parameters.AddWithValue("@CreatedAt", productWarehouse.CreatedAt);
            await con.OpenAsync();
            decimal result = (decimal)await com.ExecuteScalarAsync();

            return Decimal.ToInt32(result);
        }

    }
    

    public class Response
    {

        public int Status { get; set; }
        public string Message { get; set; }

        public Response(int status, string message)
        {
            Status = status;
            Message = message;
        }
    }
}
