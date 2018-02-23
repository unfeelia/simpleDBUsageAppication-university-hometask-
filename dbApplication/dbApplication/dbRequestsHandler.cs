using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace dbApplication
{
    class dbRequestsHandler
    {
        private static dbRequestsHandler instance;
        private static string nickName;
        private static string id;
        private dbRequestsHandler(string nick)
        {
            nickName = nick;
        }

        public dbRequestsHandler createInstance(string name, string password)
        {
            verifyUser(name, password);
            if (instance == null)
            {
                instance = new dbRequestsHandler(name);
                return instance;
            }
            else
            {
                return instance;
            }
        }
        public static void verifyUser(string nickName, string password)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;
                        command.CommandText = string.Concat("SELECT id FROM users WHERE (nickname = '", nickName, "');");


                        var id = "";
                        var hashedPassword = "";
                        NpgsqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            id = reader["id"].ToString();
                        }
                        reader.Close();

                        command.CommandText = string.Concat("SELECT password FROM passwords WHERE (user_id = '", id, "');");

                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            hashedPassword = reader["password"].ToString();
                        }
                        reader.Close();
                        transaction.Commit();

                        byte[] hashBytes = Convert.FromBase64String(hashedPassword);
                        /* Get the salt */
                        byte[] salt = new byte[16];
                        Array.Copy(hashBytes, 0, salt, 0, 16);
                        /* Compute the hash on the password the user entered */
                        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
                        byte[] hash = pbkdf2.GetBytes(20);
                        /* Compare the results */
                        for (int i = 0; i < 20; i++)
                            if (hashBytes[i + 16] != hash[i])
                                throw new UnauthorizedAccessException();

                        dbRequestsHandler.nickName = nickName;
                        dbRequestsHandler.id = id;

                        MessageBox.Show("You are logged succesessfully");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        public static void verifyUser(string password)
        {
            if (nickName != null)
            {
                verifyUser(dbRequestsHandler.nickName, password);
            }
            else
            {
                throw new Exception("You are not authorized");
            }
        }
        public dbRequestsHandler getInstance()
        {
            if (instance == null)
            {
                throw new Exception("Unautorized request");
            }
            return instance;
        }

        public static void addIngredient(string name, string description, string proteinsValue, string fatsValues, string carbohydratesValue, string caloricityValue)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        command.Connection = con;
                        command.CommandText = string.Concat("INSERT INTO ingredients VALUES ('", name, "', '", description, "', '", proteinsValue, "', '", fatsValues, "', '", carbohydratesValue, "', '", caloricityValue, "', '", id, "');");
                        command.ExecuteNonQuery();
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        public static void updateIngredient(string currentName, string name, string description, string proteinsValue, string fatsValues, string carbohydratesValue, string caloricityValue)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        command.Connection = con;
                        command.CommandText = string.Concat("UPDATE ingredients SET ", "name = '", name, "', description = '", description, "', proteins = '", proteinsValue, "', fats = '", fatsValues, "', carbohydrates = '", carbohydratesValue, "', caloricity = '", caloricityValue, "' WHERE (name = '", currentName, "' AND user_id = '", id, "');");

                        MessageBox.Show(command.CommandText);
                        command.ExecuteNonQuery();
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
        public static void removeIngredient(string name)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;
                        command.CommandText = string.Concat("DELETE FROM ingredients WHERE (name = '", name, "' AND user_id = '", id, "');");
                        command.ExecuteNonQuery();

                        nickName = null;
                        id = null;
                        instance = null;

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        public static void selectAllIngredients(out IList<Ingredient> ingredients)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;

                        command.CommandText = string.Concat("SELECT * FROM ingredients;");

                        ingredients = new List<Ingredient>();
                        NpgsqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            Ingredient ingr = new Ingredient();
                            ingr.caloricity = reader["caloricity"].ToString();
                            ingr.carbohydrates = reader["carbohydrates"].ToString();
                            ingr.description = reader["description"].ToString();
                            ingr.fats = reader["fats"].ToString();
                            ingr.id = reader["id"].ToString();
                            ingr.name = reader["name"].ToString();
                            ingr.proteins = reader["proteins"].ToString();
                            ingr.user_id = reader["user_id"].ToString();
                            ingredients.Add(ingr);
                        }
                        reader.Close();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        public static void selectAllMineIngredients(out IList<Ingredient> ingredients)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;

                        command.CommandText = string.Concat("SELECT * FROM ingredients WHERE (user_id = '", id, "');");

                        ingredients = new List<Ingredient>();
                        NpgsqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            Ingredient ingr = new Ingredient();
                            ingr.caloricity = reader["caloricity"].ToString();
                            ingr.carbohydrates = reader["carbohydrates"].ToString();
                            ingr.description = reader["description"].ToString();
                            ingr.fats = reader["fats"].ToString();
                            ingr.id = reader["id"].ToString();
                            ingr.name = reader["name"].ToString();
                            ingr.proteins = reader["proteins"].ToString();
                            ingr.user_id = reader["user_id"].ToString();
                            ingredients.Add(ingr);
                        }
                        reader.Close();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }

        private static void AddUser(string name, string hashedPassword, string email)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;
                        command.CommandText = string.Concat("INSERT INTO users VALUES ('", name, "', '", email, "');");
                        command.ExecuteNonQuery();

                        command.CommandText = string.Concat("SELECT id FROM users WHERE (nickname = '", name, "');");


                        var id = "";
                        NpgsqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            id = reader["id"].ToString();
                        }
                        reader.Close();

                        command.CommandText = string.Concat("INSERT INTO passwords VALUES ('", id, "', '", hashedPassword, "');");
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        public static void addUser(string name, string password, string email)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            string savedPasswordHash = Convert.ToBase64String(hashBytes);
            try
            {
                AddUser(name, savedPasswordHash, email);
                MessageBox.Show("You are registrated succesessfully");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }
            //throw new Exception("Trying to authorize when it is already done");
        }
        public static void updatePassword(string newPassword, string oldPassword)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        verifyUser(nickName, oldPassword);

                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;
                        //command.CommandText = string.Concat("UPDATE passwords SET ", "password = '", newPassword, "' WHERE (user_id = '", id, "');");
                        //command.CommandText = string.Concat("DELETE FROM users WHERE(id = '", id, "');");
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        public static void updateNickname(string newNickName)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;
                        command.CommandText = string.Concat("UPDATE users SET ", "nickname = '", newNickName, "' WHERE (id = '", id, "');");
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        public static void updateEmail(string newEmail)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;
                        command.CommandText = string.Concat("UPDATE users SET ", "email_address = '", newEmail, "' WHERE (id = '", id, "');");
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        public static void removeUser(string password)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        verifyUser(nickName, password);

                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;
                        command.CommandText = string.Concat("DELETE FROM users WHERE (id = '", id, "');");
                        command.ExecuteNonQuery();
                        command.CommandText = string.Concat("DELETE FROM passwords WHERE (user_id = '", id, "');");
                        command.ExecuteNonQuery();

                        nickName = null;
                        id = null;
                        instance = null;

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        public static void selectAllUsers(out List<User> users)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;

                        command.CommandText = string.Concat("SELECT * FROM users;");

                        users = new List<User>();
                        NpgsqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            User user = new User();
                            user.id = reader["id"].ToString();
                            user.nickname = reader["nickname"].ToString();
                            user.email_address = reader["email_address"].ToString();
                            users.Add(user);
                        }
                        reader.Close();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        private static User selectUser(string id)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;

                        command.CommandText = string.Concat("SELECT * FROM users WHERE(id = '", id, "');");

                        //friends = new List<User>();
                        NpgsqlDataReader reader = command.ExecuteReader();
                        User user = new User();
                        while (reader.Read())
                        {
                            user.id = reader["id"].ToString();
                            user.nickname = reader["nickname"].ToString();
                            user.email_address = reader["email_address"].ToString();
                        }
                        reader.Close();
                        transaction.Commit();
                        return user;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        public static void selectAllFriends(out List<User> friends)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;

                        //selectAllUsers(out friends);

                        command.CommandText = string.Concat("SELECT * FROM users_friends WHERE(user_id = '", id, "' OR friend_id = '", id, "');");

                        friends = new List<User>();
                        NpgsqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            User user = new User();
                            if (reader["user_id"].ToString() != id)
                            {
                                user = selectUser(reader["user_id"].ToString());
                            }
                            else
                            {
                                user = selectUser(reader["friend_id"].ToString());
                            }
                            friends.Add(user);
                        }
                        reader.Close();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        public static void selectAllRequesters(out List<User> requesters)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;

                        //selectAllUsers(out friends);

                        command.CommandText = string.Concat("SELECT from_id FROM friendship_requests WHERE(to_id = '", id, "');");

                        requesters = new List<User>();
                        NpgsqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            User user = new User();
                            user = selectUser(reader["from_id"].ToString());
                            requesters.Add(user);
                        }
                        reader.Close();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        public static User selectCurrentUser()
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;

                        command.CommandText = string.Concat("SELECT * FROM users WHERE(id = '", id, "');");

                        //friends = new List<User>();
                        NpgsqlDataReader reader = command.ExecuteReader();
                        User user = new User();
                        while (reader.Read())
                        {
                            user.id = reader["id"].ToString();
                            user.nickname = reader["nickname"].ToString();
                            user.email_address = reader["email_address"].ToString();
                        }
                        reader.Close();
                        transaction.Commit();
                        return user;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }

        public static void acceptFriendship(string from_id)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;

                        command.CommandText = string.Concat("SELECT id FROM friendship_requests WHERE (to_id = '", id, " AND from_id = '", from_id, "');");

                        bool isWhereRequest = false;
                        NpgsqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            isWhereRequest = true;
                            //id = reader["id"].ToString();
                        }
                        reader.Close();

                        if (isWhereRequest)
                        {
                            command.CommandText = string.Concat("DELETE FROM friendship_requests WHERE (to_id = '", id, " AND from_id = '", from_id, "');");
                            command.ExecuteNonQuery();
                            command.CommandText = string.Concat("INSERT INTO users_friends VALUES ('", id, "', '", from_id, "');");
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        public static void refuseFromFriendship(string friend_id)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;
                        command.CommandText = string.Concat("DELETE FROM users_friends WHERE (user_id = '", id, " AND friend_id = '", friend_id, "');");
                        command.ExecuteNonQuery();
                        command.CommandText = string.Concat("DELETE FROM users_friends WHERE (user_id = '", friend_id, " AND friend_id = '", id, "');");
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }

        public static void createRecipe(string category_id, string name, string publicity, string description)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        command.Connection = con;
                        command.CommandText = string.Concat("INSERT INTO recipe VALUES ('", id, "', '", category_id, "', '", name, "', '", publicity, "', '", description, "');");
                        command.ExecuteNonQuery();
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        public static void updateRecipe(string currentName, string category_id, string name, string publicity, string description)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        command.Connection = con;
                        command.CommandText = string.Concat("UPDATE recipe SET ", "name = '", name, "', description = '", description, "', publicity = '", publicity, "', category_id = '", category_id, "' WHERE (name = '", currentName, "' AND user_id = '", id, "');");
                        MessageBox.Show(command.CommandText);
                        command.ExecuteNonQuery();
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
        public static void addIngredientToRecipe(string recipe_id, string ingredient_id)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;

                        command.CommandText = string.Concat("SELECT user_id FROM recipes WHERE (user_id = '", id, " AND id = '", recipe_id, "');");

                        bool isItMineRecipe = false;
                        NpgsqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            isItMineRecipe = true;
                            //id = reader["id"].ToString();
                        }
                        reader.Close();

                        if (isItMineRecipe)
                        {
                            command.CommandText = string.Concat("INSERT INTO recipes_compositions VALUES ('", recipe_id, "', '", ingredient_id, "');");
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        public static void removeIngredientFromRecipe(string recipe_id, string ingredient_id)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;

                        command.CommandText = string.Concat("SELECT user_id FROM recipes WHERE (user_id = '", id, " AND id = '", recipe_id, "');");

                        bool isItMineRecipe = false;
                        NpgsqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            isItMineRecipe = true;
                            //id = reader["id"].ToString();
                        }
                        reader.Close();

                        if (isItMineRecipe)
                        {
                            command.CommandText = string.Concat("DELETE FROM recipes_compositions WHERE (recipe_id = '", recipe_id, "' AND ingredient_id = '", ingredient_id, "');");
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        public static void removeRecipe(string recipe_id)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;

                        command.CommandText = string.Concat("SELECT user_id FROM recipes WHERE (user_id = '", id, " AND id = '", recipe_id, "');");

                        bool isItMineRecipe = false;
                        NpgsqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            isItMineRecipe = true;
                            //id = reader["id"].ToString();
                        }
                        reader.Close();

                        if (isItMineRecipe)
                        {
                            command.CommandText = string.Concat("DELETE FROM recipes WHERE (recipe_id = '", recipe_id, "');");
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }

        public static void addFavoriteRecipe(string recipe_id)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;
                        command.CommandText = string.Concat("INSERT INTO favorites_recipes VALUES ('", id, "', '", recipe_id, "');");
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }
        public static void removeFavoriteRecipe(string recipe_id)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(DBConnection.connString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand())
                {
                    try
                    {
                        con.Open();
                        var transaction = con.BeginTransaction();
                        command.Connection = con;
                        command.Transaction = transaction;
                        command.CommandText = string.Concat("DELETE FROM favorites_recipes WHERE (recipe_id = '", recipe_id, " AND user_id = '", id, "');");
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }
                }
            }
        }

    }


    class Recipe
    {
        public string user_id;
        public string category_id;
        public string name;
        public string publicity;
        public string description;
        public string id;
        public Recipe()
        {

        }
    }

    class Ingredient
    {
        public string user_id;
        public string fats;
        public string name;
        public string proteins;
        public string description;
        public string carbohydrates;
        public string caloricity;
        public string id;
        public Ingredient()
        {

        }
    }
    class User
    {
        public string nickname;
        public string email_address;
        public string id;

        public override string ToString()
        {
            return nickname.ToString();
        }

        public User()
        {

        }
    }
    class RecipeCategory
    {
        public string description;
        public string id;
        public RecipeCategory()
        {

        }
    }
}
