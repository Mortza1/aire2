using aire2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/addUser")]
    public class YourController : ControllerBase
    {
        private readonly FirestoreDb _firestoreDb;

        public YourController(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }
        static string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder stringBuilder = new StringBuilder();

            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(chars.Length);
                stringBuilder.Append(chars[index]);
            }

            return stringBuilder.ToString();
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] AddUserModel model)
        {
            try
            {
                CollectionReference collection = _firestoreDb.Collection("users");
                DocumentReference document = collection.Document(model.UserEmail);

                Dictionary<string, string> user = new Dictionary<string, string>
        {
            { "full_name", model.UserName },
            { "age", model.UserAge.ToString() },
            { "examSeries", model.UserExamDate }
        };

                QuerySnapshot snapshot = await collection.GetSnapshotAsync();
                List<string> referrals = snapshot.Documents
                    .Select(document => document.GetValue<string>("referralID"))
                    .ToList();

                string referralID = GenerateUniqueReferralID(referrals);
                user.Add("referralID", referralID);

                DocumentReference reference = document.Collection("Completed").Document("Physics");
                Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "1", new List<object>() }
        };

                await Task.WhenAll(
                    reference.SetAsync(data),
                    document.SetAsync(user)
                );

                if (!string.IsNullOrEmpty(model.ReferrerEmail))
                {
                    DocumentReference referrer = collection.Document(model.ReferrerEmail);
                    await referrer.UpdateAsync(new Dictionary<string, object>
            {
                { "referrals", FieldValue.Increment(1) }
            });
                }

                return Ok("success");
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                return StatusCode(500, ex.Message);
            }
        }

        private string GenerateUniqueReferralID(List<string> existingReferrals)
        {
            string referralID = GenerateRandomString(15);
            while (existingReferrals.Contains(referralID))
            {
                referralID = GenerateRandomString(15);
            }
            return referralID;
        }


        // Define your API endpoints here
    }
}
