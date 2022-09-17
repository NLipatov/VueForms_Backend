using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;
using VueFormsApi.DataStructures;

namespace VueFormsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VueFormsController : ControllerBase
    {
        [HttpPost]
        [Route("SaveOwner")]
        public async Task<Guid> SaveOwnerAsync(string JsonObject)
        {
            Dictionary<string, string>? values = JsonSerializer.Deserialize<Dictionary<string, string>>(JsonObject);
            if(values != null && values.Any())
            {
                Owner owner = new Owner
                {
                    Id = Guid.NewGuid(),
                    JsonString = JsonObject
                };
                List<Token> tokens = new();
                foreach (var value in values)
                {
                    Token token = new Token
                    {
                        Owner = owner,
                        Value = value.Value,
                    };
                    tokens.Add(token);
                }

                foreach (var token in tokens)
                {
                    if(token.Value.Length == 0)
                    {
                        continue;
                    }

                    Store? store = Mall.stores.FirstOrDefault(x => x.Char == Char.ToLower(token.Value[0]));
                    if (store == null)
                    {
                        store = new Store
                        {
                            Char = Char.ToLower(token.Value[0]),
                            Tokens = new()
                            {
                                token
                            }
                        };
                    }
                    else
                    {
                        store.Tokens.Add(token);
                    }

                    Mall.stores.Add(store);
                }

                return owner.Id;
            }
            else
            {
                throw new ArgumentException("Error: invalid object passed in.");
            }
        }

        [HttpGet]
        [Route("SearchForOwners/{paramString}")]
        public async Task<List<string>> SearchForOwnersAsync(string paramString)
        {
            HashSet<Guid> ownersIncluded = new();

            List<string> keywords = paramString.Split(';').ToList();

            List<string> serializedOwners = new();
            foreach (var rawKeyword in keywords)
            {
                string keyword = rawKeyword.ToLower().Trim();
                Store? targetStore = Mall.stores.FirstOrDefault(x=>x.Char == keyword[0]);
                if(targetStore == null)
                {
                    continue;
                }

                List<Token> tokens = targetStore.Tokens
                    .Where(x=>x.Value.ToLower().Trim().Contains(keyword)).ToList();
                foreach (var token in tokens)
                {
                    if(!ownersIncluded.Contains(token.Owner.Id))
                    {
                        serializedOwners.Add(token.Owner.JsonString);
                        ownersIncluded.Add(token.Owner.Id);
                    }
                }
            }

            return serializedOwners;
        }
    }
}
